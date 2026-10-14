#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## DefaultOneD20WorldModule Class

The 1\.d20 Department\-7 mission's shipped content, reimplemented against
[IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule') instead of as static fields — the 9 rooms' fixed grid
positions, the pool of 9 skill\-check challenges and roster of 5 monsters session content draws
from, the monster→item loot table, the pregenerated PC's sheet, and ability scores\. A
relocation of `OneD20RulebookContent`'s values, not a rewrite\. Which specific
challenge/monster fills a given room is session\-assigned by [OneD20SessionAssembler](../OneD20SessionAssembler/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssembler'),
not fixed here\. Translated into GridMap's/D20System's/Loot's own fact shapes by
`Program.cs` \(the composition root, exempt from the Rulebook/ROM boundary\)\. Pure data; no
logic, no fact\-store access\. Flavor text lives in the `FlavorText` partial
\(`DefaultOneD20WorldModule.FlavorText.cs`\), mirroring today's
`OneD20RulebookContent`/`OneD20FlavorText` file split\.

```csharp
public sealed class DefaultOneD20WorldModule : LunaFlow.Rulebook.OneD20.IOneD20WorldModule
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DefaultOneD20WorldModule

Implements [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule')

| Properties | |
| :--- | :--- |
| [Challenges](Challenges.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.Challenges') | The pool of 9 skill\-check challenges plus, per phase\-2\-10\-content\-coverage\-spec\.md, 2 save\-check challenges \(11 total\), keyed by [Key](../OneD20ChallengeTemplate/Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key')\. Session\-assembly assigns 6 of these to the grid's 6 fixed skill\-check positions\. |
| [FlavorText](FlavorText.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.FlavorText') | Flavor text, keyed per `loot-rom-content-expansion-flavor.md`\. |
| [ItemIdByMonsterId](ItemIdByMonsterId.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.ItemIdByMonsterId') | The static, deterministic monster→item drop table \(1:1\) — same monster always drops the same item, per loot\-rom\-content\-expansion\-spec\.md\. Translated into Loot's `MonsterLootTable` by `Program.cs`\. |
| [Monsters](Monsters.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.Monsters') | The roster of 5 monsters, keyed by [Id](../OneD20MonsterTemplate/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id')\. Session\-assembly assigns 3 of these to the grid's 3 fixed fight positions\. Stats per loot\-rom\-content\-expansion\-spec\.md's roster table, all checked against the PC \(attack \+4, Defense 11\) to land in a plausible\-either\-way band\. |
| [PlayerAbilityScores](PlayerAbilityScores.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.PlayerAbilityScores') | The PC's six ability scores, per the Strong\-hero sheet\. |
| [PlayerCharacter](PlayerCharacter.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.PlayerCharacter') | The pregenerated level\-1 Strong\-hero PC's stats\. |
| [Rooms](Rooms.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.Rooms') | The nine rooms' fixed grid position and slot\-kind, keyed by [Id](../OneD20RoomData/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.Id')\.             Rooms 1, 5, 8 are Fight; the rest are SkillCheck — this topology never varies per session\. |
| [StartRoomId](StartRoomId.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule\.StartRoomId') | The room a new player starts in \(the center of the grid — also a Fight room\)\. |
