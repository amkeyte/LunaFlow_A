# Phase 2.10 — Content coverage stress test

**Status:** pending. Built per [`Specs/phase-2-10-content-coverage-spec.md`](../Specs/phase-2-10-content-coverage-spec.md)
against the reviewed proposal in
[`Proposals/2-10-through-2-12-content-framework-stress-test.md`](Proposals/2-10-through-2-12-content-framework-stress-test.md),
approved via ticket [0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md).

## Overview

The first of three deliberate stress-test phases on what Phases 2.5–2.9 shipped. This one presses on
1.d20's content/fact vocabulary specifically — five items chosen because each exercises a fact shape
that's already in the codebase but never actually used: a saving-throw check kind alongside the
existing skill-check kind, a second weapon that isn't just a reskinned melee stat block, a status
marker that has to survive a room-transition boundary, an item with a real mechanical effect instead
of a display-only inventory line, and a deliberately engineered soft-lock that turns a flagged open
question into a demonstrated, transcript-able finding.

**Explicitly not in scope:** any new framework capability (`ComponentRegistry`/`Capability`/
`EngineTier` untouched), a new room or a change to the grid's 9-position topology, and soft-lock
*detection* — item 5 proves the gap is reachable, it doesn't close it. If implementation finds a real
framework need, that's a ticket to the Architect, same toolbox-first discipline every content-only
phase since 2.7.1 has held to.

## Ownership

- Content design: [Game Designer (Juno)](../ClaudeWiki/Roles/game-designer.md) — spec already
  drafted and reviewed (ticket 0067).
- Implementation: [Dev (Baraka)](../ClaudeWiki/Roles/dev.md), against the spec.
- No framework/boundary work anticipated ([Architect (Amara)](../ClaudeWiki/Roles/architect.md)) —
  reviewed the proposal and flagged item 3 (marker crossing a room boundary) as relevant to the
  still-draft `gm-overlay.md`, not as framework work this phase owes.

## Deliverables

- A **Save**-kind `RoomChallenge`, alongside the existing **Skill** kind, resolved against a new
  `SavingThrows` record derived Rulebook-side from data already supplied — no new World Module field.
- A mechanical profile for the already-named **Backup Piece** item: an `equip` command (Loot-owned)
  that swaps the PC's attack resolution from the default sword to a Dex-keyed ranged weapon.
- A **Poisoned** marker, inflicted by one roster monster's hit, that persists across a room
  transition and applies a roll penalty until it expires — the load-bearing test of whether the
  Marker/`StandingRule` suppression pattern holds across a boundary it's never crossed before.
- `OneD20ChallengeTemplate`/`OneD20MonsterTemplate` (World Module tier) and
  `OneD20WorldModuleValidator` grow matching fields/checks so the new content is authorable from a
  World Module, not hardcoded ROM-tier data — same discipline Phase 2.8 established.
- A documented, reproducible input sequence that exhausts every win line via `RoomFailed`, captured
  as a transcript showing the session continues silently with zero reachable win lines.

## Done when

- All five acceptance-criteria groups in `phase-2-10-content-coverage-spec.md` are met, verified
  end to end (World Module → ROM-tier translation → session behavior), not just at the fact-shape
  level.
- No existing acceptance criterion from `1-d20-spec.md` or `loot-rom-content-expansion-spec.md`
  regresses.
- Full suite green, read from an Arryn-run `latest.trx`.
- No new Cartridge-tier or `ComponentRegistry`-tier framework capability was needed to ship this. If
  one was, it's ticketed to the Architect and this phase's Done-When doesn't quietly close around a
  workaround.

## Related phases

- Previous: [Phase 2.9 — Lua World Module authoring](phase-2-9.md)
- Sibling (same proposal, independent scope): Phase 2.11 (cross-ROM capability mechanism stress
  test) and Phase 2.12 (Lua World Module authoring-surface stress test) — both still proposal-stage,
  not yet ratified into this sequence table.
- Next: none scheduled against this phase specifically; Phase 4 kickoff should read item 3's result
  before finalizing `gm-overlay.md`'s marker-plus-guard assumption, per ticket 0067's ruling.

## Related documents

- [`Specs/phase-2-10-content-coverage-spec.md`](../Specs/phase-2-10-content-coverage-spec.md) — the
  buildable spec
- [`Proposals/2-10-through-2-12-content-framework-stress-test.md`](Proposals/2-10-through-2-12-content-framework-stress-test.md) —
  the reviewed proposal this phase is drawn from
- [Ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) —
  the proposal's review/approval
- [`design/1-d20.md`](../design/1-d20.md) — names Fortitude/Reflex/Will and a ranged option as
  anticipated future content, both realized here
- [`design/gm-overlay.md`](../design/gm-overlay.md) — the still-draft Phase 4 tier item 3's result
  speaks to
