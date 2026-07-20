using LunaFlow.Rules;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// The concrete <see cref="IMechanic"/> backing 1.d20's dice rolling and generic
    /// <c>d20 + modifier vs. target number</c> check resolution. Per ticket 0015's ruling, these
    /// operations live here — on a concrete class — rather than on <see cref="IMechanic"/> itself.
    /// Used by both <c>check</c> (skill checks) and <c>fight</c> (attack rolls): both are the same
    /// underlying resolution operation with different inputs.
    /// </summary>
    public sealed class D20ResolutionMechanic : IMechanic
    {
        private readonly Random _random = new();

        /// <inheritdoc/>
        public ILunaFlowReference Reference { get; } = new OneD20Reference("D20ResolutionMechanic");

        /// <summary>Rolls dice per <paramref name="die"/>'s "NdM" notation (e.g. "1d20", "1d8") and returns the sum.</summary>
        public int Roll(string die)
        {
            var parts = die.Split('d', 2);
            var count = int.Parse(parts[0]);
            var sides = int.Parse(parts[1]);

            var total = 0;
            for (var i = 0; i < count; i++)
                total += _random.Next(1, sides + 1);
            return total;
        }

        /// <summary>Rolls 1d20 + <paramref name="modifier"/> and reports whether it meets or beats <paramref name="targetNumber"/>.</summary>
        public bool ResolveCheck(int modifier, int targetNumber) => Roll("1d20") + modifier >= targetNumber;
    }
}
