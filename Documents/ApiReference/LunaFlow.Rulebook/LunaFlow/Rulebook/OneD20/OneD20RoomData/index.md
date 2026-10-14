#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20RoomData Class

Rulebook\-owned data for one room's fixed grid position and which slot\-kind it is — the
topology \(position, fight\-vs\-skill\-check\) never varies session to session\. The specific
challenge/monster filling the slot is session\-assigned data instead \(see
`OneD20SessionAssembler`\), no longer carried on this record\.

```csharp
public sealed record OneD20RoomData : System.IEquatable<LunaFlow.Rulebook.OneD20.OneD20RoomData>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20RoomData

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[OneD20RoomData](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [OneD20RoomData\(int, int, int, OneD20ChallengeType, string\)](OneD20RoomData(int,int,int,OneD20ChallengeType,string).md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.OneD20RoomData\(int, int, int, LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeType, string\)') | Rulebook\-owned data for one room's fixed grid position and which slot\-kind it is — the topology \(position, fight\-vs\-skill\-check\) never varies session to session\. The specific challenge/monster filling the slot is session\-assigned data instead \(see `OneD20SessionAssembler`\), no longer carried on this record\. |

| Properties | |
| :--- | :--- |
| [ChallengeType](ChallengeType.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.ChallengeType') | |
| [Col](Col.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.Col') | |
| [DescriptionKey](DescriptionKey.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.DescriptionKey') | |
| [Id](Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.Id') | |
| [Row](Row.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.Row') | |
