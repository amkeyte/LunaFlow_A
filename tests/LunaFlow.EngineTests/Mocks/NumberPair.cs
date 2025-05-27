namespace LunaFlow.EngineTests.Mocks
{
    /// <summary>
    /// Represents a simple pair of integers used for rule evaluation testing.
    /// </summary>
    public class NumberPair
    {
        public int A { get; }
        public int B { get; }

        public NumberPair(int a, int b)
        {
            A = a;
            B = b;
        }
    }
}
