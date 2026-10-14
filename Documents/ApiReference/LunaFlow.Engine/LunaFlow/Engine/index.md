#### [LunaFlow\.Engine](../../index.md 'index')

## LunaFlow\.Engine Namespace

| Classes | |
| :--- | :--- |
| [LunaFlowEngine](LunaFlowEngine/index.md 'LunaFlow\.Engine\.LunaFlowEngine') | Public entry point for content assemblies to reach the engine\. GameEngineCore itself stays internal, so this gateway is the only way to reach IEngineHost from outside LunaFlow\.Engine\. Named LunaFlowEngine \(not Engine\) because a type named the same as its containing namespace's last segment is unusable from sibling namespaces under the same root \(e\.g\. LunaFlow\.Rom\.NineRooms\) — enclosing\-namespace lookup resolves the bare name "Engine" to the namespace LunaFlow\.Engine before it considers a using\-imported type of the same simple name, so "Engine\.Host" fails to compile from that kind of caller\. |
| [RuleEngine](RuleEngine/index.md 'LunaFlow\.Engine\.RuleEngine') | Provides a simplified wrapper around the NRules engine for dynamic rule loading and execution\. |

| Interfaces | |
| :--- | :--- |
| [IEngineHost](IEngineHost/index.md 'LunaFlow\.Engine\.IEngineHost') | The engine surface exposed to content assemblies \(ROM today, Rulebook once that tier exists\)\. Deliberately smaller than GameEngineCore's full surface — content code should never reach GameEngineCore itself, which stays internal\. |
