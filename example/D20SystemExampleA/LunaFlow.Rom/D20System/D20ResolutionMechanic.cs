namespace LunaFlow.Rom.D20System
{
    /// <summary>Dice rolling engine for D20 checks and attack resolution.</summary>
    public sealed class D20ResolutionMechanic
    {
        private readonly Random _rng;

        /// <summary>Constructs the mechanic with optional seeded RNG for testing.</summary>
        public D20ResolutionMechanic(Random? rng = null)
        {
            _rng = rng ?? new Random();
        }

        /// <summary>Rolls a die string like "1d20" and returns the total.</summary>
        public int Roll(string die)
        {
            var parts = die.Split('d');
            int count = int.Parse(parts[0]);
            int sides = int.Parse(parts[1]);
            int total = 0;
            for (int i = 0; i < count; i++)
                total += _rng.Next(1, sides + 1);
            return total;
        }

        /// <summary>Resolves a skill check: rolls 1d20 + modifier and compares to DC.</summary>
        public CheckResult ResolveCheck(int modifier, int targetNumber)
        {
            int d20 = Roll("1d20");
            int total = d20 + modifier;
            return new CheckResult(d20, modifier, total, targetNumber, total >= targetNumber);
        }
    }

    /// <summary>Result of a skill check resolution.</summary>
    public sealed record CheckResult(int D20, int Modifier, int Total, int TargetNumber, bool Success);
}
