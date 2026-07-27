using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rulebook.OneD20;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type (intentional in null-argument tests)

namespace LunaFlow.EngineTests.Core
{
    [TestClass]
    [TestCategory("Engine_Components")]
    [TestCategory("Phase2.6.5")]
    public class ComponentRegistryTests
    {
        #region Capability creation validation

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void Capability_ValidNameAndType_CreatesSuccessfully()
        {
            var cap = new Capability("TestCap", typeof(object));
            Assert.AreEqual("TestCap", cap.Name);
            Assert.AreEqual(typeof(object), cap.ContractType);
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        [ExpectedException(typeof(ArgumentException))]
        public void Capability_NullName_ThrowsArgumentException()
        {
            _ = new Capability(null, typeof(object));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        [ExpectedException(typeof(ArgumentException))]
        public void Capability_EmptyName_ThrowsArgumentException()
        {
            _ = new Capability("", typeof(object));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        [ExpectedException(typeof(ArgumentException))]
        public void Capability_WhitespaceName_ThrowsArgumentException()
        {
            _ = new Capability("   ", typeof(object));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Capability_NullContractType_ThrowsArgumentNullException()
        {
            _ = new Capability("TestCap", null);
        }

        #endregion

        #region Registry basics

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_RegisterSingleComponent_Succeeds()
        {
            var registry = new ComponentRegistry();
            var component = new TestComponent();

            registry.Register(component, EngineTier.Rom, "TestRom");
            registry.Validate();
            // If no exception, registration and validation succeeded
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_RegisterMultipleComponents_Succeeds()
        {
            var registry = new ComponentRegistry();
            var rom = new TestComponent();
            var rulebook = new TestComponent();

            registry.Register(rom, EngineTier.Rom, "TestRom");
            registry.Register(rulebook, EngineTier.Rulebook, "TestRulebook");
            registry.Validate();
            // If no exception, both registrations succeeded
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComponentRegistry_RegisterNullComponent_ThrowsArgumentNullException()
        {
            var registry = new ComponentRegistry();
            registry.Register(null, EngineTier.Rom, "TestRom");
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        [ExpectedException(typeof(ArgumentException))]
        public void ComponentRegistry_RegisterNullComponentName_ThrowsArgumentException()
        {
            var registry = new ComponentRegistry();
            var component = new TestComponent();
            registry.Register(component, EngineTier.Rom, null);
        }

        #endregion

        #region Two-phase validation: Phase 1 (collect)

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_SingleProviderCollected_Succeeds()
        {
            var registry = new ComponentRegistry();
            var provider = new ComponentWithCapabilities(
                provides: new[] { new Capability("MyService", typeof(object)) },
                requires: Array.Empty<Capability>()
            );

            registry.Register(provider, EngineTier.Rom, "Provider");
            registry.Validate();
            // Phase 1 succeeds: capability collected
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_DuplicateCapabilityName_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var provider1 = new ComponentWithCapabilities(
                provides: new[] { new Capability("Service", typeof(object)) },
                requires: Array.Empty<Capability>()
            );
            var provider2 = new ComponentWithCapabilities(
                provides: new[] { new Capability("Service", typeof(object)) },
                requires: Array.Empty<Capability>()
            );

            registry.Register(provider1, EngineTier.Rom, "Provider1");
            registry.Register(provider2, EngineTier.Rom, "Provider2");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.Validate()
            );
            Assert.IsTrue(ex.Message.Contains("Duplicate capability"));
            Assert.IsTrue(ex.Message.Contains("Service"));
            Assert.IsTrue(ex.Message.Contains("Provider1"));
            Assert.IsTrue(ex.Message.Contains("Provider2"));
        }

        #endregion

        #region Two-phase validation: Phase 2 (satisfy)

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_RequirementSatisfiedByProvider_Succeeds()
        {
            var registry = new ComponentRegistry();
            var provider = new ComponentWithCapabilities(
                provides: new[] { new Capability("MyService", typeof(object)) },
                requires: Array.Empty<Capability>()
            );
            var consumer = new ComponentWithCapabilities(
                provides: Array.Empty<Capability>(),
                requires: new[] { new Capability("MyService", typeof(object)) }
            );

            registry.Register(provider, EngineTier.Rom, "Provider");
            registry.Register(consumer, EngineTier.Rulebook, "Consumer");
            registry.Validate();
            // Phase 2 succeeds: requirement is satisfied
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_UnsatisfiedRequirement_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var consumer = new ComponentWithCapabilities(
                provides: Array.Empty<Capability>(),
                requires: new[] { new Capability("MissingService", typeof(object)) }
            );

            registry.Register(consumer, EngineTier.Rulebook, "Consumer");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.Validate()
            );
            Assert.IsTrue(ex.Message.Contains("Unsatisfied requirement"));
            Assert.IsTrue(ex.Message.Contains("Consumer"));
            Assert.IsTrue(ex.Message.Contains("MissingService"));
        }

        #endregion

        #region Two-phase validation: Phase 3 (tier ordering)

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_LowerTierDependsOnHigherTier_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var gmOverlay = new ComponentWithCapabilities(
                provides: Array.Empty<Capability>(),
                requires: new[] { new Capability("PlayerCosmeticFeature", typeof(object)) }
            );
            var player = new ComponentWithCapabilities(
                provides: new[] { new Capability("PlayerCosmeticFeature", typeof(object)) },
                requires: Array.Empty<Capability>()
            );

            registry.Register(gmOverlay, EngineTier.GmOverlay, "GmOverlay");
            registry.Register(player, EngineTier.Player, "Player");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.Validate()
            );
            Assert.IsTrue(ex.Message.Contains("Tier violation"));
            Assert.IsTrue(ex.Message.Contains("GmOverlay"));
            Assert.IsTrue(ex.Message.Contains("Player"));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_HigherTierDependsOnLowerTier_Succeeds()
        {
            var registry = new ComponentRegistry();
            var rom = new ComponentWithCapabilities(
                provides: new[] { new Capability("GridNavigation", typeof(object)) },
                requires: Array.Empty<Capability>()
            );
            var rulebook = new ComponentWithCapabilities(
                provides: Array.Empty<Capability>(),
                requires: new[] { new Capability("GridNavigation", typeof(object)) }
            );

            registry.Register(rom, EngineTier.Rom, "GridRom");
            registry.Register(rulebook, EngineTier.Rulebook, "MyRulebook");
            registry.Validate();
            // Tier ordering is valid: Rulebook (tier 1) depends on Rom (tier 0)
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_SameTierDependsOnSameTier_Succeeds()
        {
            var registry = new ComponentRegistry();
            var rom1 = new ComponentWithCapabilities(
                provides: new[] { new Capability("Service1", typeof(object)) },
                requires: Array.Empty<Capability>()
            );
            var rom2 = new ComponentWithCapabilities(
                provides: Array.Empty<Capability>(),
                requires: new[] { new Capability("Service1", typeof(object)) }
            );

            registry.Register(rom1, EngineTier.Rom, "Rom1");
            registry.Register(rom2, EngineTier.Rom, "Rom2");
            registry.Validate();
            // Same-tier dependency is allowed (both Rom tier)
        }

        #endregion

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

        #region Cycle detection (Phase 4 of validation)

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_TwoPeerMutualCycle_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var romA = new ComponentWithCapabilities(
                provides: new[] { new Capability("ServiceA", typeof(object)) },
                requires: new[] { new Capability("ServiceB", typeof(object)) }
            );
            var romB = new ComponentWithCapabilities(
                provides: new[] { new Capability("ServiceB", typeof(object)) },
                requires: new[] { new Capability("ServiceA", typeof(object)) }
            );

            registry.Register(romA, EngineTier.Rom, "RomA");
            registry.Register(romB, EngineTier.Rom, "RomB");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.Validate()
            );
            Assert.IsTrue(ex.Message.Contains("Circular dependency"));
            Assert.IsTrue(ex.Message.Contains("RomA") || ex.Message.Contains("RomB"));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_SelfCycle_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var romSelfDep = new ComponentWithCapabilities(
                provides: new[] { new Capability("MySelfService", typeof(object)) },
                requires: new[] { new Capability("MySelfService", typeof(object)) }
            );

            registry.Register(romSelfDep, EngineTier.Rom, "SelfDepRom");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.Validate()
            );
            Assert.IsTrue(ex.Message.Contains("Circular dependency"));
            Assert.IsTrue(ex.Message.Contains("SelfDepRom"));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_ThreePeerCycle_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var romA = new ComponentWithCapabilities(
                provides: new[] { new Capability("ServiceA", typeof(object)) },
                requires: new[] { new Capability("ServiceC", typeof(object)) }
            );
            var romB = new ComponentWithCapabilities(
                provides: new[] { new Capability("ServiceB", typeof(object)) },
                requires: new[] { new Capability("ServiceA", typeof(object)) }
            );
            var romC = new ComponentWithCapabilities(
                provides: new[] { new Capability("ServiceC", typeof(object)) },
                requires: new[] { new Capability("ServiceB", typeof(object)) }
            );

            registry.Register(romA, EngineTier.Rom, "RomA");
            registry.Register(romB, EngineTier.Rom, "RomB");
            registry.Register(romC, EngineTier.Rom, "RomC");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => registry.Validate()
            );
            Assert.IsTrue(ex.Message.Contains("Circular dependency"));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_DiamondNoDependencyCycle_Succeeds()
        {
            // Diamond shape: C1 and C2 both require the same capability from C3.
            // This is NOT a cycle — it's a valid multi-consumer pattern.
            // The DFS should visit C3, return from C3, then visit C2, and not false-positive on C3 being already-visited.
            var registry = new ComponentRegistry();
            var provider = new ComponentWithCapabilities(
                provides: new[] { new Capability("CommonService", typeof(object)) },
                requires: Array.Empty<Capability>()
            );
            var consumer1 = new ComponentWithCapabilities(
                provides: new[] { new Capability("C1Service", typeof(object)) },
                requires: new[] { new Capability("CommonService", typeof(object)) }
            );
            var consumer2 = new ComponentWithCapabilities(
                provides: new[] { new Capability("C2Service", typeof(object)) },
                requires: new[] { new Capability("CommonService", typeof(object)) }
            );

            registry.Register(provider, EngineTier.Rom, "Provider");
            registry.Register(consumer1, EngineTier.Rom, "Consumer1");
            registry.Register(consumer2, EngineTier.Rom, "Consumer2");
            registry.Validate();
            // If we reach here, the diamond was handled correctly (no false positive)
        }

        #endregion

        #region Duplicate component name guard (A1)

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_RegisterDuplicateComponentName_ThrowsWithClearMessage()
        {
            var registry = new ComponentRegistry();
            var component1 = new TestComponent();
            var component2 = new TestComponent();

            registry.Register(component1, EngineTier.Rom, "DuplicateName");

            var ex = Assert.ThrowsException<ArgumentException>(
                () => registry.Register(component2, EngineTier.Rom, "DuplicateName")
            );
            Assert.IsTrue(ex.Message.Contains("already registered"));
            Assert.IsTrue(ex.Message.Contains("DuplicateName"));
        }

        [TestMethod]
        [TestCategory("Engine_Components")]
        [TestCategory("Phase2.6.5")]
        public void ComponentRegistry_MultipleRegistrations_AllowsDifferentNames()
        {
            var registry = new ComponentRegistry();
            var component1 = new TestComponent();
            var component2 = new TestComponent();
            var component3 = new TestComponent();

            registry.Register(component1, EngineTier.Rom, "Rom1");
            registry.Register(component2, EngineTier.Rom, "Rom2");
            registry.Register(component3, EngineTier.Rulebook, "Rulebook1");
            registry.Validate();
            // If no exception, all three registered successfully with different names
        }

        #endregion

        #region Test helpers

        /// <summary>
        /// Minimal test implementation of IHasDependencies with no dependencies.
        /// </summary>
        private class TestComponent : IHasDependencies
        {
            public IReadOnlyList<Capability> Provides => Array.Empty<Capability>();
            public IReadOnlyList<Capability> Requires => Array.Empty<Capability>();
        }

        /// <summary>
        /// Test implementation of IHasDependencies with configurable provides/requires.
        /// </summary>
        private class ComponentWithCapabilities : IHasDependencies
        {
            public IReadOnlyList<Capability> Provides { get; }
            public IReadOnlyList<Capability> Requires { get; }

            public ComponentWithCapabilities(
                IEnumerable<Capability> provides,
                IEnumerable<Capability> requires)
            {
                Provides = provides.ToList().AsReadOnly();
                Requires = requires.ToList().AsReadOnly();
            }
        }

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

#pragma warning restore CS8625
