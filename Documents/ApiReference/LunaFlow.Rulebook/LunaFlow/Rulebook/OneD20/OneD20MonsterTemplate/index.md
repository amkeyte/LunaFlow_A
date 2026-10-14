#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20MonsterTemplate Class

Rulebook\-owned data for one roster monster \(5 total this session's content ships\) —
assignable to any of the grid's fixed fight positions\. Keyed by [Id](Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id'), which also
keys the monster→item loot table\. `InflictsPoison` mirrors D20System's
`MonsterTemplate` extension — true for exactly one roster monster\.
`EnrageThresholdFraction`/`EnrageDefenseBonus` \(phase\-2\-12\-lua\-worldmodule\-stress\-test\-spec\.md
item 11\) are both nullable, both default null, and either both set or neither — enforced by
[OneD20WorldModuleValidator](../OneD20WorldModuleValidator/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20WorldModuleValidator'), not the type system, since a record can't express a
paired\-nullability constraint\. `EnrageThresholdFraction` is a fraction of MaxHp in
`(0, 1]`; `EnrageDefenseBonus` is a flat bonus\. Purely defensive — see
`MonsterTemplate`'s mirror on the ROM tier for the mechanic itself\.

```csharp
public sealed record OneD20MonsterTemplate : System.IEquatable<LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20MonsterTemplate

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[OneD20MonsterTemplate](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [OneD20MonsterTemplate\(string, string, string, OneD20CombatantStats, bool, Nullable&lt;double&gt;, Nullable&lt;int&gt;\)](OneD20MonsterTemplate(string,string,string,OneD20CombatantStats,bool,Nullable_double_,Nullable_int_).md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.OneD20MonsterTemplate\(string, string, string, LunaFlow\.Rulebook\.OneD20\.OneD20CombatantStats, bool, System\.Nullable\<double\>, System\.Nullable\<int\>\)') | Rulebook\-owned data for one roster monster \(5 total this session's content ships\) — assignable to any of the grid's fixed fight positions\. Keyed by [Id](Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id'), which also keys the monster→item loot table\. `InflictsPoison` mirrors D20System's `MonsterTemplate` extension — true for exactly one roster monster\. `EnrageThresholdFraction`/`EnrageDefenseBonus` \(phase\-2\-12\-lua\-worldmodule\-stress\-test\-spec\.md item 11\) are both nullable, both default null, and either both set or neither — enforced by [OneD20WorldModuleValidator](../OneD20WorldModuleValidator/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20WorldModuleValidator'), not the type system, since a record can't express a paired\-nullability constraint\. `EnrageThresholdFraction` is a fraction of MaxHp in `(0, 1]`; `EnrageDefenseBonus` is a flat bonus\. Purely defensive — see `MonsterTemplate`'s mirror on the ROM tier for the mechanic itself\. |

| Properties | |
| :--- | :--- |
| [DescriptionKey](DescriptionKey.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.DescriptionKey') | |
| [EnrageDefenseBonus](EnrageDefenseBonus.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.EnrageDefenseBonus') | |
| [EnrageThresholdFraction](EnrageThresholdFraction.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.EnrageThresholdFraction') | |
| [Id](Id.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Id') | |
| [InflictsPoison](InflictsPoison.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.InflictsPoison') | |
| [Name](Name.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Name') | |
| [Stats](Stats.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate\.Stats') | |
