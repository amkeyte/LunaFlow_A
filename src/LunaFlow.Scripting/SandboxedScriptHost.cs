using MoonSharp.Interpreter;

namespace LunaFlow.Scripting
{
    /// <summary>
    /// <see cref="ISandboxedScriptHost"/> backed by a MoonSharp <see cref="Script"/> configured with
    /// <see cref="CoreModules.Preset_HardSandbox"/> — verified against the MoonSharp 2.0.0 source
    /// (<c>CoreModules.cs</c> at the <c>v2.0.0.0</c> tag) to compose exactly
    /// <c>GlobalConsts | TableIterators | String | Table | Basic | Math | Bit32</c>, excluding
    /// <c>IO</c>/<c>OS_System</c>/<c>OS_Time</c>/<c>LoadMethods</c>/<c>Debug</c> — no filesystem,
    /// process, or OS-time access is reachable from a script run through this host. A fresh
    /// <see cref="Script"/> is constructed per <see cref="Execute"/> call so one script's globals can
    /// never leak into another's.
    /// </summary>
    public sealed class SandboxedScriptHost : ISandboxedScriptHost
    {
        /// <inheritdoc/>
        public DynValue Execute(string luaSource)
        {
            var script = new Script(CoreModules.Preset_HardSandbox);
            return script.DoString(luaSource);
        }
    }
}
