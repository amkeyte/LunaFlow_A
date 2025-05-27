namespace LunaFlow.EngineTests.Mocks
{
    /// <summary>
    /// Represents a mutable counter of rule matches across the session.
    /// </summary>
    public class MatchCounter
    {
        public int Count { get; private set; }

        public void Increment() => Count++;
    }
}
