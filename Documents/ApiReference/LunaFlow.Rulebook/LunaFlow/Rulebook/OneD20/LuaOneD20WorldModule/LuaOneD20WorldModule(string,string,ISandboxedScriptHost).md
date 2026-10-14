#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[LuaOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule')

## LuaOneD20WorldModule\(string, string, ISandboxedScriptHost\) Constructor

Executes [luaSource](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).luaSource 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.luaSource') through [host](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).host 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.host') \(a fresh
[LunaFlow\.Scripting\.SandboxedScriptHost](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.scripting.sandboxedscripthost 'LunaFlow\.Scripting\.SandboxedScriptHost') if omitted\) and marshals its returned table into this
module's properties, synchronously, before returning\. [sourceName](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).sourceName 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.sourceName') — a file
path, embedded\-resource name, or any other human\-readable label for where this script came
from — is threaded into every exception this constructor throws, so a failure names \*which\*
script broke, not just what broke; it has no effect on marshaling itself\.

```csharp
public LuaOneD20WorldModule(string luaSource, string sourceName="<inline script>", LunaFlow.Scripting.ISandboxedScriptHost? host=null);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).luaSource'></a>

`luaSource` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).sourceName'></a>

`sourceName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).host'></a>

`host` [LunaFlow\.Scripting\.ISandboxedScriptHost](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.scripting.isandboxedscripthost 'LunaFlow\.Scripting\.ISandboxedScriptHost')