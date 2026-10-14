#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[OneD20ChallengeTemplate](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate')

## OneD20ChallengeTemplate\(string, string, string, int, string, string, string, Nullable\<OneD20CheckKind\>, string\) Constructor

Rulebook\-owned data for one pool skill\-check challenge \(9 total this session's content
ships\) — assignable to any of the grid's fixed skill\-check positions\. Keyed by
[Key](Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key') \(e\.g\. `"search"`\), which doubles as the flavor\-text key prefix
\(`{key}-prompt`/`{key}-success`/`{key}-failure`\)\. `Kind`/`SaveType`
mirror D20System's `RoomChallenge` extension \(phase\-2\-10\-content\-coverage\-spec\.md\): a
Skill\-kind entry sets `Skill`/`SkillAbility` and leaves `SaveType` null; a
Save\-kind entry does the reverse\. Trailing optional parameters so existing Skill\-kind pool
entries need no positional\-argument changes\.

```csharp
public OneD20ChallengeTemplate(string Key, string? Skill, string? SkillAbility, int DC, string SuccessKey, string FailureKey, string PromptKey, System.Nullable<LunaFlow.Rulebook.OneD20.OneD20CheckKind> Kind=null, string? SaveType=null);
```
#### Parameters

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).Key'></a>

`Key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).Skill'></a>

`Skill` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).SkillAbility'></a>

`SkillAbility` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).DC'></a>

`DC` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).SuccessKey'></a>

`SuccessKey` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).FailureKey'></a>

`FailureKey` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).PromptKey'></a>

`PromptKey` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).Kind'></a>

`Kind` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[OneD20CheckKind](../OneD20CheckKind/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20CheckKind')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

<a name='LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate.OneD20ChallengeTemplate(string,string,string,int,string,string,string,System.Nullable_LunaFlow.Rulebook.OneD20.OneD20CheckKind_,string).SaveType'></a>

`SaveType` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')