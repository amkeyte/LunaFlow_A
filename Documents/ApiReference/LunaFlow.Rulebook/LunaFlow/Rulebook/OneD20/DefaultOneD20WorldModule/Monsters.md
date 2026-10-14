#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[DefaultOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')

## DefaultOneD20WorldModule\.Monsters Property

The roster of 5 monsters, keyed by [Id](../OneD20MonsterTemplate/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id')\. Session\-assembly
assigns 3 of these to the grid's 3 fixed fight positions\. Stats per
loot\-rom\-content\-expansion\-spec\.md's roster table, all checked against the PC \(attack \+4,
Defense 11\) to land in a plausible\-either\-way band\.

```csharp
public System.Collections.Generic.IReadOnlyDictionary<string,LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate> Monsters { get; }
```

Implements [Monsters](../IOneD20WorldModule/Monsters.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Monsters')

#### Property Value
[System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[OneD20MonsterTemplate](../OneD20MonsterTemplate/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')