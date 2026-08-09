using LunaFlow.Engine;
using LunaFlow.Engine.Game;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rom.GridMap.Handlers;
using LunaFlow.Cartridge;
using LunaFlow.Rulebook.OneD20;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.ContentTests.Rom.D20System
{
    /// <summary>
    /// A test-owned deterministic <see cref="Random"/> that returns a pre-scripted queue of values —
    /// same technique <c>example/Phase2_12Spike/ScriptedRandom.cs</c> introduced (a queue exhausts
    /// loudly rather than wrapping/repeating, so a scenario consuming more rolls than scripted fails
    /// immediately and legibly, unlike a seeded <see cref="Random"/> where a wrong roll count just
    /// silently drifts to different numbers). Deliberately re-declared here rather than referencing
    /// the spike project — spikes are throwaway/unmaintained by their own convention (see that
    /// project's own header comment), and the maintained suite shouldn't depend on one.
    /// </summary>
    internal sealed class ScriptedRolls : Random
    {
        private readonly Queue<int> _values;
        public ScriptedRolls(params int[] values) => _values = new Queue<int>(values);
        public override int Next(int minValue, int maxValue) =>
            _values.Count > 0
                ? _values.Dequeue()
                : throw new InvalidOperationException("ScriptedRolls exhausted — scenario consumed more rolls than scripted.");
    }

    /// <summary>
    /// Maintained coverage for item 11's Enrage mechanic (phase-2-12-lua-worldmodule-stress-test-spec.md),
    /// driven live through the real <see cref="GameEngineCore"/> singleton and
    /// <see cref="DispatchGameCommand"/> pipeline, same shape as <c>D20SystemHandlerTests</c> but
    /// against "Signal Static" (<see cref="LuaOneD20WorldModule"/>) instead of
    /// <c>DefaultOneD20WorldModule</c> — its C#-authored twin has no Enrage-capable monster.
    /// Room/monster/challenge assignment is pinned (bypasses <see cref="OneD20SessionAssembler"/>'s
    /// randomness, same technique <c>D20SystemHandlerTests</c> uses), matching
    /// <c>example/Phase2_12Spike/Program.cs</c>'s fixed assignment exactly. Roll sequences for the
    /// threshold-crossing/defensive-effect/flee/retry chain are reused verbatim from that spike's
    /// Scenario B, which the spike (re-run as part of writing this class) confirmed still passes —
    /// the loss and timeout-while-enraged sequences are this class's own, new derivations (the spike
    /// only covers a timeout where the monster was never enraged at all). Rebuilt to fit
    /// <c>D20SystemHandlerTests</c>' own inline-<c>StartSession</c> convention rather than promoting
    /// <c>SpikeSession.cs</c> as a separate reusable class — ticket 0085 left that a judgment call.
    /// Signal Static's own <c>signal-monster-1</c>: Defense 12, AttackBonus 2, DamageDie 1d6+0, MaxHp
    /// 8, EnrageThresholdFraction 0.5 (crosses at MonsterHp &lt;= 4), EnrageDefenseBonus 3. PC: Defense
    /// 11, AttackBonus 4, DamageDie 1d8+3 (min roll 1 always deals 4 damage — every connecting PC hit
    /// against this MaxHp-8 monster crosses the threshold on the first hit, by design).
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.12")]
    [DoNotParallelize] // GameEngineCore.Instance is a singleton; tests must run sequentially
    public class SignalStaticEnrageHandlerTests
    {
        private static IOneD20WorldModule WorldModule = null!;
        private GameEngineCore _engine = null!;
        private DefaultCommandDispatcher _dispatcher = null!;
        private readonly List<string> _messages = new();
        private EventHandler<GameEngineMessagePostedEventArgs>? _messageHandler;
        private D20SystemRom _d20System = null!;

        // Fixed assignment matching example/Phase2_12Spike/Program.cs exactly. Only room 1's monster
        // (signal-monster-1) is Enrage-capable; rooms 5/8 need a monster too since D20System's
        // challenge dictionary is built for every room regardless of which this class's tests touch.
        private static readonly IReadOnlyDictionary<int, string> ChallengeAssignment = new Dictionary<int, string>
        {
            [2] = "signal-wade", [3] = "signal-scan", [4] = "signal-listen",
            [6] = "signal-climb", [7] = "signal-bypass", [9] = "signal-balance",
        };
        private static readonly IReadOnlyDictionary<int, string> MonsterAssignment = new Dictionary<int, string>
        {
            [1] = "signal-monster-1", [5] = "signal-monster-5", [8] = "signal-monster-2",
        };

        private static RoomChallenge ToRoomChallenge(int roomId, OneD20ChallengeTemplate challenge)
        {
            CheckKind? kind = challenge.Kind switch
            {
                OneD20CheckKind.Skill => CheckKind.Skill,
                OneD20CheckKind.Save => CheckKind.Save,
                _ => null,
            };
            return new RoomChallenge(roomId, RoomChallengeType.SkillCheck, kind,
                challenge.Skill, challenge.SkillAbility, challenge.DC, challenge.SaveType,
                challenge.SuccessKey, challenge.FailureKey, challenge.PromptKey, MonsterId: null);
        }

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
            _ = typeof(System.Linq.Expressions.Expression).Assembly;
            _ = typeof(NRules.RuleModel.IContext).Assembly;
            _ = typeof(NRules.ISession).Assembly;
            WorldModule = LuaOneD20WorldModule.FromEmbeddedResource(
                resourceName: "LunaFlow.Rulebook.OneD20.Content.SignalStaticWorldModule.lua");
        }

        [TestInitialize]
        public void Setup()
        {
            _engine = GameEngineCore.Instance;
            if (_engine.Running) _engine.Stop(force: true);
            _messages.Clear();
            _dispatcher = new DefaultCommandDispatcher();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_messageHandler is not null)
                _engine.MessagePosted -= _messageHandler;
            _messageHandler = null;
            if (_engine.Running) _engine.Stop(force: true);
        }

        /// <summary>Registers handlers, seeds facts mirroring Program.cs's seed shape, and starts a
        /// fresh session against the pinned assignment — same technique as
        /// <c>D20SystemHandlerTests.StartSession</c>, adapted for Signal Static.</summary>
        private void StartSession(D20ResolutionMechanic? fightMechanic = null, D20ResolutionMechanic? checkMechanic = null)
        {
            var gridMapData = WorldModule.Rooms.Values
                .Select(r => new GridRoomData(r.Id, r.Row, r.Col, r.DescriptionKey))
                .ToList();
            var gridMap = new GridMapRom(gridMapData, WorldModule.StartRoomId);

            var d20Challenges = WorldModule.Rooms.Values
                .Select(r => r.ChallengeType == OneD20ChallengeType.Fight
                    ? new RoomChallenge(r.Id, RoomChallengeType.Fight, null, null, null, null, null, null, null, null,
                        MonsterId: MonsterAssignment[r.Id])
                    : ToRoomChallenge(r.Id, WorldModule.Challenges[ChallengeAssignment[r.Id]]))
                .ToDictionary(c => c.Id);
            var playerStats = WorldModule.PlayerCharacter;
            var monsters = WorldModule.Monsters.Values
                .Select(m => new MonsterTemplate(m.Id, m.Name, m.DescriptionKey,
                    new CombatantStats(m.Stats.Defense, m.Stats.AttackBonus, m.Stats.DamageDie, m.Stats.DamageBonus, m.Stats.MaxHp),
                    m.InflictsPoison, m.EnrageThresholdFraction, m.EnrageDefenseBonus))
                .ToDictionary(m => m.Id);
            _d20System = new D20SystemRom(
                new PlayerCharacter(new CombatantStats(playerStats.Defense, playerStats.AttackBonus, playerStats.DamageDie, playerStats.DamageBonus, playerStats.MaxHp)),
                monsters, d20Challenges, WorldModule.PlayerAbilityScores,
                fightMechanic ?? new D20ResolutionMechanic());

            var movement = new GridMovementService();
            var flavorText = WorldModule.FlavorText;
            var monsterMaxHpByRoom = MonsterAssignment.ToDictionary(
                kv => kv.Key, kv => WorldModule.Monsters[kv.Value].Stats.MaxHp);

            _dispatcher.RegisterHandler("move", new MoveCommandHandler(gridMap, flavorText));
            _dispatcher.RegisterHandler("look", new LookCommandHandler(gridMap, flavorText, playerStats.MaxHp, monsterMaxHpByRoom));
            _dispatcher.RegisterHandler("check", new CheckCommandHandler(_d20System, flavorText, checkMechanic));
            _dispatcher.RegisterHandler("fight", new FightCommandHandler(_d20System, flavorText, fightMechanic));
            _dispatcher.RegisterHandler("flee", new FleeCommandHandler(movement, flavorText));

            _messageHandler = (s, e) => _messages.Add(e.Message);
            _engine.MessagePosted += _messageHandler;

            var options = new GameEngineOptions();
            options.Rules
                .FromAssembly(RuleExport.Compile(typeof(GridMapRuleGroup)))
                .FromAssembly(RuleExport.Compile(typeof(D20SystemRuleGroup)))
                .Seed(() =>
                {
                    var startRoom = WorldModule.Rooms[WorldModule.StartRoomId];
                    LunaFlowEngine.Host.Announce(flavorText[startRoom.DescriptionKey]);
                    return new object[] { gridMap, _d20System, new PlayerPosition(WorldModule.StartRoomId), _d20System.PlayerCharacter };
                })
                .Seed(() =>
                {
                    var prompts = WorldModule.Rooms.Values
                        .Select(r => (object)new RoomChallengePrompt(
                            r.Id,
                            r.ChallengeType == OneD20ChallengeType.Fight
                                ? flavorText[WorldModule.Monsters[MonsterAssignment[r.Id]].DescriptionKey]
                                : flavorText[WorldModule.Challenges[ChallengeAssignment[r.Id]].PromptKey]));
                    return prompts.Append(new GameEndAnnouncement(flavorText["win-announce"]));
                });
            _engine.SetOptions(options);
            _engine.Start();
        }

        private void Submit(string commandLine)
        {
            var command = new DefaultCommand(commandLine);
            using var completed = new ManualResetEventSlim(false);
            _engine.Submit(new DispatchGameCommand(command, _dispatcher, completed));
            Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(2)), $"Command '{commandLine}' should complete");
        }

        private CombatRound CombatRoundInRoom(int roomId) =>
            _engine.Rules.Query<CombatRound>().First(cr => cr.RoomId == roomId);

        /// <summary>5 -> 1: the path every test uses to reach the Enrage monster (west then north),
        /// setting PreviousRoomId = 4 along the way — mirrors the spike's own path exactly.</summary>
        private void MoveToRoom1()
        {
            Submit("move west");  // 5 -> 4
            Submit("move north"); // 4 -> 1
        }

        // ---- Item 3: full field-carry chain (structural, no fight needed) ----

        [TestMethod]
        public void StartSession_TranslatesEnrageFieldsFromWorldModuleIntoMonsterTemplate_Unchanged()
        {
            StartSession();

            var monster = _d20System.Monsters["signal-monster-1"];
            Assert.AreEqual(0.5, monster.EnrageThresholdFraction,
                "EnrageThresholdFraction must carry unchanged from the World Module through to the ROM-tier MonsterTemplate FightCommandHandler actually reads");
            Assert.AreEqual(3, monster.EnrageDefenseBonus);

            // A monster with no Enrage fields set in the World Module must carry both as null, not a
            // default-zero or some other silent substitution.
            var plain = _d20System.Monsters["signal-monster-2"];
            Assert.IsNull(plain.EnrageThresholdFraction);
            Assert.IsNull(plain.EnrageDefenseBonus);
        }

        // ---- Item 1: threshold crossing ----

        [TestMethod]
        public void Fight_FirstHitCrossingThreshold_InsertsMonsterEnragedExactlyOnce_AtMonsterHpEqualToThreshold()
        {
            // pcRoll=8 hits at baseline Defense 12 (8+4=12>=12); damage roll=1 -> dmg 4, MonsterHp
            // 8-4=4, exactly at the threshold (0.5*8=4) -> crosses, enrage inserted. monsterRoll=1
            // misses (1+2=3<11) so the round completes without further state change.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1)));
            MoveToRoom1();

            Submit("fight");

            Assert.AreEqual(1, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));
            Assert.AreEqual(4, CombatRoundInRoom(1).MonsterHp);
            Assert.IsTrue(_messages.Contains(WorldModule.FlavorText["monster-enraged"]));
        }

        // ---- Item 1: purely defensive effect ----

        [TestMethod]
        public void Fight_WhileEnraged_IdenticalPcRollThatHitBefore_NowMissesAgainstBoostedDefense()
        {
            // Round 1 as above (enrage triggers, MonsterHp 4). Round 2: the SAME roll (8) that hit at
            // baseline Defense 12 now misses against the enraged effective Defense 15 (8+4=12<15) —
            // demonstrates the effect is real against this exact roll, not just that some roll misses.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1, 8, 1)));
            MoveToRoom1();
            Submit("fight"); // round 1: enrages

            Submit("fight"); // round 2: same roll now misses

            Assert.AreEqual(4, CombatRoundInRoom(1).MonsterHp, "A miss must leave MonsterHp unchanged");
            Assert.AreEqual(1, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1),
                "MonsterEnraged must not be cleared or re-inserted by an ordinary round");
        }

        // ---- Item 1: named regression — Enrage never touches the monster's own attack roll ----

        [TestMethod]
        public void Fight_MonsterCounterattack_UnaffectedByEnrage_IdenticalRollSameOutcomeBeforeAndAfter()
        {
            // Round 1 (pre-enrage): PC misses (roll 1); monster attacks with roll 9 (9+2=11>=11,
            // PC's fixed Defense 11) -> hits for a scripted 3 damage. MonsterHp untouched (8), not
            // enraged yet. Round 2: PC hits and crosses the threshold (roll 8/damage 1 -> MonsterHp
            // 4, enraged) — then the monster attacks again with the SAME roll 9, for the SAME
            // scripted 3 damage. If Enrage leaked into the monster's own attack calculation in any
            // way, this second hit's outcome (or damage) would differ from the first; per ticket
            // 0083's ruling and the handler's own code (the counterattack branch never reads
            // monsterTemplate.Stats.Defense at all — only pc.Defense), it must not.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(1, 9, 3, 8, 1, 9, 3)));
            MoveToRoom1();

            Submit("fight"); // round 1
            var afterRound1 = CombatRoundInRoom(1);
            Assert.AreEqual(8, afterRound1.MonsterHp, "PC missed round 1 — MonsterHp untouched");
            Assert.AreEqual(8, afterRound1.PlayerHp, "PC's starting Hp (11) minus round 1's 3 damage");
            Assert.AreEqual(0, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));

            Submit("fight"); // round 2: PC hits, crosses threshold, THEN the identical monsterRoll=9 fires again
            var afterRound2 = CombatRoundInRoom(1);
            Assert.AreEqual(1, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1),
                "Now enraged, from this round's PC hit");
            Assert.AreEqual(5, afterRound2.PlayerHp,
                "The same monsterRoll=9 must deal the same 3 damage it dealt pre-enrage (8 - 3 = 5), " +
                "proving the monster's own attack calculation is unaffected by its own Enrage state");
        }

        // ---- Item 1: retraction on all four CombatRound terminal outcomes ----

        [TestMethod]
        public void MonsterEnraged_RetractsOnWin()
        {
            // Round 1: enrages (MonsterHp 4). Round 2: pcRoll=11 hits the enraged Defense 15
            // (11+4=15>=15); damage roll=1 -> dmg 4, MonsterHp 4-4=0 -> RoomSolved.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1, 11, 1)));
            MoveToRoom1();
            Submit("fight"); // round 1: enrages

            Submit("fight"); // round 2: finishes it while enraged

            Assert.IsTrue(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<CombatRound>().Count(cr => cr.RoomId == 1));
        }

        [TestMethod]
        public void MonsterEnraged_RetractsOnLoss()
        {
            // Round 1: enrages (MonsterHp 4); monster misses (roll 1). Round 2: PC misses at the
            // enraged Defense 15 (roll 10: 10+4=14<15); monster hits (roll 9) for 6 damage (PlayerHp
            // 11-6=5). Round 3: PC misses again; monster hits again for 6 damage (PlayerHp 5-6=-1)
            // -> RoomFailed.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1, 10, 9, 6, 10, 9, 6)));
            MoveToRoom1();
            Submit("fight"); // round 1: enrages

            Submit("fight"); // round 2
            Submit("fight"); // round 3: PC loses

            Assert.IsTrue(_engine.Rules.Query<RoomFailed>().Any(rf => rf.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<CombatRound>().Count(cr => cr.RoomId == 1));
        }

        [TestMethod]
        public void MonsterEnraged_RetractsOnThreeRoundTimeout()
        {
            // Round 1: enrages (MonsterHp 4); monster misses. Rounds 2-3: PC misses at the enraged
            // Defense 15 (roll 10 each time), monster misses each time (roll 1) -> neither reduced to
            // 0, timeout at round 3.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1, 10, 1, 10, 1)));
            MoveToRoom1();
            Submit("fight"); // round 1: enrages
            Submit("fight"); // round 2
            var baseline = _messages.Count;

            Submit("fight"); // round 3: timeout

            Assert.IsTrue(_messages.Skip(baseline).Contains(WorldModule.FlavorText["monster-fled"]));
            Assert.IsTrue(_engine.Rules.Query<RoomFailed>().Any(rf => rf.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<CombatRound>().Count(cr => cr.RoomId == 1));
        }

        [TestMethod]
        public void MonsterEnraged_RetractsOnFlee()
        {
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1)));
            MoveToRoom1();
            Submit("fight"); // round 1: enrages

            Submit("flee");

            Assert.AreEqual(0, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));
            Assert.AreEqual(0, _engine.Rules.Query<CombatRound>().Count(cr => cr.RoomId == 1));
            Assert.IsFalse(_engine.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == 1));
            Assert.IsFalse(_engine.Rules.Query<RoomFailed>().Any(rf => rf.RoomId == 1));
        }

        // ---- Item 1: retroactive-safety property ----

        [TestMethod]
        public void Fight_RetryAfterFlee_StartsUnenraged_IdenticalRollThatMissedNowHitsAgain()
        {
            // Round 1: enrages (MonsterHp 4). Round 2: roll 8 misses the enraged Defense 15 (same
            // roll Fight_WhileEnraged... pins as a miss). Flee, move back in — fresh CombatRound
            // (MaxHp 8 again). Retry round 1: the SAME roll 8 now HITS again, at baseline Defense 12
            // (8+4=12>=12) — proof the retry didn't inherit the fled fight's enraged state. It
            // re-crosses the threshold (any connecting hit does, given this monster's numbers), so
            // MonsterEnraged re-triggers on the retry's own crossing.
            StartSession(fightMechanic: new D20ResolutionMechanic(new ScriptedRolls(8, 1, 1, 8, 1, 8, 1, 1)));
            MoveToRoom1();
            Submit("fight"); // round 1: enrages
            Submit("fight"); // round 2: same roll misses while enraged
            Submit("flee");
            Assert.AreEqual(0, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1));

            MoveToRoom1(); // back in — fresh CombatRound
            Submit("fight"); // retry round 1: same roll 8 hits again at baseline

            Assert.AreEqual(4, CombatRoundInRoom(1).MonsterHp,
                "Roll 8 must connect again at baseline Defense — it would have missed had the retry inherited the enraged state");
            Assert.AreEqual(1, _engine.Rules.Query<MonsterEnraged>().Count(e => e.RoomId == 1),
                "The retry's own hit re-crosses the threshold and re-triggers Enrage");
        }
    }
}
