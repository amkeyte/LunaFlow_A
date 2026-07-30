# GridMap / D20System Split — Implementation Spec

**Status:** in review — Cartridge-pivot correction (ticket 0041) applied; built per ticket
[0037](../ClaudeWiki/Processes/Handoffs/0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md).
**Amended 2026-07-29** to add the fifth Cartridge contract, `IGameWon`, per
[ticket 0045](../ClaudeWiki/Processes/Handoffs/0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md)'s
Architect ruling — this doc previously undercounted the contract surface at four; five is what
shipped and is verified. **Amended 2026-07-30** to resolve this doc's own open question on how
generated declarative rules reach Rulebook-supplied flavor text, per
[ticket 0043](../ClaudeWiki/Processes/Handoffs/0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md)'s
Architect ruling — not yet built as of this amendment; describes the decided shape, not shipped
status (see the roadmap/QA verdict for that).
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
(challenge/combat). This spec splits those into two peer ROMs — **GridMap** (spatial: position,
movement, win-line topology) and a **trimmed D20System** (challenge resolution: skill checks,
combat) — both living inside one **Cartridge** (`design/cartridge.md`), the published unit that
pairs a contract surface with the implementation behind it. The contract surface (project
`LunaFlow.Cartridge`) holds the five shared interfaces (four at this spec's original writing, plus
`IGameWon` added post-build — see [The Cartridge contract
surface](#the-cartridge-contract-surface-lunaflowcartridge)); it is not a ROM, is not registered,
and brokers nothing — GridMap and D20System are peers correlating through it directly, per
[ticket 0041](../ClaudeWiki/Processes/Handoffs/0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md)'s
correction of this spec's original (superseded) framing. Zero project reference exists between the
two peers; both reference the contract surface downward, and it references neither of them. This
spec also retires `D20SystemContent.Configure`'s static holder in favor of constructor injection,
per §4.3.

**This spec does not implement:** anything in `LunaFlow.Cartridge` beyond the five interfaces
themselves — there is no broker class, no `Resolve`, nothing to size or build there. The
composition root (`Program.cs`) is what wires implementations to consumers, plain constructor
injection.

## Scope

**In:**
- Splitting 1.d20's current fact vocabulary and command handlers between GridMap and a trimmed
  D20System.
- The capability contracts each ROM provides/requires, carried by the Cartridge's contract-surface
  project (`LunaFlow.Cartridge`), and which existing rule/handler behavior crosses the boundary
  through them.
- Retiring `D20SystemContent.Configure` — Scenario data (rooms, flavor text, PC/monster stats,
  ability scores, start room) reaches both ROMs' handlers by constructor injection instead
  of a static holder, per §4.3.
- `OneD20RuleBook.DependsOnRoms` naming both ROMs.

**Out:**
- `DependsOnRoms` *validation* enforcement (`ComponentRegistry` reading it) — already tracked as
  live framework work in phase-2-7.md's Deliverables, not a new ask from this spec.
- NineRooms — untouched, stays deprecated/regression-only, per phase-2-7.md's struck retrofit
  bullet.
- Any new rooms, mechanics, or win conditions. This is a structural split of shipped content, not a
  content redesign — no fact shape changes beyond relocation, no behavior changes beyond what
  crossing the boundary requires.

## Fact / marker vocabulary

Three groups: GridMap's own facts/content, D20System's trimmed facts/content, and the Cartridge
contract surface's shared interfaces. Everything not listed as changed keeps its exact shape from
[`1-d20-spec.md`](1-d20/1-d20-spec.md).

### GridMap (`LunaFlow.Rom.GridMap`, own project in `example/` — location confirmed per ticket 0036's example scaffold and ticket 0038, unaffected by the Cartridge pivot)

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
public sealed record WinMarker(IReadOnlyList<int> Line) : IGameWon;
```

`PlayerPosition` implements `IPlayerLocation` directly — see [The Cartridge contract
surface](#the-cartridge-contract-surface-lunaflowcartridge). `PreviousRoomId` crosses no boundary
and stays a plain GridMap fact. **`WinMarker` was originally written the same way** ("crosses no
boundary, stays plain"), which this spec's own acceptance criteria and NineRooms' inherited
`Not<WinMarker>()` guard contradicted once built — see [ticket
0045](../ClaudeWiki/Processes/Handoffs/0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md).
`WinMarker` now implements `IGameWon`, the fifth contract, same pattern as `PlayerPosition`. Its own
`Line` field does not cross; only the marker's existence does.

Also in this project: `GridMovementService : IPlayerMovement` — a small adapter, not a fact, that
performs the actual `Retract`/`Insert` on `PlayerPosition`/`PreviousRoomId`. It references
`LunaFlow.Cartridge` only for the interface it implements; nothing else needs to reference
`D20System`. See [Capability declarations](#capability-declarations).

### D20System (trimmed; `LunaFlow.Rom.D20System`, own project in `example/`, unchanged namespace)

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

### The Cartridge contract surface (`LunaFlow.Cartridge`)

**Corrected 2026-07-27, [ticket 0041](../ClaudeWiki/Processes/Handoffs/0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md),
location fixed same-day by [ticket 0042](../ClaudeWiki/Processes/Handoffs/0042-correct-cartridge-project-location-in-spec.md):**
this spec originally modeled this as a third ROM (`D20SystemInterop`, implementing `IInteropRom`),
per [ticket 0036](../ClaudeWiki/Processes/Handoffs/0036-grid-rom-d20system-project-layout-decision.md)'s
then-ruling. That's superseded — [`design/cartridge.md`](../design/cartridge.md) names this exactly
the passive contract surface the original framing said it wasn't: content, not framework, so it
**stays in `example/D20SystemExampleC`** (already its own project — that alone satisfies
`cartridge.md`'s "own assembly, never inside a ROM" invariant; `src/` is reserved for the genuinely
engine-agnostic tiers, same reasoning ticket 0038 already applied to keep GridMap/D20System out of
`src/`), retyped from the original `D20SystemInteropRom : IInteropRom` to bare interfaces, holding
the five shared interfaces (`IGameWon` added post-build, ticket 0045) and performing no negotiation
of its own. GridMap and D20System are
peers declaring `Provides`/`Requires` directly against each other; `ComponentRegistry` validates the
pair with no third component mediating. It holds:

```csharp
// Fact-shaped contracts — matched in rules (Match<T>) or queried in handlers (rules.Query<T>()).
// Ordinary NRules polymorphic matching does the work once both peers reference this project for
// the interface — no runtime call into this project at all, per cross-rom-capability-spec.md §1.
public interface IPlayerLocation { int RoomId { get; } }
public interface IChallengeSolved { int RoomId { get; } }
public interface ICombatStatus { int RoomId { get; } int MonsterHp { get; } int PlayerHp { get; } }

// Marker contract, no members. Added post-build (ticket 0045), not at this spec's original
// writing. D20System's challenge-prompt rules need to stop firing once the game is won, but
// WinMarker is GridMap's and Not<T>() never reads a property — so the contract carries nothing,
// not even WinMarker's own Line. Guarded with Not<IGameWon>(), never Not<WinMarker>() by name.
public interface IGameWon { }

// Service-shaped contract — GridMap implements it; Program.cs constructs GridMap's implementation
// and passes it directly to FleeCommandHandler's constructor as this interface. No resolve call
// anywhere; never touched from rule code (§4.3).
public interface IPlayerMovement
{
    /// <summary>Moves the player back to <c>PreviousRoomId</c>. Returns false (no-op) if none
    /// exists yet — the room-5-pre-any-move edge case 1-d20-spec.md already names.</summary>
    bool ReturnToPreviousRoom();
}
```

**This project holds interfaces only — no broker class, no `Resolve`, no registration.** It
implements no `IRom`, declares no capabilities, and is never registered with `ComponentRegistry`.
**It references neither GridMap nor D20System**, and — unlike the superseded framing — nothing in
it needs to, because there is no negotiation logic here to construct anything from either peer's
concrete types. Both ROMs reference this project downward for the interfaces; that's the
one-directional, governed edge cross-rom-capability-spec.md §4.4 describes, not a peer-to-peer one.

**Why three contracts are fact-shaped, one is a bare marker, and one is service-shaped:**
`IPlayerLocation`, `IChallengeSolved`, and `ICombatStatus` are all *observations* of a peer's own
state — exactly §3's worked example, applied three times. `IGameWon` is the same kind of
observation stripped to existence only — the one guard that needs it (`Not<IGameWon>()`) never
reads a member, so nothing crosses but the type. `IPlayerMovement` is the one case that's genuinely an
*action* a consumer needs a peer to perform on its own facts (`flee` moving the PC) — mutation
stays with the owner (§3.2 rule 2, §6), so the consumer is handed a capability the owner
implements, never mutates the fact itself. This is §4.2's constructor-injection pattern, generalized
from a query (`ILocationQuery` in the spec's own example) to an action — supplied directly by
`Program.cs`, not resolved from anywhere.

### Capability declarations

```csharp
// GridMap
Provides: [ new("PlayerLocation", typeof(IPlayerLocation)),
            new("PlayerMovement", typeof(IPlayerMovement)),
            new("GameWon", typeof(IGameWon)) ]          // added post-build, ticket 0045
Requires: [ new("ChallengeSolved", typeof(IChallengeSolved)) ]

// D20System
Provides: [ new("ChallengeSolved", typeof(IChallengeSolved)),
            new("CombatStatus", typeof(ICombatStatus)) ]
Requires: [ new("PlayerLocation", typeof(IPlayerLocation)),
            new("PlayerMovement", typeof(IPlayerMovement)),
            new("GameWon", typeof(IGameWon)) ]          // added post-build, ticket 0045

// LunaFlow.Cartridge isn't registered at all — not an IRom, provides/requires nothing itself.
// GridMap and D20System declare Provides/Requires directly against each other; ComponentRegistry
// validates the pair with no third component in the loop.
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
`LunaFlow.Cartridge`-defined contract.

**Win-line rules** (8 total, relocated to a `GridMapRuleGroup`): unchanged shape and unchanged
room-id groupings. The one change: each `Match<RoomSolved>(...)` becomes
`Match<IChallengeSolved>(...)` — the rule correlates against the contract D20System provides, never
the concrete fact. **No `RoomFailed` involvement needed here, same as today:** a room that ends up
`RoomFailed` can never later produce `RoomSolved`/`IChallengeSolved`, so it permanently kills every
line through it without an extra guard — `D20SystemRuleGroup.WinLineRule`'s existing doc comment
already states this reasoning; it carries over unchanged.

**`GameEndRule`** (relocated): matches GridMap's own `WinMarker`, no cross-ROM read — unchanged on
that count. **One addition, per ticket 0043:** the rule also matches a seeded, one-instance
`GameEndAnnouncement(string Text)` fact (`Match<GameEndAnnouncement>(() => end)`) and announces
`end.Text` instead of the hardcoded placeholder `"You win!\n"` this spec's own build originally
shipped with. `Program.cs` seeds the single instance once, resolved from `1-d20-flavor.md`'s
`win-announce` key, via the same `.Seed(...)` call that already seeds `GridRoomData`/`PlayerPosition`/
`PlayerCharacter`/`Monster`. No static holder; the rule never performs a dictionary lookup.

### D20System

**Challenge-prompt rules** (9 total, relocated): unchanged shape. Two changes: `Match<Room>(...)`
becomes `Match<RoomChallenge>(...)` (D20System's own trimmed fact, no cross-ROM read), and
`Match<PlayerPosition>(p => p.RoomId == roomId)` becomes `Match<IPlayerLocation>(loc => loc.RoomId
== roomId)` — the rule correlates against GridMap's provided contract, never `PlayerPosition` by
name. `Not<RoomSolved>`/`Not<RoomFailed>` stay concrete — D20System owns both, no cross-ROM read
needed for its own facts. **A third guard, `Not<IGameWon>()`, is required and was originally missed
in translation** (ticket 0045): the pre-split rule this family descends from carries a bare
`Not<WinMarker>()` (see NineRooms' `ChallengeRules.cs`), and `1-d20-spec.md` inherits it silently as
"the same guard Nine Rooms already relies on." `WinMarker` is GridMap's and crosses no boundary by
name, so the guard is written against the fifth Cartridge contract instead — the one cross-ROM read
this rule family needs, same contract-typed pattern as `Match<IPlayerLocation>`.

**A second addition, per ticket 0043:** the rule also matches a seeded `RoomChallengePrompt(int
RoomId, string Text)` fact (`Match<RoomChallengePrompt>(() => prompt, p => p.RoomId == roomId)`) and
announces `prompt.Text`, replacing the hardcoded placeholder `"[Room {roomId}] Challenge prompt."`
this spec's own build originally shipped with. `Program.cs` seeds one instance per room, resolving
`flavorText[room.PromptKey]` for skill-check rooms and `flavorText["monster-desc"]` for fight rooms
— the same branching `1-d20-spec.md` already specifies — via the same `.Seed(...)` call, same
reasoning as `GameEndAnnouncement` above. `RoomChallenge`'s own `PromptKey`/`SuccessKey`/`FailureKey`
fields are untouched; the hand-written `check`/`fight` handlers keep resolving those themselves via
their own constructor-injected `flavorText`, unaffected by this addition.

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
`Program.cs` constructs GridMap's `GridMovementService` directly and passes it in; no resolve call,
no broker, plain DI. A `false` return (no `PreviousRoomId` yet) maps to the existing "stay put"
behavior — same outcome, now reached through the capability instead of a direct fact mutation.

## Commands & handlers

| Command | Owner ROM | Reads | Writes | Cross-ROM |
| --- | --- | --- | --- | --- |
| `move` | GridMap | `PlayerPosition`, `GridRoomData` | `PlayerPosition`, `PreviousRoomId` | none |
| `look` | GridMap | `PlayerPosition`, `GridRoomData`, `ICombatStatus` | — | `ICombatStatus` (query) |
| `check` | D20System | `IPlayerLocation`, `RoomChallenge`, ability scores | `RoomSolved` | `IPlayerLocation` (query) |
| `fight` | D20System | `IPlayerLocation`, `PlayerCharacter`, `MonsterTemplate`, `CombatRound`, `RoomChallenge` | `CombatRound`, `RoomSolved`, `RoomFailed` | `IPlayerLocation` (query) |
| `flee` | D20System | `CombatRound` | `CombatRound` | `IPlayerMovement` (constructor-injected by `Program.cs`, no resolve) |

## Acceptance criteria

- [ ] `IRom` instances exist for GridMap and D20System only; `ComponentRegistry.Validate()` passes
      with both registered. `LunaFlow.Cartridge` implements no `IRom` and is not registered.
- [ ] `OneD20RuleBook.DependsOnRoms` names both: `"GridMap"`, `"D20System"` — confirmed by Juno per
      ticket 0041 (there's nothing for a third entry to validate against once the contract surface
      isn't an `IRom`).
- [ ] Every behavior in `1-d20-spec.md`'s existing acceptance criteria still holds unchanged,
      verified against the split structure — this spec introduces no new player-facing behavior,
      only relocates the fact/handler ownership behind it.
- [ ] The challenge-prompt rule fires correctly off `IPlayerLocation`, matched from a `PlayerPosition`
      fact GridMap inserts — verified with zero project reference from D20System to GridMap (assembly
      graph, not just a passing test, per `cross-rom-capability-spec.md`'s own acceptance criterion 1).
- [ ] Win-line rules fire correctly off `IChallengeSolved`, matched from a `RoomSolved` fact
      D20System inserts — same zero-reference bar, opposite direction.
- [ ] Once any `WinMarker` exists, every challenge-prompt rule stops announcing — for the room it
      just won *and* for every other unsolved room — verified via `Not<IGameWon>()`, matched from a
      `WinMarker` fact GridMap inserts, with zero project reference from D20System to GridMap (same
      assembly-graph bar as the two criteria above). Added by ticket 0045; the fifth Cartridge
      contract.
- [ ] `look`'s combat-status line reports correctly via `ICombatStatus`, sourced from D20System's
      `CombatRound` with no reference from GridMap to D20System.
- [ ] `flee` returns the PC to `PreviousRoomId` via `IPlayerMovement.ReturnToPreviousRoom()`, and the
      no-`PreviousRoomId`-yet case returns `false` without throwing and without moving the PC.
- [ ] `LunaFlow.Cartridge` has zero project reference to either GridMap or D20System — verified
      against the assembly graph. It contains interfaces only; there is nothing else to reference.
- [ ] `D20SystemContent.Configure` and its backing static state are gone from both content ROMs;
      `Program.cs` passes Scenario data (rooms, flavor text, PC/monster stats, ability scores, start
      room) by constructor argument.
- [ ] Removing either ROM from the registration list produces a clear load-time error (from the
      framework's registry validation, not this spec's content) rather than a silent failure.
- [ ] The 9 challenge-prompt rules and `GameEndRule` announce `1-d20-flavor.md`'s real content
      (room-specific prompt text, `monster-desc`, `win-announce`) via the seeded
      `RoomChallengePrompt`/`GameEndAnnouncement` facts — not the generic placeholder text this
      spec's own build originally shipped with. Added by ticket 0043; closes the one gap
      `grid-rom-d20-split-spec.md`'s "unchanged behavior" bar didn't hold at first build.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

- **`DependsOnRoms` validation enforcement** — already tracked as live framework work in
  phase-2-7.md's Deliverables (`ComponentRegistry.Validate()` doesn't read it yet). Not a new ask.
- **The bootstrap contract probe** (`cross-rom-capability-spec.md` §5) — a Phase 2.7 framework
  deliverable, not this spec's to build, but this spec's five contracts are exactly what it exercises
  at bootstrap. Named here so the connection is visible, not as a new ask.
- Project layout: `GridMap`, `D20System`, and `LunaFlow.Cartridge` (the contract surface, retyped
  from `D20SystemInteropRom : IInteropRom` to bare interfaces) all stay in `example/` — content, not
  framework, so `src/` doesn't apply to any of the three; the Cartridge's "own assembly" invariant is
  satisfied by `example/D20SystemExampleC` already being its own project. Confirmed by Arryn (tickets
  0038, 0040). No longer an open framework dependency — superseded framing struck via
  [ticket 0036](../ClaudeWiki/Processes/Handoffs/0036-grid-rom-d20system-project-layout-decision.md),
  corrected via [ticket 0041](../ClaudeWiki/Processes/Handoffs/0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md),
  location fixed via [ticket 0042](../ClaudeWiki/Processes/Handoffs/0042-correct-cartridge-project-location-in-spec.md).

## Open questions / edge cases

- **`ICombatStatus`/`IPlayerLocation` as handler-side `Query<T>` — settled, not open** (was flagged
  as an open risk in this spec's original draft; resolved by the Cartridge pivot rather than by
  Baraka's build). There is no broker `Resolve` under any reading now — `cross-rom-capability-spec.md`
  §4.1 cuts it outright, on trust-boundary grounds independent of this spec. Fact-shaped contracts
  are queried/matched directly through the shared NRules session; `IPlayerMovement` is supplied by
  constructor injection from `Program.cs`. No divergence to catch.
- **Whether generated rules need a seam to Rulebook-supplied flavor text — settled, not open**
  (this bullet originally read "not decided here," and the spec's first build shipped generic
  placeholder text rather than deciding it inline — see
  [ticket 0043](../ClaudeWiki/Processes/Handoffs/0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md)).
  **Ruled:** not `RoomChallenge`/`GridRoomData` themselves — two small, purpose-built facts,
  `RoomChallengePrompt`/`GameEndAnnouncement`, carrying already-*resolved* text (never a key),
  seeded once by `Program.cs` alongside the content facts that already are seeded today
  (`GridRoomData`, per `D20SystemRuleGroup.ChallengeRule`'s pre-split `Match<Room>` precedent).
  Restores the pre-split `NineRooms.Room.ChallengePrompt` pattern rather than inventing one — see
  [Mechanics / rules](#mechanics--rules) above for the concrete shapes. General answer for any
  future case: the fact a generated rule matches always carries resolved text, never a key: the
  flavor dictionary itself never needs a seam into generated code.
- **NineRooms** — confirmed untouched by this spec; it keeps its own `PlayerPosition`/`RoomSolved`/
  `WinMarker` facts, duplicated rather than shared, exactly as it does today against D20System.

## Related documents

- [`1-d20/1-d20-spec.md`](1-d20/1-d20-spec.md) — the shipped content this spec splits; reconciled
  afterward, not superseded
- [`1-d20/1-d20-flavor.md`](1-d20/1-d20-flavor.md) — flavor keys, unchanged, now split in ownership
  between `GridRoomData.DescriptionKey` and `RoomChallenge`'s prompt/success/failure keys
- [`cross-rom-capability-spec.md`](cross-rom-capability-spec.md) — the authoring contract (§3) this
  spec builds against, and §4 (behaviour across the boundary — no broker, constructor injection
  only) that this spec's `IPlayerMovement` wiring follows
- [`../design/cartridge.md`](../design/cartridge.md) — the published-unit design this spec's
  contract-surface framing follows; its own worked example is this exact split
- [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md) — the phase this spec's content belongs to
- [`../Roadmap/phase-2-6-5.md`](../Roadmap/phase-2-6-5.md) — `IRom`/`IInteropRom`/`Capability`/
  `IHasDependencies`, shipped and load-bearing for this spec
- [`../design/engine-core.md`](../design/engine-core.md) — companion design doc
- [`../design/glossary.md`](../design/glossary.md) — `Capability`, `IRom`, `IInteropRom`, `EngineTier`
  definitions used throughout this spec
- [Ticket 0034](../ClaudeWiki/Processes/Handoffs/0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md) —
  the handoff this spec answers
- [Ticket 0036](../ClaudeWiki/Processes/Handoffs/0036-grid-rom-d20system-project-layout-decision.md) —
  superseded in part (the `D20SystemInterop`-as-ROM framing); its project-location ruling for
  GridMap/D20System in `example/` stands
- [Ticket 0041](../ClaudeWiki/Processes/Handoffs/0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md) —
  the correction this revision applies
- [Ticket 0045](../ClaudeWiki/Processes/Handoffs/0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md) —
  the missing-guard defect that surfaced `IGameWon`, the fifth Cartridge contract; this doc's
  2026-07-29 amendment brings it into agreement with what shipped
- [Ticket 0043](../ClaudeWiki/Processes/Handoffs/0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md) —
  the flavor-text-seam ruling this doc's 2026-07-30 amendment describes; not yet built as of that
  amendment
- [Specs README](README.md)
