namespace LunaFlow.Rules
{
    /// <summary>
    /// An object that tracks and or mutates multiple facts in a structured hard-wired way
    /// that defines core functionality within a RuleSet.
    /// </summary>
    /// <remarks>
    /// Category marker only, mirroring <see cref="ARule"/>'s <see cref="IHasLunaFlowReference"/>
    /// shape exactly — per ticket 0015's ruling, the actual operations (dice rolling, check
    /// resolution, or a future Physics ROM's trajectory math) belong on the concrete class
    /// implementing this interface, not here. Public so a concrete Mechanic living in
    /// <c>LunaFlow.Rom</c> (a different assembly) can implement it.
    /// </remarks>
    public interface IMechanic : IHasLunaFlowReference
    {
    }
}
