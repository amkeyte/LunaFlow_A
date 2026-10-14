#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms')

## WinMarker Class

Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. Fact marking that a win
line \(a row, column, or diagonal of solved rooms\) is complete\.

```csharp
public sealed record WinMarker : System.IEquatable<LunaFlow.Rom.NineRooms.WinMarker>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; WinMarker

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[WinMarker](index.md 'LunaFlow\.Rom\.NineRooms\.WinMarker')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [WinMarker\(IReadOnlyList&lt;int&gt;\)](WinMarker(IReadOnlyList_int_).md 'LunaFlow\.Rom\.NineRooms\.WinMarker\.WinMarker\(System\.Collections\.Generic\.IReadOnlyList\<int\>\)') | Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. Fact marking that a win line \(a row, column, or diagonal of solved rooms\) is complete\. |

| Properties | |
| :--- | :--- |
| [Line](Line.md 'LunaFlow\.Rom\.NineRooms\.WinMarker\.Line') | The room ids making up the completed line\. |
