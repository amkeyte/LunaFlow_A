### [LunaFlow\.Scripting](../index.md 'LunaFlow\.Scripting')

## SandboxedScriptHost Class

[ISandboxedScriptHost](../ISandboxedScriptHost/index.md 'LunaFlow\.Scripting\.ISandboxedScriptHost') backed by a MoonSharp [MoonSharp\.Interpreter\.Script](https://learn.microsoft.com/en-us/dotnet/api/moonsharp.interpreter.script 'MoonSharp\.Interpreter\.Script') configured with
            [MoonSharp\.Interpreter\.CoreModules\.Preset\_HardSandbox](https://learn.microsoft.com/en-us/dotnet/api/moonsharp.interpreter.coremodules.preset_hardsandbox 'MoonSharp\.Interpreter\.CoreModules\.Preset\_HardSandbox') — verified against the MoonSharp 2\.0\.0 source
            \(`CoreModules.cs` at the `v2.0.0.0` tag\) to compose exactly
            `GlobalConsts | TableIterators | String | Table | Basic | Math | Bit32`, excluding
            `IO`/`OS_System`/`OS_Time`/`LoadMethods`/`Debug` — no filesystem,
            process, or OS\-time access is reachable from a script run through this host\. A fresh
            [MoonSharp\.Interpreter\.Script](https://learn.microsoft.com/en-us/dotnet/api/moonsharp.interpreter.script 'MoonSharp\.Interpreter\.Script') is constructed per [Execute\(string\)](Execute(string).md 'LunaFlow\.Scripting\.SandboxedScriptHost\.Execute\(string\)') call so one script's globals can
            never leak into another's\.

```csharp
public sealed class SandboxedScriptHost : LunaFlow.Scripting.ISandboxedScriptHost
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; SandboxedScriptHost

Implements [ISandboxedScriptHost](../ISandboxedScriptHost/index.md 'LunaFlow\.Scripting\.ISandboxedScriptHost')

| Methods | |
| :--- | :--- |
| [Execute\(string\)](Execute(string).md 'LunaFlow\.Scripting\.SandboxedScriptHost\.Execute\(string\)') | Runs [luaSource](Execute(string).md#LunaFlow.Scripting.SandboxedScriptHost.Execute(string).luaSource 'LunaFlow\.Scripting\.SandboxedScriptHost\.Execute\(string\)\.luaSource') as a Lua chunk and returns its top\-level             `return` value\. Lua syntax/runtime errors propagate as MoonSharp's own exceptions,             uncaught — the caller is expected to add source\-identifying context\. |
