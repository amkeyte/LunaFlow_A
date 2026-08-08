using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.Reputation
{
    /// <summary>
    /// The Reputation ROM: tracks accumulated reputation from solved challenges and the tier
    /// thresholds crossed. Phase 2.11 item 7 — the first capability relay: requires D20System's
    /// IChallengeSolved and provides IReputationTier, which Loot's bonus-drop rule in turn requires,
    /// proving a genuine three-deep chain (D20System → Reputation → Loot).
    /// Owns facts: ReputationPoints, ReputationTierReached, ReputationCounted.
    /// Provides: ReputationTier capability.
    /// Requires: ChallengeSolved capability (from D20System).
    /// </summary>
    public sealed class ReputationRom : IRom
    {
        /// <summary>The ROM's display name.</summary>
        public string Name => "Reputation";

        /// <summary>Credits for the Reputation ROM.</summary>
        public string Credits => "Reputation tracking and tier thresholds";

        /// <summary>Capabilities this ROM provides: ReputationTier — matched by Loot's bonus-drop rule
        /// via <see cref="LunaFlow.Cartridge.IReputationTier"/>.</summary>
        public IReadOnlyList<Capability> Provides => new[]
        {
            Capability.Provided<LunaFlow.Cartridge.IReputationTier, ReputationTierReached>("ReputationTier"),
        };

        /// <summary>Capabilities this ROM requires: ChallengeSolved (from D20System) — the same
        /// contract Loot already requires, proving one provider serving two independent requirers.</summary>
        public IReadOnlyList<Capability> Requires => new[]
        {
            new Capability("ChallengeSolved", typeof(LunaFlow.Cartridge.IChallengeSolved)),
        };

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new ReputationReference();
    }

    /// <summary>Reference identity for Reputation ROM.</summary>
    internal sealed class ReputationReference : ILunaFlowReference
    {
        public string ClassUID => "Reputation";
        public string Identifier => "Reputation";
        public string FullPath => $"Reputation.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
