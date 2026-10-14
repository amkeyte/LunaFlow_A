### [LunaFlow\.Scripting](../index.md 'LunaFlow\.Scripting').[SandboxedScriptHost](index.md 'LunaFlow\.Scripting\.SandboxedScriptHost')

## SandboxedScriptHost\.Execute\(string\) Method

Runs [luaSource](Execute(string).md#LunaFlow.Scripting.SandboxedScriptHost.Execute(string).luaSource 'LunaFlow\.Scripting\.SandboxedScriptHost\.Execute\(string\)\.luaSource') as a Lua chunk and returns its top\-level
            `return` value\. Lua syntax/runtime errors propagate as MoonSharp's own exceptions,
            uncaught — the caller is expected to add source\-identifying context\.

```csharp
public MoonSharp.Interpreter.DynValue Execute(string luaSource);
```
#### Parameters

<a name='LunaFlow.Scripting.SandboxedScriptHost.Execute(string).luaSource'></a>

`luaSource` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Implements [Execute\(string\)](../ISandboxedScriptHost/Execute(string).md 'LunaFlow\.Scripting\.ISandboxedScriptHost\.Execute\(string\)')

#### Returns
[MoonSharp\.Interpreter\.DynValue](https://learn.microsoft.com/en-us/dotnet/api/moonsharp.interpreter.dynvalue 'MoonSharp\.Interpreter\.DynValue')