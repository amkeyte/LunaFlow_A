# 0036 — Grid ROM / D20System project-layout decision

**Status:** resolved
**Opened by:** Juno (Game Designer)
**Opened:** 2026-07-26
**For:** Amara (Architect)

## Decision

**Ruled directly by Arryn, not routed through Amara's review** — disclosed as a direct exception to
the normal assignee-responds flow, same spirit as the project's other disclosed-exception calls
(e.g. phase-2-6-5's QA-verdict exception). **Three real projects, not two.** `GridMap` and
`D20System` (the corrected name — "GridRom" was my placeholder) are separate assemblies with zero
reference between them. A third project, `LunaFlow.Rom.D20SystemInterop`, is itself a registered
`IRom` implementing `IInteropRom` — it holds the four shared contract interfaces and performs the
actual negotiation between the two peers, rather than being a passive, logic-free contracts library.
Both peers reference it downward (governed, per phase-2-7.md); it references neither peer, since
`IInteropRom.Resolve` deals in `Capability`/`object`, never a peer's concrete type. General
principle stated alongside the ruling: don't ceremonially argue over which ROM plays which role —
`IRom` identity/registration is a given per module, not a per-split design question.

## Details

### Context

Filed against [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md), the content
spec answering [ticket 0034](0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md).
That spec names the capability split (GridRom provides `PlayerLocation`/`PlayerMovement`, D20System
provides `ChallengeSolved`/`CombatStatus`, each requires the other's) and is ready to build against
regardless of this ticket's answer. But `phase-2-7.md`'s Done-When bar — "neither ROM references
the other's assembly," verified "against the assembly graph, not merely by a passing test"
(`cross-rom-capability-spec.md`'s acceptance criterion 1) — only means something if GridRom and
D20System are actually separate assemblies. Today they're both namespaces inside the single
`src/LunaFlow.Rom` project, same as NineRooms; a namespace convention isn't an assembly boundary,
and nothing currently stops one from referencing the other's types directly.

`cross-rom-capability-spec.md` §7 names this explicitly as open and content-shaped: "Where the
interop assembly lives — a third ROM-tier project, or an existing ROM designated as broker...
depends on what your split produces." The split spec now exists; per ticket 0034's Ask #4, that
makes this mine to surface, not to decide (project files are `src/`, outside this role's
authority).

### Ask

Decide the physical project layout needed to make "zero project reference between GridRom and
D20System" a real, checkable property:

1. Do GridRom and D20System each need their own `.csproj` (real assembly separation), or is there a
   lighter-weight way to make the boundary enforceable/checkable that doesn't require splitting
   `LunaFlow.Rom` apart?
2. If they do split, where do the four interop contracts (`IPlayerLocation`, `IChallengeSolved`,
   `ICombatStatus`, `IPlayerMovement` — all named in the spec's Fact/marker vocabulary section, all
   deliberately minimal, no logic) live? A new shared project, or one of the two ROMs designated
   broker-adjacent? The split is symmetric — each ROM both provides and requires a capability the
   other owns — so neither GridRom nor D20System reads as a more natural home than the other; that
   symmetry itself might be the useful data point for this decision.
3. Where does NineRooms land if `LunaFlow.Rom` stops being a single project? (Likely: stays where it
   is, deprecated/regression-only, per phase-2-7.md's struck retrofit bullet — but worth confirming
   rather than assuming.)
4. Who registers as `IInteropRom` — GridRom, D20System, or a third, content-agnostic broker
   component? The spec's capability list doesn't presume an answer.

None of this blocks Baraka from starting on the spec's fact shapes and handler logic in whatever
the current project structure is; it blocks the "zero project reference, verified against the
assembly graph" acceptance criteria specifically.

### Response

**[Arryn, direct]** Added three example projects under `example/` to demonstrate the convention,
rather than answering in prose:

- `D20SystemExampleA` → namespace `LunaFlow.Rom.D20System` (trimmed content ROM, own `.csproj`).
- `D20SystemExampleB` → namespace `LunaFlow.Rom.GridMap` (the spatial ROM's real name — not
  "GridRom"). Own `.csproj`.
- `D20SystemExampleC` → namespace `LunaFlow.Rom.D20SystemInterop` (own `.csproj`). **This is itself
  a ROM implementing `IInteropRom`** — not a neutral, logic-free contracts library. It's where the
  negotiation between GridMap and D20System actually happens.

Namespace folders stay `LunaFlow.Rom.<Module>` regardless of which physical project they live in —
the physical `.csproj` boundary and the conceptual "ROM tier" namespace aren't the same axis.
Rulebooks will follow the same convention when their turn comes.

Clarified on follow-up: in general practice, shared interop contracts could just live in the
existing base `LunaFlow.Rom` project (both peers already reference it for `IRom`/`IInteropRom`, so
it wouldn't be a new dependency) — Arryn "wouldn't typically" spin up a dedicated third project for
this. It's a genuine third project here specifically because Phase 2.7 is proving the zero-reference
property for real, and a dedicated project removes any ambiguity about whether a shared-base
reference secretly is a peer-to-peer one in disguise.

Concrete implication for the spec: `PlayerPosition : IPlayerLocation`, `RoomSolved :
IChallengeSolved`, and `CombatRound : ICombatStatus` (the three fact-shaped contracts) are
implemented directly by each ROM's own fact type, each ROM referencing `D20SystemInterop` downward
for the interface only. `IPlayerMovement` (the one action-shaped contract) is implemented by a small
adapter class living inside GridMap's own project (it needs direct access to GridMap's own
`PlayerPosition`/`PreviousRoomId` to do the retract/insert — same "only the owner mutates its own
facts" rule, just realized as a class instead of a rule). `D20SystemInterop` itself never references
either peer; it deals only in `Capability`/`object`, consistent with `IInteropRom.Resolve`'s
signature.

### Resolution

Confirmed against the example scaffold (`example/D20SystemExampleA`/`B`/`C`) plus Arryn's direct
walkthrough above. [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) updated to
match: the ROM renamed `GridRom` → `GridMap`, and the Framework dependencies/Fact vocabulary
sections rewritten around the real three-project layout (`D20System`, `GridMap`,
`D20SystemInterop`) instead of the placeholder two-project guess this ticket opened with. No further
action needed from Amara — Arryn's ruling stands as the answer.
