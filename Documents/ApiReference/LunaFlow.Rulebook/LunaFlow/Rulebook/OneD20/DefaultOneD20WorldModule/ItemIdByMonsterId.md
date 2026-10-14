#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[DefaultOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')

## DefaultOneD20WorldModule\.ItemIdByMonsterId Property

The static, deterministic monster→item drop table \(1:1\) — same monster always drops the
same item, per loot\-rom\-content\-expansion\-spec\.md\. Translated into Loot's
`MonsterLootTable` by `Program.cs`\.

```csharp
public System.Collections.Generic.IReadOnlyDictionary<string,string> ItemIdByMonsterId { get; }
```

Implements [ItemIdByMonsterId](../IOneD20WorldModule/ItemIdByMonsterId.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.ItemIdByMonsterId')

#### Property Value
[System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')