# LunaFlow Test Execution Strategy

## Overview

This guide outlines the recommended test execution order and techniques to efficiently diagnose issues without running unrelated tests. The strategy prioritizes high-risk areas first to surface concurrency bugs early.

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

**VS2022 Test Playlists (.playlist files)** provide visual, reorderable test grouping:

1. **Create a playlist file** (`GameEngineCorePhase1.playlist`):
   ```xml
   <Playlist Version="2.0">
     <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core.GameEngineCoreeTurnLoopTests" />
     <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core.GameEngineCoreThreadSafetyTests" />
   </Playlist>
   ```

2. **Open in Test Explorer:**
   - Test Explorer → File → Open Playlist
   - Select the `.playlist` file
   - Run tests in that playlist (Run All, or individual tests)

3. **Benefits:**
   - Visual grouping in Test Explorer
   - Easy to reorder tests
   - Can create multiple playlists (Phase1, Phase2, etc.)
   - Results are grouped and sortable

### Option B: Command Line (.NET CLI)

**Run all tests in a class:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests"
```

**Run all GameEngineCore tests:**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCore"
```

**Run tests matching a pattern (e.g., all Phase 1):**
```bash
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~(GameEngineCoreeTurnLoopTests|GameEngineCoreThreadSafetyTests)"
```

**Run a single test:**
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

## Recommended Playlist Files

Create these `.playlist` files in the test folder for easy reuse:

### `GameEngineCorePhase1.playlist`
```xml
<Playlist Version="2.0">
  <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core.GameEngineCoreeTurnLoopTests" />
  <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core.GameEngineCoreThreadSafetyTests" />
</Playlist>
```

### `GameEngineCorePhase2.playlist`
```xml
<Playlist Version="2.0">
  <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core.GameEngineCoreTests" />
  <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core.GameEngineCoreCommandDispatchTests" />
</Playlist>
```

### `GameEngineCoreAll.playlist`
```xml
<Playlist Version="2.0">
  <Rule Name="Include" FullyQualifiedName="LunaFlow.EngineTests.Core" />
</Playlist>
```

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
