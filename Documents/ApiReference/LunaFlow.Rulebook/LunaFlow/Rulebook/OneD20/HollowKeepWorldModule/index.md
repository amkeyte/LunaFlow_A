#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## HollowKeepWorldModule Class

The second, independently\-authored [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule') proving the World Module
tier is a genuine swap, not a relabel — per ticket 0059/0060 and
`1-d20-worldmodule-hollow-keep-spec.md`\. A reskin, not a rebalance: room count, grid
positions, Fight/SkillCheck slot assignment, challenge skill/ability/DC values, and monster/PC
stat blocks are all byte\-for\-byte identical to [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule') — only
names, descriptions, and narration differ throughout\. Flavor text lives in the
`FlavorText` partial \(`HollowKeepWorldModule.FlavorText.cs`\), same file\-split
convention as [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')\.

```csharp
public sealed class HollowKeepWorldModule : LunaFlow.Rulebook.OneD20.IOneD20WorldModule
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; HollowKeepWorldModule

Implements [IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule')

| Properties | |
| :--- | :--- |
| [Challenges](Challenges.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.Challenges') | The pool of 9 skill\-check challenges\. Skill/SkillAbility/DC values are unchanged             from [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')'s pool — only keys and flavor text are this             module's own\. |
| [FlavorText](FlavorText.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.FlavorText') | Flavor text, keyed per `1-d20-worldmodule-hollow-keep-flavor.md`\. |
| [ItemIdByMonsterId](ItemIdByMonsterId.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.ItemIdByMonsterId') | The static, deterministic monster→item drop table \(1:1\)\. |
| [Monsters](Monsters.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.Monsters') | The roster of 5 monsters\. Stat blocks are unchanged, id\-for\-id, from             [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')'s roster — only names and descriptions differ\. |
| [PlayerAbilityScores](PlayerAbilityScores.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.PlayerAbilityScores') | The PC's six ability scores — unchanged from the Strong\-hero sheet\. |
| [PlayerCharacter](PlayerCharacter.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.PlayerCharacter') | The pregenerated PC's stats — unchanged from the Strong\-hero sheet, reflavored as             a wandering blade\-for\-hire rather than a Department\-7 operative\. |
| [Rooms](Rooms.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.Rooms') | Same 9 fixed grid positions and slot\-kinds as [DefaultOneD20WorldModule](../DefaultOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')             — rooms 1, 5, 8 are Fight; the rest are SkillCheck\. |
| [StartRoomId](StartRoomId.md 'LunaFlow\.Rulebook\.OneD20\.HollowKeepWorldModule\.StartRoomId') | The room a new player starts in — same center\-of\-grid Fight room as the default\. |
