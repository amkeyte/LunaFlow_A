using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// 1.d20's declarative rules — challenge-prompt (9), win-line (8), and <c>GameEnd</c> — authored
    /// as <see cref="RulePrototype"/>s through <see cref="RuleExport"/> per Phase 2.5's load-bearing
    /// condition (see 1-d20-spec.md's Purpose section). <c>check</c>/<c>fight</c>/<c>flee</c>/
    /// <c>move</c>/<c>look</c> are hand-written <c>ICommandHandler</c>s instead — see the
    /// <c>Handlers</c> folder — and are not part of this group.
    /// </summary>
    [RuleGroup("OneD20")]
    public static partial class OneD20RuleGroup
    {
        private static readonly int[] CombatRoomIds = { 1, 5, 8 };

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
        /// Builds a room's challenge-prompt <see cref="RulePrototype"/>: announces the room's
        /// description while the player is there and it's neither solved nor failed, and no win
        /// has fired yet. Combat rooms additionally announce <c>monster-desc</c> under the same
        /// guard — no separate first-visit tracking fact, per 1-d20-spec.md.
        /// </summary>
        private static RulePrototype ChallengeRule(int roomId)
        {
            // NRules' Then().Do() takes an Expression<Action<IContext>> — a statement-bodied
            // lambda ({ a; b; }) can't convert to an expression tree, so a combat room's second
            // Announce is a second chained .Do() (each one a single-expression lambda), not one
            // block-bodied Do().
            var thenStatements = new List<string> { "Do(ctx => LunaFlowEngine.Host.Announce(OneD20Flavor.Text[room.DescriptionKey]))" };
            if (CombatRoomIds.Contains(roomId))
                thenStatements.Add("Do(ctx => LunaFlowEngine.Host.Announce(OneD20Flavor.Text[\"monster-desc\"]))");

            return new RulePrototype
            {
                Name = $"Room{roomId}Challenge",
                Description = $"Announces room {roomId}'s challenge prompt while unsolved/unfailed and no win yet.",
                RequiredUsings = new() { "LunaFlow.Engine", "LunaFlow.Rom.OneD20" },
                PreambleStatements = new() { "Room room = null;" },
                WhenStatements = new()
                {
                    $"Match<PlayerPosition>(p => p.RoomId == {roomId})",
                    $"Match<Room>(() => room, r => r.Id == {roomId})",
                    $"Not<RoomSolved>(rs => rs.RoomId == {roomId})",
                    $"Not<RoomFailed>(rf => rf.RoomId == {roomId})",
                    "Not<WinMarker>()",
                },
                ThenStatements = thenStatements,
            };
        }

        /// <summary>Room 1's challenge-prompt rule (Fight).</summary>
        [RuleTemplate] public static RulePrototype Room1Challenge => ChallengeRule(1);
        /// <summary>Room 2's challenge-prompt rule (SkillCheck: Search).</summary>
        [RuleTemplate] public static RulePrototype Room2Challenge => ChallengeRule(2);
        /// <summary>Room 3's challenge-prompt rule (SkillCheck: Climb).</summary>
        [RuleTemplate] public static RulePrototype Room3Challenge => ChallengeRule(3);
        /// <summary>Room 4's challenge-prompt rule (SkillCheck: Listen).</summary>
        [RuleTemplate] public static RulePrototype Room4Challenge => ChallengeRule(4);
        /// <summary>Room 5's challenge-prompt rule (Fight, the start room).</summary>
        [RuleTemplate] public static RulePrototype Room5Challenge => ChallengeRule(5);
        /// <summary>Room 6's challenge-prompt rule (SkillCheck: Swim).</summary>
        [RuleTemplate] public static RulePrototype Room6Challenge => ChallengeRule(6);
        /// <summary>Room 7's challenge-prompt rule (SkillCheck: Disable Device).</summary>
        [RuleTemplate] public static RulePrototype Room7Challenge => ChallengeRule(7);
        /// <summary>Room 8's challenge-prompt rule (Fight).</summary>
        [RuleTemplate] public static RulePrototype Room8Challenge => ChallengeRule(8);
        /// <summary>Room 9's challenge-prompt rule (SkillCheck: Jump).</summary>
        [RuleTemplate] public static RulePrototype Room9Challenge => ChallengeRule(9);

        /// <summary>
        /// Builds a win-line's <see cref="RulePrototype"/>: inserts a <see cref="WinMarker"/> once
        /// every room in <paramref name="line"/> is solved. Unchanged shape from Nine Rooms — a
        /// room that ends up <see cref="RoomFailed"/> instead of <see cref="RoomSolved"/> can never
        /// satisfy this rule's own Match guards, so a failed room permanently kills every line
        /// through it without needing an extra guard here.
        /// </summary>
        private static RulePrototype WinLineRule(string name, int[] line) => new()
        {
            Name = name,
            Description = $"Inserts a WinMarker once rooms {string.Join(", ", line)} are all solved.",
            RequiredUsings = new() { "LunaFlow.Rom.OneD20" },
            WhenStatements = line
                .Select(id => $"Match<RoomSolved>(rs => rs.RoomId == {id})")
                .Append("Not<WinMarker>()")
                .ToList(),
            ThenStatements = new() { $"Do(ctx => ctx.Insert(new WinMarker(new[] {{ {string.Join(", ", line)} }})))" },
        };

        /// <summary>Win line: top row (rooms 1, 2, 3) — touches combat room 1.</summary>
        [RuleTemplate] public static RulePrototype WinLineTopRow => WinLineRule(WinLines[0].Name, WinLines[0].Line);
        /// <summary>Win line: middle row (rooms 4, 5, 6).</summary>
        [RuleTemplate] public static RulePrototype WinLineMiddleRow => WinLineRule(WinLines[1].Name, WinLines[1].Line);
        /// <summary>Win line: bottom row (rooms 7, 8, 9).</summary>
        [RuleTemplate] public static RulePrototype WinLineBottomRow => WinLineRule(WinLines[2].Name, WinLines[2].Line);
        /// <summary>Win line: left column (rooms 1, 4, 7).</summary>
        [RuleTemplate] public static RulePrototype WinLineLeftColumn => WinLineRule(WinLines[3].Name, WinLines[3].Line);
        /// <summary>Win line: middle column (rooms 2, 5, 8) — routes through two combat rooms.</summary>
        [RuleTemplate] public static RulePrototype WinLineMiddleColumn => WinLineRule(WinLines[4].Name, WinLines[4].Line);
        /// <summary>Win line: right column (rooms 3, 6, 9) — the combat-free win path.</summary>
        [RuleTemplate] public static RulePrototype WinLineRightColumn => WinLineRule(WinLines[5].Name, WinLines[5].Line);
        /// <summary>Win line: top-left-to-bottom-right diagonal (rooms 1, 5, 9) — routes through two combat rooms.</summary>
        [RuleTemplate] public static RulePrototype WinLineDiagonalDown => WinLineRule(WinLines[6].Name, WinLines[6].Line);
        /// <summary>Win line: top-right-to-bottom-left diagonal (rooms 3, 5, 7).</summary>
        [RuleTemplate] public static RulePrototype WinLineDiagonalUp => WinLineRule(WinLines[7].Name, WinLines[7].Line);

        /// <summary>Announces the win text and winning line's rooms once any <see cref="WinMarker"/> exists.</summary>
        [RuleTemplate]
        public static RulePrototype GameEndRule => new()
        {
            Name = "GameEnd",
            Description = "Announces win-announce plus the winning line's rooms once any WinMarker exists.",
            RequiredUsings = new() { "LunaFlow.Engine", "LunaFlow.Rom.OneD20" },
            PreambleStatements = new() { "WinMarker marker = null;" },
            WhenStatements = new() { "Match<WinMarker>(() => marker)" },
            ThenStatements = new() { "Do(ctx => LunaFlowEngine.Host.Announce(OneD20Flavor.WinAnnouncement(marker.Line)))" },
        };
    }
}
