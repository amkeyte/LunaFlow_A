#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20SessionAssembler Class

Session\-start content assembly: chooses 6 of 9 skill\-check challenges and 3 of 5 monsters,
assigns them to the grid's fixed room\-type slots, and derives each fight room's loot from its
monster assignment — same session, same source of truth\. Pure Rulebook\-tier data
transformation \(no `ICommandHandler`, no `LunaFlowEngine.Host`, no fact\-store
access\) — `Program.cs` calls [Assemble\(\)](Assemble().md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssembler\.Assemble\(\)') once and passes the result to ROM
constructors, per the authored\-content boundary in `engine-core.md`\.

```csharp
public sealed class OneD20SessionAssembler
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20SessionAssembler

| Constructors | |
| :--- | :--- |
| [OneD20SessionAssembler\(IOneD20WorldModule, Random\)](OneD20SessionAssembler(IOneD20WorldModule,Random).md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssembler\.OneD20SessionAssembler\(LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule, System\.Random\)') | Constructs the assembler against a World Module's pools, with an injectable RNG —             defaults to a real [System\.Random](https://learn.microsoft.com/en-us/dotnet/api/system.random 'System\.Random'), swappable for a seeded one in tests, same seam as             ticket 0019's `D20ResolutionMechanic`\. |

| Methods | |
| :--- | :--- |
| [Assemble\(\)](Assemble().md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssembler\.Assemble\(\)') | Produces one session's randomized room→challenge/room→monster/room→item assignment\. |
