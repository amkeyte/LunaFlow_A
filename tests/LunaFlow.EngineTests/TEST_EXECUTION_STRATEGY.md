# LunaFlow Test Execution Strategy

## Overview

This guide outlines the recommended test execution order and techniques to efficiently diagnose issues without running unrelated tests. The strategy prioritizes high-risk areas first to surface concurrency bugs early.

---

## Sequential Execution Model

**All GameEngineCore tests use `[DoNotParallelize]`** because `GameEngineCore.Instance` is a singleton. In production, the engine persists for the app lifetime as a single shared instance. Tests run sequentially to match this model and prevent cross-test interference on the shared singleton state.

This is not a test limitation—it's a fidelity requirement. The threading tests themselves still validate concurrent *operations* within the engine; they just don't run tests in parallel against it.

---

## Execution Phases

### Phase 1: Turn Loop & Concurrency (HIGH RISK)
**Goal:** Validate core engine state machine and thread safety under stress.

**Tests to run:**
1. `GameEngineCoreeTurnLoopTests` — Turn counter, TurnTick events, gate behavior
2. `GameEngineCoreThreadSafetyTests` — Race conditions, concurrent operations

**Why first:** If the turn loop or event system has bugs, they'll cascade into all other tests. Better to catch them early.

**Expected outcome:** No flaky tests. All tests pass consistently (run 3× if any timeout).

---

### Phase 2: Lifecycle & Command Dispatch (MEDIUM RISK)
**Goal:** Validate basic engine lifecycle and command flow.

**Tests to run:**
1. `GameEngineCoreTests` — Start/Stop/Restart, options, state initialization
2. `GameEngineCoreCommandDispatchTests` — Queue ordering, completion signals, exception handling

**Why second:** These depend on Phase 1 working. If turn loop is solid, lifecycle bugs are simpler to diagnose.

**Expected outcome:** All tests pass on first run (no timing sensitivity).

---

### Phase 3: Integration & Scenarios (SYSTEM-LEVEL)
**Goal:** Validate engine + rules + commands working together.

**Tests to run (when ready):**
1. `GameWorldTests` — Session management
2. `DefaultCommandParserTests` — Input parsing
3. `DefaultCommandDispatcherTests` — Handler routing
4. `EngineCommandsIntegrationTests` — End-to-end command flow
5. `EngineRulesIntegrationTests` — Rule loading and execution
6. `NineRoomsGameFlowTests` — Full QA 1.1 playable transcript

**Why third:** These compose the lower layers; they'll fail if Phase 1–2 have issues.

---

## How to Run Tests

### Option A: Visual Studio Test Explorer (Recommended for Development)

**Test Categories** provide precise filtering via VS2022 Test Explorer:

All tests are tagged with `[TestCategory]` attributes for reliable filtering:

- **Phase1:** Turn Loop & Threading tests
  - `[TestCategory("Phase1")]`
  - Includes: `TurnLoop`, `ThreadSafety` categories
  
- **Phase2:** Lifecycle & Command Dispatch tests
  - `[TestCategory("Phase2")]`
  - Includes: `Lifecycle`, `CommandDispatch` categories

**To run in Test Explorer:**
1. Open Test Explorer (Test → Test Explorer)
2. Click the filter icon
3. Enter filter: `Category=Phase1` or `Category=Phase2`
4. View and run filtered tests
5. Results show in Test Explorer with category grouping

**Benefits:**
- Precise test selection
- TRX output matches exact tests run
- Easily grepable results
- No playlist file management needed

### Option B: Command Line (.NET CLI) — Most Reliable

**Run Phase 1 tests only (HIGH RISK):**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase1"
```

**Run Phase 2 tests only (MEDIUM RISK):**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase2"
```

**Run all GameEngineCore tests:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase1|Category=Phase2"
```

**Run only turn loop tests:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "Category=TurnLoop"
```

**Run only thread safety tests:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "Category=ThreadSafety"
```

**Run a single test class:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests"
```

**Run a single test method:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests.TurnCounter_StartsAtZero"
```

### Option C: VS Test Explorer GUI (Simplest)

1. Build solution (`dotnet build LunaFlow_A.sln`)
2. Open Test Explorer (Test → Test Explorer)
3. Right-click test class → "Run Tests"
4. Filter by class name in the search box

---

## Diagnostic Workflow

### When tests fail:

1. **Check test name for clues:**
   - `ThreadSafety` test fails → concurrency bug
   - `TurnLoop` test fails → state machine issue
   - `CommandDispatch` test fails → queue or threading issue
   - Timeout assertion → deadlock

2. **Run that test in isolation:**
   ```bash
   dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~[FullTestName]"
   ```

3. **If flaky (pass/fail randomly):**
   - Run 5–10 times in a loop: `for i in {1..5}; do dotnet test ...; done`
   - Indicates race condition, not logic error

4. **If consistent failure:**
   - Read assertion message carefully
   - Check GameEngineCore.cs for the corresponding code
   - Add breakpoints and step through

### Red flags to watch for:

| Symptom | Likely Cause | Next Step |
|---------|--------------|-----------|
| Test times out (30s+) | Deadlock in turn gate or wait | Check ManualResetEventSlim usage |
| Flaky (pass/fail randomly) | Race condition | Run 10× to confirm, then add synchronization |
| TurnCounter ≠ State.TurnNumber | State sync bug | Check AdvanceOneTurnSafe() thread safety |
| Commands execute out of order | Queue corruption | Check BlockingCollection usage |
| Exception in handler crashes test | Exception not caught | Check ExecuteCommandSafe() wrapper |

---

## Test Categories Reference

Tests are tagged with `[TestCategory]` attributes for reliable filtering:

| Category | Description | Test Count |
|----------|-------------|-----------|
| `Phase1` | Turn loop & threading tests (run first) | 24 |
| `Phase2` | Lifecycle & command dispatch tests (run second) | 32 |
| `TurnLoop` | Turn counter, TurnTick events, gate behavior | 14 |
| `ThreadSafety` | Race conditions, concurrent operations | 10 |
| `Lifecycle` | Start/Stop/Restart, options, state | 20 |
| `CommandDispatch` | Queue ordering, completion signals, exceptions | 12 |

**Filter by category in Test Explorer:**
- Click filter icon in Test Explorer
- Type: `Category=Phase1` or `Category=ThreadSafety` etc.

**Playlist files** (optional, in test folder):
- `GameEngineCorePhase1.playlist` — Phase 1 tests
- `GameEngineCorePhase2.playlist` — Phase 2 tests
- `GameEngineCoreAll.playlist` — All GameEngineCore tests

These can be opened via Test → Open Playlist if preferred for visual organization.

---

## Stress Testing

Once Phase 1–2 pass, validate under load:

```bash
# Run Phase 1 tests 10 times to check for flakiness
for i in {1..10}; do 
  dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreThreadSafetyTests" || break
done
```

If any test flakes (fails on run N but not N+1), there's a race condition to debug.

---

## Test Naming Convention

All test methods follow this pattern for easy filtering:

- `[Feature]_[Condition]_[ExpectedBehavior]`
- Example: `TurnCounter_StartsAtZero`, `CommandException_DoesNotStopEngine`

Filter by feature:
```bash
dotnet test --filter "Name~TurnCounter"
dotnet test --filter "Name~CommandException"
```

---

## Next Steps

1. **Run Phase 1 tests** → Fix any turn loop or threading issues
2. **Run Phase 2 tests** → Validate lifecycle and dispatch
3. **Create `.playlist` files** above in test folder for future use
4. **Document any failures** in GitHub issues with:
   - Test name
   - Error message
   - Repro steps (flaky? consistent? specific to one test?)
   - Local environment (OS, .NET version)

---

## Performance Expectations

- **Phase 1 tests:** ~5–10 seconds (threading tests add latency)
- **Phase 2 tests:** ~3–5 seconds
- **Full suite (all phases):** ~30–60 seconds

If any test takes >30 seconds, there's likely a deadlock or starvation issue.
