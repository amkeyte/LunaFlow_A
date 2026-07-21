# Phase 2.7 — Multi-ROM composition

**Status:** pending

## Overview

Phase 2.6 fortifies the ROM/Rulebook boundary with exactly one ROM. `architecture.md` has claimed
pluggability since before any of this was real — "a Rulebook depends on whichever ROM modules it
needs, not one fixed model" — but that claim has never been tested against more than one. This phase
is the first real test: prove a single Rulebook can depend on and coordinate two independently-loaded
ROMs at once.

**Which specific ROMs split apart is deliberately not named here.** That's content-design territory —
Juno's call once she's designing against this phase's real content, not something architecture
prescribes ahead of it. This doc states the mechanism the split has to support, not the split itself.

The design conversation that shaped this phase (2026-07-20) surfaced the mechanism: cross-ROM
interaction should happen entirely through facts and declarative guards, never through one ROM
calling or referencing another directly. A ROM inserts a fact because that's its own concern; another
ROM's rule or handler guards against that fact's presence because that's *its* own concern. Neither
needs to know the other exists. This is the existing Marker/`[StandingRule]` pattern
(`ExitGameMarker`, `.Not<T>()` auto-injection) generalized from same-ROM rule-suppression to the
general answer for cross-ROM coordination.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md).
- Which ROMs actually split apart, and what content exercises the split: [Game Designer
  (Juno)](../ClaudeWiki/Roles/game-designer.md), against real content — same relationship 2.6 has to
  1.d20, not architecture inventing an example in advance.

## Deliverables

- **A shared, ROM-agnostic marker vocabulary in `LunaFlow.Rules`** (alongside `IHasLunaFlowReference`,
  `IMechanic`) that any ROM can produce (insert a fact) or consume (guard against it in a rule or
  handler) without referencing the other ROM's assembly. Start narrow — a small, concrete family of
  markers proven against real cross-ROM cases as they appear, not a fully general "any fact can
  suppress any action" system on the first pass.
- **`IRuleBook` grows a declared list of which ROM(s) it depends on** (extends 2.6's minimal shape).
  Loading validates every declared ROM is actually present and errors out clearly on a mismatch —
  fail fast, not silent degradation.
- **Two real, independently-loadable ROMs**, proven by one Rulebook depending on and coordinating
  both, with their only interaction being fact/marker-mediated (no project reference between them).
- **Retrofit Nine Rooms onto whichever navigation-shaped ROM this phase produces**, so the split has
  two real consumers proving it, not one content module divided on spec. (Without this, the split is
  tidiness, not fortification — same reasoning 2.6 applied to itself.)
- **Keep each ROM's actually-consumed public surface small and deliberate** — same instinct as
  `IEngineHost`/`IMechanic` staying thin. A narrow surface breaks less often, independent of whatever
  versioning eventually sits on top of it.

**Explicitly deferred, with a named trigger condition:**

- **Full `AEngineComponent`/`EngineTier`/`DependsOn`-ordering machinery.** Nothing in this phase's
  actual content needs startup or lifecycle ordering between ROMs — only fact-level interaction and
  load-time presence-checking, both much lighter. Build the heavier model only once a concrete case
  needs one ROM initialized before another (a Combat ROM genuinely depending on a Physics ROM, per
  `engine-core.md`'s own original example) — still hypothetical, not proven by this phase.
- **ROM versioning/compatibility.** No independent-development trigger exists yet — one team, one
  solution, plain project references; a breaking change surfaces as a same-repo compile error, not a
  version-mismatch problem. When ROMs and Rulebooks are actually maintained independently (different
  people, different release timing) or a second Rulebook needs to pin an older ROM version, the answer
  is standard NuGet/SemVer versioning, using `nbgv` (already wired into this solution) — not a
  bespoke system. Considered and explicitly parked, not forgotten.

## Done when

One Rulebook genuinely depends on and coordinates two independently-loadable ROMs. Neither ROM
references the other's assembly; their interaction is entirely fact/marker-mediated; removing either
ROM from the load list produces a clear, immediate error rather than a silent failure or a partial
game. Nine Rooms runs against the shared navigation-shaped ROM alongside whatever new content proves
the split.

## Related phases

- Previous: [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md)
- Next: [Phase 2.8 — World Module extraction](phase-2-8.md)

## Related documents

- [`design/architecture.md`](../design/architecture.md) — the pluggability claim this phase tests
  for the first time
- [`design/engine-core.md`](../design/engine-core.md) — the Marker/Standing-Rule pattern this
  generalizes; the "Two interfaces, not one" / component-model section this phase deliberately
  doesn't build
- [`design/non-goals.md`](../design/non-goals.md) — why full dependency-ordering and ROM versioning
  both stay out here
- [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md) — the `IRuleBook` shape and
  boundary this phase extends
