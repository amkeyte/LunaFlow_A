using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.Rom.Loot
{
    /// <summary>
    /// Loot's declarative rules: one generic award rule, not a per-room family — the session's
    /// room-to-item assignment is data (LootAssignment), not a content-generation-time constant, so
    /// one rule correlating IChallengeSolved against it covers every fight room.
    /// </summary>
    [RuleGroup("Loot")]
    public static partial class LootRuleGroup
    {
        /// <summary>
        /// Fires once per fight room that both solves and has a loot assignment. Skill-check rooms
        /// produce IChallengeSolved too (via RoomSolved : IChallengeSolved) but never have a
        /// matching LootAssignment, so this rule simply never fires for them.
        /// </summary>
        [RuleTemplate]
        public static RulePrototype AwardItem => new()
        {
            Name = "AwardItem",
            Description = "Awards the session-assigned item once a fight room's challenge is solved.",
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
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Insert(new ItemAwarded(award.ItemId, solved.RoomId)))",
                "Do(ctx => LunaFlowEngine.Host.Announce(pickup.Text))",
            },
        };
    }
}
