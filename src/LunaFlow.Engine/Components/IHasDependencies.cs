namespace LunaFlow.Engine.Components
{
    /// <summary>
    /// Contract for components (ROMs, Ruleooks, World Modules, etc.) that declare
    /// what capabilities they provide and require.
    /// </summary>
    public interface IHasDependencies
    {
        /// <summary>
        /// Capabilities this component advertises for other components to depend on.
        /// </summary>
        IReadOnlyList<Capability> Provides { get; }

        /// <summary>
        /// Capabilities this component needs from other components.
        /// </summary>
        IReadOnlyList<Capability> Requires { get; }
    }
}
