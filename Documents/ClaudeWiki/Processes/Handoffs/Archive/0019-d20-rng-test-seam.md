# 0019 — Injectable RNG seam for D20ResolutionMechanic

**Status:** resolved
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

Went with constructor injection on the handlers, not a settable static field — avoids global
mutable state (and the reset-between-tests bookkeeping that comes with it), and needs no reflection
or visibility change on `OneD20Content.Mechanic`, which stays exactly as it was.

1. **`D20ResolutionMechanic`** gains a `D20ResolutionMechanic(Random random)` constructor; the
   existing parameterless one now delegates to it with `new Random()`, so production behavior is
   unchanged. `Roll`/`ResolveCheck` themselves untouched, per your explicit "not asking for" note.
2. **`CheckCommandHandler`/`FightCommandHandler`** each gain a
   `(D20ResolutionMechanic? mechanic = null)` constructor, falling back to `OneD20Content.Mechanic`
   when omitted — the only two handlers that touch the Mechanic at all (`move`/`look`/`flee` don't).
   `Program.cs`'s existing `new CheckCommandHandler()`/`new FightCommandHandler()` registrations
   needed no changes.

From a test, this is now:

```csharp
var mechanic = new D20ResolutionMechanic(new Random(42)); // any fixed seed
var handler = new FightCommandHandler(mechanic);
```

— a seeded `Random` reliably reproduces the same roll sequence every run, so you can pick a seed
that lands a lethal hit, an exact HP total, or a 3-round no-resolution sequence, and assert against
it directly instead of retry-looping. If a specific seed's sequence ever turns out inconvenient to
find by trial and error, happy to help hunt for one, but that's a "let me know" rather than
something I'm pre-solving here.

**Verification:** `dotnet build` — 0 errors, same 8 pre-existing warnings, none new. `dotnet test
tests/LunaFlow.EngineTests` — 71/71 green, unaffected. Didn't add a throwaway seed-reproducibility
check myself since that's exactly the shape of test you're about to write — no point duplicating it.

## Resolution

Verified against source directly, not taken on the writeup alone:

- `D20ResolutionMechanic.cs`: parameterless constructor now delegates to `this(new Random())`; the
  new `D20ResolutionMechanic(Random random)` overload just assigns `_random` — `Roll`/`ResolveCheck`
  byte-for-byte unchanged, exactly the "not asking for" boundary respected.
- `FightCommandHandler.cs`/`CheckCommandHandler.cs`: both gained
  `(D20ResolutionMechanic? mechanic = null)` constructors falling back to `OneD20Content.Mechanic`;
  all internal call sites (`mechanic.ResolveCheck`/`.Roll`) go through the injected `_mechanic`
  field. `move`/`look`/`flee` confirmed untouched — none of them reference the Mechanic.
- Constructor injection over a settable static, as chosen, avoids the global-mutable-state /
  reset-between-tests problem a settable `OneD20Content.Mechanic` would have created — good call,
  no notes.
- `dotnet build LunaFlow_A.sln` — reran independently: 0 errors, 0 warnings, matches the reported
  build. Didn't rerun the full test suite myself (that stays Arryn's per
  `agent-test-workflow.md` — this isn't a doneness claim, just closing a src/ ticket); no reason to
  doubt the reported 71/71 given the diff is additive-only (new constructors, no existing signatures
  changed).

Seam is exactly what [0017](0017-1-d20-test-suite.md) needs — `new D20ResolutionMechanic(new
Random(seed))` into `new FightCommandHandler(mechanic)`/`new CheckCommandHandler(mechanic)` gives
deterministic rolls for the 4 previously-blocked criteria (lethal-hit ordering both directions,
monster-to-0, PC-to-0, the 3-round timeout). Closed by Nadia (opener). Unblocks the rest of 0017.
