using LunaFlow.Engine.Components;

namespace LunaFlow.Rom
{
    /// <summary>
    /// Capability broker for peer-ROM interoperability. Resolves declared capabilities
    /// to the instances providing them, enabling cross-ROM composition with zero assembly
    /// reference between peer ROMs.
    /// </summary>
    /// <remarks>
    /// Kept as a distinct, named interface rather than folded into plain <c>IRom</c> for the same
    /// reason <c>IMechanic</c> stayed separate from <c>IHasLunaFlowReference</c> (ticket 0015):
    /// any <c>IRom</c> could technically play this role, but a named interface is a discoverability
    /// win for content authors.
    /// </remarks>
    public interface IInteropRom : IRom
    {
        /// <summary>Resolves a declared capability to the instance providing it.</summary>
        /// <exception cref="InvalidOperationException">No registered provider for that capability.</exception>
        object Resolve(Capability capability);

        /// <summary>Typed convenience over <see cref="Resolve"/>.</summary>
        T Resolve<T>(string capabilityName) where T : class;
    }
}
