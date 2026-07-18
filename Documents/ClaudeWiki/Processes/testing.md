# Testing — where it lives, how it runs, what the rules are

The findable entry point for testing on LunaFlow. If you're a test-dev chat (or anyone writing
tests), **start here**, then follow the links. This page exists because the previous test-dev
session had no signpost to the tests folder or the strategy doc at all, and wrote around
conventions it never saw.

> **Execution is a two-person job.** The user (Arryn) runs the test pass from VS2022; the agent
> triages the resulting `.trx`. Don't assume the agent runs the suite — see
> [Who runs the tests](#who-runs-the-tests--agent-assisted) below and
> [`agent-test-workflow.md`](agent-test-workflow.md) for the full split.

## Where the tests live

- **Project:** `tests/LunaFlow.EngineTests` (MSTest, .NET 9).
- **Test classes:** `tests/LunaFlow.EngineTests/Core/` — engine tests (`GameEngineCore*`) plus the
  Nine Rooms fixture (`NineRoomsEngineFixtureTests`).
- **Parallelism setting:** `tests/LunaFlow.EngineTests/MSTestSettings.cs`.
- **Execution order & diagnostics:** [`test-runbook.md`](test-runbook.md) — run order, category
  filters, diagnostic table.
- **Authoring patterns:** [`test-patterns.md`](test-patterns.md) — completion gates (not sleeps),
  event waiting, subscriber cleanup, one-behavior-per-test.

## How to run (quick reference)

Full detail in [`test-runbook.md`](test-runbook.md). The essentials:

```bash
dotnet build LunaFlow_A.sln
dotnet test tests/LunaFlow.EngineTests                                    # everything
dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase1"         # by category
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreTests"  # one class
```

## Who runs the tests — agent-assisted

Test execution is deliberately split so neither side burns time on the wrong half, and so the user
stays hands-on with results:

- **The user (Arryn) runs the full pass** from VS2022 (Test Explorer, a playlist, or the integrated
  terminal), producing a `.trx` at the pinned path `tests/LunaFlow.EngineTests/TestResults/latest.trx`
  (`LunaFlow_A.runsettings` handles this — no flags to remember).
- **The agent reads that `.trx`** and triages only the failures — it does not re-run the whole
  suite. Mid-fix, an agent may run a single narrow `dotnet test --filter` for a quick yes/no; a
  session without a toolchain reads results rather than running, and says so.

This is a first-class part of the testing workflow, not an afterthought — the full division of
labor, how to produce the report, and how the agent reads it live in
[`agent-test-workflow.md`](agent-test-workflow.md). Read it before driving a fix cycle.

## The conventions you must not miss

1. **Serial by default, opt in for pure tests.** `MSTestSettings.cs` sets `[assembly: DoNotParallelize]` —
   the default is serial. This is fail-safe: `GameEngineCore.Instance` is a singleton (load-bearing,
   production-fidelity requirement), and most tests in this suite touch it. Parallelism is only safe for
   tests with zero shared state — rare enough that requiring explicit opt-in (per-method
   `[Parallelize(Scope = ExecutionScope.MethodLevel)]`) is the right default. The old setup
   (parallel-by-default, opt-out for singleton) was a trap: forgetting `[DoNotParallelize]` on one
   class creates flaky cross-contamination that looks like an engine bug. Serial-first eliminates
   that pit. All existing singleton-touching test classes carry `[DoNotParallelize]` as
   belt-and-suspenders documentation (even if not strictly necessary now) — they clarify intent.

2. **Test the real path, capture real effects.** Rules and handlers produce player-visible output
   via `LunaFlowEngine.Host.Announce(...)` → `GameEngineCore.Instance.Post(...)` →
   the public `GameEngineCore.Instance.MessagePosted` event. To verify a rule fired or a handler
   behaved, **subscribe to `MessagePosted` and assert on captured announcements** — drive the actual
   handler/rule, don't hand-poke the fact it was supposed to produce. (This is the seam the previous
   fixture missed; it's `[DoNotParallelize]` territory because it uses the singleton.)

3. **Naming:** `[Feature]_[Condition]_[ExpectedBehavior]` (e.g. `TurnCounter_StartsAtZero`).
   Category-tag tests (`[TestCategory("PhaseN")]`, etc.) so they're filterable.

4. **Git:** never run state-changing git commands (`CLAUDE.md`). Build/test/run only.

## Anti-patterns (a green test that proves nothing is worse than no test)

- **Asserting fixture data instead of behavior** — e.g. asserting a `Room` record's `Description`
  string when the test's stated purpose is to verify an *announcement* occurred. It passes even if
  the rule that announces is deleted.
- **Tautologies** — inserting a fact, then asserting the fact exists. It tests the test, not the code.
- **Simulating the code instead of calling it** — "restore the position to simulate rejection"
  instead of invoking the real handler's rejection path. If you're re-implementing the logic in the
  test, you're not testing the logic.
- **Timing by `Thread.Sleep()`** — asserting after a fixed delay instead of waiting for the actual
  signal. Flakes under load and looks like an engine bug. Use completion gates — see
  [`test-patterns.md`](test-patterns.md).

If an acceptance criterion genuinely can't be expressed as a test given the current code shape,
**file a ticket to the [Architect](../Roles/architect.md)** asking for a testability seam — don't
work around it with a hollow assertion.

## Authoritative sources (what owns what)

| Concern | Owner doc |
| --- | --- |
| Run order, category filters, diagnostic workflow | [`test-runbook.md`](test-runbook.md) |
| Concurrency-safe authoring patterns (gates, subscriber cleanup) | [`test-patterns.md`](test-patterns.md) |
| Parallelism setting (the actual attribute) | `tests/LunaFlow.EngineTests/MSTestSettings.cs` |
| Build/test commands, git convention | `CLAUDE.md` |
| Conventions + anti-patterns + where-to-start (this page) | here |

## Related pages

- [ClaudeWiki index](../index.md)
- [Test runbook — execution order & diagnostics](test-runbook.md)
- [Test patterns — authoring concurrency-safe tests](test-patterns.md)
- [Agent-assisted test workflow (user runs, agent triages)](agent-test-workflow.md)
- [Dev — Test Suite (Nadia)](../Roles/dev-test.md)
- [Dev — Implementation](../Roles/dev.md)
- [QA / Verification (Lothar)](../Roles/qa.md)
