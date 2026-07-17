namespace LunaFlow.Engine.Game
{
    /// <summary>
    /// Represents the overall state of the game engine at a point in time.
    /// Contains global properties and mechanisms for snapshotting or rollback.
    /// </summary>
    internal sealed class GameState
    {
        public int TurnNumber { get; internal set; }
    }
}
