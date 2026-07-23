# Testing Quick-Start

Fast path to writing and running tests. **Full guidance in these four files** (read in order for deep dives):

1. [`testing.md`](testing.md) — conventions & anti-patterns (start here)
2. [`test-runbook.md`](test-runbook.md) — how to run, filtering by tag
3. [`test-patterns.md`](test-patterns.md) — concurrency-safe authoring
4. [`agent-test-workflow.md`](agent-test-workflow.md) — who runs what

---

## The essentials (10 minutes)

**Where tests live:** `tests/LunaFlow.EngineTests/Core/` and `.../GeneratorServices/`

**Serial by default.** `GameEngineCore.Instance` is a singleton. Tests run serially (`MSTestSettings.cs`). If your test is truly stateless, opt into parallel explicitly per-method — otherwise don't.

**Test the real path.** Subscribe to `GameEngineCore.Instance.MessagePosted` event to verify output; don't hand-poke the fact it should have produced.

**Name tests:** `[Feature]_[Condition]_[ExpectedBehavior]` — e.g. `TurnCounter_StartsAtZero`.

**Tag format:** two tags per test class, using `[TestCategory]` attribute:
- Primary: namespace (e.g., `Engine_Game`, `Rules_GeneratorServices`)
- Secondary: roadmap phase (e.g., `Phase1`, `Phase2`, `Phase2.5`)

Example:
```csharp
[TestClass]
[TestCategory("Engine_Game")]
[TestCategory("Phase1")]
public class GameEngineCoreTests { ... }
```

---

## Run commands (quick reference)

```bash
dotnet build LunaFlow_A.sln                                                    # full build first
dotnet test tests/LunaFlow.EngineTests                                         # run all
dotnet test tests/LunaFlow.EngineTests --filter "TestCategory=Engine_Game"     # one namespace
dotnet test tests/LunaFlow.EngineTests --filter "Name~TurnCounter"             # one feature
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreTests"  # one class
```

⚠️ **Use `TestCategory=`, not `Category=`.** (MSTest, not NUnit.)

**One-off mid-work narrow filter is OK** (e.g., `--filter "Name~MyNewTest"`). For **doneness verification**, have the user run the full pass and send the `.trx` result.

---

## Four anti-patterns to avoid

1. **Asserting fixture data instead of behavior** — asserting a `Room` exists when you meant to verify an announcement. Passes even if the rule that announces is deleted.
2. **Tautologies** — inserting a fact, then asserting it exists. Tests the test, not the code.
3. **Timing by `Thread.Sleep()`** — deadlock/race. Use gates (`test-patterns.md`) instead.
4. **Simulating the code** — re-implementing handler logic in the test instead of calling the real path.

---

## Diagnostic quick-guide

| Problem | Cause | Check |
| --- | --- | --- |
| Test times out (30s+) | Deadlock/starvation | `ManualResetEventSlim` usage |
| Flaky (passes/fails randomly) | Race condition | Run 5–10× in isolation; add sync if confirmed |
| `TurnCounter` mismatch | State sync bug | `AdvanceOneTurnSafe()` thread safety |
| Commands out of order | Queue issue | `BlockingCollection` usage |
| Unhandled exception in test | Not caught in handler | Check `ExecuteCommandSafe()` wrapper |

**Steps:** (1) read the failure message; (2) rerun one test in isolation (`--filter "FullyQualifiedName~..."`); (3) if flaky, loop 5–10× to confirm vs. one-off; (4) if consistent, trace to source.

---

## Related pages

- [Testing — conventions & where-to-start](testing.md)
- [Test runbook — execution & filtering](test-runbook.md)
- [Test patterns — concurrency-safe authoring](test-patterns.md)
- [Agent-assisted workflow — who runs what](agent-test-workflow.md)
- [ClaudeWiki index](../index.md)
