### [LunaFlow\.Scripting](../index.md 'LunaFlow\.Scripting').[ISandboxedScriptHost](index.md 'LunaFlow\.Scripting\.ISandboxedScriptHost')

## ISandboxedScriptHost\.Execute\(string\) Method

Runs [luaSource](Execute(string).md#LunaFlow.Scripting.ISandboxedScriptHost.Execute(string).luaSource 'LunaFlow\.Scripting\.ISandboxedScriptHost\.Execute\(string\)\.luaSource') as a Lua chunk and returns its top\-level
            `return` value\. Lua syntax/runtime errors propagate as MoonSharp's own exceptions,
            uncaught — the caller is expected to add source\-identifying context\.

```csharp
MoonSharp.Interpreter.DynValue Execute(string luaSource);
```
#### Parameters

<a name='LunaFlow.Scripting.ISandboxedScriptHost.Execute(string).luaSource'></a>

`luaSource` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

#### Returns
[MoonSharp\.Interpreter\.DynValue](https://learn.microsoft.com/en-us/dotnet/api/moonsharp.interpreter.dynvalue 'MoonSharp\.Interpreter\.DynValue')