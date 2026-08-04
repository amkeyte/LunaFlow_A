using LunaFlow.Engine;
using LunaFlow.Engine.Game;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rulebook.OneD20;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.ContentTests.Rom.D20System
{
    /// <summary>
    /// Fixture-level coverage for 1.d20's declarative rules (<see cref="D20SystemRuleGroup"/>),
    /// compiled through the real <see cref="RuleExport.Compile"/> pipeline exactly like
    /// <c>Program.cs</c> does. Facts (<see cref="RoomSolved"/>/<see cref="RoomFailed"/>/
    /// <see cref="WinMarker"/>) are inserted directly, never through a handler — this class is
    /// pure rule-engine semantics: challenge-prompt guards, win-line firing, and the
    /// <see cref="RoomFailed"/>-permanently-blocks-a-line behavior that falls out of the fact
    /// model with no extra guard (see <see cref="D20SystemRuleGroup.WinLineRule"/>'s own doc comment).
    /// Handler-driven behavior (announcements' exact wording tied to real command flow, RNG-backed
    /// outcomes) is <see cref="D20SystemHandlerTests"/>'s job, not this class's.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.7")]
    public class D20SystemRuleFixtureTests
    {
        private static readonly IOneD20WorldModule WorldModule = new DefaultOneD20WorldModule();
        private static System.Reflection.Assembly _compiledD20SystemRules = null!;
        private static System.Reflection.Assembly _compiledGridMapRules = null!;
        private RuleEngine _engine = null!;
        private readonly List<string> _messages = new();

        // Fixed test session assignment — see D20SystemHandlerTests's own copy of this comment for
        // why room 5/1's monster and room 2/3/6/9's challenges are chosen to match legacy values.
        private static readonly IReadOnlyDictionary<int, string> TestChallengeAssignment = new Dictionary<int, string>
        {
            [2] = "search", [3] = "climb", [4] = "listen", [6] = "swim", [7] = "disabledevice", [9] = "jump",
        };
        private static readonly IReadOnlyDictionary<int, string> TestMonsterAssignment = new Dictionary<int, string>
        {
            [1] = "duelist", [5] = "duelist", [8] = "duelist",
        };

        private static string ChallengePromptFor(int roomId) =>
            WorldModule.FlavorText[WorldModule.Challenges[TestChallengeAssignment[roomId]].PromptKey];

        private static string MonsterDescFor(int roomId) =>
            WorldModule.FlavorText[WorldModule.Monsters[TestMonsterAssignment[roomId]].DescriptionKey];

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            // RoslynRuleCompiler only sees already-loaded, file-backed assemblies — force NRules in
            // before the first Compile() call. Same gotcha Program.cs and the B1-B5 suite hit.
            _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
            _ = typeof(System.Linq.Expressions.Expression).Assembly;
            _ = typeof(NRules.RuleModel.IContext).Assembly;
            _ = typeof(NRules.ISession).Assembly;

            // Win-line/GameEnd rules live in GridMapRuleGroup, not D20SystemRuleGroup — both must be
            // compiled and loaded for WinMarker/GameEnd behavior to be reachable from this fixture.
            _compiledD20SystemRules = RuleExport.Compile(typeof(D20SystemRuleGroup));
            _compiledGridMapRules = RuleExport.Compile(typeof(GridMapRuleGroup));
        }

        [TestInitialize]
        public void Setup()
        {
            _engine = new RuleEngine();
            _engine.LoadRulesFromAssembly(_compiledD20SystemRules);
            _engine.LoadRulesFromAssembly(_compiledGridMapRules);

            // Ticket 0043/0050 (Amara's ruling): the challenge-prompt/GameEnd rules have no
            // constructor/DI seam, so they match already-resolved flavor text via
            // RoomChallengePrompt/GameEndAnnouncement facts instead of a placeholder. Seed them here
            // exactly as Program.cs does, so this fixture exercises the real content.
            var flavorText = WorldModule.FlavorText;
            foreach (var room in WorldModule.Rooms.Values)
            {
                _engine.Insert(room);
                _engine.Insert(new RoomChallengePrompt(
                    room.Id,
                    room.ChallengeType == OneD20ChallengeType.Fight
                        ? MonsterDescFor(room.Id)
                        : ChallengePromptFor(room.Id)));
            }
            _engine.Insert(new GameEndAnnouncement(flavorText["win-announce"]));

            _messages.Clear();
            SubscribeToMessages();
        }

        [TestCleanup]
        public void Cleanup()
        {
            UnsubscribeFromMessages();
        }

        private EventHandler<GameEngineMessagePostedEventArgs>? _messageHandler;

        private void SubscribeToMessages()
        {
            // ChallengeRule/GameEndRule call the static LunaFlowEngine.Host.Announce(...), which
            // always resolves to GameEngineCore.Instance — independent of this test's own bare
            // RuleEngine (used only for fact storage/firing here). Subscribe there to observe them.
            _messageHandler = (s, e) => _messages.Add(e.Message);
            GameEngineCore.Instance.MessagePosted += _messageHandler;
        }

        private void UnsubscribeFromMessages()
        {
            if (_messageHandler is not null)
                GameEngineCore.Instance.MessagePosted -= _messageHandler;
            _messageHandler = null;
        }

        [TestMethod]
        public void ChallengePrompt_SkillCheckRoom_AnnouncesPromptKey_WhileUnsolvedAndUnfailed()
        {
            _engine.Insert(new PlayerPosition(2));

            _engine.Fire();

            Assert.AreEqual(1, _messages.Count, "Only room 2's challenge-prompt rule should fire");
            Assert.AreEqual(ChallengePromptFor(2), _messages[0]);
        }

        [TestMethod]
        public void ChallengePrompt_FightRoom_AnnouncesMonsterDesc()
        {
            _engine.Insert(new PlayerPosition(1));

            _engine.Fire();

            Assert.AreEqual(1, _messages.Count, "Only room 1's challenge-prompt rule should fire");
            Assert.AreEqual(MonsterDescFor(1), _messages[0]);
        }

        [TestMethod]
        public void ChallengePrompt_RoomSolved_StopsAnnouncing()
        {
            _engine.Insert(new PlayerPosition(2));
            _engine.Insert(new RoomSolved(2));

            _engine.Fire();

            Assert.AreEqual(0, _messages.Count, "Solved room's challenge-prompt rule must not fire");
        }

        [TestMethod]
        public void ChallengePrompt_RoomFailed_StopsAnnouncing()
        {
            _engine.Insert(new PlayerPosition(1));
            _engine.Insert(new RoomFailed(1));

            _engine.Fire();

            Assert.AreEqual(0, _messages.Count, "Failed room's challenge-prompt rule must not fire");
        }

        [TestMethod]
        public void ChallengePrompt_AfterWinMarker_StopsAnnouncing_EvenForUnrelatedUnsolvedRoom()
        {
            _engine.Insert(new PlayerPosition(4));
            _engine.Insert(new WinMarker(new[] { 3, 6, 9 }));

            _engine.Fire();

            // GameEndRule itself fires (Not<WinMarker>() isn't on it) — confirm room 4's own
            // prompt text specifically never shows up alongside it.
            Assert.IsFalse(_messages.Contains(ChallengePromptFor(4)),
                "Not<WinMarker>() should block every remaining challenge-prompt rule once any line is won");
        }

        [TestMethod]
        public void WinLine_AllThreeRoomsSolved_InsertsWinMarker()
        {
            _engine.Insert(new RoomSolved(3));
            _engine.Insert(new RoomSolved(6));
            _engine.Insert(new RoomSolved(9));

            _engine.Fire();

            var markers = _engine.Query<WinMarker>().ToList();
            Assert.AreEqual(1, markers.Count);
            CollectionAssert.AreEquivalent(new[] { 3, 6, 9 }, markers[0].Line.ToList());
        }

        [TestMethod]
        public void WinLine_CombatRoomInLine_FiresWhenSolvedByFactAlone()
        {
            // Top row [1,2,3] routes through combat room 1 — at the fact level a RoomSolved(1) is
            // indistinguishable whether it came from `fight` or direct insertion; the real
            // fight-driven path is D20SystemHandlerTests' job.
            _engine.Insert(new RoomSolved(1));
            _engine.Insert(new RoomSolved(2));
            _engine.Insert(new RoomSolved(3));

            _engine.Fire();

            var markers = _engine.Query<WinMarker>().Where(m => m.Line.Contains(1)).ToList();
            Assert.AreEqual(1, markers.Count, "WinLineTopRow should fire once all three rooms are solved");
        }

        [TestMethod]
        public void WinLine_RoomFailedInLine_NeverFires_WhileUnrelatedLineStillFires()
        {
            // Top row [1,2,3]: room 1 failed (not solved) — permanently unreachable, no extra guard
            // needed per WinLineRule's own doc comment (Match<RoomSolved> for room 1 can never be
            // satisfied once it's RoomFailed instead).
            _engine.Insert(new RoomFailed(1));
            _engine.Insert(new RoomSolved(2));
            _engine.Insert(new RoomSolved(3));

            // Unrelated line, no shared rooms with the failed one: should fire normally.
            _engine.Insert(new RoomSolved(6));
            _engine.Insert(new RoomSolved(9));

            _engine.Fire();

            var markers = _engine.Query<WinMarker>().ToList();
            Assert.IsFalse(markers.Any(m => m.Line.Contains(1)),
                "Top-row line [1,2,3] must never fire once room 1 is RoomFailed");
            Assert.IsTrue(markers.Any(m => m.Line.SequenceEqual(new[] { 3, 6, 9 })),
                "Right-column line [3,6,9] is unaffected by room 1's failure and should still fire");
        }

        [TestMethod]
        public void GameEnd_AnnouncesOnAnyWinMarker()
        {
            _engine.Insert(new WinMarker(new[] { 3, 6, 9 }));

            _engine.Fire();

            Assert.IsTrue(_messages.Contains(WorldModule.FlavorText["win-announce"]),
                "GameEndRule should announce the real win text on any WinMarker");
        }
    }
}
