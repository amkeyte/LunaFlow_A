# 0040 — Pivot to 2.6.5 and re-sequence Phase 2.7

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-07-27
**For:** Kofi (PM)

## Decision

**Operational companion to [0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md).**
0039 records *what was decided and why* (Cartridge; push-only; `Resolve` cut) and will be referenced
long after it closes. This ticket is *what to unwind and in what order*, and closes once executed —
different concern, different lifespan, hence a separate number rather than a third addendum. **Phase
2.6.5 remains the last verified-coherent state and nothing in it is disturbed;** everything built on
top of it during 2.7 rests on assumptions 0039 superseded. No git rollback — the code involved is
cheap to redo and the lessons live in tickets and design docs, which a rollback would not touch.

## Details

### Context

Read [0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md) first for the reasoning;
this ticket assumes it and does not restate it. Arryn's words for the intent: *"back ourselves out to
2.6.5 complete and pivot from that point forward."*

**Nothing about Phase 2.6.5 is in question.** `IRom`, `Capability`, `IHasDependencies`, `EngineTier`
and `ComponentRegistry` — including the cycle check and the name-uniqueness guard added in
[0031](0031-code-currency-vs-2-6-5-design.md) — all survive untouched, along with the 119/119 suite.
Everything load-bearing sits *below* the line being redrawn.

Not a git rollback, deliberately. The affected work is a codeplan, three scaffold projects and a test
harness — cheap to redo. What's expensive is the reasoning, and that lives in tickets and design docs
that a rollback wouldn't touch.

### State of play

**Survives, do not disturb:**

- All Phase 2.6.5 framework code and its tests.
- [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §2 (NRules interface
  matching, verified against source at the pinned v1.0.2 tag) and §3 (the authoring contract — how a
  crossing fact is declared and matched). Both survive the Cartridge change unaltered.
- [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md)'s **fact shapes and
  content**. Only its framing of the interop project is affected.

**Superseded by 0039:**

- `D20SystemInterop` as a **ROM** implementing `IInteropRom` — it is a Cartridge contract surface, not
  a peer. This reverses [0036](0036-grid-rom-d20system-project-layout-decision.md), which was
  **Arryn's own ruling**, and needs his explicit word rather than my inference. *(Arryn's stated
  context: 0036 was written to stop Baraka re-inventing the assignment, and doesn't conflict now that
  a genuine conceptual gap has been named — but it should be recorded as superseded by him, not by
  me.)*
- `IInteropRom.Resolve`/`Resolve<T>` — cut entirely, never built.
- [CodePlan 0037](../../../Roadmap/CodePlans/0037-gridmap-d20system-split.md) Phase B, and its
  registration of the interop project with `ComponentRegistry`.
- `LunaFlow.Rulebook`'s project references to `LunaFlow.Rom` and `example/D20SystemExampleA`, plus
  `using LunaFlow.Rom.D20System;` in `OneD20RulebookContent.cs` — these violate the requirement that
  a Rulebook not bind to a ROM assembly.

**New since:**

- [`design/cartridge.md`](../../../design/cartridge.md) — the published unit, the trust rationale.
- The **bootstrap contract probe** (spec §5), added to `phase-2-7.md`'s Deliverables. Cheap, and
  worth having today: a stale build artefact or partial rebuild produces exactly the failure it
  catches.

### Ask

**Decisions and admin — yours:**

1. **Get Arryn's explicit word on 0036**, then record the supersession there (struck through with a
   pointer to 0039, per house convention). I did not touch it.
2. **Back out or amend [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md).**
   It is **open** and instructs Baraka to build `IInteropRom.Resolve` per `cross-rom-capability-spec.md`
   §4.1 — a section that now explains why `Resolve` was cut. Left alone deliberately: amending a live
   ticket mid-flight off a design change the assignee hasn't seen is worse than the stale pointer.
   This is the most time-sensitive item here.
3. **Reconcile [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md).** Nadia's
   tests block on ROM implementations living in `src/` rather than `example/`. That's a
   project-layout question 0036 settled and 0039 partly reopens — the interop project is no longer a
   ROM, so where the three projects live needs settling before her tests can compile. **Blocked on
   item 1**, not independently actionable.
4. **Add the requirement to the roadmap** (from 0039, restated because it's the one thing that
   outlives this pivot): *a Rulebook must not depend on a specific ROM assembly; a ROM must be
   replaceable without recompiling what sits above it.* Plausibly a standing constraint rather than a
   phase bullet.
5. **Register `cartridge.md`** wherever design docs are indexed.
6. **Judge whether 2.7 needs re-sequencing.** My read: it *shrinks* — `Resolve` unbuilt, one fewer
   ROM to model — but the Rulebook decoupling is real work nobody scoped. Your call, not mine.

**Sequencing — the part that saves rework.** Three roles have dependent work and doing it out of
order causes churn:

> **Juno → Baraka → Nadia.**
> Juno corrects `grid-rom-d20-split-spec.md`'s framing of the interop project (mechanism only; fact
> shapes and content are unaffected). Baraka replans against the corrected spec — Phase B deleted,
> interop as a plain contracts assembly, `FleeCommandHandler` taking `IPlayerMovement` directly,
> Rulebook references removed. Nadia's tests follow the settled layout.

Nadia should not restart until item 3 resolves, or she'll write against a layout that moves again.

**Not this ticket:** re-litigating the design. That's 0039, and it's settled unless Arryn reopens it.

### Response

All six items actioned, 2026-07-27. Confirmed directly with Arryn along the way rather than inferred:

1. **Arryn's word on 0036 — confirmed superseded.** Recorded on 0036 itself: struck through, pointer
   to 0039/0040, original preserved for the audit trail. Index row updated to match.
2. **0037 amended, not backed out — and flagged urgent.** Baraka was already mid-build on `Resolve`
   when this landed, which raised the stakes past a quiet edit. Ticket carries a stop-work banner and
   a corrected Ask: `Resolve` cut, the four interfaces staying in `example/D20SystemExampleC` (see
   same-day addendum below — briefly proposed as a new `src/LunaFlow.Cartridge` project, caught and
   reversed same day: they're content, not engine-agnostic infrastructure, same rule that already
   sent `GridMap`/`D20System` to `example/`) — plus `FleeCommandHandler` taking `IPlayerMovement`
   directly and the Rulebook reference removal. Flagged to Arryn that a ticket edit alone doesn't
   interrupt a live session — that message has to go to Baraka directly.
3. **0038 reconciled, not reopened.** Its location ruling (`GridMap`/`D20System` stay in `example/`,
   confirmed directly by Arryn, predates and is unaffected by this pivot) stands. Added a
   reconciliation note: its `StartSession()` wiring still treats `D20SystemInteropRom` as a registered
   `IRom`, which is superseded — tracked as follow-on work against the corrected 0037, not reopened
   here since the compile-blocker 0038 actually closed is still real and done.
4. **Roadmap requirement added.** New "Standing constraints" section on `roadmap.md` (a Rulebook must
   not depend on a specific ROM assembly), plus the concrete Deliverable/Done-When clauses on
   `phase-2-7.md` naming what has to come out of `LunaFlow.Rulebook`/`OneD20RulebookContent.cs`.
5. **`cartridge.md` registration — verified, not re-done.** Already cross-linked from `architecture.md`,
   `engine-core.md`, `non-goals.md`, and `glossary.md` per your own "Docs updated" list in 0039; not
   orphaned. No dedicated design-doc index exists to add it to beyond that.
6. **Re-sequencing: no.** Net scope reads as a wash — `Resolve`'s build comes out, retyping
   `example/D20SystemExampleC`'s interfaces (see addendum below — not a new project after all) and
   the Rulebook-decoupling work go in — smaller than the growth 0029 already caused this same phase
   to absorb without a boundary change. Ruled and recorded on `phase-2-7.md` directly: absorbed within
   2.7, no 2.7.5.

### Resolution

Closed 2026-07-27. Checked all six against the artifacts themselves, not just Kofi's Response text:

1. **0036** — struck through, superseded pointer to 0039/0040, original preserved beneath. Confirmed.
2. **0037** — carries the stop-work banner and the corrected Ask (`Resolve` deleted, `LunaFlow.Cartridge`
   project, `FleeCommandHandler` takes `IPlayerMovement` directly, Rulebook references removed).
   Confirmed.
3. **0038** — reconciliation note present, correctly not reopened; flags the `StartSession()`
   follow-on against corrected 0037 rather than restating the compile-blocker as unresolved.
   Confirmed.
4. **`roadmap.md`** — new "Standing constraints" section states the Rulebook/ROM-assembly rule with a
   pointer to `cartridge.md`/0039. `phase-2-7.md` carries matching Deliverable and Done-When clauses
   naming what comes out of `LunaFlow.Rulebook`/`OneD20RulebookContent.cs`. Confirmed.
5. **`cartridge.md`** — cross-linked from `architecture.md`, `engine-core.md`, `non-goals.md`, and
   `glossary.md`. Confirmed, no action needed.
6. **No re-sequencing** — `roadmap.md` still shows a single Phase 2.7 (no 2.7.5); `phase-2-7.md`
   records the scope wash and rules it absorbed. Confirmed.

Everything in 0039's design survives untouched, all six operational items land where Kofi's Response
says they do. Closing this ticket; 0039 itself is a separate close (Kofi's, since it's addressed to
him).

**Addendum, same day (Kofi) — location correction, not a design reversal.** Item 2's "new real
`LunaFlow.Cartridge` project (`src/`, alongside `Rom`/`Rulebook`)" was wrong on *where*, caught by
Arryn before Baraka built against it: the four interfaces are content (player/challenge/combat
vocabulary), not engine-agnostic infrastructure, so they follow the exact rule this same pivot
already applied to `GridMap`/`D20System` via ticket 0038 — content stays in `example/`. Corrected
shape: the interfaces stay in `example/D20SystemExampleC` (already its own assembly, which is all
`cartridge.md`'s invariant actually requires), retyped from `IRom`/`IInteropRom` to bare interfaces.
Nothing else in 0039's design changes — `Resolve` is still cut, the trust-boundary reasoning is
untouched, `GridMap`/`D20System` were never affected either way. Fixed on [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)
and [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md) directly; not
reopening this ticket for a location detail that doesn't touch its actual decisions.
