#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## GeneratedRuleReference Class

The [ILunaFlowReference](../../ILunaFlowReference/index.md 'LunaFlow\.Rules\.ILunaFlowReference') the generator assigns to every generated rule class,
via `public override ILunaFlowReference Reference { get; } = new
            GeneratedRuleReference(groupName, ruleName);` in the emitted source\.

```csharp
public sealed class GeneratedRuleReference : LunaFlow.Rules.ILunaFlowReference
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; GeneratedRuleReference

Implements [ILunaFlowReference](../../ILunaFlowReference/index.md 'LunaFlow\.Rules\.ILunaFlowReference')

### Remarks
Flat and disposable, same shape as Nine Rooms' hand\-written `NineRoomsReference`
\(`LunaFlow.Rom.NineRooms`\) — deliberately not a real parent/child chain\. A genuine
hierarchy \(group\-level reference as `Parent`, rules as its `Subreferences`\)
belongs on `IRuleBook`/`IRuleGroup` once those stop being empty marker
interfaces; building that chain here on the strength of the pipeline's first real
consumer would be the premature\-abstraction move non\-goals\.md warns against elsewhere\.
See the "D3 — generated\-rule Reference strategy" decision in rule\-pipeline\.md\.

`public`, unlike `NineRoomsReference`'s `internal` — that one's only ever
            constructed by rule classes in the same assembly (`LunaFlow.Rom`). Generated rule
            source gets in-memory-compiled (per the D2 compiler decision) into a brand-new assembly
            that merely references `LunaFlow.Rules.dll`, so the type it constructs has to be
            visible across that assembly boundary.

| Constructors | |
| :--- | :--- |
| [GeneratedRuleReference\(string, string\)](GeneratedRuleReference(string,string).md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.GeneratedRuleReference\(string, string\)') | Creates a flat reference identifying a generated rule by its group and rule name\. |

| Properties | |
| :--- | :--- |
| [ClassUID](ClassUID.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.ClassUID') | unique class ID assigned at compile time |
| [FullPath](FullPath.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.FullPath') | String path including all reference \(including top level data\) information delimited by '\.' |
| [Identifier](Identifier.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.Identifier') | String identifier for this reference |
| [Parent](Parent.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.Parent') | The reference to the parent container\. Referencing the parent of the top container is an error\. |
| [Path](Path.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.Path') | String path including reference information from the Rulebook level down\. |
| [Subreferences](Subreferences.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference\.Subreferences') | A list of sub references \(empty if none\) |
