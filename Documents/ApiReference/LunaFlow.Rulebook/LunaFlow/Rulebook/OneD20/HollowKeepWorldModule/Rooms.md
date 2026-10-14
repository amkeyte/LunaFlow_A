#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[HollowKeepWorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule')

## HollowKeepWorldModule\.Rooms Property

Same 9 fixed grid positions and slot\-kinds as [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')
            — rooms 1, 5, 8 are Fight; the rest are SkillCheck\.

```csharp
public System.Collections.Generic.IReadOnlyDictionary<int,LunaFlow.Rulebook.OneD20.OneD20RoomData> Rooms { get; }
```

Implements [Rooms](../IOneD20WorldModule/Rooms.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Rooms')

#### Property Value
[System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[OneD20RoomData](../OneD20RoomData/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')