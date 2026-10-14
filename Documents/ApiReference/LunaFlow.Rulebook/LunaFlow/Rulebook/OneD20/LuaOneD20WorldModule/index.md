#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## LuaOneD20WorldModule Class

A second, independently\-authored [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule') — populated by executing a Lua
chunk \(via [LunaFlow\.Scripting\.ISandboxedScriptHost](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.scripting.isandboxedscripthost 'LunaFlow\.Scripting\.ISandboxedScriptHost')\) and marshaling its single top\-level
`return {...}` table into the exact same shape [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')/
[HollowKeepWorldModule](../HollowKeepWorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule') populate in C\#\. Executes once, at construction \(no hot\-reload,
same lifecycle as the C\# implementations\) — every property is a materialized, already\-built
dictionary by the time the constructor returns; nothing re\-executes the Lua chunk on later
property access\. Two\-layer fail\-fast per `lua-world-module-spec.md`: this class's own
marshal\-shape checks run first, throwing [System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException') naming the
script \(`sourceName`\) and the specific field at fault; [OneD20WorldModuleValidator](../OneD20WorldModuleValidator/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20WorldModuleValidator')
\(unchanged\) then runs its own content\-validity checks on top, exactly as it does for any other
[IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule')\.

```csharp
public sealed class LuaOneD20WorldModule : LunaFlow.Rulebook.OneD20.IOneD20WorldModule
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; LuaOneD20WorldModule

Implements [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule')

| Constructors | |
| :--- | :--- |
| [LuaOneD20WorldModule\(string, string, ISandboxedScriptHost\)](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)') | Executes [luaSource](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).luaSource 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.luaSource') through [host](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).host 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.host') \(a fresh [LunaFlow\.Scripting\.SandboxedScriptHost](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.scripting.sandboxedscripthost 'LunaFlow\.Scripting\.SandboxedScriptHost') if omitted\) and marshals its returned table into this module's properties, synchronously, before returning\. [sourceName](LuaOneD20WorldModule(string,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.LuaOneD20WorldModule(string,string,LunaFlow.Scripting.ISandboxedScriptHost).sourceName 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.LuaOneD20WorldModule\(string, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.sourceName') — a file path, embedded\-resource name, or any other human\-readable label for where this script came from — is threaded into every exception this constructor throws, so a failure names \*which\* script broke, not just what broke; it has no effect on marshaling itself\. |

| Properties | |
| :--- | :--- |
| [Challenges](Challenges.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.Challenges') | The pool of skill\-check challenges session\-assembly draws from, keyed by [Key](../OneD20ChallengeTemplate/Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key')\. |
| [FlavorText](FlavorText.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FlavorText') | All flavor prose, keyed by the flavor keys [Rooms](../IOneD20WorldModule/Rooms.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Rooms')/[Challenges](../IOneD20WorldModule/Challenges.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Challenges')/[Monsters](../IOneD20WorldModule/Monsters.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Monsters') reference\. |
| [ItemIdByMonsterId](ItemIdByMonsterId.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.ItemIdByMonsterId') | The static, deterministic monster→item drop table \(1:1\)\. |
| [Monsters](Monsters.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.Monsters') | The monster roster session\-assembly draws from, keyed by [Id](../OneD20MonsterTemplate/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id')\. |
| [PlayerAbilityScores](PlayerAbilityScores.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.PlayerAbilityScores') | The PC's six ability scores\. |
| [PlayerCharacter](PlayerCharacter.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.PlayerCharacter') | The pregenerated PC's combat stats\. |
| [Rooms](Rooms.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.Rooms') | The nine rooms' fixed grid position and slot\-kind, keyed by [Id](../OneD20RoomData/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.Id')\. |
| [StartRoomId](StartRoomId.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.StartRoomId') | The room a new player starts in\. |

| Methods | |
| :--- | :--- |
| [FromEmbeddedResource\(Assembly, string, ISandboxedScriptHost\)](FromEmbeddedResource(Assembly,string,ISandboxedScriptHost).md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromEmbeddedResource\(System\.Reflection\.Assembly, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)') | Constructs a [LuaOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule') from a Lua source embedded as a             resource in [assembly](FromEmbeddedResource(Assembly,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).assembly 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromEmbeddedResource\(System\.Reflection\.Assembly, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.assembly') \(this assembly, by default\) — the path             `Program.cs` uses for the shipped demonstration script\.             [resourceName](FromEmbeddedResource(Assembly,string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromEmbeddedResource(System.Reflection.Assembly,string,LunaFlow.Scripting.ISandboxedScriptHost).resourceName 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromEmbeddedResource\(System\.Reflection\.Assembly, string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.resourceName') doubles as the constructed instance's `sourceName`\. |
| [FromFile\(string, ISandboxedScriptHost\)](FromFile(string,ISandboxedScriptHost).md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromFile\(string, LunaFlow\.Scripting\.ISandboxedScriptHost\)') | Constructs a [LuaOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule') by reading Lua source from a file on             disk\. [path](FromFile(string,ISandboxedScriptHost).md#LunaFlow.Rulebook.OneD20.LuaOneD20WorldModule.FromFile(string,LunaFlow.Scripting.ISandboxedScriptHost).path 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule\.FromFile\(string, LunaFlow\.Scripting\.ISandboxedScriptHost\)\.path') doubles as the constructed instance's `sourceName`\. |
