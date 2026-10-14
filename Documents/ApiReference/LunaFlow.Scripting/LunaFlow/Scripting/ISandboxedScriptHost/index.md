### [LunaFlow\.Scripting](../index.md 'LunaFlow\.Scripting')

## ISandboxedScriptHost Interface

A small, sandboxed Lua execution primitive — not tied to any Rulebook or content shape\. Runs a
Lua chunk and returns its top\-level return value\. See [SandboxedScriptHost](../SandboxedScriptHost/index.md 'LunaFlow\.Scripting\.SandboxedScriptHost') for the
concrete sandboxing configuration\. Per `lua-world-module-spec.md`\.

```csharp
public interface ISandboxedScriptHost
```

Derived  
&#8627; [SandboxedScriptHost](../SandboxedScriptHost/index.md 'LunaFlow\.Scripting\.SandboxedScriptHost')

| Methods | |
| :--- | :--- |
| [Execute\(string\)](Execute(string).md 'LunaFlow\.Scripting\.ISandboxedScriptHost\.Execute\(string\)') | Runs [luaSource](Execute(string).md#LunaFlow.Scripting.ISandboxedScriptHost.Execute(string).luaSource 'LunaFlow\.Scripting\.ISandboxedScriptHost\.Execute\(string\)\.luaSource') as a Lua chunk and returns its top\-level             `return` value\. Lua syntax/runtime errors propagate as MoonSharp's own exceptions,             uncaught — the caller is expected to add source\-identifying context\. |
