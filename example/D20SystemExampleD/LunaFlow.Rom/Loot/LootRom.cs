using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.Loot
{
    /// <summary>
    /// The Loot ROM: tracks item awards from defeated monsters and the <c>inventory</c> command.
    /// Owns facts: LootAssignment, ItemAwarded, LootPickupText.
    /// Provides: none — first pure-sink ROM shape.
    /// Requires: ChallengeSolved capability (from D20System).
    /// </summary>
    public sealed class LootRom : IRom
    {
        private readonly MonsterLootTable _lootTable;

        /// <summary>Constructs the Loot ROM with the static monster-to-item drop table.</summary>
        public LootRom(MonsterLootTable lootTable)
        {
            _lootTable = lootTable;
        }

        /// <summary>The ROM's display name.</summary>
        public string Name => "Loot";

        /// <summary>Credits for the Loot ROM.</summary>
        public string Credits => "Monster loot and inventory tracking";

        /// <summary>Capabilities this ROM provides: none. First pure-sink shape.</summary>
        public IReadOnlyList<Capability> Provides => Array.Empty<Capability>();

        /// <summary>Capabilities this ROM requires: ChallengeSolved (from D20System).</summary>
        public IReadOnlyList<Capability> Requires => new[]
        {
            new Capability("ChallengeSolved", typeof(LunaFlow.Cartridge.IChallengeSolved)),
        };

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new LootReference();

        /// <summary>Provides access to the monster-to-item drop table.</summary>
        public MonsterLootTable LootTable => _lootTable;
    }

    /// <summary>Reference identity for Loot ROM.</summary>
    internal sealed class LootReference : ILunaFlowReference
    {
        public string ClassUID => "Loot";
        public string Identifier => "Loot";
        public string FullPath => $"Loot.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
