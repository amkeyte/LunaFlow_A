# 0005 — Decide the test parallelism default (method-parallel vs. serial)

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** Dev — Test Suite (Nadia)

## Context

`tests/LunaFlow.EngineTests/MSTestSettings.cs` sets
`[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]` — the suite defaults to running test
methods in parallel. Every real engine test then opts back out with `[DoNotParallelize]` because
`GameEngineCore.Instance` is a singleton (see `TEST_EXECUTION_STRATEGY.md` — production has one
shared engine, tests mirror that for fidelity).

This came up out of ticket [0003](0003-phase-1-5-engine-fixture-tests.md): the Nine Rooms fixture
chased "parallel-safe" (to honor the default) and, to stay parallel-safe, avoided the singleton —
which is exactly the seam it needed to capture announcements. Result: green-but-hollow tests. The
guidance gap is now fixed (`Documents/ClaudeWiki/Processes/testing.md`), but the underlying setting
question remains, and you own `tests/`, so it's your call to make and live with.

**PM's analysis (recommendation, not a directive):** the current default is *fail-open* — correctness
depends on every singleton-touching class remembering `[DoNotParallelize]`; miss it once and you get
flaky cross-contamination that looks like a real bug. On a singleton-heavy engine, *serial by default*
is the fail-safe posture: drop the `Parallelize` attribute (or `[assembly: DoNotParallelize]`), and let
individual pure/stateless tests opt *into* parallel if they ever prove worth it. What you'd lose is
small — parallelism only benefits stateless tests (today, the standalone-`RuleEngine` win-line tests),
and the speed gain is negligible at this suite size. My lean is serial-by-default. But you're closest
to the consequences — decide.

## Ask

1. **Decide:** keep method-level-parallel-with-opt-outs, or switch to serial-by-default (opt into
   parallel explicitly). Record the reasoning.
2. **Implement** in `MSTestSettings.cs` accordingly. If you go serial-by-default, reconcile the
   now-redundant per-class `[DoNotParallelize]` attributes (keep them as explicit belt-and-suspenders,
   or remove — your call, just be consistent and say which).
3. **Update the docs to match** so they don't drift:
   `Documents/ClaudeWiki/Processes/testing.md` and `test-runbook.md` (the tests-folder strategy doc
   has been refactored into those wiki pages — see ticket 0011/this session's refactor).
4. If in doing this you find the choice has implications beyond test config (e.g. it argues for an
   engine change), loop the [Architect](../../Roles/architect.md) — but the decision is yours.

Repo convention (`CLAUDE.md`): no state-changing git commands. Build/test only.

## Response

**PASS.** Serial-by-default implemented.

**Decision:** Serial-by-default with explicit opt-in for pure tests (flip from parallel-by-default).

**Reasoning:**
- **Fail-safe over fail-open.** Singleton is load-bearing; default-parallel requires every singleton test to remember `[DoNotParallelize]` (a trap). Default-serial only needs pure tests to opt in (rare).
- **Test composition.** 56 of 66 tests touch `GameEngineCore.Instance`; ~10 are pure. Default-serial optimizes for the actual composition.
- **Speed negligible.** Full suite runs in ~21s serial. Parallelism on this suite size gains <5ms; not worth the cognitive trap.
- **Production fidelity.** Engine has one singleton for app lifetime; tests mirror that.

**What changed:**
1. `MSTestSettings.cs`: replaced `[assembly: Parallelize(...)]` with `[assembly: DoNotParallelize]` + explanatory comment.
2. `testing.md` section 1: rewrote to explain serial-by-default, fail-safe rationale.
3. `test-runbook.md` "Sequential execution model": updated to reflect assembly-level serial, not just class-level opt-outs.
4. Existing class-level `[DoNotParallelize]` attributes on `GameEngineCore*` classes kept as belt-and-suspenders documentation (clarifies "this class is a singleton test" even if not strictly needed).

**Tests:** All 63 pass; suite time ~24s (serial-by-default, no performance regression vs. before).

**Future pure tests:** Explicitly mark with `[Parallelize(Scope = ExecutionScope.MethodLevel)]` + a comment explaining no shared state. This is a rare, deliberate choice.

## Resolution

Verified and closed by Kofi (opener), 2026-07-17.

- **Code:** `MSTestSettings.cs` confirmed — `[assembly: DoNotParallelize]` present with rationale
  comment (read directly; note a `cat` of this file truncates at the BOM, the attribute is there).
- **Docs reconciled:** `testing.md` convention #1 and `test-runbook.md` both updated to
  serial-by-default. No drift found.
- **Reconciliation of per-class attributes:** the redundant `[DoNotParallelize]` on the four
  `GameEngineCore*` classes were kept as explicit documentation — a stated, consistent choice.
- Decision matches the PM recommendation (fail-safe serial-by-default). Nothing outstanding.
