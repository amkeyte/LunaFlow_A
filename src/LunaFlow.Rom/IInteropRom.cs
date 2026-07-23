namespace LunaFlow.Rom
{
    /// <summary>
    /// Marker interface for a ROM acting as a capability broker. An <c>IInteropRom</c> aggregates
    /// capabilities advertised by loaded ROMs and resolves a peer's <c>Requires</c> against them.
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
