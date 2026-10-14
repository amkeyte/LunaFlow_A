#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine').[RuleEngine](index.md 'LunaFlow\.Engine\.RuleEngine')

## RuleEngine\.LoadRulesFromAssembly\(Assembly\) Method

Loads rules from the specified [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')\. All types that derive from [NRules\.Fluent\.Dsl\.Rule](https://learn.microsoft.com/en-us/dotnet/api/nrules.fluent.dsl.rule 'NRules\.Fluent\.Dsl\.Rule') are included\.

```csharp
public void LoadRulesFromAssembly(System.Reflection.Assembly assembly);
```
#### Parameters

<a name='LunaFlow.Engine.RuleEngine.LoadRulesFromAssembly(System.Reflection.Assembly).assembly'></a>

`assembly` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')

The assembly containing rule types\.