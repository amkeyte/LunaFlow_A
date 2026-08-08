namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Contract interface for the item 6 clash probe (phase-2-11-capability-stress-test-spec.md):
    /// "how threatening is the current encounter" — exactly the kind of signal more than one content
    /// system could plausibly own (a bounty-escalation model, a hazard-escalation model). Two probe
    /// ROMs (<c>BountyThreatProbe</c>/<c>HazardThreatProbe</c>, <c>LunaFlow.Rom.Probes</c>)
    /// legitimately both declare <c>Provides</c> for this capability under the same name
    /// ("ThreatLevel") — deliberately, to exercise <c>ComponentRegistry.Validate()</c>'s duplicate-
    /// provider rejection. Never registered in the live Cartridge; test-only by design.
    /// </summary>
    public interface IThreatLevel
    {
        /// <summary>The current threat level. Never read — the probe never fires, by design.</summary>
        int Level { get; }
    }
}
