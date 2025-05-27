using LunaFlow.EngineTests.Mocks;
using NRules.Fluent.Dsl;
using NRules.RuleModel;

namespace LunaFlow.EngineTests.Rules
{
    public abstract class BaseGameSessionRule : Rule
    {
        public override void Define()
        {
            When()
                .Not<WinnerDeclared>();

            MatchWhen();

            ThenDo();

            Then()
                .Do(ctx => DoWithContext(ctx));
        }

        private void DoWithContext(IContext ctx)
        {
            this.AddResult("Winner Winner Chicken Dinner");
        }

        protected abstract void MatchWhen();
        protected virtual void ThenDo()
        {
            //noop
        }
    }


    /// <summary>
    /// Marker fact to indicate a winner has already been declared.
    /// Used to prevent repeated rule firings.
    /// </summary>
    public class WinnerDeclared
    {

    }

    /// <summary>
    /// Fires when the match counter exceeds 5 and declares a winner.
    /// </summary>
    [Repeatability(RuleRepeatability.NonRepeatable)]
    public class MatchThresholdReachedRule : BaseGameSessionRule
    {

        protected override void MatchWhen()
        {
            MatchCounter counter = null;

            When()
                .Match<MatchCounter>(() => counter, c => c.Count > 5);
        }

        protected override void ThenDo()
        {
            Then()
                .Do(ctx => DoWithContext(ctx));
        }

        private void DoWithContext(IContext ctx)
        {
            ctx.Insert(new WinnerDeclared());
        }

    }


}
