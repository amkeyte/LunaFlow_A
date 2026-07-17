using LunaFlow.Engine.Game;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Public entry point for content assemblies to reach the engine. GameEngineCore itself
    /// stays internal, so this gateway is the only way to reach IEngineHost from outside
    /// LunaFlow.Engine.
    /// </summary>
    public static class Engine
    {
        public static IEngineHost Host => GameEngineCore.Instance;
    }
}
