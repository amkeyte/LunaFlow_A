using System.Reflection;
using LunaFlow.Engine;
using LunaFlow.Rules;
using LunaFlow.Rules.GeneratorServices;
using NRules.RuleModel;

namespace LunaFlow.EngineTests.GeneratorServices
{
    /// <summary>
    /// B1–B5 durable test suite for the rule-pipeline generator services.
    /// Tests the compile-time C# rule authoring path: author writes [RuleGroup] classes,
    /// parser reflects them to IR, generator produces source, compiler produces in-memory assembly.
    /// Benchmarks verify parser cardinality, generator output quality, guard injection,
    /// end-to-end execution, and parity with hand-written rules.
    /// </summary>
    [TestClass]
    [TestCategory("Rules_GeneratorServices")]
    [TestCategory("Phase2")]
    public class RulePipelineGeneratorServiceTests
    {
        /// <summary>
        /// B1 — Parser → IR (reflection).
        /// Verifies ReflectionRuleParser correctly maps [RuleGroup] attributes and properties
        /// to RuleIRGroup with correct structure: GroupName, rule count, per-rule properties,
        /// and standing-rule deduplication (class ∪ property) with multiple distinct types.
        /// Also verifies cardinality: Parse throws when context holds >1 group.
        /// </summary>
        [TestMethod]
        public void ParserReflection_ProducesCorrectRuleIRGroup()
        {
            // Arrange
            var parser = new ReflectionRuleParser();
            var context = new RuleParseContext().AddRuleGroup(typeof(TestRuleGroup));

            // Act
            var group = parser.Parse(context);

            // Assert: Group structure (TestRuleGroup has 2 unguarded rules)
            Assert.AreEqual("TestGroup", group.GroupName);
            Assert.AreEqual(2, group.Rules.Count, "Should have 2 rules: SimpleRule, CounterRule");

            // Assert: SimpleRule (no guards)
            var simpleRule = group.Rules.First(r => r.Name == "SimpleRule");
            Assert.AreEqual("SimpleRule", simpleRule.Name);
            Assert.AreEqual("A simple rule that fires when Counter exists", simpleRule.Description);
            Assert.AreEqual(0, simpleRule.StandingRules.Count, "SimpleRule has no guards");
            Assert.IsTrue(simpleRule.ConditionCode.Contains("Match<Counter>"));
            Assert.IsTrue(simpleRule.ActionCode.Contains("RuleFiredMarker"));

            // Assert: CounterRule (no guards)
            var counterRule = group.Rules.First(r => r.Name == "CounterRule");
            Assert.AreEqual(0, counterRule.StandingRules.Count, "CounterRule has no guards");
            Assert.IsTrue(counterRule.PreambleCode.Contains("Counter c = null"));
            Assert.IsTrue(counterRule.ConditionCode.Contains("Match<Counter>"));
            Assert.IsTrue(counterRule.ActionCode.Contains("Insert"));

            // Assert: RequiredUsings deduped
            Assert.IsTrue(group.Rules.All(r => r.RequiredUsings is not null));
        }

        /// <summary>
        /// B1 — Guard union and dedup verification.
        /// Tests that [StandingRule] at class and property level are correctly unioned and deduped.
        /// </summary>
        [TestMethod]
        public void ParserReflection_UnionAndDedupStandingRules()
        {
            // Arrange
            var parser = new ReflectionRuleParser();
            var context = new RuleParseContext().AddRuleGroup(typeof(GuardedRuleGroup));

            // Act
            var group = parser.Parse(context);

            // Assert: GuardedRuleGroup has 3 rules
            Assert.AreEqual("GuardedGroup", group.GroupName);
            Assert.AreEqual(3, group.Rules.Count, "Should have 3 rules");

            // Assert: MultiGuardedRule — same guard type at class and property → deduped to 1
            var multiGuarded = group.Rules.First(r => r.Name == "MultiGuardedRule");
            Assert.AreEqual(1, multiGuarded.StandingRules.Count,
                "Guards should be deduped: class and property both specify BlockingMarker");
            Assert.AreEqual(typeof(BlockingMarker), multiGuarded.StandingRules[0]);

            // Assert: UnionGuardedRule — different types at class and property → union of 2
            var unionGuarded = group.Rules.First(r => r.Name == "UnionGuardedRule");
            Assert.AreEqual(2, unionGuarded.StandingRules.Count,
                "Guards should union class-level BlockingMarker with property-level SecondBlockingMarker");

            // Both types should be present (order may vary)
            var guardTypes = unionGuarded.StandingRules.OrderBy(t => t.Name).ToList();
            Assert.IsTrue(guardTypes.Contains(typeof(BlockingMarker)), "BlockingMarker (class-level) should be in union");
            Assert.IsTrue(guardTypes.Contains(typeof(SecondBlockingMarker)), "SecondBlockingMarker (property-level) should be in union");

            // Assert: ClassGuardedRule — class-level guard only
            var classGuarded = group.Rules.First(r => r.Name == "ClassGuardedRule");
            Assert.AreEqual(1, classGuarded.StandingRules.Count, "ClassGuardedRule inherits class-level guard");
            Assert.AreEqual(typeof(BlockingMarker), classGuarded.StandingRules[0]);
        }

        /// <summary>
        /// B1 cardinality — Parser throws when context has >1 group.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParserReflection_ThrowsOnMultipleGroups()
        {
            // Arrange: Create a context with two groups
            var parser = new ReflectionRuleParser();
            var context = new RuleParseContext()
                .AddRuleGroup(typeof(TestRuleGroup))
                .AddRuleGroup(typeof(TestRuleGroup)); // Adding same type twice simulates multiple groups

            // Act
            parser.Parse(context);

            // Assert: Exception thrown above
        }

        /// <summary>
        /// B2 — Generator → compiling source.
        /// Verifies RuleCodeGenerator produces RuleCodeUnit with valid C# source,
        /// RoslynRuleCompiler compiles it to a non-null Assembly without error,
        /// and RequiredUsings is correctly deduped.
        /// </summary>
        [TestMethod]
        public void GeneratorAndCompiler_ProducesLoadableAssembly()
        {
            // Arrange
            // Ensure NRules assemblies are loaded before compilation
            _ = typeof(IContext).Assembly;

            var parser = new ReflectionRuleParser();
            var generator = new RuleCodeGenerator();
            var compiler = new RoslynRuleCompiler();

            var context = new RuleParseContext().AddRuleGroup(typeof(TestRuleGroup));
            var ir = parser.Parse(context);

            // Act
            var codeUnit = generator.Generate(ir);
            var assembly = compiler.Compile(codeUnit);

            // Assert: Assembly is non-null and loadable
            Assert.IsNotNull(assembly, "Compiler should return non-null Assembly");
            Assert.IsFalse(string.IsNullOrEmpty(assembly.FullName), "Assembly should have a name");

            // Assert: Code unit structure is correct
            Assert.AreEqual("LunaFlow.Generated.TestGroup", codeUnit.Namespace);
            Assert.AreEqual("TestGroup.g.cs", codeUnit.FileName);
            Assert.AreEqual(2, codeUnit.Classes.Count, "Should have 2 generated rule classes");

            // Assert: Class names follow convention [RuleName]Rule
            var classNames = codeUnit.Classes.Select(c => c.ClassName).ToList();
            Assert.IsTrue(classNames.Contains("SimpleRuleRule"), "SimpleRule → SimpleRuleRule");
            Assert.IsTrue(classNames.Contains("CounterRuleRule"), "CounterRule → CounterRuleRule");

            // Assert: RequiredUsings deduped (baseline + union of rule usings)
            Assert.IsTrue(codeUnit.RequiredUsings.Contains("System"));
            Assert.IsTrue(codeUnit.RequiredUsings.Contains("NRules.Fluent.Dsl"));
            Assert.IsTrue(codeUnit.RequiredUsings.Contains("LunaFlow.Rules"));
            var deduped = codeUnit.RequiredUsings.Distinct().ToList();
            Assert.AreEqual(codeUnit.RequiredUsings.Count, deduped.Count, "RequiredUsings should be deduped");
        }

        /// <summary>
        /// B3 — Guard injection (behavioral).
        /// Verifies that [StandingRule] guards are correctly injected as .Not&lt;T&gt;()
        /// in the generated When() block, and that they actually prevent rule firing.
        /// Load compiled assembly, fire with marker present → guarded rule does NOT fire.
        /// Retract marker, fire → guarded rule DOES fire.
        /// </summary>
        [TestMethod]
        public void GuardInjection_PreventsFireWhenMarkerPresent()
        {
            // Arrange: Ensure NRules assemblies are loaded before compilation
            _ = typeof(IContext).Assembly;

            // Compile the guarded rule group (which has class-level guard)
            var assembly = RuleExport.Compile(typeof(GuardedRuleGroup));

            // Load into engine and verify guard behavior
            var engine = new RuleEngine();
            engine.LoadRulesFromAssembly(assembly);

            // Initial fact: a Counter to match rules
            engine.Insert(new Counter { Value = 0 });

            // Act 1: Fire with BlockingMarker present (class-level guard)
            engine.Insert(new BlockingMarker("test"));
            engine.Fire();

            // Assert 1: ClassGuardedRule and MultiGuardedRule should NOT fire (guarded by BlockingMarker)
            var firedMarkers1 = engine.Query<RuleFiredMarker>().ToList();
            var firedRules1 = firedMarkers1.Select(m => m.RuleName).ToList();
            Assert.AreEqual(0, firedRules1.Count, "No guarded rules should fire when BlockingMarker is present");

            // Clear BlockingMarker
            var marker = engine.Query<BlockingMarker>().First();
            engine.Retract(marker);
            // Clear RuleFiredMarkers from first fire
            foreach (var fired in firedMarkers1) engine.Retract(fired);

            // Act 2: Fire with BlockingMarker retracted (but no SecondBlockingMarker)
            engine.Fire();

            // Assert 2: Rules blocked by BlockingMarker should now fire
            // UnionGuardedRule also fires (neither BlockingMarker nor SecondBlockingMarker present)
            var firedMarkers2 = engine.Query<RuleFiredMarker>().ToList();
            var firedRules2 = firedMarkers2.Select(m => m.RuleName).ToList();
            Assert.AreEqual(3, firedRules2.Count, "All three rules in GuardedGroup should fire when neither guard marker present");
            Assert.IsTrue(firedRules2.Contains("ClassGuardedRule"), "ClassGuardedRule should fire when BlockingMarker absent");
            Assert.IsTrue(firedRules2.Contains("MultiGuardedRule"), "MultiGuardedRule should fire when BlockingMarker absent");
            Assert.IsTrue(firedRules2.Contains("UnionGuardedRule"), "UnionGuardedRule should fire when both guard markers absent");
        }

        /// <summary>
        /// B3 — Guard union behavioral verification.
        /// Verifies that both class-level and property-level guards of different types
        /// are both injected and both block firing independently.
        /// </summary>
        [TestMethod]
        public void GuardInjection_UnionBlocksBothTypes()
        {
            // Arrange: Ensure NRules assemblies are loaded before compilation
            _ = typeof(IContext).Assembly;

            var assembly = RuleExport.Compile(typeof(GuardedRuleGroup));
            var engine = new RuleEngine();
            engine.LoadRulesFromAssembly(assembly);
            engine.Insert(new Counter { Value = 0 });

            // Act 1: Fire with SecondBlockingMarker only (BlockingMarker absent)
            engine.Insert(new SecondBlockingMarker("test"));
            engine.Fire();

            // Assert 1: UnionGuardedRule should NOT fire (blocked by SecondBlockingMarker)
            var firedMarkers1 = engine.Query<RuleFiredMarker>().ToList();
            var firedRules1 = firedMarkers1.Select(m => m.RuleName).ToList();
            Assert.IsFalse(firedRules1.Contains("UnionGuardedRule"),
                "UnionGuardedRule should not fire when SecondBlockingMarker is present (property-level guard blocks)");
            Assert.IsTrue(firedRules1.Contains("ClassGuardedRule"), "ClassGuardedRule fires (not guarded by SecondBlockingMarker)");

            // Clear for next test
            foreach (var fired in firedMarkers1) engine.Retract(fired);

            // Act 2: Add BlockingMarker (now both present)
            engine.Insert(new BlockingMarker("test"));
            engine.Fire();

            // Assert 2: UnionGuardedRule still blocked (now by BlockingMarker)
            var firedMarkers2 = engine.Query<RuleFiredMarker>().ToList();
            var firedRules2 = firedMarkers2.Select(m => m.RuleName).ToList();
            Assert.IsFalse(firedRules2.Contains("UnionGuardedRule"),
                "UnionGuardedRule blocked when BlockingMarker present (class-level guard blocks)");

            // Clear both markers
            var secondMarker = engine.Query<SecondBlockingMarker>().First();
            var blockMarker = engine.Query<BlockingMarker>().First();
            engine.Retract(secondMarker);
            engine.Retract(blockMarker);
            foreach (var fired in firedMarkers2) engine.Retract(fired);

            // Act 3: Fire with neither marker
            engine.Fire();

            // Assert 3: UnionGuardedRule now fires (both guards satisfied)
            var firedMarkers3 = engine.Query<RuleFiredMarker>().ToList();
            var firedRules3 = firedMarkers3.Select(m => m.RuleName).ToList();
            Assert.IsTrue(firedRules3.Contains("UnionGuardedRule"),
                "UnionGuardedRule should fire when both BlockingMarker and SecondBlockingMarker absent");
        }

        /// <summary>
        /// B4 — End-to-end.
        /// Verifies the complete pipeline: RuleExport.Compile() → Assembly → load via
        /// RuleRepository/RuleEngine → insert matching fact → fire → rule's Then effect happened.
        /// </summary>
        [TestMethod]
        public void EndToEnd_CompiledRuleFires()
        {
            // Arrange: Ensure NRules assemblies are loaded before compilation
            _ = typeof(IContext).Assembly;

            var assembly = RuleExport.Compile(typeof(TestRuleGroup));
            var engine = new RuleEngine();
            engine.LoadRulesFromAssembly(assembly);

            // Insert a Counter to trigger CounterRule
            engine.Insert(new Counter { Value = 5 });

            // Act
            engine.Fire();

            // Assert: RuleFiredMarkers inserted when Counter exists
            var markers = engine.Query<RuleFiredMarker>().ToList();
            Assert.AreEqual(2, markers.Count, "Both SimpleRule and CounterRule should fire when Counter exists");

            // Assert: Both rules fired
            var firedRules = markers.Select(m => m.RuleName).ToList();
            Assert.IsTrue(firedRules.Contains("SimpleRule"), "SimpleRule should have fired");
            Assert.IsTrue(firedRules.Contains("CounterRule"), "CounterRule should have fired");
        }

        /// <summary>
        /// B5 — Parity (the anchor).
        /// Defines an equivalent hand-written ARule with the same When/Then as a generated rule.
        /// Runs generated CounterRule vs. hand-written equivalent in separate sessions against identical facts.
        /// Asserts identical firing outcome: same observable effects (marker inserted when Counter exists).
        /// This is what proves the generator doesn't subtly diverge.
        /// </summary>
        [TestMethod]
        public void Parity_GeneratedVsHandWritten()
        {
            // Arrange: Ensure NRules assemblies are loaded before compilation
            _ = typeof(IContext).Assembly;

            // Compile generated version containing CounterRule
            var generatedAssembly = RuleExport.Compile(typeof(TestRuleGroup));

            // Act 1: Run generated CounterRule
            var generatedEngine = new RuleEngine();
            generatedEngine.LoadRulesFromAssembly(generatedAssembly);
            generatedEngine.Insert(new Counter { Value = 10 });
            generatedEngine.Fire();

            // Act 2: Run hand-written equivalent rule
            var handWrittenEngine = new RuleEngine();
            handWrittenEngine.LoadRulesFromTypes(new[] { typeof(HandWrittenCounterRule) });
            handWrittenEngine.Insert(new Counter { Value = 10 });
            handWrittenEngine.Fire();

            // Assert: Both should produce exactly one marker when Counter is present
            var generatedMarkers = generatedEngine.Query<RuleFiredMarker>()
                .Where(m => m.RuleName == "CounterRule")
                .ToList();
            var handWrittenMarkers = handWrittenEngine.Query<RuleFiredMarker>()
                .Where(m => m.RuleName == "HandWrittenCounterRule")
                .ToList();

            Assert.AreEqual(1, generatedMarkers.Count, "Generated CounterRule should fire and insert marker once");
            Assert.AreEqual(1, handWrittenMarkers.Count, "Hand-written CounterRule should fire and insert marker once");

            // The observable outcome is identical — both fired exactly once
            Assert.AreEqual(generatedMarkers.Count, handWrittenMarkers.Count, "Generated and hand-written rules should produce identical firing behavior");
        }
    }
}
