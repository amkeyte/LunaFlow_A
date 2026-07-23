# Tool — `.trx` Triage Engine

Automated classification and prioritization of test failures from MSTest `.trx` files. Parses raw result data into actionable, ranked diagnostics.

## Purpose

After Arryn runs the full test suite, `.trx` contains raw data: test names, outcomes, messages, stack traces. The triage engine converts that into **ranked, actionable diagnostics** by:

1. Classifying failures (timeout, race condition, state sync, assertion, etc.)
2. Enriching with metadata (tier, namespace, phase)
3. Detecting cascading failures (downstream breaks from upstream issues)
4. Ranking by priority (highest-risk tier first)
5. Generating markdown or JSON reports

**Quick win:** If Tier 1 is broken, Tier 2-3 failures are usually cascades. Fix the root cause; cascades resolve automatically.

## Location & usage

**Location:** `Documents/Test/trx-triage/`

**Quick start (PowerShell):**
```powershell
cd C:\_local\source\LunaFlow_A
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings
Documents\Test\trx-triage\triage.ps1
```

**Or Python directly:**
```bash
python3 Documents/Test/trx-triage/triage.py tests/LunaFlow.EngineTests/TestResults/latest.trx
```

**Output:** Ranked failure list by tier, cascade warnings, diagnostics for each (probable cause, next step).

## Who uses it

- **Nadia (Dev — Test Suite):** Runs triage after each full suite, prioritizes fixes by tier, detects cascading failures
- **Lothar (QA):** Uses triage reports to keep verification focused; Python tool keeps his token usage slim
- **Arryn (when needed):** Quick check after a build to see if the run is clean

## Configuration

The engine reads YAML-like JSON configs (not hard-coded):

- **`tiers.json`** — test class → tier mapping; tier risk levels
- **`diagnostics.json`** — failure patterns with detection keywords and thresholds
- **`namespaces.json`** — namespace/phase tag definitions

No recompile needed to update failure patterns — edit JSON and re-run.

## Failure patterns (built-in)

| Pattern | Detection | Probable Cause | Next Step |
| --- | --- | --- | --- |
| **Timeout** | Duration >30s | Deadlock in gate/wait | Check `ManualResetEventSlim` usage |
| **Flaky/Race** | Keywords: flaky, race | Race condition | Run 5-10× in isolation; add sync |
| **State Sync** | Keywords: TurnCounter, sync | State out of sync | Check `AdvanceOneTurnSafe()` thread safety |
| **Queue Corruption** | Keywords: order, queue | Commands out of order | Check `BlockingCollection` semantics |
| **Uncaught Exception** | Stack trace: Exception/throw | Exception not caught | Check `ExecuteCommandSafe()` wrapper |
| **Assertion** | Keywords: Assert, expected | Logic error | Review assertion and test logic |

## Cascade detection

Core insight: A single root cause often triggers downstream failures.

```
Tier 1 broken (turn gate deadlock)
  ↓
Tier 2 fails (command dispatch hangs)
  ↓
Tier 3 fails (rule pipeline times out)
```

Instead of three independent failures, triage flags Tier 2-3 as cascades and prioritizes Tier 1 as the root cause to debug.

## Caching behavior

The engine caches results by `.trx` file hash. Running triage twice on the same `.trx` (without Arryn re-running the suite) uses the cache:

```powershell
# First run: full triage (~1 sec)
.\triage.ps1
# Output: full report

# Second run: cached
.\triage.ps1
# Output: "Already triaged: latest.trx ... (Same results as last run)"
```

Use `--force` to skip cache and re-process:
```powershell
.\triage.ps1 --force
```

## Programmatic use (future)

The engine can be imported as a Python module for dashboards, Slack bots, email reports:

```python
from triage import TriageEngine

engine = TriageEngine(Path("config"))
failures = engine.parse_trx(Path("latest.trx"))
failures = engine.classify_failures(failures)
failures = engine.detect_cascades(failures)
failures = engine.rank_failures(failures)

for f in failures:
    print(f"{f.priority}. {f.test_name}: {f.failure_type}")
```

## Documentation

- **Technical:** [`Documents/Test/trx-triage/README.md`](../../Test/trx-triage/README.md) — architecture, config, patterns, advanced usage
- **Workflow:** [`Documents/Test/trx-triage/WORKFLOW.md`](../../Test/trx-triage/WORKFLOW.md) — Nadia's test-dev workflow using the tool

## Related pages

- [Testing — conventions & workflow](../Processes/testing.md)
- [Test runbook — execution order & diagnostics](../Processes/test-runbook.md)
- [Test patterns — concurrency-safe authoring](../Processes/test-patterns.md)
- [Agent-assisted test workflow](../Processes/agent-test-workflow.md)
- [QA / Verification role](../Roles/qa.md)
