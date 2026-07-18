# Test Runbook — execution order & diagnostics

How to run the `LunaFlow.EngineTests` suite and diagnose failures. For conventions and the
parallelism model see [`testing.md`](testing.md); for how to *write* concurrency-safe tests see
[`test-patterns.md`](test-patterns.md).

## Sequential execution model

The entire suite runs serially by default (`[assembly: DoNotParallelize]` in `MSTestSettings.cs`) because
`GameEngineCore.Instance` is a singleton — in production the engine is one shared instance for the app's
lifetime, and the tests mirror that for fidelity. It's a fidelity requirement, not a limitation: the
thread-safety tests still validate concurrent *operations inside* the engine; they just don't run test
methods in parallel against the shared instance. Pure stateless tests may opt into parallelism
explicitly, but today that set is small. (See [`testing.md`](testing.md) for the full rationale.)

## Execution order

Run high-risk first, so a low-level break surfaces before it cascades into everything above it.

**Phase 1 — Turn loop & concurrency (highest risk).** Validate the core state machine and thread
safety first; bugs here cascade.

- `GameEngineCoreeTurnLoopTests` — turn counter, `TurnTick` events, gate behavior
- `GameEngineCoreThreadSafetyTests` — race conditions, concurrent operations

**Phase 2 — Lifecycle & command dispatch (medium risk).** Depends on Phase 1 being solid.

- `GameEngineCoreTests` — Start/Stop/Restart, options, state initialization
- `GameEngineCoreCommandDispatchTests` — queue ordering, completion signals, exception handling

**Phase 1.5 fixture.** `NineRoomsEngineFixtureTests` exercises the engine loop end-to-end using
Nine Rooms as a fixture (see [handoff 0003](Handoffs/0003-phase-1-5-engine-fixture-tests.md)).

**Phase 3 — integration (future).** Broader integration classes (session management, parser/dispatcher
integration, rule-loading end-to-end) are not built yet; add them here as they land rather than
listing aspirational names.

## How to run

```bash
dotnet build LunaFlow_A.sln
dotnet test tests/LunaFlow.EngineTests                                        # everything
dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase1"             # by phase
dotnet test tests/LunaFlow.EngineTests --filter "Category=ThreadSafety"       # by area
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests"   # one class
dotnet test tests/LunaFlow.EngineTests --filter "Name~TurnCounter"           # by feature
```

Tests carry `[TestCategory(...)]` tags (`Phase1`, `Phase2`, `TurnLoop`, `ThreadSafety`,
`Lifecycle`, `CommandDispatch`) for filtering — in VS2022 Test Explorer, filter with
`Category=Phase1` etc. Optional `.playlist` files in the test folder
(`GameEngineCorePhase1/Phase2/All`) group the same sets for the Test Explorer GUI.

## Diagnostic workflow

When a test fails, the name usually points at the cause:

| Symptom | Likely cause | Next step |
| --- | --- | --- |
| Test times out (30s+) | Deadlock in turn gate or wait | Check `ManualResetEventSlim` usage |
| Flaky (passes/fails randomly) | Race condition | Run 10× to confirm, then add synchronization |
| `TurnCounter` ≠ `State.TurnNumber` | State sync bug | Check `AdvanceOneTurnSafe()` thread safety |
| Commands execute out of order | Queue corruption | Check `BlockingCollection` usage |
| Exception in handler crashes test | Exception not caught | Check `ExecuteCommandSafe()` wrapper |

Steps: (1) read the assertion message; (2) rerun the one test in isolation
(`--filter "FullyQualifiedName~..."`); (3) if it's flaky, loop it 5–10× to confirm a race vs. a
logic error; (4) if it's a consistent failure, trace to the corresponding `GameEngineCore.cs` code.

A test taking >30s almost always means a deadlock or starvation, not slow logic.

## Related pages

- [Testing — conventions & workflow](testing.md)
- [Test patterns — authoring concurrency-safe tests](test-patterns.md)
- [Wiki style](wiki-style.md)
