using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.Rom.Loot
{
    /// <summary>
    /// Loot's declarative rules: the award rule, split into two mutually-exclusive variants by
    /// Phase 2.11 item 7 (see <see cref="AwardItem"/>/<see cref="AwardItemWithBonus"/> below for why
    /// it's a split, not the separate second rule the spec's Mechanics section sketches).
    /// </summary>
    [RuleGroup("Loot")]
    public static partial class LootRuleGroup
    {
        /// <summary>The fixed bonus item's id — not session-randomized, unlike the monster-drop
        /// table. Program.cs seeds a <see cref="LootPickupText"/> for this id unconditionally (it
        /// isn't derived from any session's monster assignment).</summary>
        public const string BonusItemId = "bonus-charm";

        /// <summary>
        /// Fires once per fight room that both solves and has a loot assignment, <i>while the party
        /// is still below Veteran</i> — see <see cref="AwardItemWithBonus"/> for the Veteran-or-above
        /// half. Skill-check rooms produce IChallengeSolved too (via RoomSolved : IChallengeSolved)
        /// but never have a matching LootAssignment, so neither variant ever fires for them.
        /// </summary>
        [RuleTemplate]
        public static RulePrototype AwardItem => new()
        {
            Name = "AwardItem",
            Description = "Awards the session-assigned item once a fight room's challenge is solved, below Veteran.",
            RequiredUsings = new() { "LunaFlow.Engine", "LunaFlow.Rom.Loot", "LunaFlow.Cartridge" },
            PreambleStatements = new()
            {
                "IChallengeSolved solved = null!;",
                "LootAssignment award = null!;",
                "LootPickupText pickup = null!;",
            },
            WhenStatements = new()
            {
                "Match<IChallengeSolved>(() => solved)",
                "Match<LootAssignment>(() => award, a => a.RoomId == solved.RoomId)",
                "Match<LootPickupText>(() => pickup, lp => lp.ItemId == award.ItemId)",
                "Not<ItemAwarded>(ia => ia.RoomId == solved.RoomId)",
                "Not<IReputationTier>(t => t.Tier == \"Veteran\" || t.Tier == \"Legend\")",
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Insert(new ItemAwarded(award.ItemId, solved.RoomId)))",
                "Do(ctx => LunaFlowEngine.Host.Announce(pickup.Text))",
            },
        };

        /// <summary>
        /// Phase 2.11 item 7's Veteran-or-above half: same trigger as <see cref="AwardItem"/>, but
        /// also inserts <see cref="BonusItemAwarded"/> in the same firing. <b>Deliberately not a
        /// separate rule joining against an already-existing <see cref="ItemAwarded"/></b> (the
        /// spec's Mechanics section sketches exactly that shape) — a separate rule would re-evaluate
        /// every time a new <c>IReputationTier</c> fact is inserted, and NRules' Rete join would pair
        /// that new tier fact against <i>every already-existing</i> <c>ItemAwarded</c>, retroactively
        /// granting a bonus for fights that happened before Veteran was reached. Deciding
        /// bonus-or-not atomically, at the same moment <c>ItemAwarded</c> is created, and mutually
        /// excluding this rule from <see cref="AwardItem"/> via the tier <c>Match</c>/<c>Not</c> pair,
        /// closes that gap by construction — confirmed against the retroactive-grant failure mode
        /// empirically via <c>example/Phase2_11Spike</c> before landing this shape.
        /// </summary>
        [RuleTemplate]
        public static RulePrototype AwardItemWithBonus => new()
        {
            Name = "AwardItemWithBonus",
            Description = "Awards the session-assigned item plus a fixed bonus item once a fight room's challenge is solved, at Veteran or above.",
            RequiredUsings = new() { "LunaFlow.Engine", "LunaFlow.Rom.Loot", "LunaFlow.Cartridge" },
            PreambleStatements = new()
            {
                "IChallengeSolved solved = null!;",
                "LootAssignment award = null!;",
                "LootPickupText pickup = null!;",
                "IReputationTier tier = null!;",
                "LootPickupText bonusPickup = null!;",
            },
            WhenStatements = new()
            {
                "Match<IChallengeSolved>(() => solved)",
                "Match<LootAssignment>(() => award, a => a.RoomId == solved.RoomId)",
                "Match<LootPickupText>(() => pickup, lp => lp.ItemId == award.ItemId)",
                "Not<ItemAwarded>(ia => ia.RoomId == solved.RoomId)",
                "Match<IReputationTier>(() => tier, t => t.Tier == \"Veteran\" || t.Tier == \"Legend\")",
                $"Match<LootPickupText>(() => bonusPickup, lp => lp.ItemId == \"{BonusItemId}\")",
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Insert(new ItemAwarded(award.ItemId, solved.RoomId)))",
                "Do(ctx => LunaFlowEngine.Host.Announce(pickup.Text))",
                $"Do(ctx => ctx.Insert(new BonusItemAwarded(\"{BonusItemId}\", solved.RoomId)))",
                "Do(ctx => LunaFlowEngine.Host.Announce(bonusPickup.Text))",
            },
        };
    }
}
