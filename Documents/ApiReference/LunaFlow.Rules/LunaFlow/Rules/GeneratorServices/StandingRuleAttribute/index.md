#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## StandingRuleAttribute Class

Specifies that a rule or rule group requires the absence of a marker component in the context\.

```csharp
public sealed class StandingRuleAttribute : System.Attribute
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [System\.Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.attribute 'System\.Attribute') &#129106; StandingRuleAttribute

| Constructors | |
| :--- | :--- |
| [StandingRuleAttribute\(Type\)](StandingRuleAttribute(Type).md 'LunaFlow\.Rules\.GeneratorServices\.StandingRuleAttribute\.StandingRuleAttribute\(System\.Type\)') | Initializes a new instance of the [StandingRuleAttribute](index.md 'LunaFlow\.Rules\.GeneratorServices\.StandingRuleAttribute') class\. |

| Properties | |
| :--- | :--- |
| [NotType](NotType.md 'LunaFlow\.Rules\.GeneratorServices\.StandingRuleAttribute\.NotType') | The type of marker to be tested with `!ctx.Has(type)`\. |
