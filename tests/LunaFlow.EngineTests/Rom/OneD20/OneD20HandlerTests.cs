using LunaFlow.Engine;
using LunaFlow.Engine.Game;
using LunaFlow.Rom.OneD20;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.EngineTests.Rom.OneD20
{
    /// <summary>
    /// Handler-level coverage for 1.d20's <c>move</c>/<c>look</c>/<c>check</c>/<c>fight</c>/
    /// <c>flee</c> command handlers, driven through the real <see cref="GameEngineCore"/> singleton
    /// and <see cref="DispatchGameCommand"/> pipeline (dispatch, then <c>Fire()</c>) — the same path
    /// production goes through via <c>ConsoleEngine.Run()</c>. <see cref="LunaFlowEngine.Host"/> is
    /// hard-wired to <see cref="GameEngineCore.Instance"/>, so there is no way to unit-test a
    /// handler in isolation; this is the first handler-level test class in the suite.
    /// <c>check</c>/<c>fight</c> outcomes are forced via the injectable RNG seam from ticket 0019
    /// (<see cref="D20ResolutionMechanic(Random)"/>) rather than retry-looping — seeds below were
    /// found and verified deterministic with a throwaway search against the real class (not
    /// hand-computed), documented per scenario.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.5")]
    [DoNotParallelize] // GameEngineCore.Instance is a singleton; tests must run sequentially
    public class OneD20HandlerTests
    {
        private GameEngineCore _engine = null!;
        private DefaultCommandDispatcher _dispatcher = null!;
        private readonly List<string> _messages = new();
        private EventHandler<GameEngineMessagePostedEventArgs>? _messageHandler;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
            _ = typeof(System.Linq.Expressions.Expression).Assembly;
            _ = typeof(NRules.RuleModel.IContext).Assembly;
            _ = typeof(NRules.ISession).Assembly;
        }

        [TestInitialize]
        public void Setup()
        {
            _engine = GameEngineCore.Instance;
            if (_engine.Running)
                _engine.Stop(force: true);
            _messages.Clear();
            _dispatcher = new DefaultCommandDispatcher();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_messageHandler is not null)
                _engine.MessagePosted -= _messageHandler;
            _messageHandler = null;
            if (_engine.Running)
                _engine.Stop(force: true);
        }

        /// <summary>
        /// Registers the OneD20 handlers (optionally with seeded mechanics for determinism),
        /// subscribes to <c>MessagePosted</c> <em>before</em> <c>Start()</c> (per test-patterns.md
        /// — needed to observe the initial-settle <c>Fire()</c>'s announcements), and starts a
        /// fresh session, replicating Program.cs's own seed-lambda shape exactly (including the
        /// room-5-startup-description trick from ticket 0018's second playtest fix).
        /// </summary>
        private void StartSession(D20ResolutionMechanic? fightMechanic = null, D20ResolutionMechanic? checkMechanic = null)
        {
            _dispatcher.RegisterHandler("move", new MoveCommandHandler());
            _dispatcher.RegisterHandler("look", new LookCommandHandler());
            _dispatcher.RegisterHandler("flee", new FleeCommandHandler());
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(checkMechanic));
            _dispatcher.RegisterHandler("fight", new FightCommandHandler(fightMechanic));

            _messageHandler = (s, e) => _messages.Add(e.Message);
            _engine.MessagePosted += _messageHandler;

            var options = new GameEngineOptions();
            options.Rules
                .FromAssembly(RuleExport.Compile(typeof(OneD20RuleGroup)))
                .Seed(() => OneD20Content.Rooms.Values)
                .Seed(() =>
                {
                    LunaFlowEngine.Host.Announce(OneD20Flavor.Text[OneD20Content.Rooms[OneD20Content.StartRoomId].DescriptionKey]);
                    return new object[]
                    {
                        new PlayerPosition(OneD20Content.StartRoomId),
                        OneD20Content.PlayerCharacter,
                        OneD20Content.Monster,
                    };
                });
            _engine.SetOptions(options);
            _engine.Start();
        }

        /// <summary>Submits <paramref name="commandLine"/> through the real dispatch+Fire pipeline and waits for completion.</summary>
        private void Submit(string commandLine)
        {
            var command = new DefaultCommand(commandLine);
            using var completed = new ManualResetEventSlim(false);
            _engine.Submit(new DispatchGameCommand(command, _dispatcher, completed));
            Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(2)), $"Command '{commandLine}' should complete");
        }

        private List<string> MessagesSince(int baseline) => _messages.Skip(baseline).ToList();

        private CombatRound CombatRoundInRoom(int roomId) =>
            _engine.Rules.Query<CombatRound>().First(cr => cr.RoomId == roomId);

        // ---- move / look parity (criteria 1, 1b) ----

        [TestMethod]
        public void Move_MatchesNineRoomsAdjacency_ValidAndRejected()
        {
            StartSession();
            var baseline = _messages.Count;

            Submit("move north"); // 5 -> 2 (row-1 to row-0)

            Assert.AreEqual(2, _engine.Rules.Query<PlayerPosition>().First().RoomId);
            Assert.IsTrue(MessagesSince(baseline).Contains(OneD20Flavor.Text["room-2-desc"]));

            baseline = _messages.Count;
            Submit("move north"); // off-grid from row 0: rejected

            Assert.AreEqual(2, _engine.Rules.Query<PlayerPosition>().First().RoomId, "Rejected move must not change position");
            Assert.IsTrue(MessagesSince(baseline).Any(m => m.Contains("can't go")));
        }

        [TestMethod]
        public void Move_TracksPreviousRoomId_UpdatingOnEachSuccessfulMove()
        {
            StartSession();

            Submit("move north"); // 5 -> 2
            Assert.AreEqual(5, _engine.Rules.Query<PreviousRoomId>().First().RoomId);

            Submit("move south"); // 2 -> 5
            Assert.AreEqual(2, _engine.Rules.Query<PreviousRoomId>().First().RoomId,
                "PreviousRoomId should update to the room just left, not stay pinned to the first move");
        }

        // ---- description-once / prompt-once (criterion 2) ----

        [TestMethod]
        public void EnterSkillCheckRoom_AnnouncesDescriptionOnce_AndPromptOnce_NeverDuplicated()
        {
            StartSession();
            var baseline = _messages.Count;

            Submit("move north"); // 5 -> 2, a SkillCheck room

            var entered = MessagesSince(baseline);
            Assert.AreEqual(1, entered.Count(m => m == OneD20Flavor.Text["room-2-desc"]),
                "DescriptionKey should announce exactly once, from the handler");
            Assert.AreEqual(1, entered.Count(m => m == OneD20Flavor.Text["room-2-prompt"]),
                "PromptKey should announce exactly once, from the rule");
            Assert.AreNotEqual(OneD20Flavor.Text["room-2-desc"], OneD20Flavor.Text["room-2-prompt"],
                "Description and prompt must never be identical text");
        }

        // ---- room-5 startup announcement (criterion 3) ----

        [TestMethod]
        public void SessionStart_AnnouncesRoom5Description_BeforeMonsterDesc()
        {
            StartSession();

            var descIndex = _messages.IndexOf(OneD20Flavor.Text["room-5-desc"]);
            var monsterIndex = _messages.IndexOf(OneD20Flavor.Text["monster-desc"]);

            Assert.AreNotEqual(-1, descIndex, "Room 5's description should be announced at startup");
            Assert.AreNotEqual(-1, monsterIndex, "Room 5 is a Fight room; monster-desc should be announced at startup too");
            Assert.IsTrue(descIndex < monsterIndex, "Description must print before monster-desc, not just be present");
        }

        // ---- fight: lethal PC hit (criteria 4, 10) ----
        // Seed 0: a fresh Random(0)'s first d20 roll is 15 (verified). Against PC AttackBonus 4 vs
        // Monster Defense 13 that's a hit (total 19); damage roll (1d8) comes out 7, +3 bonus = 10,
        // which exceeds the monster's 8 MaxHp in one blow.

        [TestMethod]
        public void Fight_LethalPcHit_AnnouncesMonsterDefeatedOnly_SolvesRoom_SkipsCounterattack()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new Random(0)));
            var baseline = _messages.Count;

            Submit("fight"); // room 5, fresh combat

            var round = MessagesSince(baseline);
            Assert.IsTrue(round.Contains(OneD20Flavor.Text["monster-defeated"]));
            Assert.IsFalse(round.Any(m => m.Contains(OneD20Flavor.Text["pc-hit"])),
                "A lethal PC hit must never announce pc-hit first");
            Assert.IsFalse(round.Any(m => m.Contains(OneD20Flavor.Text["monster-miss"]) || m.Contains(OneD20Flavor.Text["monster-hit"])),
                "Counterattack step must be skipped entirely when the PC's hit ends the fight");
            Assert.IsTrue(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 5));
            Assert.IsFalse(_engine.Rules.Query<CombatRound>().Any(cr => cr.RoomId == 5));
        }

        // ---- fight: lethal monster hit across two rounds (criteria 4, 11) ----
        // Round 1, seed 41: PC misses (d20=3, total 7 < 13); monster hits (d20=17, total 20 >= 11)
        // for exactly 4 damage (1d6=3, +1) -> PlayerHp 11 -> 7.
        // Round 2, seed 3 (fresh instance): PC misses again (d20=6, total 10 < 13); monster hits
        // (d20=14, total 17 >= 11) for exactly 7 damage (1d6=6, +1, the die's max) -> PlayerHp 7 -> 0.

        [TestMethod]
        public void Fight_LethalMonsterHit_AcrossTwoRounds_AnnouncesPcLostOnly_FailsRoom_SessionContinues()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new Random(41)));

            Submit("fight"); // round 1: PC misses, monster hits for 4
            var round1 = CombatRoundInRoom(5);
            Assert.AreEqual(7, round1.PlayerHp);
            Assert.AreEqual(8, round1.MonsterHp, "Monster HP untouched — PC missed this round");

            _dispatcher.RegisterHandler("fight", new FightCommandHandler(new D20ResolutionMechanic(new Random(3))));
            var baseline = _messages.Count;
            Submit("fight"); // round 2: PC misses, monster's hit drops PlayerHp to 0

            var round2 = MessagesSince(baseline);
            Assert.IsTrue(round2.Contains(OneD20Flavor.Text["pc-lost"]));
            Assert.IsFalse(round2.Any(m => m.Contains(OneD20Flavor.Text["monster-hit"])),
                "A lethal monster hit must never announce monster-hit first");
            Assert.IsTrue(_engine.Rules.Query<RoomFailed>().Any(rf => rf.RoomId == 5));
            Assert.IsFalse(_engine.Rules.Query<CombatRound>().Any(cr => cr.RoomId == 5));

            // Session continues: losing a fight fails only that room, not the run.
            Submit("move north"); // 5 -> 2
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // room 2, Search/Int mod 0 vs DC 10 — seed 0's d20=15 succeeds

            Assert.IsTrue(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 2),
                "Other rooms remain playable after a lost fight elsewhere");
        }

        // ---- fight: 3-round timeout (criterion 12) ----
        // Seed 35, reused across all three Submit calls on the SAME mechanic instance: PC misses
        // and monster misses in every one of the first three rounds (verified) — zero damage either
        // side, so the timeout (not an HP-based loss) is what ends the fight.

        [TestMethod]
        public void Fight_ThreeRoundTimeout_NeitherReducedToZero_FailsRoomPermanently()
        {
            var mechanic = new D20ResolutionMechanic(new Random(35));
            StartSession(fightMechanic: mechanic);

            Submit("fight"); // round 1
            Submit("fight"); // round 2
            var baseline = _messages.Count;
            Submit("fight"); // round 3 -> timeout

            Assert.IsTrue(MessagesSince(baseline).Contains(OneD20Flavor.Text["monster-fled"]));
            Assert.IsTrue(_engine.Rules.Query<RoomFailed>().Any(rf => rf.RoomId == 5));
            Assert.IsFalse(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 5));
            Assert.IsFalse(_engine.Rules.Query<CombatRound>().Any(cr => cr.RoomId == 5));

            baseline = _messages.Count;
            Submit("fight"); // room already failed: no-op

            Assert.AreEqual(0, MessagesSince(baseline).Count, "A failed room's fight must be rejected — nothing to fight");
        }

        // ---- fresh HP seeding, no carryover (criterion 7, ties into 13) ----
        // Seed 1: a fresh Random(1) produces a PC miss (d20=5) and a monster miss (d20=3) —
        // zero damage either side, so CombatRound's seeded values are directly observable.

        [TestMethod]
        public void Fight_FreshRound_SeedsBothCombatantsAtMaxHp_NoCarryoverAfterFlee()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new Random(1)));

            Submit("fight"); // double-miss: HP unchanged, so this IS the seeded value
            var fresh = CombatRoundInRoom(5);
            Assert.AreEqual(11, fresh.PlayerHp, "PlayerHp must seed from PlayerCharacter.Stats.MaxHp");
            Assert.AreEqual(8, fresh.MonsterHp, "MonsterHp must seed from MonsterTemplate.Stats.MaxHp");

            // Take damage, then flee, then retry — the retry must not inherit the damage.
            _dispatcher.RegisterHandler("fight", new FightCommandHandler(new D20ResolutionMechanic(new Random(41))));
            Submit("fight"); // PlayerHp 11 -> 7
            Assert.AreEqual(7, CombatRoundInRoom(5).PlayerHp);

            Submit("flee");
            Assert.IsFalse(_engine.Rules.Query<CombatRound>().Any(cr => cr.RoomId == 5));

            _dispatcher.RegisterHandler("fight", new FightCommandHandler(new D20ResolutionMechanic(new Random(1))));
            Submit("fight"); // fresh fight in the same room after fleeing

            var retried = CombatRoundInRoom(5);
            Assert.AreEqual(11, retried.PlayerHp, "A retried fight must not inherit HP lost in the fled attempt");
            Assert.AreEqual(8, retried.MonsterHp);
        }

        // ---- every round announces (criterion 8) ----

        [TestMethod]
        public void Fight_EveryRound_AnnouncesSomething()
        {
            StartSession(); // real, unseeded randomness — any outcome still announces
            var baseline = _messages.Count;

            Submit("fight");

            Assert.IsTrue(MessagesSince(baseline).Count > 0, "Every fight round must narrate something, hit or miss");
        }

        // ---- look mid-fight (criterion 9) ----

        [TestMethod]
        public void Look_DuringActiveCombatRound_ReportsBothCombatantsHp()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new Random(41)));
            Submit("fight"); // PC misses, monster hits for 4 -> PlayerHp 7, MonsterHp 8
            var baseline = _messages.Count;

            Submit("look");

            Assert.IsTrue(MessagesSince(baseline).Contains(OneD20Flavor.CombatStatus(7, 11, 8, 8)));
        }

        // ---- check: success + readout, solves once, no re-prompt (criteria 5, 6) ----
        // Seed 0: fresh Random(0)'s first d20 is 15. Room 2 (Search/Int, modifier 0) needs total
        // >= DC 10 -> succeeds with total 15.

        [TestMethod]
        public void Check_RoomSpecificSuccess_AnnouncesTextAndReadout_SolvesOnce_NoRePrompt()
        {
            StartSession(checkMechanic: new D20ResolutionMechanic(new Random(0)));
            Submit("move north"); // 5 -> 2
            var baseline = _messages.Count;

            Submit("check");

            var afterCheck = MessagesSince(baseline);
            Assert.IsTrue(afterCheck.Contains(OneD20Flavor.CheckNarration("check-2-success", "Search", 15, 10)));
            Assert.AreEqual(1, _engine.Rules.Query<RoomSolved>().Count(rs => rs.RoomId == 2));

            baseline = _messages.Count;
            Submit("check"); // already solved: no-op

            Assert.AreEqual(0, MessagesSince(baseline).Count, "A solved room's check must be rejected");
            Assert.AreEqual(1, _messages.Count(m => m == OneD20Flavor.Text["room-2-prompt"]),
                "Solving must not re-trigger the challenge-prompt announcement");
        }

        // ---- check: failure leaves room retryable (criterion 5) ----
        // Seed 1: fresh Random(1)'s first d20 is 5 -> total 5 < DC 10, a failure.

        [TestMethod]
        public void Check_Failure_LeavesRoomUnsolved_RetryableWithNoPenalty()
        {
            StartSession(checkMechanic: new D20ResolutionMechanic(new Random(1)));
            Submit("move north"); // 5 -> 2
            var baseline = _messages.Count;

            Submit("check");

            Assert.IsTrue(MessagesSince(baseline).Contains(OneD20Flavor.CheckNarration("check-2-failure", "Search", 5, 10)));
            Assert.IsFalse(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 2));

            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // retry succeeds

            Assert.IsTrue(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 2));
        }

        // ---- flee: clean retract, returns to previous room (criterion 13) ----

        [TestMethod]
        public void Flee_DuringActiveCombat_RetractsCleanly_NoSolveNoFail_ReturnsToPreviousRoom()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new Random(1))); // double-miss
            Submit("move north"); // 5 -> 2, PreviousRoomId = 5
            Submit("move south"); // 2 -> 5, PreviousRoomId = 2
            Submit("fight"); // starts a CombatRound in room 5
            var baseline = _messages.Count;

            Submit("flee");

            var afterFlee = MessagesSince(baseline);
            Assert.IsTrue(afterFlee.Contains(OneD20Flavor.Text["pc-flee"]));
            Assert.IsFalse(_engine.Rules.Query<CombatRound>().Any(cr => cr.RoomId == 5));
            Assert.IsFalse(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 5));
            Assert.IsFalse(_engine.Rules.Query<RoomFailed>().Any(rf => rf.RoomId == 5));
            Assert.AreEqual(2, _engine.Rules.Query<PlayerPosition>().First().RoomId, "Flee returns the PC to PreviousRoomId");
        }

        // ---- flee: room 5 with no prior move (criterion 14) ----

        [TestMethod]
        public void Flee_Room5_NoPriorMove_DoesNotMoveOrThrow()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new Random(1)));
            Submit("fight"); // fresh session: no PreviousRoomId fact exists yet
            var baseline = _messages.Count;

            Submit("flee"); // must not throw — Submit() itself asserts completion within the timeout

            Assert.IsTrue(MessagesSince(baseline).Contains(OneD20Flavor.Text["pc-flee"]));
            Assert.AreEqual(5, _engine.Rules.Query<PlayerPosition>().First().RoomId, "No PreviousRoomId to retreat to — PC stays put");
            Assert.IsFalse(_engine.Rules.Query<CombatRound>().Any(cr => cr.RoomId == 5));
        }

        // ---- win: combat-free column (criterion 15, first half) ----
        // Seed 0 reused fresh each check (a NEW Random(0) instance per call — its first roll is
        // always 15): mod 3 vs DC 12/10/8 (rooms 3, 6, 9) all succeed with total 18.

        [TestMethod]
        public void Win_CombatFreeColumn_FiresWinMarkerAndGameEnd()
        {
            StartSession();
            Submit("move north"); // 5 -> 2
            Submit("move east");  // 2 -> 3

            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // room 3 (Climb/Str, DC 12) solved

            Submit("move south"); // 3 -> 6
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // room 6 (Swim/Str, DC 10) solved

            Submit("move south"); // 6 -> 9
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // room 9 (Jump/Str, DC 8) solved

            var markers = _engine.Rules.Query<WinMarker>().ToList();
            Assert.IsTrue(markers.Any(m => m.Line.SequenceEqual(new[] { 3, 6, 9 })));
            Assert.IsTrue(_messages.Contains(OneD20Flavor.WinAnnouncement(new[] { 3, 6, 9 })));
        }

        // ---- win: combat-inclusive line (criterion 15, second half) ----

        [TestMethod]
        public void Win_LineIncludingCombatRoom_FiresWinMarker_WhenSolvedViaFight()
        {
            StartSession();
            Submit("move north"); // 5 -> 2
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // room 2 solved (Search/Int, DC 10)

            Submit("move west"); // 2 -> 1 (Fight room)
            _dispatcher.RegisterHandler("fight", new FightCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("fight"); // lethal PC hit solves room 1 in one round

            Submit("move east"); // 1 -> 2
            Submit("move east"); // 2 -> 3
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(new D20ResolutionMechanic(new Random(0))));
            Submit("check"); // room 3 solved (Climb/Str, DC 12)

            var markers = _engine.Rules.Query<WinMarker>().ToList();
            Assert.IsTrue(markers.Any(m => m.Line.SequenceEqual(new[] { 1, 2, 3 })),
                "Top-row line, which required room 1 solved via fight, should fire");
            Assert.IsTrue(_messages.Contains(OneD20Flavor.WinAnnouncement(new[] { 1, 2, 3 })));
        }
    }
}
