#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[OneD20MonsterTemplate](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20MonsterTemplate')

## OneD20MonsterTemplate\(string, string, string, OneD20CombatantStats, bool, Nullable\<double\>, Nullable\<int\>\) Constructor

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
public OneD20MonsterTemplate(string Id, string Name, string DescriptionKey, LunaFlow.Rulebook.OneD20.OneD20CombatantStats Stats, bool InflictsPoison=false, System.Nullable<double> EnrageThresholdFraction=null, System.Nullable<int> EnrageDefenseBonus=null);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).Id'></a>

`Id` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).Name'></a>

`Name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).DescriptionKey'></a>

`DescriptionKey` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).Stats'></a>

`Stats` [OneD20CombatantStats](../OneD20CombatantStats/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20CombatantStats')

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).InflictsPoison'></a>

`InflictsPoison` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).EnrageThresholdFraction'></a>

`EnrageThresholdFraction` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

<a name='LunaFlow.Rulebook.OneD20.OneD20MonsterTemplate.OneD20MonsterTemplate(string,string,string,LunaFlow.Rulebook.OneD20.OneD20CombatantStats,bool,System.Nullable_double_,System.Nullable_int_).EnrageDefenseBonus'></a>

`EnrageDefenseBonus` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')