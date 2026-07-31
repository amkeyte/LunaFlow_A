using LunaFlow.Engine;
using LunaFlow.Engine.Game;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.Loot;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.EngineTests.Rom.Loot
{
    /// <summary>
    /// Fixture-level coverage for <see cref="LootRuleGroup.AwardItem"/> — Loot's one generic rule,
    /// correlating <c>IChallengeSolved</c> against a session's <c>LootAssignment</c> data rather than
    /// a per-room rule family. Facts are inserted directly, never through a handler, same split as
    /// <c>D20SystemRuleFixtureTests</c> (pure rule-engine semantics here; handler-driven wiring is
    /// <c>InventoryCommandHandlerTests</c>' job). Uses <see cref="RoomSolved"/> (D20System's
    /// <c>IChallengeSolved</c> implementer) as the trigger fact — the same type production seeds.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.7.1")]
    public class LootRuleGroupFixtureTests
    {
        private static System.Reflection.Assembly _compiledLootRules = null!;
        private RuleEngine _engine = null!;
        private readonly List<string> _messages = new();
        private EventHandler<GameEngineMessagePostedEventArgs>? _messageHandler;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            // RoslynRuleCompiler only sees already-loaded, file-backed assemblies — force NRules in
            // before the first Compile() call. Same gotcha D20SystemRuleFixtureTests works around.
            // LootRuleGroup.AwardItem also names LunaFlow.Cartridge.IChallengeSolved in its
            // RequiredUsings, so that assembly needs forcing too (see InventoryCommandHandlerTests's
            // copy of this comment for the run-order-dependent failure this prevents).
            _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
            _ = typeof(System.Linq.Expressions.Expression).Assembly;
            _ = typeof(NRules.RuleModel.IContext).Assembly;
            _ = typeof(NRules.ISession).Assembly;
            _ = typeof(LunaFlow.Cartridge.IChallengeSolved).Assembly;

            _compiledLootRules = RuleExport.Compile(typeof(LootRuleGroup));
        }

        [TestInitialize]
        public void Setup()
        {
            _engine = new RuleEngine();
            _engine.LoadRulesFromAssembly(_compiledLootRules);
            _messages.Clear();
            _messageHandler = (s, e) => _messages.Add(e.Message);
            GameEngineCore.Instance.MessagePosted += _messageHandler;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_messageHandler is not null)
                GameEngineCore.Instance.MessagePosted -= _messageHandler;
            _messageHandler = null;
        }

        [TestMethod]
        public void FightRoomSolved_WithMatchingAssignment_InsertsItemAwarded_AnnouncesPickupText()
        {
            _engine.Insert(new LootAssignment(RoomId: 1, ItemId: "rapier"));
            _engine.Insert(new LootPickupText("rapier", "You take his rapier."));
            _engine.Insert(new RoomSolved(1));

            _engine.Fire();

            var awarded = _engine.Query<ItemAwarded>().ToList();
            Assert.AreEqual(1, awarded.Count);
            Assert.AreEqual("rapier", awarded[0].ItemId);
            Assert.AreEqual(1, awarded[0].RoomId);
            Assert.IsTrue(_messages.Contains("You take his rapier."));
        }

        [TestMethod]
        public void SkillCheckRoomSolved_WithNoLootAssignment_NeverAwardsAnything()
        {
            // Skill-check rooms produce IChallengeSolved too (RoomSolved : IChallengeSolved) but never
            // have a matching LootAssignment — the rule must simply never fire for them, with no
            // guard needed beyond the Match<LootAssignment> failing to find a fact.
            _engine.Insert(new LootAssignment(RoomId: 1, ItemId: "rapier"));
            _engine.Insert(new LootPickupText("rapier", "You take his rapier."));
            _engine.Insert(new RoomSolved(2)); // room 2 has no LootAssignment

            _engine.Fire();

            Assert.AreEqual(0, _engine.Query<ItemAwarded>().Count());
            Assert.AreEqual(0, _messages.Count);
        }

        [TestMethod]
        public void AlreadyAwardedRoom_NeverAwardsTwice()
        {
            _engine.Insert(new LootAssignment(RoomId: 1, ItemId: "rapier"));
            _engine.Insert(new LootPickupText("rapier", "You take his rapier."));
            _engine.Insert(new ItemAwarded("rapier", 1)); // pre-existing: as if already awarded
            _engine.Insert(new RoomSolved(1));

            _engine.Fire();

            Assert.AreEqual(1, _engine.Query<ItemAwarded>().Count(),
                "Not<ItemAwarded> guard must block a second award for the same room");
        }

        [TestMethod]
        public void MultipleFightRoomsSolved_EachAwardsItsOwnItem_OneGenericRuleNotAPerRoomFamily()
        {
            _engine.Insert(new LootAssignment(RoomId: 1, ItemId: "rapier"));
            _engine.Insert(new LootPickupText("rapier", "You take his rapier."));
            _engine.Insert(new LootAssignment(RoomId: 5, ItemId: "vest"));
            _engine.Insert(new LootPickupText("vest", "You strip the vest off him."));

            _engine.Insert(new RoomSolved(1));
            _engine.Insert(new RoomSolved(5));

            _engine.Fire();

            var awarded = _engine.Query<ItemAwarded>().ToList();
            Assert.AreEqual(2, awarded.Count);
            Assert.IsTrue(awarded.Any(a => a.RoomId == 1 && a.ItemId == "rapier"));
            Assert.IsTrue(awarded.Any(a => a.RoomId == 5 && a.ItemId == "vest"));
        }
    }
}
