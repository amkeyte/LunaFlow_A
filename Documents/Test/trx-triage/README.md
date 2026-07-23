# .trx Triage Engine

Automated classification and prioritization of test failures from MSTest `.trx` files.

When you run the LunaFlow test suite and failures occur, the `.trx` result file contains raw data: test names, outcomes, messages, stack traces. This tool converts that raw data into **actionable, ranked diagnostics** by:

1. **Classifying failures** — timeout, exception, assertion, race condition, state sync bug, etc.
2. **Enriching with metadata** — tier (Tier 1/2/3), namespace, phase tags
3. **Detecting cascades** — identifying which failures are consequences of upstream breaks
4. **Ranking by priority** — highest-risk tier first, then by severity
5. **Generating reports** — markdown (human-readable) or JSON (machine-parseable)

## Quick Start

### Prerequisites
- Python 3.x
- A `.trx` result file from `dotnet test`

### Run the triage

**PowerShell (Windows):**
```powershell
cd C:\_local\source\LunaFlow_A\Documents\Test\trx-triage
.\triage.ps1
```

**Bash/Python:**
```bash
cd C:\_local\source\LunaFlow_A/Documents/Test/trx-triage
python3 triage.py ../../tests/LunaFlow.EngineTests/TestResults/latest.trx
```

**With custom output format:**
```bash
python3 triage.py /path/to/results.trx --output json
```

**Force re-triage (bypass cache):**
```bash
python3 triage.py /path/to/results.trx --force
```

### Caching behavior

The triage engine caches results by `.trx` file hash. If you run triage on the same `.trx` file twice without Arryn re-running the suite, the second run short-circuits immediately and shows a preview of the cached results.

```powershell
# First run: full triage
.\triage.ps1
# Output: full report (~1 sec)

# Second run: cached
.\triage.ps1
# Output: "Already triaged: latest.trx ... (Same results as last run)"
```

Use `--force` to bypass the cache if you need a fresh run:
```bash
python3 triage.py latest.trx --force
```

State is stored in `.triaged-state.json` (tracks file hashes and preview summaries).

### Sample output (markdown)

```
# Test Triage Report
Generated: 2026-07-23 14:32:15

## Summary by Tier
- **Tier 1 (Turn loop & concurrency):** 3 failures (0 cascades)
- **Tier 2 (Lifecycle & command dispatch):** 2 failures (1 cascade)
- **Tier 3 (Content & pipeline fixtures):** 5 failures (4 cascades)

## ⚠️ Cascade Warning
**5 failures appear to be cascades from higher-tier breaks.**
Fix the highest-tier failures first; cascades will likely resolve.

## Failures (ranked by priority)
| Priority | Test | Type | Tier | Probable Cause | Next Step | Cascade? |
|----------|------|------|------|----------------|-----------|----------|
| 1 | `GameEngineCoreeTurnLoopTests.TurnCounter_StartsAtZero` | timeout | Tier 1 | Deadlock in turn gate | Check ManualResetEventSlim | |
| 2 | `GameEngineCoreThreadSafetyTests.ConcurrentCommands_RaceCondition` | flaky_race | Tier 1 | Race condition | Run 5-10× to confirm | |
| 3 | `GameEngineCoreTests.Start_Stop_Restart` | assertion | Tier 2 | Logic error | Review assertion | ⚠️ CASCADE |
```

## Architecture

### Configuration-driven classification

Rather than hard-coding failure patterns, the engine reads from YAML-like JSON configs:

- **`tiers.json`** — test class → tier mapping; tier risk levels
- **`diagnostics.json`** — failure patterns (timeout, exception, race, etc.) with detection heuristics
- **`namespaces.json`** — namespace/phase tag definitions

This keeps the tool testable and updatable without recompiling.

### Failure classification pipeline

1. **Parse `.trx` XML** → extract test name, outcome, message, duration, stack trace
2. **Classify** by pattern match (keywords, duration threshold)
3. **Enrich** with tier/namespace metadata by looking up test class name
4. **Detect cascades** — if Tier 1 is broken, mark Tier 2-3 failures as suspect
5. **Rank** by tier (primary), cascade status, failure severity
6. **Report** in markdown or JSON

### Key insight: Cascade detection

The engine recognizes that **a single root cause can trigger many downstream failures**:

```
Tier 1 broken (turn gate deadlock)
  ↓
Tier 2 fails (command dispatch hangs waiting for turn)
  ↓
Tier 3 fails (rule pipeline times out)
```

Instead of showing three independent failures, the tool flags Tier 2-3 as cascades and ranks Tier 1 as the primary failure to debug.

## Diagnostic patterns

Each pattern has keywords and/or duration thresholds to detect it:

| Pattern | Detection | Probable Cause | Next Step |
|---------|-----------|----------------|-----------|
| **Timeout** | Duration >30s | Deadlock in turn gate/wait | Check ManualResetEventSlim usage |
| **Flaky/Race** | Keywords: flaky, race, intermittent | Race condition; test passes/fails randomly | Run 5-10× in isolation; add sync |
| **State Sync** | Keywords: TurnCounter, sync, mismatch | State out of sync | Check AdvanceOneTurnSafe() thread safety |
| **Queue Corruption** | Keywords: order, queue, BlockingCollection | Commands execute out of order | Check BlockingCollection semantics |
| **Uncaught Exception** | Stack trace has Exception/throw | Exception not caught in handler | Check ExecuteCommandSafe() wrapper |
| **Assertion** | Keywords: Assert, expected, actual | Logic error | Review assertion and test logic |

## Usage patterns

### As Nadia (test dev) mid-work

1. **Run a narrow test filter** to check a specific area
2. **Arryn runs the full suite**, produces `.trx`
3. **Run triage** — get a ranked report of what's broken
4. **Fix highest-priority failures** (usually Tier 1)
5. **Re-run triage** to verify cascades resolved

### Programmatically (future)

The engine can be imported as a Python module:

```python
from triage import TriageEngine

engine = TriageEngine(Path("config"))
failures = engine.parse_trx(Path("latest.trx"))
failures = engine.classify_failures(failures)
failures = engine.detect_cascades(failures)
failures = engine.rank_failures(failures)

# Process failures programmatically
for f in failures:
    print(f"{f.priority}. {f.test_name}: {f.failure_type}")
```

## Configuration updates

### Adding a new test class to a tier

Edit `config/tiers.json`:
```json
{
  "tier": 2,
  "test_classes": [
    "GameEngineCoreTests",
    "GameEngineCoreCommandDispatchTests",
    "NewTestClass"  // Add here
  ]
}
```

### Adding a new failure pattern

Edit `config/diagnostics.json` under `failure_patterns`:
```json
{
  "id": "my_pattern",
  "name": "My Custom Pattern",
  "detection": {
    "keywords": ["myKeyword1", "myKeyword2"],
    "duration_threshold_seconds": 5
  },
  "probable_cause": "...",
  "next_step": "...",
  "severity": "high"
}
```

### Adding a cascade rule

Edit `config/diagnostics.json` under `cascade_rules`:
```json
{
  "broken_tier": 2,
  "cascade_to": [3],
  "reason": "Tier 2 bugs cascade into Tier 3 integration tests"
}
```

## Limitations & future work

**Current:**
- Single-run triage (can't detect regression vs. first-time failure)
- Pattern matching only — doesn't read test code to find untested paths
- Markdown report shows only first 5 failures in detail

**Future enhancements:**
- **`.trx` history** — track results over time, detect regressions
- **Test source analysis** — read test `.cs` files to extract test categories, detect flaky patterns
- **Configurable report depth** — inline all failures, or just summary
- **Slack/email export** — send triage report to team channel
- **Trend tracking** — "this test has failed N times in the last M days"

## See also

- [Testing — conventions & workflow](../../ClaudeWiki/Processes/testing.md)
- [Test runbook](../../ClaudeWiki/Processes/test-runbook.md) — execution order and diagnostic red-flags
- [Test patterns](../../ClaudeWiki/Processes/test-patterns.md) — concurrency-safe authoring
