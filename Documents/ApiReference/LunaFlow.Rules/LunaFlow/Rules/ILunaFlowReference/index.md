#### [LunaFlow\.Rules](../../../index.md 'index')
### [LunaFlow\.Rules](../index.md 'LunaFlow\.Rules')

## ILunaFlowReference Interface

Each Rule, Mechanic or Fact has a reference\. \(MyRuleBook\.Chapter\(A\)\.Subsection\(s\)\) or something similar\. These are
for tracking throughout the system as well as

```csharp
public interface ILunaFlowReference
```

Derived  
&#8627; [GeneratedRuleReference](../GeneratorServices/GeneratedRuleReference/index.md 'LunaFlow\.Rules\.GeneratorServices\.GeneratedRuleReference')

| Properties | |
| :--- | :--- |
| [ClassUID](ClassUID.md 'LunaFlow\.Rules\.ILunaFlowReference\.ClassUID') | unique class ID assigned at compile time |
| [FullPath](FullPath.md 'LunaFlow\.Rules\.ILunaFlowReference\.FullPath') | String path including all reference \(including top level data\) information delimited by '\.' |
| [Identifier](Identifier.md 'LunaFlow\.Rules\.ILunaFlowReference\.Identifier') | String identifier for this reference |
| [Parent](Parent.md 'LunaFlow\.Rules\.ILunaFlowReference\.Parent') | The reference to the parent container\. Referencing the parent of the top container is an error\. |
| [Path](Path.md 'LunaFlow\.Rules\.ILunaFlowReference\.Path') | String path including reference information from the Rulebook level down\. |
| [Subreferences](Subreferences.md 'LunaFlow\.Rules\.ILunaFlowReference\.Subreferences') | A list of sub references \(empty if none\) |
