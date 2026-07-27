# Phase 2.7 Implementation Plan: GridMap/D20System Split & IInteropRom Broker

## Context

Phase 2.6.5 shipped ROM identity (`IRom`/`IInteropRom`) and capability-mediation infrastructure (`Capability`, `IHasDependencies`, `ComponentRegistry`), but left `IInteropRom` as an empty marker interface. Phase 2.7 implements the actual broker's `Resolve`/`Resolve<T>` methods, splits the monolithic D20System ROM into two peer ROMs (GridMap and D20System), and wires them through a third broker ROM (`D20SystemInterop`) that holds the shared contract interfaces.

Current state: One `LunaFlow.Rom` project in `src/` contains D20System and NineRooms as namespaced folders. The spec and ticket 0036 require three separate `.csproj` files to enforce the "zero project reference between peers" constraint, verified against the assembly graph.

**Key change from phase plan:** ROM content code belongs in `example/` as a second container for source code, not `src/`. Three example projects are already scaffolded with the correct namespace structure:
- `example/D20SystemExampleA/LunaFlow.Rom/D20System/` — D20System ROM (trimmed)
- `example/D20SystemExampleB/LunaFlow.Rom/GridMap/` — GridMap ROM (spatial)
- `example/D20SystemExampleC/LunaFlow.Rom/D20SystemInterop/` — D20SystemInterop broker ROM

Goal: Fill in these three example projects with actual code such that GridMap and D20System communicate only through contracts in D20SystemInterop, with all 1-d20 player-facing behavior unchanged.

---

## Implementation Approach

### Phase A: Fill In Three Example ROM Projects

**Existing projects to populate** (already scaffolded with namespace folders in `example/`):

1. **`example/D20SystemExampleB/LunaFlow.Rom.GridMap/`** (GridMap ROM)
   - `.csproj` setup: Add project references to `LunaFlow.Rom`, `LunaFlow.Rules`, `LunaFlow.Engine`, `LunaFlow.Rom.D20SystemInterop`
   - Namespace: `LunaFlow.Rom.GridMap`
   - Facts: `PlayerPosition(int RoomId) : IPlayerLocation`, `PreviousRoomId(int RoomId)`, `WinMarker`
   - Data: `GridRoomData` record (Id, Row, Col, DescriptionKey) + StartRoomId (constructor-injected)
   - Handlers: `MoveCommandHandler` (grid movement), `LookCommandHandler` (queries `ICombatStatus` from shared session)
   - Adapter: `GridMovementService : IPlayerMovement` (implements `ReturnToPreviousRoom()` by retracting/inserting player facts)
   - Rules: `GridMapRuleGroup` with 8 win-line rules + `GameEndRule` (all matching `IChallengeSolved` from D20System)
   - IRom: `GridMapRom : IRom` with `Provides: ["PlayerLocation", "PlayerMovement"]`, `Requires: ["ChallengeSolved"]`

2. **`example/D20SystemExampleA/LunaFlow.Rom.D20System/`** (D20System ROM — trimmed)
   - `.csproj` setup: Add project references to `LunaFlow.Rom`, `LunaFlow.Rules`, `LunaFlow.Engine`, `LunaFlow.Rom.D20SystemInterop`
   - Namespace: `LunaFlow.Rom.D20System`
   - Facts: `RoomChallenge` (challenge/combat data), `CombatRound : ICombatStatus` (active combat), `RoomFailed` (lost room)
   - (Removed: `PlayerPosition`, `PreviousRoomId`, `WinMarker` → moved to GridMap)
   - Data: Constructor-inject PC stats, Monster stats, challenge definitions, ability scores
   - Handlers: `CheckCommandHandler` (skill check via `IPlayerLocation`), `FightCommandHandler` (combat via `IPlayerLocation`), `FleeCommandHandler` (uses `IPlayerMovement.ReturnToPreviousRoom()`)
   - Rules: `D20SystemRuleGroup` with 9 challenge-prompt rules (all matching `IPlayerLocation` from GridMap)
   - IRom: `D20SystemRom : IRom` with `Provides: ["ChallengeSolved", "CombatStatus"]`, `Requires: ["PlayerLocation", "PlayerMovement"]`
   - Deprecation note: Keep `D20SystemContent` in `src/LunaFlow.Rom/` for Nine Rooms regression tests; mark methods `[Obsolete]` but don't delete

3. **`example/D20SystemExampleC/LunaFlow.Rom.D20SystemInterop/`** (D20SystemInterop broker ROM)
   - `.csproj` setup: Add project references to `LunaFlow.Rules`, `LunaFlow.Engine` (NO GridMap, NO D20System)
   - Namespace: `LunaFlow.Rom.D20SystemInterop`
   - Contract interfaces (fact-shaped, matched in NRules):
     - `IPlayerLocation { int RoomId { get; } }`
     - `IChallengeSolved { int RoomId { get; } }`
     - `ICombatStatus { int RoomId { get; } int MonsterHp { get; } int PlayerHp { get; } }`
   - Contract interface (service-shaped, resolved from broker):
     - `IPlayerMovement { bool ReturnToPreviousRoom(); }`
   - IRom: `D20SystemInteropRom : IInteropRom` with:
     - Empty `Provides`/`Requires` (broker is infrastructure, not content)
     - `Resolve(Capability cap) → object` — lookup provider by capability name; throw if missing
     - `Resolve<T>(string capabilityName) where T : class → T` — typed convenience wrapper

### Phase B: Implement IInteropRom.Resolve Methods

**File: `src/LunaFlow.Rom/IInteropRom.cs`** (extend existing empty interface)

Add two methods to `IInteropRom`:

```csharp
/// <summary>Resolves a declared capability to the instance providing it.</summary>
/// <exception cref="InvalidOperationException">No registered provider for that capability.</exception>
object Resolve(Capability capability);

/// <summary>Typed convenience over <see cref="Resolve"/>.</summary>
T Resolve<T>(string capabilityName) where T : class;
```

**Implementation: `example/D20SystemExampleC/LunaFlow.Rom.D20SystemInterop/D20SystemInteropRom.cs`**

- Implement `IRom` properties: `Name`, `Credits`, `Provides`, `Requires` (all empty since this is the broker)
- Store capability → provider mappings (populated by composition root via a setup method or constructor)
- `Resolve(Capability cap)`: Look up provider by capability name; throw `InvalidOperationException` if missing
- `Resolve<T>(string name)`: Call `Resolve(new Capability(name, typeof(T)))` and cast result, throw if cast fails

**Design choice**: The broker doesn't "provide" any capabilities itself (it's infrastructure, not content). Composition root constructs `D20SystemInteropRom` and manually registers providers (e.g., `interop.RegisterProvider("PlayerLocation", gridMapRom)`). This mirrors the pattern in `cross-rom-capability-spec.md` §4.2 where handlers are constructor-injected with what they need.

### Phase C: Migrate D20SystemContent → Constructor Injection

**Pattern** (per spec §4.3):

Old (Phase 2.6):
```csharp
D20SystemContent.Configure(rooms, flavor, pc, monster, abilityScores, startId);
// Then handlers use: D20SystemContent.Rooms, etc.
```

New (Phase 2.7):
```csharp
var gridMap = new GridMapRom(gridRoomData, startId);
var d20System = new D20SystemRom(pc, monster, challenges, abilityScores);
var interop = new D20SystemInteropRom();
```

**File changes**:

1. **`example/D20SystemExampleB/LunaFlow.Rom.GridMap/GridMapRom.cs`**: Constructor takes `IReadOnlyList<GridRoomData>`, `int StartRoomId`
2. **`example/D20SystemExampleA/LunaFlow.Rom.D20System/D20SystemRom.cs`**: Constructor takes scenario data (PC, Monster, challenges, ability scores)
3. **Handlers in GridMap/D20System**: Update to accept injected data in constructor (not from static `D20SystemContent`)
4. **`src/LunaFlow.Rom/D20SystemContent.cs`**: Mark `[Obsolete("Use constructor injection instead")]` (keep the class for now; don't delete)
5. **`src/MyLunaFlowServer/Program.cs`**: Update composition root to instantiate three ROMs separately, register with `ComponentRegistry`, wire handlers with injected dependencies

### Phase D: Split Facts, Handlers, Rules

**GridMap ownership:**
- Facts: `PlayerPosition`, `PreviousRoomId`, `WinMarker`
- Handlers: 
  - `MoveCommandHandler` (read/write own facts, no cross-ROM dependency)
  - `LookCommandHandler` (read own `PlayerPosition`; cross-ROM read via `rules.Query<ICombatStatus>()` — no broker call)
- Rules: 8 Win-line rules (match `IChallengeSolved`), `GameEndRule` (match own `WinMarker`)
- Adapter: `GridMovementService : IPlayerMovement` (in GridMap; implements `ReturnToPreviousRoom()`)

**D20System ownership:**
- Facts: `RoomChallenge`, `CombatRound : ICombatStatus`, `RoomFailed`
- Handlers:
  - `CheckCommandHandler` (query own room challenges; cross-ROM read via `rules.Query<IPlayerLocation>()`)
  - `FightCommandHandler` (same pattern)
  - `FleeCommandHandler` (calls `IPlayerMovement.ReturnToPreviousRoom()` via constructor-injected `IInteropRom.Resolve<IPlayerMovement>()`)
- Rules: 9 Challenge-prompt rules (match `IPlayerLocation`)

**Key pattern for cross-ROM reads**:
- Fact-shaped contracts (`IPlayerLocation`, `IChallengeSolved`, `ICombatStatus`): Read via `rules.Query<IContractType>()` in handler code (no broker call, pure NRules polymorphic matching per spec §1)
- Service-shaped contract (`IPlayerMovement`): Resolve via `interop.Resolve<IPlayerMovement>()` at handler composition time, constructor-inject into handler

### Phase E: Wire Program.cs & ComponentRegistry

**Composition root steps** (in `src/MyLunaFlowServer/Program.cs`):

1. **Import and instantiate the three ROMs** (from the example namespaces):
   ```csharp
   using LunaFlow.Rom.GridMap;
   using LunaFlow.Rom.D20System;
   using LunaFlow.Rom.D20SystemInterop;
   
   var gridMap = new GridMapRom(
       gridRoomData: OneD20RulebookContent.Rooms.Values.ToList(),
       startRoomId: OneD20RulebookContent.StartRoomId);
   
   var d20System = new D20SystemRom(
       playerCharacter: OneD20RulebookContent.PlayerCharacter,
       monster: OneD20RulebookContent.Monster,
       challenges: ..., // from OneD20RulebookContent
       abilityScores: OneD20RulebookContent.PlayerAbilityScores);
   
   var interop = new D20SystemInteropRom();
   ```

2. **Register capability providers with the broker**:
   ```csharp
   interop.RegisterProvider("PlayerLocation", gridMap);
   interop.RegisterProvider("PlayerMovement", gridMap);
   interop.RegisterProvider("ChallengeSolved", d20System);
   interop.RegisterProvider("CombatStatus", d20System);
   ```

3. **Register components with ComponentRegistry and validate**:
   ```csharp
   var registry = new ComponentRegistry();
   registry.Register(gridMap, EngineTier.Rom, "GridMap");
   registry.Register(d20System, EngineTier.Rom, "D20System");
   registry.Register(interop, EngineTier.Rom, "D20SystemInterop");
   registry.Register(oneD20, EngineTier.Rulebook, "OneD20");
   registry.Validate();  // Fail-fast if any capability mismatch
   ```

4. **Compile rules from all three ROMs**:
   ```csharp
   opts.Rules
       .FromAssembly(RuleExport.Compile(typeof(GridMapRuleGroup)))
       .FromAssembly(RuleExport.Compile(typeof(D20SystemRuleGroup)));
   ```

5. **Seed initial facts** (from all three ROMs):
   ```csharp
   opts.Rules.Seed(() => new object[]
   {
       new PlayerPosition(startRoomId),         // GridMap
       D20SystemContent.PlayerCharacter,        // D20System
       D20SystemContent.Monster,                // D20System
   });
   ```

6. **Register command handlers** with constructor injection:
   ```csharp
   dispatcher.Register(new MoveCommandHandler(gridMap));
   dispatcher.Register(new CheckCommandHandler(d20System, mechanic));
   dispatcher.Register(new FightCommandHandler(d20System, mechanic));
   dispatcher.Register(new FleeCommandHandler(d20System, interop));  // ← passes IInteropRom
   ```

7. **Update OneD20RuleBook.DependsOnRoms** (per Amara's ruling on tier model):
   - `DependsOnRoms` names all three: `["GridMap", "D20System", "D20SystemInterop"]`
   - `Requires` stays empty — the Rulebook→ROM relationship is downward (direct reference expected), not peer-to-peer (capability-mediated)
   - Delete any existing incorrect `Requires` entries from OneD20RuleBook

### Phase F: Verify Assembly Boundary & Zero Peer References

**Constraint** (per spec acceptance criteria): D20SystemInterop references neither GridMap nor D20System; verify by inspecting project files.

**Expected project reference pattern**: 
- `D20SystemExampleB.csproj` (GridMap) → `LunaFlow.Rom`, `LunaFlow.Rules`, `LunaFlow.Engine`, `LunaFlow.Rom.D20SystemInterop` (no ExampleA, no ExampleC)
- `D20SystemExampleA.csproj` (D20System) → `LunaFlow.Rom`, `LunaFlow.Rules`, `LunaFlow.Engine`, `LunaFlow.Rom.D20SystemInterop` (no ExampleB, no ExampleC)
- `D20SystemExampleC.csproj` (D20SystemInterop) → `LunaFlow.Rom`, `LunaFlow.Rules`, `LunaFlow.Engine` (zero references to ExampleA or ExampleB)

**Verification step**: Before declaring done, inspect `.csproj` files to confirm no mutual/peer references exist. The assembly build itself will fail if any are added (compile error), but the spec requires verification "against the assembly graph, not just a passing test."

---

## Critical Files to Modify/Create

### ROM Content Code (in `example/`)
**GridMap** (`example/D20SystemExampleB/`):
- `D20SystemExampleB.csproj` — update project references to add LunaFlow.Rules, Engine, D20SystemInterop
- `LunaFlow.Rom/GridMap/GridMapRom.cs` — main IRom class
- `LunaFlow.Rom/GridMap/Facts.cs` — `PlayerPosition`, `PreviousRoomId`, `WinMarker`
- `LunaFlow.Rom/GridMap/Handlers/MoveCommandHandler.cs`, `LookCommandHandler.cs` (adapted from existing)
- `LunaFlow.Rom/GridMap/GridMovementService.cs` — `IPlayerMovement` adapter
- `LunaFlow.Rom/GridMap/Rules/GridMapRuleGroup.cs` — 8 win-line rules + GameEndRule

**D20System** (`example/D20SystemExampleA/`):
- `D20SystemExampleA.csproj` — update project references
- `LunaFlow.Rom/D20System/D20SystemRom.cs` — main IRom class
- `LunaFlow.Rom/D20System/Facts.cs` — trim to `RoomChallenge`, `CombatRound : ICombatStatus`, `RoomFailed` only
- `LunaFlow.Rom/D20System/Handlers/CheckCommandHandler.cs`, `FightCommandHandler.cs`, `FleeCommandHandler.cs` (adapted)
- `LunaFlow.Rom/D20System/Rules/D20SystemRuleGroup.cs` — 9 challenge-prompt rules (adapted to match `IPlayerLocation`)

**D20SystemInterop** (`example/D20SystemExampleC/`):
- `D20SystemExampleC.csproj` — set project references (only LunaFlow.Rules, Engine)
- `LunaFlow.Rom/D20SystemInterop/D20SystemInteropRom.cs` — main broker class with Resolve/Resolve<T>
- `LunaFlow.Rom/D20SystemInterop/Contracts/IPlayerLocation.cs`, `IChallengeSolved.cs`, `ICombatStatus.cs`, `IPlayerMovement.cs`

### Framework Code (in `src/`)
- `src/LunaFlow.Rom/IInteropRom.cs` — add `Resolve`/`Resolve<T>` method signatures

### Composition Root & Wiring
- `src/MyLunaFlowServer/Program.cs` — instantiate three ROMs, wire providers, register with ComponentRegistry, update handler construction
- `src/LunaFlow.Rulebook/OneD20/OneD20RuleBook.cs` — update `DependsOnRoms` to name all three ROMs; delete incorrect `Requires` entries

### Test Updates
- Test Dev (Nadia) responsibility: adapt existing D20System test structure for split projects (not Baraka's work)
- If tests import from D20System namespaces, ensure imports adjust to the split structure

---

## Known Edge Cases & Decisions

1. **Who registers capability providers with the broker?**
   - Answer: Composition root in Program.cs, after instantiating the three ROMs but before `ComponentRegistry.Validate()`.
   - Alternative considered: Broker discovers/auto-registers via reflection or interface inspection. Rejected: manual registration is explicit, testable, and avoids hidden coupling.

2. **Where does D20SystemContent live after deprecation?**
   - Answer: Keep the class, mark methods `[Obsolete]`, leave imports in place for Nine Rooms (regression-only).
   - Alternative: Delete immediately. Rejected: Nine Rooms still uses it; Phase 2.7 doesn't retrofit Nine Rooms per spec.

3. **Do handlers need to know about the broker?**
   - Answer: Only `FleeCommandHandler` needs it (for `IPlayerMovement.ReturnToPreviousRoom()`); others use `rules.Query<IContractType>()` directly.
   - Implication: `FleeCommandHandler` constructor takes `IInteropRom` (constructor injection), not a static holder.

4. **How does the Rulebook depend on the ROMs?**
   - Design question: parked in Phase 2.6.5 — "Rolling IRuleBook.DependsOnRoms into the unified IHasDependencies shape."
   - Amara's ruling: `IRuleBook.Requires` stays empty for OneD20. Rulebook→ROM is a *downward* reference (Rulebook tier 1, ROM tier 0), not peer-to-peer. Capability-mediation is only for same-tier peers. Direct references are expected for tier transitions. The dependency is carried by `DependsOnRoms` alone.
   - Consequence: Delete the existing incorrect `Requires` entry from `OneD20RuleBook` (currently `new Capability("D20System", typeof(object))`). The Rulebook is a data source for the ROMs, not a consumer of their contracts.

5. **Order of composition root steps:**
   - Phase 1: Instantiate three ROMs with constructor-injected data
   - Phase 2: Register providers with broker
   - Phase 3: Register components with registry + validate
   - Phase 4: Compile rules from all three rule groups
   - Phase 5: Seed engine with initial facts
   - Phase 6: Register command handlers (pass interop to handlers that need it)

---

## Verification Strategy

**Build & Test**:
1. `dotnet build LunaFlow_A.sln` — all projects compile
2. `dotnet test tests/LunaFlow.EngineTests` — existing suite passes (96+ tests green)
3. Manual assembly graph inspection: Confirm no references between GridMap and D20System projects
4. Play through 1-d20 game: All five commands (move/look/check/fight/flee) work; behavior unchanged vs. Phase 2.6

**Test Coverage** (Baraka does not write these; Test Dev does):
- One test per contract type: GridMap fact matches `IPlayerLocation`, D20System fact matches `IChallengeSolved`, etc.
- One test: `FleeCommandHandler` correctly resolves `IPlayerMovement` from broker and calls it
- One test: Registry validation catches duplicate capability names (e.g., two ROMs both providing "PlayerLocation")
- One test: Registry validation passes with all three ROMs registered

**Assembly Graph Verification**:
- Inspect GridMap project references: only `D20SystemInterop` (not D20System)
- Inspect D20System project references: only `D20SystemInterop` (not GridMap)
- Inspect D20SystemInterop project references: only `LunaFlow.Rom`, `LunaFlow.Engine`, `LunaFlow.Rules` (neither peer)

---

## Risk & Mitigation

| Risk | Mitigation |
|------|-----------|
| Circular reference between GridMap & D20System during refactor | Explicit `.csproj` separation + static analysis before merge |
| Handlers instantiated before broker is populated | Composition root order: ROMs → register providers → register handlers |
| NineRooms breaks due to D20SystemContent changes | Keep D20SystemContent class intact, mark obsolete instead of deleting |
| Contract types end up in wrong project | All four contracts live in D20SystemInterop only; both peer ROMs reference it downward for the interfaces; no intra-peer reference |
| `FleeCommandHandler` tries to call broker from rule code | Code inspection: confirm no rule code calls Resolve; only handlers do |
| Existing handler tests fail | Update test setup to construct new D20System/GridMap ROMs with injected data |

---

## Summary

- **Location**: ROM content code fills in three existing example projects (`example/D20SystemExampleA/B/C`), not new projects in `src/`
- **Projects**: Three example projects (D20SystemExampleA for D20System, D20SystemExampleB for GridMap, D20SystemExampleC for D20SystemInterop)
  - Need project references added to `.csproj` files to LunaFlow.Rules, LunaFlow.Engine, and cross-ROM dependencies
- **Key new types**: 
  - `D20SystemInteropRom : IInteropRom` (implements `Resolve`/`Resolve<T>` broker pattern)
  - Four contract interfaces: `IPlayerLocation`, `IChallengeSolved`, `ICombatStatus`, `IPlayerMovement`
  - `GridMapRom : IRom` (spatial concerns)
  - `D20SystemRom : IRom` (trimmed challenge/combat concerns)
- **Code changes**: 
  - Framework: Add `Resolve`/`Resolve<T>` methods to `IInteropRom`
  - ROM content: Facts split, handlers adapted to use contracts, rules updated to match contract types
  - Composition root: Instantiate three ROMs, register providers, update handler injection
  - Rulebook: Update `DependsOnRoms` to name all three ROMs; delete incorrect `Requires` entries (Rulebook→ROM is downward, not capability-mediated)
- **Deprecated**: `D20SystemContent.Configure` (class stays for Nine Rooms; methods marked obsolete)
- **Verified**: Assembly graph (zero peer references between GridMap & D20System), all 1-d20 behavior unchanged, suite green on Arryn-run `latest.trx`
