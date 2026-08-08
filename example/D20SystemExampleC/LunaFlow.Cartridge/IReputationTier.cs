namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Contract interface for cross-ROM fact: the party's current reputation tier
    /// ("Rookie"/"Veteran"/"Legend"). Written by Reputation once a threshold is crossed, matched by
    /// Loot's bonus-drop rule — the first capability required from two independent providers
    /// (D20System's IChallengeSolved and Reputation's own provision), per
    /// phase-2-11-capability-stress-test-spec.md item 7. "Rookie" is the implicit no-fact default;
    /// no concrete type implements this contract for that tier.
    /// </summary>
    public interface IReputationTier
    {
        /// <summary>The tier reached: "Veteran" or "Legend". "Rookie" is never represented by a fact.</summary>
        string Tier { get; }
    }
}
