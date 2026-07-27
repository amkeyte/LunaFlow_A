using LunaFlow.Rom.D20SystemInterop;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.Rom.GridMap
{
    /// <summary>
    /// GridMap's declarative rules: 8 win-line rules + GameEndRule.
    /// Win-line rules insert a WinMarker once all three rooms in a line are solved.
    /// GameEndRule announces victory when any WinMarker exists.
    /// </summary>
    [RuleGroup("GridMap")]
    public static partial class GridMapRuleGroup
    {
        private static readonly (string Name, int[] Line)[] WinLines =
        {
            ("WinLineTopRow", new[] { 1, 2, 3 }),
            ("WinLineMiddleRow", new[] { 4, 5, 6 }),
            ("WinLineBottomRow", new[] { 7, 8, 9 }),
            ("WinLineLeftColumn", new[] { 1, 4, 7 }),
            ("WinLineMiddleColumn", new[] { 2, 5, 8 }),
            ("WinLineRightColumn", new[] { 3, 6, 9 }),
            ("WinLineDiagonalDown", new[] { 1, 5, 9 }),
            ("WinLineDiagonalUp", new[] { 3, 5, 7 }),
        };

        /// <summary>
        /// Builds a win-line's RulePrototype: inserts a WinMarker once
        /// every room in the line is solved. A room that ends up failed can never
        /// satisfy this rule's Match guards, so a failed room permanently kills every
        /// line through it without needing an extra guard.
        /// </summary>
        private static RulePrototype WinLineRule(string name, int[] line) => new()
        {
            Name = name,
            Description = $"Inserts a WinMarker once rooms {string.Join(", ", line)} are all solved.",
            RequiredUsings = new() { "LunaFlow.Rom.GridMap", "LunaFlow.Rom.D20SystemInterop" },
            WhenStatements = line
                .Select(id => $"Match<IChallengeSolved>(cs => cs.RoomId == {id})")
                .Append("Not<WinMarker>()")
                .ToList(),
            ThenStatements = new() { $"Do(ctx => ctx.Insert(new WinMarker(new[] {{ {string.Join(", ", line)} }})))" },
        };

        /// <summary>Win line: top row (rooms 1, 2, 3).</summary>
        [RuleTemplate] public static RulePrototype WinLineTopRow => WinLineRule("WinLineTopRow", new[] { 1, 2, 3 });

        /// <summary>Win line: middle row (rooms 4, 5, 6).</summary>
        [RuleTemplate] public static RulePrototype WinLineMiddleRow => WinLineRule("WinLineMiddleRow", new[] { 4, 5, 6 });

        /// <summary>Win line: bottom row (rooms 7, 8, 9).</summary>
        [RuleTemplate] public static RulePrototype WinLineBottomRow => WinLineRule("WinLineBottomRow", new[] { 7, 8, 9 });

        /// <summary>Win line: left column (rooms 1, 4, 7).</summary>
        [RuleTemplate] public static RulePrototype WinLineLeftColumn => WinLineRule("WinLineLeftColumn", new[] { 1, 4, 7 });

        /// <summary>Win line: middle column (rooms 2, 5, 8).</summary>
        [RuleTemplate] public static RulePrototype WinLineMiddleColumn => WinLineRule("WinLineMiddleColumn", new[] { 2, 5, 8 });

        /// <summary>Win line: right column (rooms 3, 6, 9).</summary>
        [RuleTemplate] public static RulePrototype WinLineRightColumn => WinLineRule("WinLineRightColumn", new[] { 3, 6, 9 });

        /// <summary>Win line: diagonal down (rooms 1, 5, 9).</summary>
        [RuleTemplate] public static RulePrototype WinLineDiagonalDown => WinLineRule("WinLineDiagonalDown", new[] { 1, 5, 9 });

        /// <summary>Win line: diagonal up (rooms 3, 5, 7).</summary>
        [RuleTemplate] public static RulePrototype WinLineDiagonalUp => WinLineRule("WinLineDiagonalUp", new[] { 3, 5, 7 });

        /// <summary>
        /// GameEndRule: announces victory once any WinMarker exists.
        /// </summary>
        [RuleTemplate]
        public static RulePrototype GameEndRule => new()
        {
            Name = "GameEnd",
            Description = "Announces victory when any win line is complete.",
            RequiredUsings = new() { "LunaFlow.Engine", "LunaFlow.Rom.GridMap" },
            WhenStatements = new() { "Match<WinMarker>()" },
            ThenStatements = new()
            {
                "Do(ctx => LunaFlowEngine.Host.Announce(\"You win!\\n\"))",
            },
        };
    }
}
