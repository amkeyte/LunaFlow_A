using MoonSharp.Interpreter;

namespace LunaFlow.Scripting
{
    /// <summary>
    /// A small, sandboxed Lua execution primitive — not tied to any Rulebook or content shape. Runs a
    /// Lua chunk and returns its top-level return value. See <see cref="SandboxedScriptHost"/> for the
    /// concrete sandboxing configuration. Per <c>lua-world-module-spec.md</c>.
    /// </summary>
    public interface ISandboxedScriptHost
    {
        /// <summary>Runs <paramref name="luaSource"/> as a Lua chunk and returns its top-level
        /// <c>return</c> value. Lua syntax/runtime errors propagate as MoonSharp's own exceptions,
        /// uncaught — the caller is expected to add source-identifying context.</summary>
        DynValue Execute(string luaSource);
    }
}
