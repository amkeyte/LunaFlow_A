#### [LunaFlow\.Rom](../../../index.md 'index')
### [LunaFlow\.Rom](../index.md 'LunaFlow\.Rom')

## IRom Interface

Real type identity for a ROM module\. A ROM is a reusable, domain\-agnostic game\-system
toolbox \(e\.g\., a D20 resolution system, a Physics ROM, a Dice ROM\) that one or more
Rulebooks can depend on\.

```csharp
public interface IRom : LunaFlow.Rules.IHasLunaFlowReference, LunaFlow.Engine.Components.IHasDependencies
```

Implements [LunaFlow\.Rules\.IHasLunaFlowReference](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.rules.ihaslunaflowreference 'LunaFlow\.Rules\.IHasLunaFlowReference'), [LunaFlow\.Engine\.Components\.IHasDependencies](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.engine.components.ihasdependencies 'LunaFlow\.Engine\.Components\.IHasDependencies')

### Remarks
Prior to Phase 2\.6\.5, a ROM had no identity type — it was just a namespace and a folder\.
`IRom` makes the identity explicit, carries publishing metadata, and participates
in the dependency\-validation system via `IHasDependencies`\.

| Properties | |
| :--- | :--- |
| [Credits](Credits.md 'LunaFlow\.Rom\.IRom\.Credits') | A short description or attribution for the ROM\. |
| [Name](Name.md 'LunaFlow\.Rom\.IRom\.Name') | The ROM's display name\. |
