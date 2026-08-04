using LunaFlow.Engine;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Shared -2 roll-penalty application for <c>Poisoned</c>, used identically by <c>check</c>'s and
    /// <c>fight</c>'s PC-roll steps — read before rolling, decremented (replace-not-update, same
    /// convention as <c>PlayerPosition</c>) after, retracted at 0. Applies whether the roll happens in
    /// the same room the poison was inflicted in or a different one; <c>Poisoned</c> is not room-scoped.
    /// </summary>
    internal static class PoisonEffect
    {
        /// <summary>Returns the roll penalty to apply (0 if not poisoned, -2 if poisoned) and advances
        /// <c>Poisoned</c>'s remaining-checks count as a side effect.</summary>
        public static int ApplyAndAdvance(RuleEngine rules)
        {
            var poisoned = rules.Query<Poisoned>().FirstOrDefault();
            if (poisoned is null) return 0;

            rules.Retract(poisoned);
            if (poisoned.ChecksRemaining > 1)
                rules.Insert(new Poisoned(poisoned.ChecksRemaining - 1));

            return -2;
        }
    }
}
