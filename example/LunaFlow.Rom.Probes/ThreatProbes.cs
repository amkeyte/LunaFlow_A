using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.Probes
{
    /// <summary>
    /// Item 6's clash probe (phase-2-11-capability-stress-test-spec.md): a bounty-escalation model
    /// legitimately declaring <c>Provides</c> for <see cref="LunaFlow.Cartridge.IThreatLevel"/> under
    /// the name "ThreatLevel" — the same name <see cref="HazardThreatProbe"/> also declares.
    /// Registering both in the same <c>ComponentRegistry</c> and calling <c>Validate()</c> must throw,
    /// naming both components and the capability. Never registered in the live Cartridge; this ROM
    /// exists to be rejected, not to ship.
    /// </summary>
    public sealed class BountyThreatProbe : IRom
    {
        /// <summary>The ROM's display name.</summary>
        public string Name => "BountyThreatProbe";

        /// <summary>Credits for the probe.</summary>
        public string Credits => "Phase 2.11 item 6 clash probe — deliberately never ships";

        /// <summary>Provides ThreatLevel — clashes with <see cref="HazardThreatProbe"/>'s identically-named entry.</summary>
        public IReadOnlyList<Capability> Provides { get; } =
            new[] { Capability.Provided<LunaFlow.Cartridge.IThreatLevel, BountyThreatFact>("ThreatLevel") };

        /// <summary>No requirements.</summary>
        public IReadOnlyList<Capability> Requires { get; } = Array.Empty<Capability>();

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new ProbeReference("BountyThreatProbe");
    }

    /// <summary>
    /// Item 6's other clash probe: a hazard-escalation model, also legitimately declaring
    /// <c>Provides</c> for <see cref="LunaFlow.Cartridge.IThreatLevel"/> under the name "ThreatLevel"
    /// — see <see cref="BountyThreatProbe"/>'s doc comment for the full scenario.
    /// </summary>
    public sealed class HazardThreatProbe : IRom
    {
        /// <summary>The ROM's display name.</summary>
        public string Name => "HazardThreatProbe";

        /// <summary>Credits for the probe.</summary>
        public string Credits => "Phase 2.11 item 6 clash probe — deliberately never ships";

        /// <summary>Provides ThreatLevel — clashes with <see cref="BountyThreatProbe"/>'s identically-named entry.</summary>
        public IReadOnlyList<Capability> Provides { get; } =
            new[] { Capability.Provided<LunaFlow.Cartridge.IThreatLevel, HazardThreatFact>("ThreatLevel") };

        /// <summary>No requirements.</summary>
        public IReadOnlyList<Capability> Requires { get; } = Array.Empty<Capability>();

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new ProbeReference("HazardThreatProbe");
    }

    /// <summary>Minimal fact shape satisfying <see cref="BountyThreatProbe"/>'s <c>Provides</c> pairing.
    /// No rule or handler ever constructs one — this content is never fired.</summary>
    internal sealed record BountyThreatFact(int Level) : LunaFlow.Cartridge.IThreatLevel;

    /// <summary>Minimal fact shape satisfying <see cref="HazardThreatProbe"/>'s <c>Provides</c> pairing.
    /// No rule or handler ever constructs one — this content is never fired.</summary>
    internal sealed record HazardThreatFact(int Level) : LunaFlow.Cartridge.IThreatLevel;

    /// <summary>Shared reference identity for every probe ROM in this project — flat, no hierarchy,
    /// same simplicity tier as the shipped ROMs' reference types.</summary>
    internal sealed class ProbeReference : ILunaFlowReference
    {
        public ProbeReference(string identifier) { Identifier = identifier; }
        public string ClassUID => Identifier;
        public string Identifier { get; }
        public string FullPath => $"Probes.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
