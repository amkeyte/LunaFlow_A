using LunaFlow.Rules;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.EngineTests.GeneratorServices
{
    /// <summary>
    /// Marker fact used to test guard injection (.Not&lt;T&gt;() guards).
    /// </summary>
    public sealed class BlockingMarker
    {
        public BlockingMarker(string id = "default")
        {
            Id = id;
        }

        public string Id { get; }
    }

    /// <summary>
    /// Second marker type for testing class-level vs. property-level guard union.
    /// </summary>
    public sealed class SecondBlockingMarker
    {
        public SecondBlockingMarker(string id = "default")
        {
            Id = id;
        }

        public string Id { get; }
    }

    /// <summary>
    /// Marker fact inserted by rules to make their firing observable in tests.
    /// </summary>
    public sealed class RuleFiredMarker
    {
        public RuleFiredMarker(string ruleName)
        {
            RuleName = ruleName;
        }

        public string RuleName { get; }
    }

    /// <summary>
    /// Counter fact incremented by rules for testing multiple activations.
    /// </summary>
    public sealed class Counter
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Sample rule group with plain rules (no class-level guards).
    /// Used to test B1 (parsing), B2 (generation/compilation), B4 (end-to-end).
    /// </summary>
    [RuleGroup("TestGroup")]
    public static partial class TestRuleGroup
    {
        /// <summary>
        /// Simple rule that matches a Counter fact and inserts a marker.
        /// Observable effect: inserts RuleFiredMarker("SimpleRule").
        /// No guards at class or property level.
        /// </summary>
        [RuleTemplate]
        public static RulePrototype SimpleRule => new()
        {
            Name = "SimpleRule",
            Description = "A simple rule that fires when Counter exists",
            RequiredUsings = new() { "LunaFlow.EngineTests.GeneratorServices" },
            PreambleStatements = new() { "Counter c = null;" },
            WhenStatements = new() { "Match<Counter>(() => c)" },
            ThenStatements = new() { "Do(ctx => ctx.Insert(new RuleFiredMarker(\"SimpleRule\")))" }
        };

        /// <summary>
        /// Rule that matches a Counter fact and inserts a marker.
        /// Observable effect: inserts RuleFiredMarker("CounterRule") when Counter exists.
        /// Used for B5 parity test (comparing generated vs. hand-written).
        /// </summary>
        [RuleTemplate]
        public static RulePrototype CounterRule => new()
        {
            Name = "CounterRule",
            Description = "Rule that fires when Counter exists",
            RequiredUsings = new() { "LunaFlow.EngineTests.GeneratorServices" },
            PreambleStatements = new() { "Counter c = null;" },
            WhenStatements = new() { "Match<Counter>(() => c)" },
            ThenStatements = new() { "Do(ctx => ctx.Insert(new RuleFiredMarker(\"CounterRule\")))" }
        };
    }

    /// <summary>
    /// Sample rule group with guarded rules for testing guard injection.
    /// Used to test B3 (guard injection behavior).
    /// </summary>
    [RuleGroup("GuardedGroup")]
    [StandingRule(typeof(BlockingMarker))]
    public static partial class GuardedRuleGroup
    {
        /// <summary>
        /// Rule with a class-level guard (BlockingMarker from [RuleGroup] attribute).
        /// Observable effect: inserts RuleFiredMarker("ClassGuardedRule").
        /// Fires when BlockingMarker is absent, blocked when present.
        /// </summary>
        [RuleTemplate]
        public static RulePrototype ClassGuardedRule => new()
        {
            Name = "ClassGuardedRule",
            Description = "Rule guarded at class level",
            RequiredUsings = new() { "LunaFlow.EngineTests.GeneratorServices" },
            PreambleStatements = new() { "Counter c = null;" },
            WhenStatements = new() { "Match<Counter>(() => c)" },
            ThenStatements = new() { "Do(ctx => ctx.Insert(new RuleFiredMarker(\"ClassGuardedRule\")))" }
        };

        /// <summary>
        /// Rule with both class-level and property-level guards (same type).
        /// Both guard BlockingMarker at class and property level.
        /// Observable effect: inserts RuleFiredMarker("MultiGuardedRule").
        /// Guards are deduped: RuleIR.StandingRules.Count == 1 (proves dedup).
        /// </summary>
        [RuleTemplate]
        [StandingRule(typeof(BlockingMarker))]
        public static RulePrototype MultiGuardedRule => new()
        {
            Name = "MultiGuardedRule",
            Description = "Rule with both class and property level guards (same type)",
            RequiredUsings = new() { "LunaFlow.EngineTests.GeneratorServices" },
            PreambleStatements = new() { "Counter c = null;" },
            WhenStatements = new() { "Match<Counter>(() => c)" },
            ThenStatements = new() { "Do(ctx => ctx.Insert(new RuleFiredMarker(\"MultiGuardedRule\")))" }
        };

        /// <summary>
        /// Rule with class-level and property-level guards of DIFFERENT types.
        /// Class-level: BlockingMarker. Property-level: SecondBlockingMarker.
        /// Observable effect: inserts RuleFiredMarker("UnionGuardedRule").
        /// Tests the union and behavioral blocking of both guard types.
        /// </summary>
        [RuleTemplate]
        [StandingRule(typeof(SecondBlockingMarker))]
        public static RulePrototype UnionGuardedRule => new()
        {
            Name = "UnionGuardedRule",
            Description = "Rule with class and property level guards (different types)",
            RequiredUsings = new() { "LunaFlow.EngineTests.GeneratorServices" },
            PreambleStatements = new() { "Counter c = null;" },
            WhenStatements = new() { "Match<Counter>(() => c)" },
            ThenStatements = new() { "Do(ctx => ctx.Insert(new RuleFiredMarker(\"UnionGuardedRule\")))" }
        };
    }

    /// <summary>
    /// Hand-written equivalent rule for B5 parity test.
    /// Matches Counter and inserts a marker — identical behavior to generated CounterRule
    /// to verify the generator produces equivalent rules.
    /// </summary>
    public sealed class HandWrittenCounterRule : ARule
    {
        public override ILunaFlowReference Reference { get; } =
            new CounterRuleReference();

        public override void Define()
        {
            Counter c = null!;
            When()
                .Match<Counter>(() => c);

            Then()
                .Do(ctx => ctx.Insert(new RuleFiredMarker("HandWrittenCounterRule")));
        }

        private sealed class CounterRuleReference : ILunaFlowReference
        {
            public string ClassUID => "HandWrittenCounterRule";
            public string Identifier => "CounterRule";
            public string FullPath => "HandWrittenCounterRule";
            public string Path => "HandWrittenCounterRule";
            public ILunaFlowReference Parent => null!;
            public IEnumerable<ILunaFlowReference> Subreferences => Enumerable.Empty<ILunaFlowReference>();
        }
    }
}
