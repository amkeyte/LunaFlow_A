namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Contract interface for the item 8 cycle probe (phase-2-11-capability-stress-test-spec.md):
    /// one half of a deliberately-mutual <c>Requires</c>/<c>Provides</c> pair with
    /// <see cref="IBetaSignal"/> — <c>CycleProbeAlpha</c> provides this and requires
    /// <see cref="IBetaSignal"/>, <c>CycleProbeBeta</c> provides that and requires this. Content is
    /// arbitrary; only the mutual dependency shape matters, to exercise
    /// <c>ComponentRegistry.Validate()</c>'s cycle detection. Never registered in the live Cartridge;
    /// test-only by design.
    /// </summary>
    public interface IAlphaSignal
    {
    }
}
