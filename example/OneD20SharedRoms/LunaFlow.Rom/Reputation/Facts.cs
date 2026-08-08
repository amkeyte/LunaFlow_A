using LunaFlow.Cartridge;

namespace LunaFlow.Rom.Reputation
{
    /// <summary>
    /// The party's accumulated reputation score. Session-scoped, replace-not-update (retracted and
    /// reinserted on every solved challenge), same convention as GridMap's <c>PlayerPosition</c> and
    /// D20System's <c>CombatRound</c>. Not itself a Cartridge contract — only a tier crossing
    /// (<see cref="ReputationTierReached"/>) is observable outside this ROM.
    /// </summary>
    public sealed record ReputationPoints(int Total);

    /// <summary>
    /// Inserted once per threshold crossed ("Veteran" at 3, "Legend" at 6), never retracted and
    /// never re-inserted for a tier already reached. "Rookie" — the implicit default below the
    /// first threshold — has no corresponding fact; its absence <i>is</i> the Rookie state.
    /// </summary>
    public sealed record ReputationTierReached(string Tier) : IReputationTier;

    /// <summary>
    /// Reputation's own bookkeeping: marks that <c>RoomId</c>'s solved challenge has already been
    /// counted toward <see cref="ReputationPoints"/>. Not part of the spec's stated fact vocabulary —
    /// added because <see cref="ReputationPoints"/>'s replace-not-update convention means every solve
    /// retracts and reinserts it, and NRules' Rete join would otherwise re-pair the fresh
    /// <c>ReputationPoints</c> against every already-solved room's <c>IChallengeSolved</c> fact,
    /// re-firing the accrual rule once per prior solve instead of once for the new one. Same
    /// per-room-marker idiom Loot's <c>ItemAwarded</c> guard already uses against
    /// <c>LootAssignment</c>/<c>LootPickupText</c> re-matching, applied here against a fact that
    /// additionally churns.
    /// </summary>
    public sealed record ReputationCounted(int RoomId);
}
