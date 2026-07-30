# Nadia's Workflow with the Triage Engine

How to use the `.trx` triage engine as part of your test-dev cycle.

## Standard workflow: test failure triage

### 1. Arryn runs the full test suite
```bash
# In VS2022 terminal or any shell in repo root:
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings
```
This produces `.trx` at `tests/LunaFlow.EngineTests/TestResults/latest.trx`.

### 2. You run triage
```powershell
# From anywhere in the repo:
Documents\Test\trx-triage\triage.ps1
```

The script automatically finds `latest.trx` and generates a markdown report.

**Pro tip:** If you run triage twice on the same `.trx` (e.g., you want to double-check), the second run short-circuits instantly and shows a cached preview. Use `--force` to skip the cache if needed:
```powershell
Documents\Test\trx-triage\triage.ps1 --force
```

This keeps you from redundantly re-processing the same results while Arryn is re-running the suite.

**Output:** Failures ranked by priority, cascades flagged, diagnostics for each.

### 3. Scan the report
```
## Summary by Tier
- **Tier 1:** 3 failures (0 cascades)  ← Fix these first
- **Tier 2:** 2 failures (2 cascades)   ← Likely noise if Tier 1 is broken
- **Tier 3:** 5 failures (4 cascades)   ← Definitely noise

## ⚠️ Cascade Warning
5 failures appear to be cascades...
```

**Key insight:** If Tier 1 is broken, ignore Tier 2-3 for now. Fix Tier 1 first.

### 4. Debug the top failure

Example top failure from report:
```
1. GameEngineCoreeTurnLoopTests.TurnCounter_StartsAtZero
   Type: timeout
   Probable Cause: Deadlock in turn gate or wait
   Next Step: Check ManualResetEventSlim usage in test
```

Action:
```bash
# Run just that test to confirm it's reproducible
dotnet test tests/LunaFlow.EngineTests \
  --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests.TurnCounter_StartsAtZero"

# Read the test (GameEngineCoreeTurnLoopTests.cs)
# Look for ManualResetEventSlim usage
# Check if there's a missing .Set() or .Dispose() call
```

### 5. File a handoff ticket to the Implementation Dev (if it's a defect)

If the test reveals a bug in `src/`, **don't fix it yourself** — create a handoff ticket:

1. Go to `Documents/ClaudeWiki/Processes/Handoffs/`
2. Create a new file: `NNNN-short-description.md` (use next number from `index.md`)
3. Fill in the template:
   ```markdown
   # NNNN — Engine: Deadlock in GameEngineCoreeTurnLoopTests.TurnCounter_StartsAtZero

   **Status:** open
   **Opened by:** Test Dev
   **Opened:** 2026-07-23
   **For:** Implementation Dev

   ## Details

   ### Context
   Test [LunaFlow.EngineTests.Core.GameEngineCoreeTurnLoopTests.TurnCounter_StartsAtZero] 
   is timing out (>30s), indicating a deadlock in the turn gate.

   ### Ask
   Investigate ManualResetEventSlim usage in GameEngineCore.TurnTick event handling.
   The test hangs waiting for a gate that never fires.

   ### Response
   [To be filled by Implementation Dev]
   ```
4. Update `Handoffs/_index.md` to add a row for the ticket.

### 6. Verify the fix (once Implementation Dev responds)

Once the defect is fixed in `src/`:
1. Arryn re-runs the test suite
2. You run triage again
3. Verify that Tier 1 failures resolve and Tier 2-3 cascades clear up

## Variants

### Testing a narrow fix mid-work

If you want to check a single failing test without waiting for a full run:

```bash
# Run just that test class
dotnet test tests/LunaFlow.EngineTests \
  --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests"

# No need to run triage; just a quick yes/no check
```

### Comparing before/after

If you want to keep two `.trx` files for comparison:

```bash
# Before fix (move latest.trx to a named file)
move tests\LunaFlow.EngineTests\TestResults\latest.trx before-fix.trx

# Run Arryn's test suite (produces new latest.trx)
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings

# Triage both
python3 Documents/Test/trx-triage/triage.py tests/LunaFlow.EngineTests/TestResults/latest.trx --output json > after-fix.json
python3 Documents/Test/trx-triage/triage.py before-fix.trx --output json > before-fix.json

# Spot the difference in failure counts/cascades
```

### JSON output for programmatic use

```bash
Documents\Test\trx-triage\triage.ps1 -Output json > triage-report.json
```

Useful if you're building a Slack bot, email report, or dashboard that consumes the triage data.

## Tips

### Red flags in the report

- **Multiple Tier 1 failures** → Root cause likely in core engine loop or thread safety
- **Tier 2-3 failures with 0 Tier 1 failures** → Likely genuine mid-tier bugs, not cascades
- **All failures are cascades** → One root cause; fix it and re-run
- **Flaky_race type** → Don't trust a single pass; might be intermittent until fixed

### Reading the diagnostics

```
Type: timeout
Duration: 35.23s (> 30s threshold)
Probable Cause: Deadlock in turn gate or wait
Next Step: Check ManualResetEventSlim usage
```

This tells you: the engine stalled for 35 seconds, which is way longer than test timing, so it's definitely a deadlock. Look for the gate that's never signaled.

### When you're unsure

1. Check the test class's `[TestCategory]` tags — tells you which tier and phase
2. Read the test name — follows `[Feature]_[Condition]_[ExpectedBehavior]` convention
3. Look at the stack trace — shows where it timed out or what assertion failed
4. Read the message — often the test author left a hint about what went wrong

If it still doesn't make sense, that's a good sign you should file a ticket to the Architect asking for a testability seam.

## See also

- [Test runbook](../../ClaudeWiki/Processes/test-runbook.md) — execution order and red-flag table
- [Test patterns](../../ClaudeWiki/Processes/test-patterns.md) — how to write concurrency-safe tests
- [Handoff tickets](../../ClaudeWiki/Processes/handoff-tickets.md) — how to file findings
