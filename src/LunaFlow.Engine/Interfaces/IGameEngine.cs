namespace LunaFlow.Engine
{
    /// <summary>
    /// Defines the contract for the core LunaFlow game engine.
    /// Exposes methods for initialization, input handling, and lifecycle control.
    /// </summary>
    public interface IGameEngine
    {
        /// <summary>Prepares the engine for its run loop.</summary>
        public void Initialize();
    }
}
