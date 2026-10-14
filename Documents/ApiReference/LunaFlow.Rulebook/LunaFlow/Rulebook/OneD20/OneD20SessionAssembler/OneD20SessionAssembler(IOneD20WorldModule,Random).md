#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[OneD20SessionAssembler](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssembler')

## OneD20SessionAssembler\(IOneD20WorldModule, Random\) Constructor

Constructs the assembler against a World Module's pools, with an injectable RNG —
            defaults to a real [System\.Random](https://learn.microsoft.com/en-us/dotnet/api/system.random 'System\.Random'), swappable for a seeded one in tests, same seam as
            ticket 0019's `D20ResolutionMechanic`\.

```csharp
public OneD20SessionAssembler(LunaFlow.Rulebook.OneD20.IOneD20WorldModule worldModule, System.Random? random=null);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.OneD20SessionAssembler.OneD20SessionAssembler(LunaFlow.Rulebook.OneD20.IOneD20WorldModule,System.Random).worldModule'></a>

`worldModule` [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule')

<a name='LunaFlow.Rulebook.OneD20.OneD20SessionAssembler.OneD20SessionAssembler(LunaFlow.Rulebook.OneD20.IOneD20WorldModule,System.Random).random'></a>

`random` [System\.Random](https://learn.microsoft.com/en-us/dotnet/api/system.random 'System\.Random')