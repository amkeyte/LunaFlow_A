#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms')

## Room Class

Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. A single room in the Nine
Rooms grid: its position, flavor text, and challenge/answer pair\.

```csharp
public sealed record Room : System.IEquatable<LunaFlow.Rom.NineRooms.Room>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; Room

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[Room](index.md 'LunaFlow\.Rom\.NineRooms\.Room')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [Room\(int, int, int, string, string, string, string\)](Room(int,int,int,string,string,string,string).md 'LunaFlow\.Rom\.NineRooms\.Room\.Room\(int, int, int, string, string, string, string\)') | Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. A single room in the Nine Rooms grid: its position, flavor text, and challenge/answer pair\. |

| Properties | |
| :--- | :--- |
| [Answer](Answer.md 'LunaFlow\.Rom\.NineRooms\.Room\.Answer') | The expected answer text, or null if the room has none\. |
| [ChallengePrompt](ChallengePrompt.md 'LunaFlow\.Rom\.NineRooms\.Room\.ChallengePrompt') | The riddle/question posed to the player, or null if the room has none\. |
| [Col](Col.md 'LunaFlow\.Rom\.NineRooms\.Room\.Col') | Zero\-based grid column \(0\-2\)\. |
| [Description](Description.md 'LunaFlow\.Rom\.NineRooms\.Room\.Description') | The room's flavor text, shown by `look` and on entry\. |
| [Id](Id.md 'LunaFlow\.Rom\.NineRooms\.Room\.Id') | The room's 1\-9 identifier, row\-major\. |
| [Name](Name.md 'LunaFlow\.Rom\.NineRooms\.Room\.Name') | The room's display name\. |
| [Row](Row.md 'LunaFlow\.Rom\.NineRooms\.Room\.Row') | Zero\-based grid row \(0\-2\)\. |
