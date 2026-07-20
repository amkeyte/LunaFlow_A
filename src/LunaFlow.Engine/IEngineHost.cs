namespace LunaFlow.Engine
{
    /// <summary>
    /// The engine surface exposed to content assemblies (ROM today, Rulebook once that tier
    /// exists). Deliberately smaller than GameEngineCore's full surface — content code should
    /// never reach GameEngineCore itself, which stays internal.
    /// </summary>
    public interface IEngineHost
    {
        /// <summary>The rule engine content can insert facts into and fire.</summary>
        RuleEngine Rules { get; }

        /// <summary>Posts <paramref name="message"/> to the engine's <c>MessagePosted</c> event for the host translator to display.</summary>
        void Announce(string message);
    }
}
