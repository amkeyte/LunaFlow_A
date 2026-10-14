#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## IOneD20WorldModule Interface

The shape contract a World Module implements to supply 1\.d20 its concrete content instances —
the 9 rooms' fixed grid positions, the pool of skill\-check challenges and monster roster
session content draws from, the monster→item loot table, the pregenerated PC's sheet, ability
scores, and flavor text\. The Rulebook owns this contract and the record shapes it returns
\([OneD20RoomData](../OneD20RoomData/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData')/[OneD20ChallengeTemplate](../OneD20ChallengeTemplate/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate')/
[OneD20MonsterTemplate](../OneD20MonsterTemplate/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate')/[OneD20CombatantStats](../OneD20CombatantStats/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20CombatantStats')\); a World Module
populates them, never redefines them\. See `design/world-module.md`\.

```csharp
public interface IOneD20WorldModule
```

Derived  
&#8627; [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')  
&#8627; [HollowKeepWorldModule](../HollowKeepWorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule')  
&#8627; [LuaOneD20WorldModule](../LuaOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.LuaOneD20WorldModule')

| Properties | |
| :--- | :--- |
| [Challenges](Challenges.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Challenges') | The pool of skill\-check challenges session\-assembly draws from, keyed by [Key](../OneD20ChallengeTemplate/Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key')\. |
| [FlavorText](FlavorText.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.FlavorText') | All flavor prose, keyed by the flavor keys [Rooms](Rooms.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Rooms')/[Challenges](Challenges.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Challenges')/[Monsters](Monsters.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Monsters') reference\. |
| [ItemIdByMonsterId](ItemIdByMonsterId.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.ItemIdByMonsterId') | The static, deterministic monster→item drop table \(1:1\)\. |
| [Monsters](Monsters.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Monsters') | The monster roster session\-assembly draws from, keyed by [Id](../OneD20MonsterTemplate/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id')\. |
| [PlayerAbilityScores](PlayerAbilityScores.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.PlayerAbilityScores') | The PC's six ability scores\. |
| [PlayerCharacter](PlayerCharacter.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.PlayerCharacter') | The pregenerated PC's combat stats\. |
| [Rooms](Rooms.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Rooms') | The nine rooms' fixed grid position and slot\-kind, keyed by [Id](../OneD20RoomData/Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RoomData\.Id')\. |
| [StartRoomId](StartRoomId.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.StartRoomId') | The room a new player starts in\. |
