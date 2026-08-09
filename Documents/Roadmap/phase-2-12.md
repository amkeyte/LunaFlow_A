# Phase 2.12 — World Module authoring surface stress test (Lua path)

**Status:** pending. Built per [`Specs/phase-2-12-lua-worldmodule-stress-test-spec.md`](../Specs/phase-2-12-lua-worldmodule-stress-test-spec.md)
against the reviewed proposal in
[`Proposals/2-10-through-2-12-content-framework-stress-test.md`](Proposals/2-10-through-2-12-content-framework-stress-test.md),
approved via ticket [0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md).
Ratified via ticket [0081](../ClaudeWiki/Processes/Handoffs/0081-phase-2-12-spec-ready-for-ratification-read.md)
(Amara's review, including item 11's Mechanics-section fix on ticket
[0083](../ClaudeWiki/Processes/Handoffs/0083-phase-2-12-item-11-mechanics-section-misdescribes-enrage-s-defense-effect.md)).
Build ticket cut to Baraka as [0084](../ClaudeWiki/Processes/Handoffs/0084-implement-phase-2-12-lua-worldmodule-stress-test-spec-md.md).

## Overview

The third and last of the three deliberate stress-test phases on what Phases 2.5–2.9 shipped
(sibling to [2.10](phase-2-10.md) and [2.11](phase-2-11.md), same reviewed proposal). Where 2.11
pressure-tested the capability mechanism, this one pressure-tests the Lua World Module authoring
path 2.9 only proved narrowly — 2.9's own proof was a verbatim, diff-verified transcription of
existing C#-authored content, never a genuinely new scenario or a script doing real computation.

Four items: a new Lua-only World Module with no C# equivalent to diff against (item 9), a Lua script
that generates content procedurally instead of returning a flat table literal (item 10), a monster
mechanic that forces the first extension to `IOneD20WorldModule`'s fixed shape since Phase 2.8 —
Enrage, a threshold-triggered Defense boost (item 11), and a World Module deliberately authored to
fail validation through the Lua path specifically (item 12).

**Item 11 is this phase's real framework-adjacent item**, flagged since the original proposal: it's
the first case where 2.8's `IOneD20WorldModule` contract itself needs to grow, and per Amara's
standing ruling on ticket 0067, its result is a direct input to `design/gm-overlay.md`'s still-draft
"effect fact"/`MonsterBehaviorOverride` idea for Phase 4 — whoever runs Phase 4's kickoff pass should
read this item's outcome first, not treat it as just another content addition.

**Explicitly not in scope:** generalizing `IOneD20WorldModule` into an engine-wide `IWorldModule`
(`non-goals.md`'s standing deferral — one extended field on one Rulebook-owned contract doesn't
trigger it), and generalizing Enrage into a reusable "conditional stat modifier" shape past this one
concept (same "don't abstract past one real consumer" discipline).

## Ownership

- Content design: [Game Designer (Juno)](../ClaudeWiki/Roles/game-designer.md) — spec drafted and
  reviewed (tickets 0067, 0081, 0083).
- Implementation: [Dev (Baraka)](../ClaudeWiki/Roles/dev.md), against the spec.
- Framework/boundary sign-off: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — reviewed item
  11's contract extension directly against source (ticket 0081); no `ComponentRegistry`/`Capability`/
  `EngineTier` involvement, no ticket needed for the mechanism itself.

## Deliverables

- **Item 9:** "Signal Static," a new Lua-authored `IOneD20WorldModule` with no C# equivalent —
  win-via-combat-free-line, win-via-combat-line, and at least one `RoomFailed`, plus a Save-kind
  challenge and an `InflictsPoison` monster round-tripping through the Lua marshal path.
- **Item 10:** a procedurally-generated (loop/conditional, not a literal table) five-monster roster
  inside "Signal Static"'s Lua source, stats verified against the same base-plus-step arithmetic by
  hand.
- **Item 11:** `OneD20MonsterTemplate`/ROM-tier `MonsterTemplate` grow two nullable fields
  (`EnrageThresholdFraction`, `EnrageDefenseBonus`); a new generic D20System rule inserts
  `MonsterEnraged` once an HP threshold is crossed, boosting the monster's effective Defense —
  purely defensive, no effect on the monster's own attack roll (per ticket 0083's correction).
  `OneD20WorldModuleValidator` gains the pairing/range check; every existing monster (Phase 2.7.1's
  five, Hollow Keep's roster) keeps constructing and fighting correctly with both fields absent.
- **Item 12:** a World Module authored to fail at each of the Lua path's two stages — the marshal
  step and the existing `OneD20WorldModuleValidator` pool-size check — exercised through Lua
  specifically, not the C# path 2.9 already covered.

## Done when

- All twelve acceptance criteria in `phase-2-12-lua-worldmodule-stress-test-spec.md` are met:
  "Signal Static" plays through end to end with zero changes to `IOneD20WorldModule`/
  `OneD20WorldModuleValidator`/`OneD20SessionAssembler`; the procedural roster's stats match hand-
  computed values exactly; Enrage's Defense boost and its retraction at `CombatRound` end are both
  shown live; the Enrage fields are verified end to end from Lua source through to the ROM-tier fact
  the `fight` rule reads; both Lua-path failure probes (marshal-stage, validator-stage) are
  demonstrated distinguishable and correctly ordered.
- No existing acceptance criterion from `lua-world-module-spec.md`, `1-d20-spec.md`,
  `loot-rom-content-expansion-spec.md`, or `phase-2-10-content-coverage-spec.md` regresses.
- Full suite green, read from an Arryn-run `latest.trx`.
- Item 11's result is recorded somewhere Phase 4's kickoff pass will actually see it before that pass
  finalizes `gm-overlay.md`'s "effect fact" draft — per Amara's ruling on ticket 0067, this is a
  sequencing dependency, not a suggestion.

## Related phases

- Previous: [Phase 2.11 — Cross-ROM capability mechanism stress test](phase-2-11.md)
- Sibling (same proposal): [Phase 2.10 — Content coverage stress test](phase-2-10.md)
- Next: closes out the 2.10–2.12 proposal in full. Phase 4's kickoff design pass is the next real
  work after this, and is blocked on item 11's result per the dependency stated above.

## Related documents

- [`Specs/phase-2-12-lua-worldmodule-stress-test-spec.md`](../Specs/phase-2-12-lua-worldmodule-stress-test-spec.md) —
  the buildable spec
- [`Proposals/2-10-through-2-12-content-framework-stress-test.md`](Proposals/2-10-through-2-12-content-framework-stress-test.md) —
  the reviewed proposal this phase is drawn from
- [Ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) —
  the proposal's review/approval, including the Phase-4-sequencing ruling on item 11
- [Ticket 0081](../ClaudeWiki/Processes/Handoffs/0081-phase-2-12-spec-ready-for-ratification-read.md) —
  Amara's ratification-readiness review
- [Ticket 0083](../ClaudeWiki/Processes/Handoffs/0083-phase-2-12-item-11-mechanics-section-misdescribes-enrage-s-defense-effect.md) —
  the Mechanics-section defect found and fixed during that review
- [`design/world-module.md`](../design/world-module.md) — the two-authoring-paths design this phase
  stress-tests
- [`design/gm-overlay.md`](../design/gm-overlay.md) — the Phase 4 tier item 11's result feeds into
- [`design/non-goals.md`](../design/non-goals.md) — the `IWorldModule` generalization deferral this
  phase's item 11 deliberately does not trigger
