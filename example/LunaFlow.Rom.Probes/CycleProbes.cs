using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.Probes
{
    /// <summary>
    /// Item 8's cycle probe (phase-2-11-capability-stress-test-spec.md): provides
    /// <see cref="LunaFlow.Cartridge.IAlphaSignal"/> and requires
    /// <see cref="LunaFlow.Cartridge.IBetaSignal"/> — <see cref="CycleProbeBeta"/> requires this
    /// ROM's provision in return, forming a genuine mutual dependency. Registering both in the same
    /// <c>ComponentRegistry</c> and calling <c>Validate()</c> must throw with an error identifying a
    /// dependency cycle, distinguishable from item 6's clash error and from a plain
    /// missing-requirement error. Never registered in the live Cartridge; this ROM exists to be
    /// rejected, not to ship.
    /// </summary>
    public sealed class CycleProbeAlpha : IRom
    {
        /// <summary>The ROM's display name.</summary>
        public string Name => "CycleProbeAlpha";

        /// <summary>Credits for the probe.</summary>
        public string Credits => "Phase 2.11 item 8 cycle probe — deliberately never ships";

        /// <summary>Provides AlphaSignal.</summary>
        public IReadOnlyList<Capability> Provides { get; } =
            new[] { Capability.Provided<LunaFlow.Cartridge.IAlphaSignal, AlphaFact>("AlphaSignal") };

        /// <summary>Requires BetaSignal — provided by <see cref="CycleProbeBeta"/>, which in turn requires this ROM's AlphaSignal.</summary>
        public IReadOnlyList<Capability> Requires { get; } =
            new[] { new Capability("BetaSignal", typeof(LunaFlow.Cartridge.IBetaSignal)) };

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new ProbeReference("CycleProbeAlpha");
    }

    /// <summary>
    /// Item 8's other cycle probe: provides <see cref="LunaFlow.Cartridge.IBetaSignal"/> and requires
    /// <see cref="LunaFlow.Cartridge.IAlphaSignal"/> — the mutual half of <see cref="CycleProbeAlpha"/>.
    /// See that type's doc comment for the full scenario.
    /// </summary>
    public sealed class CycleProbeBeta : IRom
    {
        /// <summary>The ROM's display name.</summary>
        public string Name => "CycleProbeBeta";

        /// <summary>Credits for the probe.</summary>
        public string Credits => "Phase 2.11 item 8 cycle probe — deliberately never ships";

        /// <summary>Provides BetaSignal.</summary>
        public IReadOnlyList<Capability> Provides { get; } =
            new[] { Capability.Provided<LunaFlow.Cartridge.IBetaSignal, BetaFact>("BetaSignal") };

        /// <summary>Requires AlphaSignal — provided by <see cref="CycleProbeAlpha"/>, which in turn requires this ROM's BetaSignal.</summary>
        public IReadOnlyList<Capability> Requires { get; } =
            new[] { new Capability("AlphaSignal", typeof(LunaFlow.Cartridge.IAlphaSignal)) };

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new ProbeReference("CycleProbeBeta");
    }

    /// <summary>Minimal fact shape satisfying <see cref="CycleProbeAlpha"/>'s <c>Provides</c> pairing.
    /// No rule or handler ever constructs one — this content is never fired.</summary>
    internal sealed record AlphaFact : LunaFlow.Cartridge.IAlphaSignal;

    /// <summary>Minimal fact shape satisfying <see cref="CycleProbeBeta"/>'s <c>Provides</c> pairing.
    /// No rule or handler ever constructs one — this content is never fired.</summary>
    internal sealed record BetaFact : LunaFlow.Cartridge.IBetaSignal;
}
