using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.Rom.Reputation
{
    /// <summary>
    /// Reputation's declarative rules: two accrual rules (first point vs. subsequent points — NRules
    /// has no "match or default" combinator, so presence/absence of <see cref="ReputationPoints"/>
    /// splits it into two guarded rules, same idiom as the rest of this content) plus two
    /// threshold rules (Veteran at 3, Legend at 6).
    /// </summary>
    [RuleGroup("Reputation")]
    public static partial class ReputationRuleGroup
    {
        /// <summary>Fires once, for the very first solved challenge of the session (no
        /// <see cref="ReputationPoints"/> fact exists yet).</summary>
        [RuleTemplate]
        public static RulePrototype FirstPoint => new()
        {
            Name = "ReputationFirstPoint",
            Description = "Awards the first reputation point on the session's first solved challenge.",
            RequiredUsings = new() { "LunaFlow.Rom.Reputation", "LunaFlow.Cartridge" },
            PreambleStatements = new() { "IChallengeSolved solved = null!;" },
            WhenStatements = new()
            {
                "Match<IChallengeSolved>(() => solved)",
                "Not<ReputationPoints>()",
                "Not<ReputationCounted>(rc => rc.RoomId == solved.RoomId)",
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Insert(new ReputationPoints(1)))",
                "Do(ctx => ctx.Insert(new ReputationCounted(solved.RoomId)))",
            },
        };

        /// <summary>Fires once per solved challenge after the first — replaces the existing
        /// <see cref="ReputationPoints"/> with its total incremented by one. The
        /// <see cref="ReputationCounted"/> guard is load-bearing: without it, retracting/reinserting
        /// <see cref="ReputationPoints"/> would re-pair the fresh fact against every already-solved
        /// room's <c>IChallengeSolved</c>, re-firing this rule once per prior solve instead of once
        /// for the new one (see <c>Facts.cs</c>'s doc comment on <see cref="ReputationCounted"/>).</summary>
        [RuleTemplate]
        public static RulePrototype Accrue => new()
        {
            Name = "ReputationAccrue",
            Description = "Replaces ReputationPoints with its total incremented by one on each subsequent solved challenge.",
            RequiredUsings = new() { "LunaFlow.Rom.Reputation", "LunaFlow.Cartridge" },
            PreambleStatements = new() { "IChallengeSolved solved = null!;", "ReputationPoints pts = null!;" },
            WhenStatements = new()
            {
                "Match<IChallengeSolved>(() => solved)",
                "Match<ReputationPoints>(() => pts)",
                "Not<ReputationCounted>(rc => rc.RoomId == solved.RoomId)",
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Retract(pts))",
                "Do(ctx => ctx.Insert(new ReputationPoints(pts.Total + 1)))",
                "Do(ctx => ctx.Insert(new ReputationCounted(solved.RoomId)))",
            },
        };

        /// <summary>Inserts ReputationTierReached("Veteran") the first time Total reaches 3. Guarded
        /// by Not so it never re-inserts on later solves.</summary>
        [RuleTemplate]
        public static RulePrototype VeteranReached => new()
        {
            Name = "ReputationVeteranReached",
            Description = "Marks the Veteran tier reached once ReputationPoints.Total >= 3.",
            RequiredUsings = new() { "LunaFlow.Rom.Reputation" },
            PreambleStatements = new() { "ReputationPoints pts = null!;" },
            WhenStatements = new()
            {
                "Match<ReputationPoints>(() => pts, p => p.Total >= 3)",
                "Not<ReputationTierReached>(t => t.Tier == \"Veteran\")",
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Insert(new ReputationTierReached(\"Veteran\")))",
            },
        };

        /// <summary>Inserts ReputationTierReached("Legend") the first time Total reaches 6. Guarded
        /// by Not so it never re-inserts on later solves. No distinct Legend-tier payoff is wired
        /// elsewhere — deliberate, per the spec's open questions: proving the relay doesn't need a
        /// third distinct effect.</summary>
        [RuleTemplate]
        public static RulePrototype LegendReached => new()
        {
            Name = "ReputationLegendReached",
            Description = "Marks the Legend tier reached once ReputationPoints.Total >= 6.",
            RequiredUsings = new() { "LunaFlow.Rom.Reputation" },
            PreambleStatements = new() { "ReputationPoints pts = null!;" },
            WhenStatements = new()
            {
                "Match<ReputationPoints>(() => pts, p => p.Total >= 6)",
                "Not<ReputationTierReached>(t => t.Tier == \"Legend\")",
            },
            ThenStatements = new()
            {
                "Do(ctx => ctx.Insert(new ReputationTierReached(\"Legend\")))",
            },
        };
    }
}
