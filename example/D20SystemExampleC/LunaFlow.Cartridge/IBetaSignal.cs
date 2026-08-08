namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Contract interface for the item 8 cycle probe (phase-2-11-capability-stress-test-spec.md):
    /// the other half of the deliberately-mutual <c>Requires</c>/<c>Provides</c> pair with
    /// <see cref="IAlphaSignal"/> — see that interface's doc comment for the full shape. Never
    /// registered in the live Cartridge; test-only by design.
    /// </summary>
    public interface IBetaSignal
    {
    }
}
