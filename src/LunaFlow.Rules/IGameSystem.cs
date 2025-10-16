namespace LunaFlow.Rules
{
    public interface IGameSystem : IHasLunaFlowReference
    {
        string Name { get; }
        string Description { get; }
        IEnumerable<IRuleBook> Books { get; }
    }
}
