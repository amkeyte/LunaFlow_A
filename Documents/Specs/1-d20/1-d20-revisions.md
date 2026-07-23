# 1.d20 — Revision History

**Status:** reference only — not required reading to build against
[`1-d20-spec.md`](1-d20-spec.md)'s current sections. Split out per Arryn's ask (2026-07-22) so Baraka
can read the spec end-to-end without wading through playtest-bug history to get to what to build.
Read this only when you need the *why* behind a specific spec decision, not to implement it.

**Companion to:** [`1-d20-spec.md`](1-d20-spec.md) — the dated log of every material change made to
that spec, oldest first.

## 2026-07-20 — Amara's review

Conflated declarative rules with command handlers — `check`/`fight`/`flee` are handler-shaped
(branching, multi-fact mutation, early returns), not rule-shaped. Fixed: [Mechanics /
rules](1-d20-spec.md#mechanics--rules) splits declarative rules from command handlers explicitly; the
removed `MissionFailedRule` turned out to be redundant once framed correctly.

## 2026-07-20 — playtest pass, after ticket 0016's build

Arryn ran the actual console build and surfaced four real content-spec gaps, confirmed against
`OneD20RuleGroup`/`FightCommandHandler`/`CheckCommandHandler` source (pre-Phase-2.6 namespace — see
[1-d20-spec.md](1-d20-spec.md)'s Framework-dependencies section for the current one), not guessed:

1. **Double-announced room text** — `MoveCommandHandler` and the challenge-prompt rule both
   announced the same `DescriptionKey`. Nine Rooms never hit this because it has two distinct fields
   (`Description` always, `ChallengePrompt` only while unsolved); this spec had collapsed them into
   one. Fixed: `Room` gains a `PromptKey`, separate from `DescriptionKey` — see [Fact / marker
   vocabulary](1-d20-spec.md#fact--marker-vocabulary).
2. **Silent `fight` rounds** — only 3 of the (many) possible outcomes ever called `Announce()`; an
   inconclusive round produced no output at all, reading as broken. Fixed: every roll narrates.
3. **No HP visibility, anywhere** — neither combatant's HP was ever surfaced. Fixed: combat narration
   states remaining HP, and `look` reports it while a fight is active.
4. **Generic `check` messages** — one shared success/failure pair across all 6 different skill
   checks. Fixed: each room gets its own pair, plus a mechanical roll/DC readout.

One more, found by design review rather than reported directly: **PC HP never recovered between
fights**, including after a `flee` — meaning a session's difficulty silently ratcheted with no way to
see it coming, and losing a fight was a full session-ending `PlayerDown` state with no way to continue
toward a different win line. Redesigned: `CombatRound` now seeds the PC's HP fresh (same moment it
seeds the monster's), matching `flee`'s existing "fresh fight" language, and losing a fight now
inserts `RoomFailed` — same as the 3-round timeout — instead of ending the whole session.
`PlayerDown` is removed. This also resolves an asymmetry that was never a deliberate choice: two of
combat's three outcomes already only failed the room; the third shouldn't have been different by
accident.

Ticket 0016's build matched this spec's *previous* revision faithfully — Baraka disclosed the
double-announce as a cosmetic quirk rather than silently patching around it, which is exactly right
and is how this got caught. [Ticket 0018](../../ClaudeWiki/Processes/Handoffs/0018-recode-1-d20-per-revised-spec.md)
asked for a recode against this revision; the next entry is what 0018's build applied on top of.

## 2026-07-20 — second playtest, after 0018's build

Two more, both confirmed against `FightCommandHandler.cs`/`Program.cs` before concluding they were
spec bugs, not execution bugs:

1. **A lethal hit narrated its own contradiction** — "he staggers but he's still on his feet" (the
   `pc-hit` line) immediately followed by "he drops the rapier and doesn't get up"
   (`monster-defeated`), because this spec's steps 2/3 said "announce the hit, then separately check
   if it was lethal" — a literal read always announces `pc-hit` before the death check runs. Fixed:
   lethality is now checked *before* choosing which line to announce, both directions (`pc-hit`/
   `monster-defeated` and `monster-hit`/`pc-lost`).
2. **Room 5 never announced its `DescriptionKey` at startup** — a side effect of the previous
   revision's own fix. The old (buggy) challenge-prompt rule used to announce `DescriptionKey` too,
   so it accidentally showed at startup via the initial-settle `Fire()`; correctly narrowing that rule
   to `PromptKey`-only removed the only thing that had been showing it, since `MoveCommandHandler` —
   the sole remaining `DescriptionKey` announcer — never runs for a seeded starting position. Fixed:
   startup now explicitly announces it once.

## 2026-07-22 — reconciled against the shipped Phase 2.6 split (ticket 0026)

Tier/path framing brought into agreement with the shipped, QA-passed Phase 2.6 ROM/Rulebook boundary
retrofit ([ticket 0022](../../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md)) —
reconcile-to-reality, not a redesign; fact shapes were unchanged by 2.6 and stay unchanged here. All
five command handlers and the declarative rule group are now stated as ROM-tier, living in
`LunaFlow.Rom.D20System` (`Handlers/`/`Rules/`), replacing the flat `LunaFlow.Rom.OneD20` namespace
this spec previously pointed at. `IRuleBook`'s pending move to `LunaFlow.Rulebook` and
`D20SystemContent.Configure(...)`'s deprecation (both Phase 2.6.5) are flagged as transitional in the
spec, not pinned as current fact. Full detail: [ticket
0026](../../ClaudeWiki/Processes/Handoffs/0026-reconcile-1-d20-spec.md).

## 2026-07-22 — revision history split out (this doc)

This file created; the revision-note block above was previously inline at the top of
`1-d20-spec.md`, read before the Purpose section on every pass. Moved out at Arryn's request so the
main spec reads as current state, not an accumulating changelog — same instinct as the flavor-text
split (`content-spec-template.md`), applied to history instead of prose.

## 2026-07-22 — status corrected to `active`

`1-d20-spec.md`'s (and `1-d20-flavor.md`'s) `**Status:**` header still read `in review`, unchanged
since before the second playtest — stale by the content-spec-template's own bar
("`active` once Baraka has implemented against it and QA has verified the acceptance criteria
pass"). Both conditions were already met, independently, before this session: [Phase 2.5
verdict](../../QA/phase-2-5-verdict.md) (2026-07-20) PASS'd all 17 acceptance criteria against real
automated coverage, and [Phase 2.6 verdict](../../QA/phase-2-6-verdict.md) (2026-07-22) PASS'd the
structural retrofit with behavior confirmed unchanged. Nobody had gone back to flip the header.
Corrected both files' `Status:` lines to `active`, citing both verdicts. No new implementation or
QA work triggered this — a bookkeeping catch-up, not a state change.

## Related documents

- [`1-d20-spec.md`](1-d20-spec.md) — the spec this history belongs to
- [`../../design/1-d20.md`](../../design/1-d20.md) — concept doc
- [Ticket 0016](../../ClaudeWiki/Processes/Handoffs/0016-implement-1-d20.md) — first build, against
  the pre-playtest revision
- [Ticket 0018](../../ClaudeWiki/Processes/Handoffs/0018-recode-1-d20-per-revised-spec.md) — recode
  against the first playtest revision
- [Ticket 0022](../../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md) — Phase 2.6's
  ROM/Rulebook boundary retrofit
- [Ticket 0026](../../ClaudeWiki/Processes/Handoffs/0026-reconcile-1-d20-spec.md) — the 2026-07-22
  reconcile
