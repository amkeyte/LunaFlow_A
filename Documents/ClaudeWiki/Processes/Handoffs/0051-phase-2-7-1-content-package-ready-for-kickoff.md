# 0051 — Phase 2.7.1 content package ready for kickoff

**Status:** resolved
**Opened by:** Juno (Game Designer)
**Opened:** 2026-07-30
**For:** Kofi (PM)

## Decision

Package confirmed internally consistent and ready to build: item/monster/skill-check naming lines
up across spec and flavor, room-description keys stay untouched, and the "second monster/ranged
option" claim checks out against `1-d20.md` itself. `phase-2-7-1.md` wired into `roadmap.md` between
2.7 and 2.8 — no new Standing Constraint needed, matching Juno's own note. Build ticket cut to
Baraka as [0052](0052-implement-loot-rom-content-expansion-spec-md.md).

## Details

### Context

Phase 2.7 is QA-PASS'd and done. Per Arryn's ask, this was a content-only design pass to find
additions that exercise the shipped Cartridge mechanism further without touching the framework. The
resulting package is three documents:

- [`../../../Roadmap/phase-2-7-1.md`](../../../Roadmap/phase-2-7-1.md) — the phase doc. States up
  front that this is content-only: a third peer ROM (Loot) inside the existing D20 Cartridge, the
  first capability with two independent requirers (`IChallengeSolved`, consumed by both GridMap's
  win-line rules and Loot's new award rule), and the first pure-sink ROM (`Requires` only, no
  `Provides`). Explicitly rules out multi-Rulebook support — that's real, deferred, future work, not
  this phase.
- [`../../Specs/loot-rom-content-expansion-spec.md`](../../Specs/loot-rom-content-expansion-spec.md)
  — the buildable spec. Five monster templates (duelist/brawler/marksman/enforcer/scrapper)
  replacing the single fixed monster, three new skill checks (9-pool, 6 drawn per session), a new
  `example/D20SystemExampleD` project for Loot, `RoomChallenge` growing a nullable `MonsterId`
  field, a static `MonsterLootTable` (1:1, deterministic, learnable) plus session-derived
  `LootAssignment` facts, one generic declarative rule (`IChallengeSolved` + `LootAssignment` →
  `ItemAwarded`), and a new `inventory`/`inv` command. Session-start randomization (which 6 of 9
  skill-checks, which 3 of 5 monsters fill the fixed room-type slots) is specced as Rulebook-tier
  content-assembly code invoked by `Program.cs`, not living inside it — per Arryn's standing note in
  `Program.cs` against content code depending on the composition root's shape.
- [`../../Specs/loot-rom-content-expansion-flavor.md`](../../Specs/loot-rom-content-expansion-flavor.md)
  — the prose companion. Prompt/success/failure keys move from room-indexed to challenge-indexed
  (a challenge can now land on any room of its type), and per Arryn's direction, all challenge and
  monster text is written deliberately generic so it holds up regardless of which room it lands in
  this session. Room *description* keys are untouched. Also catches and fixes a latent bug: the
  shipped `monster-defeated`/`monster-hit` lines named "the rapier" specifically — reworded to be
  weapon-agnostic since only one of five monsters carries a rapier.

Confirmed before filing: no project-naming collision (`example/` currently holds only
`D20SystemExampleA/B/C`, so `D20SystemExampleD` is free), and no open questions remain in the spec
that would block a build (the room-vs-challenge flavor question was the last one, resolved above).

### Ask

Package review and roadmap wiring:

1. Confirm the three linked docs read as internally consistent and ready to build against.
2. Wire `phase-2-7-1.md` into `roadmap.md`'s phase-sequence table (Standing Constraints shouldn't
   need a new entry — this phase adds no new constraint, but worth double-checking).
3. Cut an implementation ticket to Baraka against the spec, mirroring the 0034 → CodePlan 0037
   pattern Phase 2.7 used.

(Housekeeping note from an earlier draft of this ticket retracted: I'd flagged
`grid-rom-d20-split-spec.md` as un-reconciled against shipped reality. Rechecked — it's already
current, amended 2026-07-29 for `IGameWon` (ticket 0045) and 2026-07-30 for the
`RoomChallengePrompt`/`GameEndAnnouncement` flavor-text seam (ticket 0043). No drift, nothing to
do here.)

Separately, and not for action now: Arryn raised a Nadia-testable determinism/test-mode idea, then
deferred it to a hypothetical Phase 2.7.2. Worth a placeholder note if useful, but not part of this
package.

### Response

<Filled in by the assignee.>

### Resolution

**Resolved 2026-07-30 (Kofi, opener).** Read all three docs directly, not just this ticket's
summary: cross-checked monster/item/skill-check ids and keys between
`loot-rom-content-expansion-spec.md` and its flavor companion, confirmed `example/D20SystemExampleD`
is free (only A/B/C exist on disk), and spot-checked `1-d20.md`'s "second monster type"/"ranged
option" anticipation claim against source (§ on the fixed monster and the mechanic-generality note —
both hold). `roadmap.md`'s phase-sequence table now carries Phase 2.7.1 between 2.7 and 2.8; no
Standing Constraint added, matching Juno's own double-check. Implementation ticket cut:
[0052](0052-implement-loot-rom-content-expansion-spec-md.md), to Baraka.
