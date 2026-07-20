# Phase 2.5 — d20 vertical slice & mechanic infrastructure

**Status:** pending

## Overview

Supersedes the original Phase 3 concept (see [`phase-3.md`](phase-3.md), kept for reference, not
deleted). Where Phase 3 asked "does a generated rule fire live through a typed command," this
phase asks the same question through real, non-trivial content instead of a minimal example —
because proving the pipeline alone turned out not to be enough: the engine still has no working
game *mechanic*. `IMechanic` is currently an empty marker interface, and Phase 4 (scripting) can't
usefully script against a dice mechanic that doesn't exist yet.

Content: [`1.d20`](../design/1-d20.md) — a single-player, GM-less d20 Modern-derived Scenario
(the Department-7 mission) built on a new d20 System ROM. Same relationship Nine Rooms had to
Phase 1.5: Nine Rooms proved engine-core + content end-to-end before Phase 2; 1.d20 proves
pipeline + content + a real mechanic end-to-end before Phase 4, one layer up the stack.

**Load-bearing condition:** 1.d20's rules must be authored through `GeneratorServices`/
`RulePrototype`, not hand-authored `ARule` classes the way Nine Rooms was. That's what lets this
phase also close out Phase 3's original done-bar as a side effect, rather than leaving it
unproven. If that constraint doesn't hold at spec time, Phase 3 needs reviving separately — see
[Supersession note](#supersession-note).

## Ownership

- Content/Scenario design: [Game Designer (Juno)](../ClaudeWiki/Roles/game-designer.md) —
  `Documents/design/1-d20.md` and the buildable spec that follows it.
- Mechanic infrastructure (`IMechanic` gaining real members, dice-resolution execution): ticketed
  to the [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — framework work the content forces,
  not something Juno builds herself.

## Deliverables

- **d20 System ROM** — the System-tier list from `1-d20.md`: six ability scores + modifier
  formula, the universal `d20 + modifiers vs. target number` resolution, Defense, saving throws,
  skill checks, attack resolution, the single `Strong` archetype's math.
- **1.d20 Scenario content** — the 9-room Department-7 mission on the reused Nine Rooms grid, the
  pregenerated Strong-hero, the single `fight` action — authored as rules through the pipeline
  (see the load-bearing condition above).
- **`IMechanic` grows real members** to back skill-check and attack resolution as an actual
  Mechanic, not just a marker interface.

## Done when

Typing a command into the running console host resolves a real d20 mechanic (a skill check
against a DC, or the `fight` action against the monster) via a rule sourced from the generated
pipeline — not a hand-authored test rule, not a string match — and the result is observable
through the CLI.

## Supersession note

If 1.d20 ends up built by hand instead of through the pipeline (breaking the load-bearing
condition above), it no longer proves Phase 3's original bar, and Phase 3 should be revived as a
separate, smaller step rather than treated as closed by this phase. Not expected, but the door's
deliberately left open rather than deleting Phase 3's doc.

## Related phases

- Previous: [Phase 2 — Rule pipeline MVP](phase-2.md)
- Supersedes: [Phase 3 — Vertical slice](phase-3.md) (kept for reference; revivable)
- Next: [Phase 4 — World content & scripting](phase-4.md) — also gated on this phase's mechanic
  infrastructure, not just Phase 2's IR stability

## Related documents

- [`design/1-d20.md`](../design/1-d20.md) — the concept doc this phase builds
- [`Specs/1-d20/1-d20-spec.md`](../Specs/1-d20/1-d20-spec.md) — buildable spec, drafted (Juno) and
  reviewed (Amara); splits rule-vs-handler scope for the `GeneratorServices` condition explicitly
- [`Specs/1-d20/1-d20-flavor.md`](../Specs/1-d20/1-d20-flavor.md) — prose companion
- [`NineRooms/nine-rooms.md`](../NineRooms/nine-rooms.md) — the Phase 1.5 precedent this phase's
  shape is modeled on
