namespace LunaFlow.Rom
{
    /// <summary>
    /// Marker interface tagging a ROM as the intended capability broker for peer-ROM interoperability.
    /// The broker's concrete surface (methods to aggregate and resolve capabilities) is specified in Phase 2.7,
    /// once the consuming ROM's actual needs are known.
    /// </summary>
    /// <remarks>
    /// Kept as a distinct, named interface rather than folded into plain <c>IRom</c> for the same
    /// reason <c>IMechanic</c> stayed separate from <c>IHasLunaFlowReference</c> (ticket 0015):
    /// any <c>IRom</c> could technically play this role, but a named interface is a discoverability
    /// win for content authors.
    /// </remarks>
    public interface IInteropRom : IRom
    {
    }
}
