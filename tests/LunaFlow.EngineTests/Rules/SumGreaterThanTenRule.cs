using LunaFlow.EngineTests.Mocks;
using NRules.Fluent.Dsl;

namespace LunaFlow.EngineTests.Rules
{
    /// <summary>
    /// Increments a session counter when the sum of a number pair exceeds 10.
    /// </summary>
    public class SumGreaterThanTenRule : Rule
    {
        public override void Define()
        {
            NumberPair pair = null;
            MatchCounter counter = null;

            When()
                .Match<NumberPair>(() => pair, p => p.A + p.B > 10)
                .Match<MatchCounter>(() => counter);

            Then()
                .Do(ctx => DoWithContext(ctx, pair, counter));
        }
        private void DoWithContext(NRules.RuleModel.IContext ctx, NumberPair pair, MatchCounter counter)
        {
            counter.Increment();
            ctx.Update(counter);
            this.AddResult($"Sum matched: {pair.A} + {pair.B} = {pair.A + pair.B} (Count = {counter.Count})");
        }

    }
}
