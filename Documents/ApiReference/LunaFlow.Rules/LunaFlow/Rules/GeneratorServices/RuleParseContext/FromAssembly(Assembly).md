#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleParseContext](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext')

## RuleParseContext\.FromAssembly\(Assembly\) Method

Creates a new [RuleParseContext](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext') for a given assembly\.

```csharp
public static LunaFlow.Rules.GeneratorServices.RuleParseContext FromAssembly(System.Reflection.Assembly assembly);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.RuleParseContext.FromAssembly(System.Reflection.Assembly).assembly'></a>

`assembly` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')

The assembly to scan for rule groups\.

#### Returns
[RuleParseContext](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext')  
A populated parsing context\.