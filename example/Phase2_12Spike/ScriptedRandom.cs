namespace Phase2_12Spike
{
    /// <summary>
    /// A <see cref="Random"/> that returns a pre-scripted sequence of values instead of pseudo-random
    /// ones — every roll in a scenario is chosen up front (not hand-computed against a real PRNG
    /// seed) so a fight sequence's hit/miss/enrage outcomes are exact and auditable from the call
    /// site, not discovered by trial and error. <see cref="D20ResolutionMechanic.Roll"/> only ever
    /// calls <see cref="Next(int,int)"/>, one call per die rolled — the queue's order must match that
    /// call order exactly, documented per scenario at each construction site.
    /// </summary>
    internal sealed class ScriptedRandom : Random
    {
        private readonly Queue<int> _values;

        public ScriptedRandom(params int[] values) => _values = new Queue<int>(values);

        public override int Next(int minValue, int maxValue) =>
            _values.Count > 0
                ? _values.Dequeue()
                : throw new InvalidOperationException(
                    $"ScriptedRandom exhausted — a scenario consumed more rolls than were scripted for it.");
    }
}
