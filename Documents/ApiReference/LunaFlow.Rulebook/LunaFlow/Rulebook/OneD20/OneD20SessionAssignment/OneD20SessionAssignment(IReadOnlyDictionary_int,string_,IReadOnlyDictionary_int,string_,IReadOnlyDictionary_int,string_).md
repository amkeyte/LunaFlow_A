#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[OneD20SessionAssignment](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssignment')

## OneD20SessionAssignment\(IReadOnlyDictionary\<int,string\>, IReadOnlyDictionary\<int,string\>, IReadOnlyDictionary\<int,string\>\) Constructor

One session's randomized assignment of pool challenges and roster monsters to the grid's
fixed skill\-check/fight positions, plus the loot each fight room derives from its monster
assignment\. Room \*type\*/position \(which ids are fight vs\. skill\-check\) never varies — only
which specific challenge/monster fills each slot, per session\.

```csharp
public OneD20SessionAssignment(System.Collections.Generic.IReadOnlyDictionary<int,string> ChallengeKeyByRoom, System.Collections.Generic.IReadOnlyDictionary<int,string> MonsterIdByRoom, System.Collections.Generic.IReadOnlyDictionary<int,string> ItemIdByRoom);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.OneD20SessionAssignment.OneD20SessionAssignment(System.Collections.Generic.IReadOnlyDictionary_int,string_,System.Collections.Generic.IReadOnlyDictionary_int,string_,System.Collections.Generic.IReadOnlyDictionary_int,string_).ChallengeKeyByRoom'></a>

`ChallengeKeyByRoom` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

<a name='LunaFlow.Rulebook.OneD20.OneD20SessionAssignment.OneD20SessionAssignment(System.Collections.Generic.IReadOnlyDictionary_int,string_,System.Collections.Generic.IReadOnlyDictionary_int,string_,System.Collections.Generic.IReadOnlyDictionary_int,string_).MonsterIdByRoom'></a>

`MonsterIdByRoom` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

<a name='LunaFlow.Rulebook.OneD20.OneD20SessionAssignment.OneD20SessionAssignment(System.Collections.Generic.IReadOnlyDictionary_int,string_,System.Collections.Generic.IReadOnlyDictionary_int,string_,System.Collections.Generic.IReadOnlyDictionary_int,string_).ItemIdByRoom'></a>

`ItemIdByRoom` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')