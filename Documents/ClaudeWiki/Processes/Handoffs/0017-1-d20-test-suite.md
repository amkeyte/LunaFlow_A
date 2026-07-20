# 0017 — 1.d20 maintained test suite

**Status:** open
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

## Response

_(Nadia to fill in.)_

## Resolution

_(Baraka to fill in on close.)_
