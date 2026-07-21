# 0019 — Injectable RNG seam for D20ResolutionMechanic

**Status:** open
**Opened by:** Dev — Test Suite (Nadia)
**Opened:** 2026-07-20
**For:** Dev — Implementation (Baraka)

## Context

Building the maintained suite for [0017](0017-1-d20-test-suite.md) against
[0018](0018-recode-1-d20-per-revised-spec.md)'s revised 1.d20 shape. Traced the RNG path:
`OneD20Content.Mechanic` is a `static readonly D20ResolutionMechanic Mechanic = new()`
(`OneD20Content.cs`), called directly by `CheckCommandHandler`/`FightCommandHandler`.
`D20ResolutionMechanic` itself holds `private readonly Random _random = new()`
(`D20ResolutionMechanic.cs`) — unseeded and un-injectable, and the field holding the instance is
`static readonly`, so there's no swap point even via a settable field.

Most of 1.d20's acceptance criteria are fine without touching this — a loop-until-observed retry
(call `fight`/`check` repeatedly until the branch of interest shows up) adequately covers
single-roll branches like a hit, a miss, or a successful check. But four criteria need a *forced,
exact* outcome that a retry loop can't reliably produce:

- A lethal PC hit / lethal monster hit (verifies 0018's no-double-announce ordering fix).
- A `fight` round that drops the monster to exactly 0 HP (verifies the counterattack-skip).
- A `fight` round that drops the PC's `CombatRound.PlayerHp` to exactly 0 (verifies `RoomFailed` +
  `pc-lost`, not a session-ending state).
- The 3-round timeout **without** either combatant reaching 0 across all three rounds — a compound
  condition across 3 calls, not a single-branch retry. Forcing this via retries is the RNG
  equivalent of the `Thread.Sleep` anti-pattern `test-patterns.md` warns against: non-deterministic,
  can hang or flake under an unlucky seed.

Filed instead of worked around, per [`dev-test.md`](../../Roles/dev-test.md)'s own boundary: a test
revealing an untestable seam in `src/` is a ticket to the implementation Dev, not something Test Dev
routes around.

## Ask

Add a minimal, test-only seam so `D20ResolutionMechanic`'s rolls can be forced deterministically
from a test. Suggested shape (yours to adjust — flagging the constraint, not dictating the fix):

- A `D20ResolutionMechanic(Random random)` constructor overload (default-constructs today's
  `new Random()` if omitted, so production callers are unaffected).
- Some way to get a *test* instance of `D20ResolutionMechanic` into the handlers instead of the
  hardcoded `OneD20Content.Mechanic` static field — e.g. make `OneD20Content.Mechanic` settable
  (internal setter, or a test-only override hook), or thread the mechanic through as a handler
  constructor parameter instead of a static pull. Either is fine as long as a test can substitute a
  seeded/scripted RNG source without reflection.
- A seeded `System.Random` is enough determinism for MSTest purposes (a fixed seed reliably
  reproduces a known roll sequence) — doesn't need a full `IRandomSource` abstraction unless that's
  less work on your end.

**Not asking for:** any change to `D20ResolutionMechanic.Roll`/`ResolveCheck`'s actual logic, or to
`CheckResult` — those are correct and untouched by this ask.

## Response

_(Baraka to fill in.)_

## Resolution

_(Nadia to fill in on close.)_
