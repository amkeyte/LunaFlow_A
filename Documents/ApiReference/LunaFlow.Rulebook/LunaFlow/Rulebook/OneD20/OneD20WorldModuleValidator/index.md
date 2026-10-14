#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20WorldModuleValidator Class

Load\-time validation for an [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule'), per
`world-module-extraction-spec.md`'s Mechanics section\. `Program.cs` calls
[Validate\(IOneD20WorldModule\)](Validate(IOneD20WorldModule).md 'LunaFlow\.Rulebook\.OneD20\.OneD20WorldModuleValidator\.Validate\(LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\)') once, immediately after constructing a World Module and before anything
else touches it\. Every failure is a hard [System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException'), fail\-fast on
the first violated check — same "hard fail at bootstrap, never a degradation during play"
property `ComponentRegistry` holds one tier over\.

```csharp
public static class OneD20WorldModuleValidator
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20WorldModuleValidator

| Methods | |
| :--- | :--- |
| [Validate\(IOneD20WorldModule\)](Validate(IOneD20WorldModule).md 'LunaFlow\.Rulebook\.OneD20\.OneD20WorldModuleValidator\.Validate\(LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\)') | Validates [module](Validate(IOneD20WorldModule).md#LunaFlow.Rulebook.OneD20.OneD20WorldModuleValidator.Validate(LunaFlow.Rulebook.OneD20.IOneD20WorldModule).module 'LunaFlow\.Rulebook\.OneD20\.OneD20WorldModuleValidator\.Validate\(LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\)\.module') against every check in the spec's Mechanics             section, throwing on the first violation with a message naming what's wrong\. |
