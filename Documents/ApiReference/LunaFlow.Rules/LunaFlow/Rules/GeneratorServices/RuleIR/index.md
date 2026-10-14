#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RuleIR Class

Represents a single parsed rule in the intermediate stage\.
Contains all data required for code generation\.

```csharp
public sealed class RuleIR
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RuleIR

| Properties | |
| :--- | :--- |
| [ActionCode](ActionCode.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.ActionCode') | Gets or sets the string representation of the action logic\. This is emitted directly in the generated class\. |
| [ConditionCode](ConditionCode.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.ConditionCode') | Gets or sets the string representation of the condition logic\. This is emitted directly in the generated class\. |
| [Description](Description.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.Description') | Gets or sets a short description of the rule\. |
| [Name](Name.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.Name') | Gets or sets the unique name of the rule\. |
| [PreambleCode](PreambleCode.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.PreambleCode') | Gets or sets the C\# statements to emit before the When\(\)/Then\(\) blocks \(typically variable declarations for Match\<T\>\(\(\) =\> x, \.\.\.\)\)\. Joined from `RulePrototype.PreambleStatements` by the parser, one statement per line, verbatim — kept as an opaque string for the same reason as ConditionCode/ActionCode below \(see rule\-pipeline\.md's IR section\)\. |
| [RequiredUsings](RequiredUsings.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.RequiredUsings') | Gets or sets the namespaces the generated file needs\. Copied from `RulePrototype.RequiredUsings` by the parser; the generator unions/dedups these across every [RuleIR](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR') in a group when it writes one file's using block\. |
| [StandingRules](StandingRules.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.StandingRules') | Gets or sets the list of type\-level standing rules \(e\.g\. markers that must not be present\)\. |
