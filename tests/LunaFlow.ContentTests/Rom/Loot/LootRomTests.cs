using LunaFlow.Cartridge;
using LunaFlow.Rom.Loot;

namespace LunaFlow.ContentTests.Rom.Loot
{
    /// <summary>
    /// <see cref="LootRom"/>'s own shape: its <c>Provides</c> entry (<c>EquippedWeapon</c>, added by
    /// ticket 0071's ruling), its two <c>Requires</c> entries (<c>ChallengeSolved</c>, and — per
    /// phase-2-11-capability-stress-test-spec.md item 7 — <c>ReputationTier</c>), and the
    /// assembly-boundary guarantee (no reference to GridMap's or D20System's assembly — only the
    /// Cartridge contract surface) that ticket 0052's Done-When calls for.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.7.1")]
    public class LootRomTests
    {
        private static LootRom NewLootRom() =>
            new(new MonsterLootTable(new Dictionary<string, string>
            {
                ["duelist"] = "rapier",
                ["brawler"] = "knuckles",
            }));

        [TestMethod]
        public void Provides_NamesEquippedWeapon_PairedWithIEquippedWeaponContract()
        {
            var provides = NewLootRom().Provides;
            Assert.AreEqual(1, provides.Count);
            Assert.AreEqual("EquippedWeapon", provides[0].Name);
            Assert.AreEqual(typeof(IEquippedWeapon), provides[0].ContractType);
            Assert.AreEqual(typeof(EquippedWeapon), provides[0].ProviderType);
        }

        [TestMethod]
        public void Requires_NamesChallengeSolvedAndReputationTier_PairedWithTheirContracts()
        {
            var requires = NewLootRom().Requires;
            Assert.AreEqual(2, requires.Count);
            Assert.AreEqual("ChallengeSolved", requires[0].Name);
            Assert.AreEqual(typeof(IChallengeSolved), requires[0].ContractType);
            Assert.AreEqual("ReputationTier", requires[1].Name);
            Assert.AreEqual(typeof(IReputationTier), requires[1].ContractType);
        }

        [TestMethod]
        public void LootTable_ReturnsTheConstructedTable()
        {
            var table = new MonsterLootTable(new Dictionary<string, string> { ["duelist"] = "rapier" });
            var rom = new LootRom(table);
            Assert.AreSame(table, rom.LootTable);
        }

        [TestMethod]
        public void Name_IsLoot()
        {
            Assert.AreEqual("Loot", NewLootRom().Name);
        }

        [TestMethod]
        public void Assembly_ReferencesNeitherGridMapNorD20SystemAssembly()
        {
            // Loot's own .csproj references only D20SystemExampleC (the Cartridge contract surface)
            // plus Rom/Rules/Engine — never D20SystemExampleA (D20System) or D20SystemExampleB
            // (GridMap) directly. Asserted against the built assembly, not just the .csproj, so a
            // future accidental reference addition fails this test even if nobody re-reads the
            // project file.
            var referenced = typeof(LootRom).Assembly.GetReferencedAssemblies()
                .Select(a => a.Name)
                .ToList();

            Assert.IsFalse(referenced.Contains("D20SystemExampleA"),
                "Loot must never reference D20System's assembly directly");
            Assert.IsFalse(referenced.Contains("D20SystemExampleB"),
                "Loot must never reference GridMap's assembly directly");
        }
    }
}
