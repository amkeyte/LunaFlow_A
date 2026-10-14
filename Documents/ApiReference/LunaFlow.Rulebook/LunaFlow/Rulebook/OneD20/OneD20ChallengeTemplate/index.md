#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20ChallengeTemplate Class

Rulebook\-owned data for one pool skill\-check challenge \(9 total this session's content
ships\) — assignable to any of the grid's fixed skill\-check positions\. Keyed by
[Key](Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key') \(e\.g\. `"search"`\), which doubles as the flavor\-text key prefix
\(`{key}-prompt`/`{key}-success`/`{key}-failure`\)\. `Kind`/`SaveType`
mirror D20System's `RoomChallenge` extension \(phase\-2\-10\-content\-coverage\-spec\.md\): a
Skill\-kind entry sets `Skill`/`SkillAbility` and leaves `SaveType` null; a
Save\-kind entry does the reverse\. Trailing optional parameters so existing Skill\-kind pool
entries need no positional\-argument changes\.

```csharp
public sealed record OneD20ChallengeTemplate : System.IEquatable<LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20ChallengeTemplate

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[OneD20ChallengeTemplate](index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [OneD20ChallengeTemplate\(string, string, string, int, string, string, string, Nullable&lt;OneD20CheckKind&gt;, string\)](OneD20ChallengeTemplate(string,string,string,int,string,string,string,Nullable_OneD20CheckKind_,string).md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.OneD20ChallengeTemplate\(string, string, string, int, string, string, string, System\.Nullable\<LunaFlow\.Rulebook\.OneD20\.OneD20CheckKind\>, string\)') | Rulebook\-owned data for one pool skill\-check challenge \(9 total this session's content ships\) — assignable to any of the grid's fixed skill\-check positions\. Keyed by [Key](Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key') \(e\.g\. `"search"`\), which doubles as the flavor\-text key prefix \(`{key}-prompt`/`{key}-success`/`{key}-failure`\)\. `Kind`/`SaveType` mirror D20System's `RoomChallenge` extension \(phase\-2\-10\-content\-coverage\-spec\.md\): a Skill\-kind entry sets `Skill`/`SkillAbility` and leaves `SaveType` null; a Save\-kind entry does the reverse\. Trailing optional parameters so existing Skill\-kind pool entries need no positional\-argument changes\. |

| Properties | |
| :--- | :--- |
| [DC](DC.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.DC') | |
| [FailureKey](FailureKey.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.FailureKey') | |
| [Key](Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key') | |
| [Kind](Kind.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Kind') | |
| [PromptKey](PromptKey.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.PromptKey') | |
| [SaveType](SaveType.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.SaveType') | |
| [Skill](Skill.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Skill') | |
| [SkillAbility](SkillAbility.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.SkillAbility') | |
| [SuccessKey](SuccessKey.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.SuccessKey') | |
