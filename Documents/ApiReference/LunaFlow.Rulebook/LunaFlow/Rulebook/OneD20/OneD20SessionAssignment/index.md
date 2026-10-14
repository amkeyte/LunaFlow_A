#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20SessionAssignment Class

One session's randomized assignment of pool challenges and roster monsters to the grid's
fixed skill\-check/fight positions, plus the loot each fight room derives from its monster
assignment\. Room \*type\*/position \(which ids are fight vs\. skill\-check\) never varies — only
which specific challenge/monster fills each slot, per session\.

```csharp
public sealed record OneD20SessionAssignment : System.IEquatable<LunaFlow.Rulebook.OneD20.OneD20SessionAssignment>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20SessionAssignment

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[OneD20SessionAssignment](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssignment')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [OneD20SessionAssignment\(IReadOnlyDictionary&lt;int,string&gt;, IReadOnlyDictionary&lt;int,string&gt;, IReadOnlyDictionary&lt;int,string&gt;\)](OneD20SessionAssignment(IReadOnlyDictionary_int,string_,IReadOnlyDictionary_int,string_,IReadOnlyDictionary_int,string_).md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssignment\.OneD20SessionAssignment\(System\.Collections\.Generic\.IReadOnlyDictionary\<int,string\>, System\.Collections\.Generic\.IReadOnlyDictionary\<int,string\>, System\.Collections\.Generic\.IReadOnlyDictionary\<int,string\>\)') | One session's randomized assignment of pool challenges and roster monsters to the grid's fixed skill\-check/fight positions, plus the loot each fight room derives from its monster assignment\. Room \*type\*/position \(which ids are fight vs\. skill\-check\) never varies — only which specific challenge/monster fills each slot, per session\. |

| Properties | |
| :--- | :--- |
| [ChallengeKeyByRoom](ChallengeKeyByRoom.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssignment\.ChallengeKeyByRoom') | |
| [ItemIdByRoom](ItemIdByRoom.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssignment\.ItemIdByRoom') | |
| [MonsterIdByRoom](MonsterIdByRoom.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssignment\.MonsterIdByRoom') | |
