using LunaFlow.Engine.Components;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rom.Loot;
using LunaFlow.Rulebook.OneD20;

namespace LunaFlow.ContentTests.Rom
{
    /// <summary>
    /// <see cref="ComponentRegistry"/> exercised against the real 1.d20 ROM/Rulebook types, not
    /// synthetic doubles — split out of <c>Core/ComponentRegistryTests.cs</c> (ticket 0053) because
    /// this class alone pulls in GridMap/D20System/Loot/OneD20, which the rest of that file (pure
    /// <c>Capability</c>/<c>ComponentRegistry</c> semantics, all synthetic-component-based) has no
    /// need to depend on. The registry/capability semantics themselves stay covered there; this
    /// class only pins the concrete OneD20 graph.
    /// </summary>
    [TestClass]
    [TestCategory("Engine_Components")]
    [TestCategory("Phase2.7.1")]
    public class ComponentRegistryContentTests
    {
        #region Concrete case: OneD20RuleBook + D20System

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_OneD20PlusDd20System_ValidatesSuccessfully()
        {
            var registry = new ComponentRegistry();
            var d20System = new D20SystemComponent();
            var oneD20RuleBook = new OneD20RuleBook();

            registry.Register(d20System, EngineTier.Rom, "D20System");
            registry.Register(oneD20RuleBook, EngineTier.Rulebook, "OneD20");
            registry.Validate();
            // Concrete case: OneD20RuleBook (Rulebook) depends on D20System (Rom)
            // should validate successfully
        }

        #endregion

        #region Concrete case: three-ROM OneD20 registration (Phase 2.7.1)

        private static readonly IOneD20WorldModule WorldModule = new DefaultOneD20WorldModule();

        private static GridMapRom NewGridMap() =>
            new(WorldModule.Rooms.Values
                    .Select(r => new GridRoomData(r.Id, r.Row, r.Col, r.DescriptionKey))
                    .ToList(),
                WorldModule.StartRoomId);

        private static D20SystemRom NewD20System()
        {
            var assignment = new OneD20SessionAssembler(WorldModule, new Random(1)).Assemble();
            var challenges = WorldModule.Rooms.Values
                .Select(r => r.ChallengeType == OneD20ChallengeType.Fight
                    ? new RoomChallenge(r.Id, RoomChallengeType.Fight, null, null, null, null, null, null,
                        MonsterId: assignment.MonsterIdByRoom[r.Id])
                    : ToRoomChallenge(r.Id, WorldModule.Challenges[assignment.ChallengeKeyByRoom[r.Id]]))
                .ToDictionary(c => c.Id);
            var playerStats = WorldModule.PlayerCharacter;
            var monsters = WorldModule.Monsters.Values
                .Select(m => new MonsterTemplate(m.Id, m.Name, m.DescriptionKey,
                    new CombatantStats(m.Stats.Defense, m.Stats.AttackBonus, m.Stats.DamageDie, m.Stats.DamageBonus, m.Stats.MaxHp)))
                .ToDictionary(m => m.Id);
            return new D20SystemRom(
                new PlayerCharacter(new CombatantStats(playerStats.Defense, playerStats.AttackBonus, playerStats.DamageDie, playerStats.DamageBonus, playerStats.MaxHp)),
                monsters, challenges, WorldModule.PlayerAbilityScores);
        }

        private static RoomChallenge ToRoomChallenge(int roomId, OneD20ChallengeTemplate challenge) =>
            new(roomId, RoomChallengeType.SkillCheck, challenge.Skill, challenge.SkillAbility, challenge.DC,
                challenge.SuccessKey, challenge.FailureKey, challenge.PromptKey, MonsterId: null);

        private static LootRom NewLoot() => new(new MonsterLootTable(WorldModule.ItemIdByMonsterId));

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.7.1")]
        public void ComponentRegistry_GridMapD20SystemLootAllRegistered_ValidatesSuccessfully()
        {
            var registry = new ComponentRegistry();
            var ruleBook = new OneD20RuleBook();

            registry.Register(NewGridMap(), EngineTier.Rom, "GridMap");
            registry.Register(NewD20System(), EngineTier.Rom, "D20System");
            registry.Register(NewLoot(), EngineTier.Rom, "Loot");
            registry.Register(ruleBook, EngineTier.Rulebook, "OneD20");

            registry.ValidateDependsOn("OneD20", ruleBook.DependsOnRoms);
            registry.Validate();
            // Confirms the real three-ROM graph validates clean: Loot's Requires: [ChallengeSolved]
            // is satisfied by D20System's Provides, GridMap's own zero Requires (its win-line rules
            // match IChallengeSolved directly via NRules, not capability mediation) never collides
            // with Loot's requirement of the same capability name, no cycle, no duplicate provider.
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.7.1")]
        public void ComponentRegistry_OneD20DependsOnRoms_LootNotRegistered_ThrowsClearError()
        {
            // Same bar as ticket 0052's Done-When for GridMap/D20System: omitting a ROM the
            // Rulebook names in DependsOnRoms must fail loudly and specifically, not silently or
            // via an unrelated capability-mismatch message (ticket 0047's ordering rationale).
            var registry = new ComponentRegistry();
            var ruleBook = new OneD20RuleBook();

            registry.Register(NewGridMap(), EngineTier.Rom, "GridMap");
            registry.Register(NewD20System(), EngineTier.Rom, "D20System");
            // Loot deliberately not registered.
            registry.Register(ruleBook, EngineTier.Rulebook, "OneD20");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.ValidateDependsOn("OneD20", ruleBook.DependsOnRoms));
            Assert.IsTrue(ex.Message.Contains("Loot"));
            Assert.IsTrue(ex.Message.Contains("OneD20"));
        }

        #endregion

        #region Test helpers

        /// <summary>
        /// Minimal D20System ROM for the concrete test case.
        /// </summary>
        private class D20SystemComponent : IHasDependencies
        {
            public IReadOnlyList<Capability> Provides => new[]
            {
                new Capability("D20System", typeof(object))
            };

            public IReadOnlyList<Capability> Requires => Array.Empty<Capability>();
        }

        #endregion
    }
}
