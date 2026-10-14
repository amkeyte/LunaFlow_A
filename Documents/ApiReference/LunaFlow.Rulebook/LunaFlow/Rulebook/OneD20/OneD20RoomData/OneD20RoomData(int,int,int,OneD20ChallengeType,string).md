#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[OneD20RoomData](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData')

## OneD20RoomData\(int, int, int, OneD20ChallengeType, string\) Constructor

Rulebook\-owned data for one room's fixed grid position and which slot\-kind it is — the
topology \(position, fight\-vs\-skill\-check\) never varies session to session\. The specific
challenge/monster filling the slot is session\-assigned data instead \(see
`OneD20SessionAssembler`\), no longer carried on this record\.

```csharp
public OneD20RoomData(int Id, int Row, int Col, LunaFlow.Rulebook.OneD20.OneD20ChallengeType ChallengeType, string DescriptionKey);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.OneD20RoomData.OneD20RoomData(int,int,int,LunaFlow.Rulebook.OneD20.OneD20ChallengeType,string).Id'></a>

`Id` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='LunaFlow.Rulebook.OneD20.OneD20RoomData.OneD20RoomData(int,int,int,LunaFlow.Rulebook.OneD20.OneD20ChallengeType,string).Row'></a>

`Row` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='LunaFlow.Rulebook.OneD20.OneD20RoomData.OneD20RoomData(int,int,int,LunaFlow.Rulebook.OneD20.OneD20ChallengeType,string).Col'></a>

`Col` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='LunaFlow.Rulebook.OneD20.OneD20RoomData.OneD20RoomData(int,int,int,LunaFlow.Rulebook.OneD20.OneD20ChallengeType,string).ChallengeType'></a>

`ChallengeType` [OneD20ChallengeType](../OneD20ChallengeType/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeType')

<a name='LunaFlow.Rulebook.OneD20.OneD20RoomData.OneD20RoomData(int,int,int,LunaFlow.Rulebook.OneD20.OneD20ChallengeType,string).DescriptionKey'></a>

`DescriptionKey` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')