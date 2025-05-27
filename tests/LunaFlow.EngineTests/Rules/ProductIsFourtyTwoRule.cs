using LunaFlow.EngineTests.Mocks;
using NRules.Fluent.Dsl;

namespace LunaFlow.EngineTests.Rules
{
    /// <summary>
    /// Fires when the product of a number pair is exactly 42.
    /// </summary>
    public class ProductIsFortyTwoRule : Rule
    {
        public override void Define()
        {
            NumberPair pair = null;

            When()
                .Match<NumberPair>(() => pair, p => p.A * p.B == 42);

            Then()
                .Do(ctx => this.AddResult($"[Rule: Product == 42] Matched: {pair.A} * {pair.B} = 42"));
        }

        public string Result { get; private set; } = "";

    }
}
