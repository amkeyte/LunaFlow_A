#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20').[DefaultOneD20WorldModule](index.md 'LunaFlow\.Rulebook\.OneD20\.DefaultOneD20WorldModule')

## DefaultOneD20WorldModule\.Challenges Property

The pool of 9 skill\-check challenges plus, per phase\-2\-10\-content\-coverage\-spec\.md, 2
save\-check challenges \(11 total\), keyed by [Key](../OneD20ChallengeTemplate/Key.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate\.Key')\.
Session\-assembly assigns 6 of these to the grid's 6 fixed skill\-check positions\.

```csharp
public System.Collections.Generic.IReadOnlyDictionary<string,LunaFlow.Rulebook.OneD20.OneD20ChallengeTemplate> Challenges { get; }
```

Implements [Challenges](../IOneD20WorldModule/Challenges.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule\.Challenges')

#### Property Value
[System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[OneD20ChallengeTemplate](../OneD20ChallengeTemplate/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20ChallengeTemplate')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')