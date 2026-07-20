namespace LunaFlow.Rules
{
    /// <summary>
    /// The top-level organizational unit for a game's rules: a named system made up of one or more rulebooks.
    /// </summary>
    public interface IGameSystem : IHasLunaFlowReference
    {
        /// <summary>The system's display name.</summary>
        string Name { get; }

        /// <summary>A short description of the system.</summary>
        string Description { get; }

        /// <summary>The rulebooks that make up this system.</summary>
        IEnumerable<IRuleBook> Books { get; }
    }
}
