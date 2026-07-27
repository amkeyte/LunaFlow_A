# GridMap / D20System Split — Implementation Spec

**Status:** in review — framework layout question (ticket 0036) resolved; ready for Baraka once
picked up.
**Companion design doc:** [`../design/engine-core.md`](../design/engine-core.md) — "Component
model: ROM, Rulebook, and the event bus."
**Supersedes:** nothing. Trims and relocates [`1-d20/1-d20-spec.md`](1-d20/1-d20-spec.md)'s existing,
shipped fact vocabulary and handlers across a new ROM boundary — same "reconcile-to-reality, not a
redesign" relationship [ticket 0026](../ClaudeWiki/Processes/Handoffs/0026-reconcile-1-d20-spec.md)
had to Phase 2.6. `1-d20-spec.md` gets a reconcile pass once this ships; it is not superseded by
this doc.

## Purpose

Turns [`phase-2-7.md`](../Roadmap/phase-2-7.md)'s Grid/D20System split into buildable content,
built against [`cross-rom-capability-spec.md`](cross-rom-capability-spec.md) §3's stable authoring
contract, per [ticket 0034](../ClaudeWiki/Processes/Handoffs/0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md).

1.d20 today is one ROM module (`LunaFlow.Rom.D20System`) that conflates two concerns: *where the
player is and how the grid connects* (spatial) and *what a room's obstacle is and how it resolves*
(challenge/combat). This spec splits those into two peer content ROMs — **GridMap** (spatial:
position, movement, win-line topology) and a **trimmed D20System** (challenge resolution: skill
checks, combat) — plus a third, real ROM that is the interop broker itself
(**D20SystemInterop**, `IInteropRom`), per [ticket 0036](../ClaudeWiki/Processes/Handoffs/0036-grid-rom-d20system-project-layout-decision.md)'s
resolved ruling. Zero project reference exists between the two content peers; their only
interaction is mediated through the broker. This spec also retires
`D20SystemContent.Configure`'s static holder in favor of constructor injection, per §4.3.

**This spec does not implement:** `D20SystemInterop`'s generic aggregate/resolve internals — the
broker's own C# (framework work; this spec names the capabilities it must carry, not its code).

## Scope

**In:**
- Splitting 1.d20's current fact vocabulary and command handlers between GridMap and a trimmed
  D20System.
- The capability contracts each ROM provides/requires, carried by the third ROM
  (`D20SystemInterop`), and which existing rule/handler behavior crosses the boundary through them.
- Retiring `D20SystemContent.Configure` — Scenario data (rooms, flavor text, PC/monster stats,
  ability scores, start room) reaches both content ROMs' handlers by constructor injection instead
  of a static holder, per §4.3.
- `OneD20RuleBook.DependsOnRoms` naming all three ROMs.

**Out:**
- `D20SystemInterop`'s own aggregate/resolve C# — framework work, phase-2-7.md's Architect-owned
  deliverable, now sized against this spec's real capability list.
- `DependsOnRoms` *validation* enforcement (`ComponentRegistry` reading it) — already tracked as
  live framework work in phase-2-7.md's Deliverables, not a new ask from this spec.
- NineRooms — untouched, stays deprecated/regression-only, per phase-2-7.md's struck retrofit
  bullet.
- Any new rooms, mechanics, or win conditions. This is a structural split of shipped content, not a
  content redesign — no fact shape changes beyond relocation, no behavior changes beyond what
  crossing the boundary requires.

## Fact / marker vocabulary

Three real ROMs, three groups: GridMap's own facts/content, D20System's trimmed facts/content, and
D20SystemInterop's shared contracts. Everything not listed as changed keeps its exact shape from
[`1-d20-spec.md`](1-d20/1-d20-spec.md).

### GridMap (`LunaFlow.Rom.GridMap`, own project — confirmed per ticket 0036's example scaffold)

Static content (constructor-injected; replaces the room-position slice of `D20SystemContent`):

```csharp
public sealed record GridRoomData(
    int Id, int Row, int Col,
    string DescriptionKey);   // → 1-d20-flavor.md, unchanged keys
```

Plus `int StartRoomId` and the grid-adjacency function (`TryMove`, unchanged math) — both relocate
here verbatim from `D20SystemContent`.

Facts (NRules; unchanged shape from today's `D20System.Facts.cs`, GridMap-owned):

```csharp
public sealed record PlayerPosition(int RoomId) : IPlayerLocation;
public sealed record PreviousRoomId(int RoomId);
public sealed record WinMarker(IReadOnlyList<int> Line);
```

`PlayerPosition` implements `IPlayerLocation` directly — see [Interop
contracts](#interop-contracts-lunaflowromd20systeminterop-own-project-a-real-rom-not-a-passive-library).
`PreviousRoomId` and `WinMarker` cross no boundary and stay plain GridMap facts.

Also in this project: `GridMovementService : IPlayerMovement` — a small adapter, not a fact, that
performs the actual `Retract`/`Insert` on `PlayerPosition`/`PreviousRoomId`. It references
`D20SystemInterop` only for the interface it implements; nothing else needs to reference
`D20System`. See [Capability declarations](#capability-declarations).

### D20System (trimmed; `LunaFlow.Rom.D20System`, own project, unchanged namespace)

Static content (constructor-injected; replaces the rest of `D20SystemContent`):

```csharp
public sealed record RoomChallenge(
    int Id,
    RoomChallengeType ChallengeType,
    string? Skill, string? SkillAbility, int? DC,
    string? SuccessKey, string? FailureKey, string? PromptKey);
```

`RoomChallengeType`, `CombatantStats`, `PlayerCharacter`, `MonsterTemplate` are unchanged from
today's `Facts.cs` — pure combat/challenge data, no grid fields, nothing to trim.

Facts (NRules; unchanged shape, D20System-owned):

```csharp
public sealed record RoomSolved(int RoomId) : IChallengeSolved;
public sealed record RoomFailed(int RoomId);
public sealed record CombatRound(int RoomId, int MonsterHp, int PlayerHp, int RoundsElapsed) : ICombatStatus;
```

`RoomSolved` and `CombatRound` implement their interop interfaces directly, same pattern as
`PlayerPosition` above. `RoomFailed` crosses no boundary — see [Mechanics /
rules](#mechanics--rules) for why win-line logic needs nothing from it directly. This project needs
no adapter classes of its own — every capability D20System provides is fact-shaped, observed
directly, never invoked as an action.

### D20SystemInterop (`LunaFlow.Rom.D20SystemInterop`, own project — a real `IRom`/`IInteropRom`, not a passive library)

Per ticket 0036's ruling: this is a genuine third ROM performing the negotiation between the other
two, not merely a shared assembly of interface definitions. It holds:

```csharp
// Fact-shaped contracts — matched in rules (Match<T>) or queried in handlers (rules.Query<T>()),
// no broker call at runtime, per cross-rom-capability-spec.md §1: the shared NRules session does
// the work once both peers reference this project for the interface.
public interface IPlayerLocation { int RoomId { get; } }
public interface IChallengeSolved { int RoomId { get; } }
public interface ICombatStatus { int RoomId { get; } int MonsterHp { get; } int PlayerHp { get; } }

// Service-shaped contract — resolved once via D20SystemInterop's own Resolve<T> at
// composition-root time, invoked from a handler's constructor-injected instance. Never touched
// from rule code (§4.3).
public interface IPlayerMovement
{
    /// <summary>Moves the player back to <c>PreviousRoomId</c>. Returns false (no-op) if none
    /// exists yet — the room-5-pre-any-move edge case 1-d20-spec.md already names.</summary>
    bool ReturnToPreviousRoom();
}
```

Plus the concrete `IInteropRom` implementation (name TBD, e.g. `D20SystemInteropRom`) — the
`Resolve`/`Resolve<T>` machinery per §4.1, populated by the composition root at registration time.
**This project references neither GridMap nor D20System.** `Resolve` deals only in `Capability`/
`object`; it never needs either peer's concrete fact types, because the composition root — not the
broker — is what constructs `GridMovementService` and wires the fact-implementing records. Both
content ROMs reference `D20SystemInterop` downward for the interfaces; that's the one-directional,
governed edge phase-2-7.md's Done-When describes, not a peer-to-peer one.

**Why three contracts are fact-shaped and one is service-shaped:** `IPlayerLocation`,
`IChallengeSolved`, and `ICombatStatus` are all *observations* of a peer's own state — exactly
§3's worked example, applied three times. `IPlayerMovement` is the one case that's genuinely an
*action* a consumer needs a peer to perform on its own facts (`flee` moving the PC) — mutation
stays with the owner (§3.2 rule 2, §5), so the consumer invokes a capability the owner implements,
never mutates the fact itself. This is §4.2's handler-injection pattern, generalized from a query
(`ILocationQuery` in the spec's own example) to an action.

### Capability declarations

```csharp
// GridMap
Provides: [ new("PlayerLocation", typeof(IPlayerLocation)),
            new("PlayerMovement", typeof(IPlayerMovement)) ]
Requires: [ new("ChallengeSolved", typeof(IChallengeSolved)) ]

// D20System
Provides: [ new("ChallengeSolved", typeof(IChallengeSolved)),
            new("CombatStatus", typeof(ICombatStatus)) ]
Requires: [ new("PlayerLocation", typeof(IPlayerLocation)),
            new("PlayerMovement", typeof(IPlayerMovement)) ]

// D20SystemInterop — provides no game-facing capability of its own; it's the broker the two
// content ROMs' Provides/Requires resolve through, registered as its own IRom for identity.
```

## Mechanics / rules

Grouped by owning ROM. Behavior is unchanged from `1-d20-spec.md` except where crossing the new
boundary requires a contract type instead of a concrete one — called out explicitly at each point.

### GridMap

**`move`** (`MoveCommandHandler`, relocated verbatim): grid adjacency via `TryMove`, retracts/inserts
`PlayerPosition`, updates `PreviousRoomId`, announces the destination's `DescriptionKey` from
`GridRoomData`. No cross-ROM read or write — unchanged from today.

**`look`** (`LookCommandHandler`, relocated): announces the current room's `DescriptionKey`
unconditionally, same as today. The combat-status line ("You're at 8/11 HP. He's at 3/8 HP.") is
now a cross-ROM read: `rules.Query<ICombatStatus>().FirstOrDefault(s => s.RoomId == current.RoomId)`
against the shared session — the contract type, never `CombatRound` by name. No behavior change
visible to the player; the query source changes from `LunaFlow.Rom.D20System.CombatRound` to the
`D20SystemInterop`-defined contract.

**Win-line rules** (8 total, relocated to a `GridMapRuleGroup`): unchanged shape and unchanged
room-id groupings. The one change: each `Match<RoomSolved>(...)` becomes
`Match<IChallengeSolved>(...)` — the rule correlates against the contract D20System provides, never
the concrete fact. **No `RoomFailed` involvement needed here, same as today:** a room that ends up
`RoomFailed` can never later produce `RoomSolved`/`IChallengeSolved`, so it permanently kills every
line through it without an extra guard — `D20SystemRuleGroup.WinLineRule`'s existing doc comment
already states this reasoning; it carries over unchanged.

**`GameEndRule`** (relocated): unchanged — matches GridMap's own `WinMarker`, no cross-ROM read.

### D20System

**Challenge-prompt rules** (9 total, relocated): unchanged shape. Two changes: `Match<Room>(...)`
becomes `Match<RoomChallenge>(...)` (D20System's own trimmed fact, no cross-ROM read), and
`Match<PlayerPosition>(p => p.RoomId == roomId)` becomes `Match<IPlayerLocation>(loc => loc.RoomId
== roomId)` — the rule correlates against GridMap's provided contract, never `PlayerPosition` by
name. `Not<RoomSolved>`/`Not<RoomFailed>` stay concrete — D20System owns both, no cross-ROM read
needed for its own facts.

**`check`** (`CheckCommandHandler`, relocated): unchanged mechanics (`D20ResolutionMechanic.ResolveCheck`,
success/failure narration). The current-room lookup changes from `rules.Query<PlayerPosition>().First()`
to `rules.Query<IPlayerLocation>().First()` — same reasoning as `look` above, a contract-typed
handler-side read, no broker call.

**`fight`** (`FightCommandHandler`, relocated): unchanged mechanics (attack rolls, damage, the
lethality-first ordering `1-d20-spec.md` specifies). Same `IPlayerLocation` query change as `check`
for validity-checking the current room.

**`flee`** (`FleeCommandHandler`, relocated): unchanged narration and edge cases (silent no-op with
no active `CombatRound`; stays put if fled before any `move`). The one change is structural: instead
of directly retracting/inserting `PlayerPosition`/`PreviousRoomId` (facts it no longer owns),
`FleeCommandHandler` calls its constructor-injected `IPlayerMovement.ReturnToPreviousRoom()` —
resolved from `D20SystemInterop` at composition-root time, implemented by GridMap's
`GridMovementService`. A `false` return (no `PreviousRoomId` yet) maps to the existing "stay put"
behavior — same outcome, now reached through the capability instead of a direct fact mutation.

## Commands & handlers

| Command | Owner ROM | Reads | Writes | Cross-ROM |
| --- | --- | --- | --- | --- |
| `move` | GridMap | `PlayerPosition`, `GridRoomData` | `PlayerPosition`, `PreviousRoomId` | none |
| `look` | GridMap | `PlayerPosition`, `GridRoomData`, `ICombatStatus` | — | `ICombatStatus` (query) |
| `check` | D20System | `IPlayerLocation`, `RoomChallenge`, ability scores | `RoomSolved` | `IPlayerLocation` (query) |
| `fight` | D20System | `IPlayerLocation`, `PlayerCharacter`, `MonsterTemplate`, `CombatRound`, `RoomChallenge` | `CombatRound`, `RoomSolved`, `RoomFailed` | `IPlayerLocation` (query) |
| `flee` | D20System | `CombatRound` | `CombatRound` | `IPlayerMovement` (resolve + invoke, via `D20SystemInterop`) |

## Acceptance criteria

- [ ] `IRom` instances exist for GridMap, D20System, and `D20SystemInterop` (the latter also
      implementing `IInteropRom`); `ComponentRegistry.Validate()` passes with all three registered.
- [ ] `OneD20RuleBook.DependsOnRoms` names all three: `"GridMap"`, `"D20System"`,
      `"D20SystemInterop"`.
- [ ] Every behavior in `1-d20-spec.md`'s existing acceptance criteria still holds unchanged,
      verified against the split structure — this spec introduces no new player-facing behavior,
      only relocates the fact/handler ownership behind it.
- [ ] The challenge-prompt rule fires correctly off `IPlayerLocation`, matched from a `PlayerPosition`
      fact GridMap inserts — verified with zero project reference from D20System to GridMap (assembly
      graph, not just a passing test, per `cross-rom-capability-spec.md`'s own acceptance criterion 1).
- [ ] Win-line rules fire correctly off `IChallengeSolved`, matched from a `RoomSolved` fact
      D20System inserts — same zero-reference bar, opposite direction.
- [ ] `look`'s combat-status line reports correctly via `ICombatStatus`, sourced from D20System's
      `CombatRound` with no reference from GridMap to D20System.
- [ ] `flee` returns the PC to `PreviousRoomId` via `IPlayerMovement.ReturnToPreviousRoom()`, and the
      no-`PreviousRoomId`-yet case returns `false` without throwing and without moving the PC.
- [ ] `D20SystemInterop` has zero project reference to either GridMap or D20System — verified
      against the assembly graph, confirming the broker resolves by `Capability`/`object` alone.
- [ ] `D20SystemContent.Configure` and its backing static state are gone from both content ROMs;
      `Program.cs` passes Scenario data (rooms, flavor text, PC/monster stats, ability scores, start
      room) by constructor argument.
- [ ] Removing any of the three ROMs from the registration list produces a clear load-time error
      (from the framework's registry validation, not this spec's content) rather than a silent
      failure.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

- **`D20SystemInterop`'s aggregate/resolve implementation** — phase-2-7.md's Architect-owned
  deliverable, now sized against this spec's real capability list (three fact-shaped, one
  service-shaped) instead of a guessed example. Not a new ask; named here so the sizing has a real
  target.
- **`DependsOnRoms` validation enforcement** — already tracked as live framework work in
  phase-2-7.md's Deliverables (`ComponentRegistry.Validate()` doesn't read it yet). Not a new ask.
- Project layout itself is resolved — [ticket 0036](../ClaudeWiki/Processes/Handoffs/0036-grid-rom-d20system-project-layout-decision.md)
  closed against the example scaffold in `example/D20SystemExampleA`/`B`/`C`. No longer an open
  framework dependency.

## Open questions / edge cases

- **`ICombatStatus`/`IPlayerLocation` as handler-side `Query<T>` vs. broker `Resolve`:** this spec
  treats fact-shaped contracts as directly queryable through the shared NRules session (no broker
  call, per §1's polymorphic-matching finding extending to `Query<T>` the same way it does to
  `Match<T>`) and reserves `D20SystemInterop`'s `Resolve` for the one service-shaped case
  (`IPlayerMovement`). If the broker's implementation disagrees with this reading of §1, that's a
  real design divergence to catch before Baraka builds against this spec, not after.
- **Whether `RoomChallenge`/`GridRoomData` need to be seeded as NRules facts** (today's `Room` is —
  see `D20SystemRuleGroup.ChallengeRule`'s `Match<Room>`) or can stay plain dictionary lookups in
  generated `Then()` code. Not decided here; RulePrototype generation shape is Baraka's translation
  of this spec's stated behavior, per `content-spec-template.md`'s "state player-facing behavior,
  not NRules mechanics" instruction.
- **`D20SystemInteropRom`'s exact class name** — placeholder, Baraka's call; the project/namespace
  name (`D20SystemInterop`) is fixed by ticket 0036's example scaffold, the class name inside it
  isn't.
- **NineRooms** — confirmed untouched by this spec; it keeps its own `PlayerPosition`/`RoomSolved`/
  `WinMarker` facts, duplicated rather than shared, exactly as it does today against D20System.

## Related documents

- [`1-d20/1-d20-spec.md`](1-d20/1-d20-spec.md) — the shipped content this spec splits; reconciled
  afterward, not superseded
- [`1-d20/1-d20-flavor.md`](1-d20/1-d20-flavor.md) — flavor keys, unchanged, now split in ownership
  between `GridRoomData.DescriptionKey` and `RoomChallenge`'s prompt/success/failure keys
- [`cross-rom-capability-spec.md`](cross-rom-capability-spec.md) — the authoring contract (§3) this
  spec builds against, and the broker surface (§4) it names capabilities for
- [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md) — the phase this spec's content belongs to
- [`../Roadmap/phase-2-6-5.md`](../Roadmap/phase-2-6-5.md) — `IRom`/`IInteropRom`/`Capability`/
  `IHasDependencies`, shipped and load-bearing for this spec
- [`../design/engine-core.md`](../design/engine-core.md) — companion design doc
- [`../design/glossary.md`](../design/glossary.md) — `Capability`, `IRom`, `IInteropRom`, `EngineTier`
  definitions used throughout this spec
- [Ticket 0034](../ClaudeWiki/Processes/Handoffs/0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md) —
  the handoff this spec answers
- [Ticket 0036](../ClaudeWiki/Processes/Handoffs/0036-grid-rom-d20system-project-layout-decision.md) —
  resolved; the three-project layout this spec builds against
- [Specs README](README.md)
