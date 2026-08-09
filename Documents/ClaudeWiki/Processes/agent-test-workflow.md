# Agent-Assisted Test Workflow

How a Claude session and the user (Arryn) split test execution during bug-fixing, so neither side
burns time or tokens on the wrong half of the job — and so the user stays hands-on with the test
results rather than delegating the whole loop away.

Sibling pages cover the other two concerns: [`test-runbook.md`](test-runbook.md) — what order to
run tests in and how to run them; [`testing.md`](testing.md) — what's tested, the conventions, and
coverage gaps. This page is the third thing: the human/agent division of labor.

## Why split it this way

Per `CLAUDE.md`, the user runs the solution from VS2022 and agent sessions don't run `git` against
this repo, to avoid file-lock conflicts. The same reasoning extends to test runs: VS2022 already
owns the build, so it's the natural place to execute tests. The agent's side is reading and
triaging results — a raw `dotnet test` console run repeats a pass/fail line per test even when
filtered, whereas a structured `.trx` report lets the agent skip straight to what failed.

This also keeps the user in the loop by design: they run the pass and see the results first-hand,
which is a deliberate way to stay on the pulse of the codebase rather than handing the whole cycle
to an agent.

## Division of labor

- **User, via VS2022:** runs the actual test pass (Test Explorer or the integrated terminal) and
  produces a `.trx` report.
- **Agent:** reads the `.trx`, extracts only the failed cases (name, message, stack trace), and
  proposes a fix — or, mid-fix, runs a single narrow `dotnet test --filter` itself when it needs a
  quick yes/no without interrupting the user.

## When each side runs (hard rule)

Arryn stays in the testing loop by design — it's a primary way he keeps a finger on the project
pulse. So:

- **Agent may run solo:** a **narrow one-off** `--filter` for a quick mid-work yes/no (one test, one
  class, one component). Cheap, disposable, doesn't need the user.
- **User runs (hands-on):** any **doneness verification** — "everything's done," "all green," a phase
  sign-off, or anything claiming the suite passes — **and any troubleshooting/diagnosis session**.
  These are exactly the moments that matter, so the user runs the pass (or is in it) and the agent
  triages the resulting `.trx`.

Rule of thumb: a throwaway check is the agent's; a claim about the suite's *state*, or a hunt for
what's broken, is the user's to run.

**QA touchpoint:** this reaches [Lothar (QA)](../Roles/qa.md)'s final verdicts. A phase-doneness
verdict is the "everything's done" case above — so its suite run is **user-run**: Arryn runs it,
QA/the agent reads the `.trx` and writes the verdict from it, rather than the agent running the full
suite solo and reporting a number.

## Producing a report from VS

**Two test projects as of ticket 0053 (2026-08-02).** Content/ROM tests split out of
`LunaFlow.EngineTests` into their own `LunaFlow.ContentTests` project, and each has its own
runsettings file pinning its own output path — running both with the *same* settings file writes
both to the same `latest.trx`, silently overwriting whichever ran second. Use the matching file for
whichever project you're running:

- `LunaFlow_A.runsettings` → `tests/LunaFlow.EngineTests/TestResults/latest.trx`
- `LunaFlow_A.ContentTests.runsettings` → `tests/LunaFlow.ContentTests/TestResults/latest.trx`

**Terminal** — from VS2022's integrated terminal, or any shell in the repo root:

```bash
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings
dotnet test tests/LunaFlow.ContentTests --settings LunaFlow_A.ContentTests.runsettings
```

Add a `--filter` exactly as in [`test-runbook.md`](test-runbook.md) to scope either one to one
phase or class, e.g.:

```bash
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests"
```

**Test Explorer GUI** — Test menu → Configure Run Settings → Select Solution-Wide runsettings File
→ whichever of the two files matches the project you want captured this run; only one is active at
a time. **Confirmed hands-on 2026-08-08 (Phase 2.11 QA gate, ticket 0080):** a single "Run All Tests"
pass does merge both projects into one `.trx`, landing at whichever project's output path the active
runsettings file pointed to — that run's `latest.trx` showed 181 total results (93 EngineTests + 88
ContentTests), not just the one project its path implied. Treat a single merged file as valid
full-suite evidence as long as the total count is cross-checked against both projects' known counts,
same as that gate did — don't assume it's only the named project's tests.

## Fixed path convention

Two known paths now, one per test project — not one:

- `tests/LunaFlow.EngineTests/TestResults/latest.trx`
- `tests/LunaFlow.ContentTests/TestResults/latest.trx`

A **"full suite green" claim needs both**, not just the one that happens to be freshest. To keep a
history instead of overwriting either, override `LogFileName` for that one run (e.g.
`--logger "trx;LogFileName=before-fix.trx"`) and tell the agent which file to read.

## How the agent reads it

**Automated path (recommended):** Use the [`.trx` Triage Engine](../Tools/trx-triage-tool.md) — runs in seconds, classifies failures by type (timeout, race, state-sync, etc.), detects cascades, ranks by priority, generates a ranked markdown report. Much leaner than hand-reading XML.

```powershell
Documents\Test\trx-triage\triage.ps1
```

**Manual path (if needed):** A `.trx` is XML. To triage by hand:

1. Grep for `outcome="Failed"` to find failing `<UnitTestResult>` entries.
2. Pull each failure's `<Message>` and `<StackTrace>` — not the passing entries, not the raw console mirror.
3. Cross-reference failing test names against [`test-runbook.md`](test-runbook.md)'s diagnostic red-flag table (timeout → deadlock, flaky → race condition, etc.) before proposing a fix.

Either way, this keeps triage cost proportional to failures, not to total test count — the main win over either side running the full suite unfiltered.

## Escalation ladder during a fix

Same idea whether the agent or the user is driving the run — start narrow, widen only as
confidence grows:

1. Single failing test (`--filter "FullyQualifiedName~Class.Method"`).
2. Its test class, or its [`test-runbook.md`](test-runbook.md) phase-mate class.
3. The full `Core` namespace (`--filter "FullyQualifiedName~LunaFlow.EngineTests.Core"`).
4. Full unfiltered `dotnet test` on **both** projects — reserved for a final check before handing
   back to the user or calling a fix done, not for in-progress iteration.

## Related pages

- [ClaudeWiki index](../index.md)
- [Testing — conventions & workflow](testing.md)
- [Test runbook — execution order & diagnostics](test-runbook.md)
- [Test patterns — authoring concurrency-safe tests](test-patterns.md)
- [Tool — `.trx` Triage Engine](../Tools/trx-triage-tool.md) — automatic failure classification and prioritization
