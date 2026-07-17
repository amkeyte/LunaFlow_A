namespace LunaFlow.Engine
{
    /// <summary>
    /// The engine surface exposed to content assemblies (ROM today, Rulebook once that tier
    /// exists). Deliberately smaller than GameEngineCore's full surface — content code should
    /// never reach GameEngineCore itself, which stays internal.
    /// </summary>
    public interface IEngineHost
    {
        RuleEngine Rules { get; }
        void Announce(string message);
    }
}
