namespace LunaFlow.Rom
{
    /// <summary>
    /// Marker interface for a ROM acting as capability broker.
    /// </summary>
    /// <remarks>
    /// Kept as a distinct, named interface rather than folded into plain <c>IRom</c> for the same
    /// reason <c>IMechanic</c> stayed separate from <c>IHasLunaFlowReference</c> (ticket 0015):
    /// any <c>IRom</c> could technically play this role, but a named interface is a discoverability
    /// win for content authors.
    /// </remarks>
    /// <remarks>
    /// <c>Resolve</c>/<c>Resolve&lt;T&gt;</c> were cut in the Cartridge pivot (ticket 0039): a
    /// runtime pull API across a trust boundary is undeclared-by-construction and
    /// <c>ComponentRegistry.Validate()</c> can't see it. Service-shaped cross-ROM dependencies are
    /// satisfied by plain constructor injection from the composition root instead. This interface
    /// is an empty marker again, as originally shipped in Phase 2.6.5.
    /// </remarks>
    public interface IInteropRom : IRom
    {
    }
}
