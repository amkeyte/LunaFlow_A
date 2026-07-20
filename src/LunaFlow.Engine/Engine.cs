using LunaFlow.Engine.Game;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Public entry point for content assemblies to reach the engine. GameEngineCore itself
    /// stays internal, so this gateway is the only way to reach IEngineHost from outside
    /// LunaFlow.Engine.
    /// Named LunaFlowEngine (not Engine) because a type named the same as its containing
    /// namespace's last segment is unusable from sibling namespaces under the same root
    /// (e.g. LunaFlow.Rom.NineRooms) — enclosing-namespace lookup resolves the bare name
    /// "Engine" to the namespace LunaFlow.Engine before it considers a using-imported type
    /// of the same simple name, so "Engine.Host" fails to compile from that kind of caller.
    /// </summary>
    public static class LunaFlowEngine
    {
        /// <summary>The running engine, exposed through its content-facing <see cref="IEngineHost"/> surface.</summary>
        public static IEngineHost Host => GameEngineCore.Instance;
    }
}
