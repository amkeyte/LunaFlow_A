using LunaFlow.Engine.Components;
using LunaFlow.Rom.Probes;

namespace LunaFlow.ContentTests.Rom
{
    /// <summary>
    /// Isolated <see cref="ComponentRegistry"/> coverage for phase-2-11-capability-stress-test-spec.md's
    /// items 6 and 8: <see cref="BountyThreatProbe"/>/<see cref="HazardThreatProbe"/> (a genuine
    /// duplicate-capability clash) and <see cref="CycleProbeAlpha"/>/<see cref="CycleProbeBeta"/> (a
    /// genuine circular dependency). Each fixture builds its own standalone <see cref="ComponentRegistry"/>
    /// — never touching <c>Program.cs</c>'s real composition root, and these probes are never
    /// registered there (see <c>LunaFlow.Rom.Probes.csproj</c>'s own header comment: deliberately not
    /// referenced by <c>MyLunaFlowServer.csproj</c>). Asserts the thrown message's exact content, not
    /// just that some exception fires, and that the two failure modes are textually distinguishable
    /// from each other and from a plain missing-requirement error — same bar
    /// <c>ComponentRegistryContentTests</c> holds for the real OneD20 graph. Ticket 0079.
    /// </summary>
    [TestClass]
    [TestCategory("Engine_Components")]
    [TestCategory("Phase2.11")]
    public class ComponentRegistryProbeTests
    {
        [TestMethod]
        public void BountyAndHazardThreatProbes_BothRegistered_ThrowsNamingBothComponentsAndThreatLevel()
        {
            var registry = new ComponentRegistry();
            registry.Register(new BountyThreatProbe(), EngineTier.Rom, "BountyThreatProbe");
            registry.Register(new HazardThreatProbe(), EngineTier.Rom, "HazardThreatProbe");

            var ex = Assert.ThrowsException<InvalidOperationException>(() => registry.Validate());

            Assert.IsTrue(ex.Message.Contains("BountyThreatProbe"));
            Assert.IsTrue(ex.Message.Contains("HazardThreatProbe"));
            Assert.IsTrue(ex.Message.Contains("ThreatLevel"));
            Assert.IsFalse(ex.Message.Contains("Circular", StringComparison.OrdinalIgnoreCase),
                "The clash message must not read as item 8's cycle error");
        }

        [TestMethod]
        public void CycleProbeAlphaAndBeta_BothRegistered_ThrowsIdentifyingACircularDependency()
        {
            var registry = new ComponentRegistry();
            registry.Register(new CycleProbeAlpha(), EngineTier.Rom, "CycleProbeAlpha");
            registry.Register(new CycleProbeBeta(), EngineTier.Rom, "CycleProbeBeta");

            var ex = Assert.ThrowsException<InvalidOperationException>(() => registry.Validate());

            Assert.IsTrue(ex.Message.Contains("Circular", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(ex.Message.Contains("CycleProbeAlpha"));
            Assert.IsTrue(ex.Message.Contains("CycleProbeBeta"));
            Assert.IsFalse(ex.Message.Contains("Duplicate capability", StringComparison.OrdinalIgnoreCase),
                "The cycle message must not read as item 6's duplicate-capability clash");
            Assert.IsFalse(ex.Message.Contains("Unsatisfied requirement", StringComparison.OrdinalIgnoreCase),
                "The cycle message must not read as a plain missing-requirement error — both probes' " +
                "requirements ARE satisfied (by each other), the problem is the cycle those satisfactions form");
        }
    }
}
