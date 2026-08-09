using LunaFlow.Engine;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Shared <see cref="MonsterEnraged"/> retraction for a <c>CombatRound</c>'s four terminal
    /// outcomes (win, loss, timeout, flee) — used identically by <c>FightCommandHandler</c>'s three
    /// terminal branches and <c>FleeCommandHandler</c>, so a fresh fight against the same monster in
    /// a retry always starts un-enraged. Mirrors <see cref="PoisonEffect"/>'s shape (a small static
    /// helper shared across handlers), but for a combat-scoped marker rather than a cross-room one.
    /// </summary>
    internal static class EnrageEffect
    {
        /// <summary>Retracts <see cref="MonsterEnraged"/> for <paramref name="roomId"/> if present;
        /// no-op otherwise.</summary>
        public static void RetractIfPresent(RuleEngine rules, int roomId)
        {
            var enraged = rules.Query<MonsterEnraged>().FirstOrDefault(e => e.RoomId == roomId);
            if (enraged is not null) rules.Retract(enraged);
        }
    }
}
