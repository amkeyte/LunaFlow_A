using LunaFlow.Engine;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.Reputation;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.ContentTests.Rom.Reputation
{
    /// <summary>
    /// Fixture-level coverage for <see cref="ReputationRuleGroup"/> — accrual (first point vs.
    /// subsequent points, retract-then-insert not update) and the Veteran/Legend tier thresholds.
    /// Same shape as <c>LootRuleGroupFixtureTests</c>: facts inserted directly against a bare
    /// <see cref="RuleEngine"/>, no command dispatch. Uses <see cref="RoomSolved"/> (D20System's
    /// <c>IChallengeSolved</c> implementer) as the trigger fact, same as the Loot fixture tests.
    /// Phase 2.11 item 7 (ticket 0079).
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.11")]
    public class ReputationRuleGroupFixtureTests
    {
        private static System.Reflection.Assembly _compiledReputationRules = null!;
        private RuleEngine _engine = null!;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            // RoslynRuleCompiler only sees already-loaded, file-backed assemblies — force NRules in
            // before the first Compile() call. Same gotcha LootRuleGroupFixtureTests/
            // D20SystemRuleFixtureTests work around. ReputationRuleGroup's FirstPoint/Accrue rules
            // name LunaFlow.Cartridge.IChallengeSolved in their RequiredUsings, so force that too.
            _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
            _ = typeof(System.Linq.Expressions.Expression).Assembly;
            _ = typeof(NRules.RuleModel.IContext).Assembly;
            _ = typeof(NRules.ISession).Assembly;
            _ = typeof(LunaFlow.Cartridge.IChallengeSolved).Assembly;

            _compiledReputationRules = RuleExport.Compile(typeof(ReputationRuleGroup));
        }

        [TestInitialize]
        public void Setup()
        {
            _engine = new RuleEngine();
            _engine.LoadRulesFromAssembly(_compiledReputationRules);
        }

        [TestMethod]
        public void FirstSolve_InsertsReputationPointsAtOne()
        {
            _engine.Insert(new RoomSolved(1));

            _engine.Fire();

            var points = _engine.Query<ReputationPoints>().ToList();
            Assert.AreEqual(1, points.Count);
            Assert.AreEqual(1, points[0].Total);
        }

        [TestMethod]
        public void SecondSolve_RetractsPriorPoints_LeavesExactlyOneFactAtIncrementedTotal()
        {
            _engine.Insert(new RoomSolved(1));
            _engine.Fire();

            _engine.Insert(new RoomSolved(2));
            _engine.Fire();

            // Exactly one ReputationPoints fact after the second solve is the load-bearing check:
            // if Accrue inserted the new total without retracting the old one, working memory would
            // hold both ReputationPoints(1) and ReputationPoints(2) — this is what distinguishes
            // retract-then-insert from an additive insert, not just checking the latest total.
            var points = _engine.Query<ReputationPoints>().ToList();
            Assert.AreEqual(1, points.Count,
                "Accrual must retract the prior ReputationPoints fact, not merely insert a second one");
            Assert.AreEqual(2, points[0].Total);
        }

        [TestMethod]
        public void ThirdSolve_CrossesVeteranThreshold_InsertsReputationTierReachedExactlyOnce()
        {
            _engine.Insert(new RoomSolved(1));
            _engine.Fire();
            _engine.Insert(new RoomSolved(2));
            _engine.Fire();
            _engine.Insert(new RoomSolved(3));
            _engine.Fire();

            Assert.AreEqual(3, _engine.Query<ReputationPoints>().Single().Total);
            var veteranFacts = _engine.Query<ReputationTierReached>().Where(t => t.Tier == "Veteran").ToList();
            Assert.AreEqual(1, veteranFacts.Count);
        }

        [TestMethod]
        public void SolvesPastVeteran_NeverReinsertsVeteranTier()
        {
            for (var room = 1; room <= 5; room++)
            {
                _engine.Insert(new RoomSolved(room));
                _engine.Fire();
            }

            Assert.AreEqual(5, _engine.Query<ReputationPoints>().Single().Total);
            Assert.AreEqual(1, _engine.Query<ReputationTierReached>().Count(t => t.Tier == "Veteran"),
                "Veteran must never re-insert on solves after the threshold is first crossed");
        }

        [TestMethod]
        public void SixthSolve_CrossesLegendThreshold_InsertsLegendExactlyOnce_VeteranStillExactlyOnce()
        {
            for (var room = 1; room <= 6; room++)
            {
                _engine.Insert(new RoomSolved(room));
                _engine.Fire();
            }

            Assert.AreEqual(6, _engine.Query<ReputationPoints>().Single().Total);
            Assert.AreEqual(1, _engine.Query<ReputationTierReached>().Count(t => t.Tier == "Veteran"));
            Assert.AreEqual(1, _engine.Query<ReputationTierReached>().Count(t => t.Tier == "Legend"));
        }
    }
}
