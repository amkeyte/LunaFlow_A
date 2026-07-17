namespace LunaFlow.Engine.Game
{
    /// <summary>
    /// Contains persistent world data and global resources shared across sessions.
    /// Manages locations, entities, and other domain-level constructs.
    /// </summary>
    internal sealed class GameWorld
    {
        private readonly List<Session> _sessions = new();
        public IReadOnlyList<Session> Sessions => _sessions;

        public void AddSession(Session session) => _sessions.Add(session);
        public void RemoveSession(Session session) => _sessions.Remove(session);
    }
}
