using LunaFlow.Engine.Components;
using LunaFlow.Rules;

namespace LunaFlow.Rom
{
    /// <summary>
    /// Real type identity for a ROM module. A ROM is a reusable, domain-agnostic game-system
    /// toolbox (e.g., a D20 resolution system, a Physics ROM, a Dice ROM) that one or more
    /// Rulebooks can depend on.
    /// </summary>
    /// <remarks>
    /// Prior to Phase 2.6.5, a ROM had no identity type — it was just a namespace and a folder.
    /// <c>IRom</c> makes the identity explicit, carries publishing metadata, and participates
    /// in the dependency-validation system via <c>IHasDependencies</c>.
    /// </remarks>
    public interface IRom : IHasLunaFlowReference, IHasDependencies
    {
        /// <summary>The ROM's display name.</summary>
        string Name { get; }

        /// <summary>A short description or attribution for the ROM.</summary>
        string Credits { get; }
    }
}
