#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RuleParseContext Class

Provides context and input information to the rule parsing stage of the pipeline\.
Used to identify which assemblies or types should be inspected for rule definitions\.

```csharp
public sealed class RuleParseContext
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RuleParseContext

| Properties | |
| :--- | :--- |
| [RuleGroupTypes](RuleGroupTypes.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\.RuleGroupTypes') | Gets the list of rule group types that will be parsed\. |

| Methods | |
| :--- | :--- |
| [AddRuleGroup\(Type\)](AddRuleGroup(Type).md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\.AddRuleGroup\(System\.Type\)') | Adds a single rule group type to the context\. |
| [FromAssembly\(Assembly\)](FromAssembly(Assembly).md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\.FromAssembly\(System\.Reflection\.Assembly\)') | Creates a new [RuleParseContext](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext') for a given assembly\. |
