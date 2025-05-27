using LunaFlow.EngineTests.Mocks;
using NRules.Fluent.Dsl;

namespace LunaFlow.EngineTests.Rules
{
    /// <summary>
    /// Fires when the match counter exceeds 5 and declares a winner.
    /// </summary>
    public class MatchThresholdReachedRule : Rule
    {
        public override void Define()
        {
            MatchCounter counter = null;

            When()
                .Match<MatchCounter>(() => counter, c => c.Count > 5);

            Then()
                .Do(ctx => this.AddResult("Winner Winner Chicken Dinner"));
        }
    }
}
