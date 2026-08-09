# 0017 — 1.d20 maintained test suite

**Status:** resolved
**Opened by:** Dev — Implementation (Baraka)
**Opened:** 2026-07-20
**For:** Dev — Test Suite (Nadia)

## Context

[Ticket 0016](0016-implement-1-d20.md) implemented 1.d20 (Phase 2.5) in full against
[`1-d20-spec.md`](../../../Specs/1-d20/1-d20-spec.md): the `LunaFlow.Rom.OneD20` module (facts,
content, `D20ResolutionMechanic`, hand-written `check`/`fight`/`flee`/`move`/`look` handlers, and
the declarative rules authored through `GeneratorServices` — `OneD20RuleGroup.cs`). Per
[`dev.md`](../../Roles/dev.md)'s territory split, the maintained automated suite in `tests/` is
your territory, not mine — I only ran throwaway manual smoke transcripts against the running
console host to verify the acceptance criteria by hand (documented in 0016's Response section),
the same disposable-verification tier Nine Rooms' engine-fixture tests were built to replace for
that content.

`1-d20-spec.md`'s own Acceptance criteria section (10 items) is the done-bar this suite should
target — it's written as a checklist of observable behaviors, not implementation detail, so it
should translate fairly directly into behavioral test cases the way ticket 0010's B1–B5 suite did
for the rule pipeline.

## Ask

Build the maintained automated coverage for 1.d20's acceptance criteria. Suggested shape (yours to
adjust — this is a starting point, not a spec):

- A fixture in the style of `NineRoomsEngineFixtureTests.cs`: build a `RuleEngine` directly from
  `RuleExport.Compile(typeof(OneD20RuleGroup))` plus `OneD20Content`'s seed facts, exercise rule
  firing/guards at the fact level (challenge-prompt guards, win-line firing, `RoomFailed` killing a
  line without a separate guard — see 0016's note on why that one falls out of the fact model).
- Handler-level tests for `check`/`fight`/`flee`/`move` — these are deterministic content logic
  (adjacency, guard checks, state transitions) except where `D20ResolutionMechanic.Roll`/
  `ResolveCheck` introduces real randomness (attack rolls, damage, skill checks). You'll need a way
  to make combat/check outcomes deterministic for a repeatable suite — an injectable RNG seam on
  `D20ResolutionMechanic`, or asserting on probability-bounded outcomes, or something else — that's
  a design call in your territory, not pinned down here.
- Given the acceptance criteria explicitly calls out probabilistic branches (a `fight` round that's
  lethal, a PC-reduced-to-0 round, the 3-round timeout, a failed vs. successful `check`), some
  seam for deterministic RNG in tests is probably unavoidable — flagging so it's not a surprise
  once you're in the code, not dictating the mechanism.

Not required: re-testing `move`/`look`'s reused adjacency logic from scratch — Nine Rooms already
covers that shape; a quick equivalence note (or a shared parity assertion, your call) is enough
unless 1.d20's grid or `PreviousRoomId` addition needs its own case.

## Redirect (Kofi, 2026-07-20)

Before you start: this ticket was filed against 0016's original build. [Ticket 0018](0018-recode-1-d20-per-revised-spec.md)
has since landed — two playtest-driven spec revisions, closed and verified — and changed the
shape of what you'd be testing:

- `D20ResolutionMechanic.ResolveCheck` returns a `CheckResult` struct (`D20`/`Modifier`/`Total`/
  `TargetNumber`/`Success`), not a bare `bool`.
- `PlayerHp`/`PlayerDown` are **gone**. `CombatRound` now carries `PlayerHp` alongside `MonsterHp`,
  seeded fresh every fight. Losing a fight inserts `RoomFailed`, same as the 3-round timeout —
  there's no more session-ending state to test for.
- `check`/`fight` no longer guard on `PlayerDown` (nothing left to guard against).
- `Room` gained `PromptKey`/`SuccessKey`/`FailureKey`; the challenge-prompt rule announces
  `PromptKey`, not `DescriptionKey`.
- Combat narrates every round now (hit/miss/damage/HP, both sides) — testable per-round, not just
  at the three terminal outcomes.

`1-d20-spec.md`'s Acceptance criteria section is revised to match — build against that version,
not whatever you may have looked at before this note. Everything else in the original ask below
(fixture-level rule/guard tests, the RNG-determinism design call, `move`/`look` parity being
optional) still holds.

## Response

Built against the revised (post-0018) shape, two tiers, `tests/LunaFlow.EngineTests/Rom/OneD20/`,
tagged `Rom_OneD20`/`Phase2.5`:

- **`OneD20RuleFixtureTests.cs`** (9 tests) — bare `RuleEngine` loaded from
  `RuleExport.Compile(typeof(OneD20RuleGroup))`, facts inserted directly (never through a handler),
  same shape as `NineRoomsEngineFixtureTests`. Covers challenge-prompt guards (fires when
  unsolved/unfailed, blocked by `RoomSolved`/`RoomFailed`/`WinMarker`), win-line firing (combat-free
  and combat-room-inclusive lines, at the fact level), `RoomFailed` permanently blocking a line with
  no extra guard while an unrelated line is unaffected, and `GameEndRule`'s announcement.
- **`OneD20HandlerTests.cs`** (16 tests) — the real `GameEngineCore` singleton +
  `DispatchGameCommand` pipeline (dispatch, then `Fire()`), since `LunaFlowEngine.Host` is
  hard-wired to the singleton and there's no way to unit-test a handler in isolation. First
  handler-level tests in the suite. Covers all 15 acceptance criteria: move/look parity plus
  `PreviousRoomId`'s own case, description-once/prompt-once, the room-5 startup announcement,
  lethal-hit ordering both directions, fresh-HP seeding with no carryover across `flee`, every-round
  narration, `look` mid-fight HP, room-specific `check` text + readout with retry-on-failure, `flee`
  cleanup, flee-room-5-no-prior-move, and both win-line shapes (combat-free and
  combat-inclusive).

**RNG:** ticket [0019](0019-d20-rng-test-seam.md)'s seam (`D20ResolutionMechanic(Random)` +
constructor-injected handlers) is used everywhere an exact outcome matters — the 4 criteria that
were blocked before that ticket landed (lethal-hit ordering both directions, monster-to-0, PC-to-0,
the 3-round timeout) are now forced deterministically instead of retry-looped. Seeds weren't
hand-computed — I wrote a throwaway search script against the real `D20ResolutionMechanic` class
(not a reimplementation) to find and verify them, then documented the roll breakdown for each as a
comment at its test. One incidental finding, reused throughout: a fresh `new Random(0)`'s first d20
roll is always 15, which comfortably clears every room's DC/Defense in this content — one seed
covers most of the "force a success" cases. One test (`Fight_EveryRound_AnnouncesSomething`)
deliberately uses real unseeded randomness, since criterion 8 holds for either outcome.

**Not duplicated:** `move`/`look`'s adjacency math itself isn't re-derived from scratch — the
handler tests spot-check parity (a valid move, a rejected edge move) rather than re-walking the
whole grid, per the ticket's own note that Nine Rooms already covers that shape.

**Caught in review:** an early draft had two `IsFalse` assertions checking for the flavor-text
*dictionary key* (e.g. `"pc-hit"`) inside announced strings — but the announced text is the mapped
prose, which never contains that literal key, so the assertions were vacuously true regardless of
whether the bug they were meant to catch existed. Fixed to check for the actual
`OneD20Flavor.Text[...]` substring before running anything.

**Verification:** `dotnet build LunaFlow_A.sln` — 0 errors, 0 warnings. Narrow filtered run
(`--filter "TestCategory=Rom_OneD20"`) — 25/25 green. Full `dotnet test tests/LunaFlow.EngineTests`
— 96/96 green (71 pre-existing + 25 new), no regressions.

## Resolution

Verified and closed by Baraka (opener), 2026-07-20. Not a rubber stamp — checked directly against
running source, not taken on the response's word:

- **Fresh, clean build:** `dotnet build LunaFlow_A.sln --no-incremental` — 0 errors, same 8
  pre-existing warnings, none new.
- **Fresh, full test run:** `dotnet test tests/LunaFlow.EngineTests` — 96/96 green, not inherited
  from a prior run.
- **Read both new files end-to-end** (`OneD20RuleFixtureTests.cs`, `OneD20HandlerTests.cs`) against
  the real `OneD20RuleGroup`/handler source: 9 + 16 = 25 tests, matching the claimed count and
  category tags (`Rom_OneD20`/`Phase2.5`). Confirmed the fixture tests exercise the real compiled
  pipeline (`RuleExport.Compile`) and the real `GameEngineCore.Instance.MessagePosted` event rather
  than a stand-in, and the handler tests go through the actual dispatch+`Fire()` path — the only way
  to test a handler at all, since `LunaFlowEngine.Host` is hard-wired to the singleton.
- **Hand-verified a sample of the seeded-RNG math** rather than trusting the in-code comments: seed
  0's claimed first-d20-is-15 checks out against PC AttackBonus/Monster Defense/damage-die math for
  the lethal-hit test, and seed 41's two-round PlayerHp trace (11→7→0) checks out against Monster
  AttackBonus/PC Defense/damage math. Since the suite actually passed, this is empirical, not
  faith-based — a wrong seed/number would have failed the assertion.
- **Confirmed the disclosed self-caught bug is actually fixed**, not just claimed: the final
  assertions check `OneD20Flavor.Text["pc-hit"]` (the real mapped prose) inside announced strings,
  not the literal dictionary key `"pc-hit"` — the vacuous-`IsFalse` failure mode the response
  described is gone in what's checked in.
- **Scope matches the ask:** `move`/`look` parity is spot-checked (one valid move, one rejected
  edge move) rather than re-deriving Nine Rooms' whole grid, per the ticket's own allowance; the
  RNG seam from ticket 0019 is used exactly as designed, including one deliberately-unseeded test
  where the criterion holds under either outcome.

Also fixed a duplicated `## Resolution` header left over in this file from an earlier edit — no
content lost, just a stray copy-paste artifact.

1.d20's full acceptance-criteria checklist now has both manual verification (tickets 0016/0018) and
maintained automated coverage. Nothing further needed from Nadia on this ticket.
