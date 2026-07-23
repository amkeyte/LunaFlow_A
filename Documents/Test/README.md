# Test Tooling & Utilities

Central hub for LunaFlow test suite tooling, automation, and utilities.

## Contents

### `trx-triage/` — .trx Triage Engine

Automated failure classification and diagnostics for test results.

**What it does:**
- Parses `.trx` files from test runs
- Classifies failures by type (timeout, exception, race condition, state sync, etc.)
- Detects cascading failures (downstream breaks caused by upstream failures)
- Ranks failures by priority (highest-risk tier first)
- Generates markdown or JSON reports

**When to use it:**
- After running the full test suite with `dotnet test`
- To quickly identify what's actually broken vs. cascading noise
- To prioritize fixes (always fix highest tier first)

**Quick start:**
```powershell
cd C:\_local\source\LunaFlow_A
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings
Documents\Test\trx-triage\triage.ps1
```

**Learn more:** [trx-triage/README.md](trx-triage/README.md)

---

## Future tooling

- **Coverage gap analysis** — identify untested code paths in source
- **Handoff ticket generator** — template handoff tickets from failures
- **Test pattern validator** — check new tests against conventions (gates, subscribers, naming)
- **`.trx` history** — track failures over time, detect regressions

---

## Integration with the wiki

Test-related docs live in `Documents/ClaudeWiki/Processes/`:
- [testing.md](../ClaudeWiki/Processes/testing.md) — where tests live, conventions, anti-patterns
- [test-runbook.md](../ClaudeWiki/Processes/test-runbook.md) — how to run, tag/filter, execution order
- [test-patterns.md](../ClaudeWiki/Processes/test-patterns.md) — concurrency-safe patterns
- [agent-test-workflow.md](../ClaudeWiki/Processes/agent-test-workflow.md) — who runs what, `.trx` workflow

This folder is for **automation and tooling**, not docs; docs live in `ClaudeWiki/`.
