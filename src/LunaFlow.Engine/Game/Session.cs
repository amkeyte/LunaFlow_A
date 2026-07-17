namespace LunaFlow.Engine.Game
{
    /// <summary>
    /// Represents a single player's active session in the game world.
    /// Tracks user input history, local state, and session-specific context.
    /// </summary>
    internal sealed class Session
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string TranslatorName { get; }
        public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;

        public Session(string translatorName) => TranslatorName = translatorName;
    }
}
