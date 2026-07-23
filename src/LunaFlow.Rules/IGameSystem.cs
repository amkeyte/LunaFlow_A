namespace LunaFlow.Rules
{
    /// <summary>
    /// The top-level organizational unit for a game's rules: a named system made up of one or more rulebooks.
    /// </summary>
    /// <remarks>
    /// This is a placeholder interface for future work (Phase 3+). It is not currently in use.
    /// </remarks>
    public interface IGameSystem : IHasLunaFlowReference
    {
        /// <summary>The system's display name.</summary>
        string Name { get; }

        /// <summary>A short description of the system.</summary>
        string Description { get; }

        /// <summary>The rulebooks that make up this system (type not yet locked; see Phase 2.6.5).</summary>
        IEnumerable<object> Books { get; }
    }
}
