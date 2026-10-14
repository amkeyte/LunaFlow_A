#### [LunaFlow\.Engine](../../../index.md 'index')

## LunaFlow\.Engine\.Components Namespace

| Classes | |
| :--- | :--- |
| [Capability](Capability/index.md 'LunaFlow\.Engine\.Components\.Capability') | A named capability that a component can provide or require\. The capability acts as a mediation point for peer components to depend on each other without direct assembly references\. |
| [ComponentRegistry](ComponentRegistry/index.md 'LunaFlow\.Engine\.Components\.ComponentRegistry') | Registry and validator for ROM/Rulebook components\. Enforces that all declared capabilities are unique and that all requirements can be satisfied\. |
| [ContractSurfaceValidator](ContractSurfaceValidator/index.md 'LunaFlow\.Engine\.Components\.ContractSurfaceValidator') | Bootstrap\-time shape check for a Cartridge's contract\-surface assembly, per `cross-rom-capability-spec.md` §5\.4: `cartridge.md`'s Structure section states the contract surface holds interfaces only — "the pinout, not the chip" — but nothing enforced it until a bare concrete record \(`EquippedWeapon`\) shipped directly in `LunaFlow.Cartridge` and was caught only in review \(ticket 0071\)\. This turns that manual bar into an automated, bootstrap\-enforced one, same "hard fail before play starts, never a silent degradation" discipline the existing compatibility probe \(§5\.1–§5\.3\) already holds, over a different question: not whether a declared capability's pairing binds, but whether everything exported from the contract surface is actually an interface\. |

| Interfaces | |
| :--- | :--- |
| [IHasDependencies](IHasDependencies/index.md 'LunaFlow\.Engine\.Components\.IHasDependencies') | Contract for components \(ROMs, Ruleooks, World Modules, etc\.\) that declare what capabilities they provide and require\. |

| Enums | |
| :--- | :--- |
| [EngineTier](EngineTier/index.md 'LunaFlow\.Engine\.Components\.EngineTier') | Tags a component \(ROM, Rulebook, World Module, GM Overlay, or Player\) with its tier in the architecture's layered stack\. Used to enforce tier\-ordering rules: a component may depend on lower tiers but not higher ones\. |
