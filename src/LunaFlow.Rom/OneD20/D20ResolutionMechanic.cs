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
        public CheckResult ResolveCheck(int modifier, int targetNumber)
        {
            var d20 = Roll("1d20");
            var total = d20 + modifier;
            return new CheckResult(d20, modifier, total, targetNumber, total >= targetNumber);
        }
    }

    /// <summary>
    /// The outcome of a <see cref="D20ResolutionMechanic.ResolveCheck"/> call — carries the raw
    /// roll and total alongside the pass/fail verdict so callers can build a mechanical readout
    /// (1-d20-spec.md's "skill name, roll, DC" for <c>check</c>) without re-rolling.
    /// </summary>
    /// <param name="D20">The raw 1d20 die result, before <paramref name="Modifier"/>.</param>
    /// <param name="Modifier">The modifier applied.</param>
    /// <param name="Total"><paramref name="D20"/> + <paramref name="Modifier"/> — what's actually compared to <paramref name="TargetNumber"/>.</param>
    /// <param name="TargetNumber">The DC or Defense this check was resolved against.</param>
    /// <param name="Success">Whether <paramref name="Total"/> met or beat <paramref name="TargetNumber"/>.</param>
    public readonly record struct CheckResult(int D20, int Modifier, int Total, int TargetNumber, bool Success);
}
