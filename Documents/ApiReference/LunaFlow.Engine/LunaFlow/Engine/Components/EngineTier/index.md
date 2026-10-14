#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components')

## EngineTier Enum

Tags a component \(ROM, Rulebook, World Module, GM Overlay, or Player\) with its tier
in the architecture's layered stack\. Used to enforce tier\-ordering rules:
a component may depend on lower tiers but not higher ones\.

```csharp
public enum EngineTier
```
### Fields

<a name='LunaFlow.Engine.Components.EngineTier.Rom'></a>

`Rom` 0

The ROM \(Rule Object Model\) tier: reusable, domain\-agnostic game\-system toolboxes\.

<a name='LunaFlow.Engine.Components.EngineTier.Rulebook'></a>

`Rulebook` 1

The Rulebook tier: a specific game's rules built on one or more ROMs\.

<a name='LunaFlow.Engine.Components.EngineTier.WorldModule'></a>

`WorldModule` 2

The World Module tier: authored\-content regions, encounters, and world structure\.

<a name='LunaFlow.Engine.Components.EngineTier.GmOverlay'></a>

`GmOverlay` 3

The GM Overlay tier: live, session\-scoped rules and world\-state edits\.

<a name='LunaFlow.Engine.Components.EngineTier.Player'></a>

`Player` 4

The Player tier: cosmetic, sandboxed player\-contributed content\.