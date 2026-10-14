#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine')

## LunaFlowEngine Class

Public entry point for content assemblies to reach the engine\. GameEngineCore itself
stays internal, so this gateway is the only way to reach IEngineHost from outside
LunaFlow\.Engine\.
Named LunaFlowEngine \(not Engine\) because a type named the same as its containing
namespace's last segment is unusable from sibling namespaces under the same root
\(e\.g\. LunaFlow\.Rom\.NineRooms\) — enclosing\-namespace lookup resolves the bare name
"Engine" to the namespace LunaFlow\.Engine before it considers a using\-imported type
of the same simple name, so "Engine\.Host" fails to compile from that kind of caller\.

```csharp
public static class LunaFlowEngine
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; LunaFlowEngine

| Properties | |
| :--- | :--- |
| [Host](Host.md 'LunaFlow\.Engine\.LunaFlowEngine\.Host') | The running engine, exposed through its content\-facing [IEngineHost](../IEngineHost/index.md 'LunaFlow\.Engine\.IEngineHost') surface\. |
