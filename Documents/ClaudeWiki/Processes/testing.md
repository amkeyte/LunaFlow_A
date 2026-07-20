# Testing — where to start & the conventions you must not miss

The findable entry point for testing on LunaFlow. If you're writing or verifying tests, **start
here**, then follow the links. This page owns the *authoring conventions* and the hub role; it does
**not** duplicate how-to-run or the tag/filter mechanics — those live in
[`test-runbook.md`](test-runbook.md), linked below. (It existed as a mini-everything and drifted;
it's now a thin hub — one fact, one home.)

> **Execution is not the agent's to assume.** An agent may run a narrow one-off `--filter` mid-work,
> but **doneness verification and troubleshooting are user-run** (Arryn runs the pass, the agent
> triages the `.trx`). See [`agent-test-workflow.md`](agent-test-workflow.md).

## The testing docs (one concern each)

- **This page** — where tests live, the conventions you must not miss, anti-patterns.
- [`test-runbook.md`](test-runbook.md) — how to run, the tag/filter convention, execution order,
  diagnostics.
- [`test-patterns.md`](test-patterns.md) — concurrency-safe authoring patterns (gates, subscriber
  cleanup, one-behavior-per-test).
- [`agent-test-workflow.md`](agent-test-workflow.md) — who runs what and when; the `.trx` workflow.

## Where the tests live

- **Project:** `tests/LunaFlow.EngineTests` (MSTest, .NET 9).
- **Test classes:** `tests/LunaFlow.EngineTests/Core/` (engine + Nine Rooms fixture) and
  `tests/LunaFlow.EngineTests/GeneratorServices/` (rule-pipeline B1–B5).
- **Parallelism setting:** `tests/LunaFlow.EngineTests/MSTestSettings.cs`.

## The conventions you must not miss

1. **Serial by default, opt in for pure tests.** `MSTestSettings.cs` sets
   `[assembly: DoNotParallelize]` — the suite runs serially. This is *fail-safe*:
   `GameEngineCore.Instance` is a singleton (load-bearing, production-fidelity requirement) and most
   tests touch it. The old parallel-by-default-with-opt-outs was a trap — one forgotten
   `[DoNotParallelize]` produced flaky cross-contamination that looked like an engine bug. Only a
   genuinely stateless test opts back into parallel, explicitly, per-method. (This page is the single
   home for the *rationale*; the runbook and patterns link here.)

2. **Test the real path, capture real effects.** Rules/handlers produce output via
   `LunaFlowEngine.Host.Announce(...)` → `GameEngineCore.Instance.Post(...)` → the public
   `GameEngineCore.Instance.MessagePosted` event. To verify a rule fired, **subscribe to
   `MessagePosted` and assert on captured announcements** — drive the actual handler/rule, don't
   hand-poke the fact it should have produced.

3. **Naming:** `[Feature]_[Condition]_[ExpectedBehavior]` (e.g. `TurnCounter_StartsAtZero`). Tag per
   the [runbook's convention](test-runbook.md#tag--filter-convention) so tests are filterable.

4. **Git:** never run state-changing git commands (`CLAUDE.md`). Build/test/run only.

## Anti-patterns (a green test that proves nothing is worse than no test)

- **Asserting fixture data instead of behavior** — asserting a `Room` record's `Description` when
  the test's purpose is to verify an *announcement*. Passes even if the rule that announces is deleted.
- **Tautologies** — inserting a fact, then asserting the fact exists. Tests the test, not the code.
- **Simulating the code instead of calling it** — re-implementing the handler's logic in the test
  ("restore the position to simulate rejection") instead of invoking the real path.
- **Timing by `Thread.Sleep()`** — asserting after a fixed delay instead of waiting for the actual
  signal. Flakes under load; use completion gates ([`test-patterns.md`](test-patterns.md)).

If an acceptance criterion genuinely can't be expressed as a test given the current code shape,
**file a ticket to the [Architect](../Roles/architect.md)** for a testability seam — don't work
around it with a hollow assertion.

## Authoritative sources (what owns what)

| Concern | Owner |
| --- | --- |
| Conventions, anti-patterns, where-to-start (this page) | here |
| How to run, tag/filter convention, execution order, diagnostics | [`test-runbook.md`](test-runbook.md) |
| Concurrency-safe authoring patterns | [`test-patterns.md`](test-patterns.md) |
| Who runs what, the `.trx` workflow | [`agent-test-workflow.md`](agent-test-workflow.md) |
| Parallelism *setting* (the actual attribute) | `tests/LunaFlow.EngineTests/MSTestSettings.cs` |
| Build/test commands, git convention | `CLAUDE.md` |

## Related pages

- [ClaudeWiki index](../index.md)
- [Test runbook](test-runbook.md) · [Test patterns](test-patterns.md) · [Agent-assisted workflow](agent-test-workflow.md)
- [Dev — Test Suite (Nadia)](../Roles/dev-test.md) · [Dev — Implementation](../Roles/dev.md) · [QA (Lothar)](../Roles/qa.md)
