#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[LuaOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule')

## LuaOneD20WorldModule\.FromEmbeddedResource\(Assembly, string, ISandboxedScriptHost\) Method

Constructs a [LuaOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule') from a Lua source embedded as a
            resource in [assembly](FromEmbeddedResource(Assembly,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).assembly 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromEmbeddedResource\(System\.Reflection\.Assembly, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.assembly') \(this assembly, by default\) — the path
            `Program.cs` uses for the shipped demonstration script\.
            [resourceName](FromEmbeddedResource(Assembly,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).resourceName 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromEmbeddedResource\(System\.Reflection\.Assembly, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.resourceName') doubles as the constructed instance's `sourceName`\.

```csharp
public static LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule FromEmbeddedResource(System.Reflection.Assembly? assembly=null, string resourceName="LunaFlow.Rulebook.OneD20.Content.DefaultOneD20WorldModule.lua", LunaFlow.Scripting.ISandboxedScriptHost? host=null);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).assembly'></a>

`assembly` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')

<a name='LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).resourceName'></a>

`resourceName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).host'></a>

`host` [LunaFlow\.Scripting\.ISandboxedScriptHost](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.scripting.isandboxedscripthost 'LunaFlow\.Scripting\.ISandboxedScriptHost')

#### Returns
[LuaOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule')