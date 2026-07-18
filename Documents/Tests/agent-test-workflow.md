# Agent-Assisted Test Workflow

## Purpose

`TEST_EXECUTION_STRATEGY.md` covers *what order* to run tests in and *how* to run them from
VS2022. `testing-strategy.md` covers *what's tested* and the coverage gaps. This doc covers a
third thing: how a Claude Code session and the user split test execution during bug-fixing, so
neither side burns time/tokens on the wrong half of the job.

## Why split it this way

Per `CLAUDE.md`, the user runs the solution from VS2022 and Claude Code sessions don't run `git`
against this repo, to avoid file-lock conflicts. The same reasoning extends naturally to test
runs: VS2022 already owns the build, so it's the natural place to *execute* tests. Claude's side
is *reading and triaging results* — a `dotnet test` console run repeats a full pass/fail line per
test even when filtered, where a structured report lets an agent skip straight to what failed.

**Division of labor:**
- **User, via VS2022:** runs the actual test pass (Test Explorer, a playlist from
  `TEST_EXECUTION_STRATEGY.md`, or the integrated terminal) and produces a `.trx` report.
- **Agent:** reads the `.trx`, extracts only the failed cases (name, message, stack trace),
  and proposes a fix — or, mid-fix, runs a single narrow `dotnet test --filter` itself when it
  needs a quick yes/no answer without interrupting the user.

## Producing a report from VS

**`LunaFlow_A.runsettings`** (repo root) pins this for both entry points — no flags to remember:
`ResultsDirectory` is `tests/LunaFlow.EngineTests/TestResults`, and a `trx` logger always writes
`latest.trx` there, overwriting on each run.

**Terminal:** from VS2022's integrated terminal, or any shell in the repo root:

```bash
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings
```

Add a `--filter` exactly as in `TEST_EXECUTION_STRATEGY.md` to scope it to one phase or class,
e.g.:

```bash
dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings --filter "FullyQualifiedName~GameEngineCoreeTurnLoopTests"
```

**Test Explorer GUI:** Test menu > Configure Run Settings > Select Solution-Wide runsettings File
> `LunaFlow_A.runsettings`. After that, any Test Explorer run (including a playlist) writes to
the same pinned path automatically — no separate export step needed.

**Fixed path convention:** `tests/LunaFlow.EngineTests/TestResults/latest.trx`, always. A single,
known filename means the agent can go read it without being told the path every time. If you want
to keep a history instead of overwriting, override `LogFileName` for that one run (e.g.
`--logger "trx;LogFileName=before-fix.trx"`) and just tell the agent which file to look at.

## How the agent reads it

A `.trx` is XML. Rather than dumping the whole file, the agent should:

1. Grep for `outcome="Failed"` to find failing `<UnitTestResult>` entries.
2. Pull each failure's `<Message>` and `<StackTrace>` — not the passing entries, not the raw
   console mirror.
3. Cross-reference failing test names against `TEST_EXECUTION_STRATEGY.md`'s red-flag table
   (timeout → deadlock, flaky → race condition, etc.) before proposing a fix.

This keeps triage cost proportional to *failures*, not to total test count — the main win over
either side running the full suite unfiltered.

## Escalation ladder during a fix

Same idea whether the agent or the user is driving the run — start narrow, widen only as
confidence grows:

1. Single failing test (`--filter "FullyQualifiedName~Class.Method"`).
2. Its test class, or its `TEST_EXECUTION_STRATEGY.md` phase-mate class.
3. The full `Core` namespace (matches the `GameEngineCoreAll.playlist` scope).
4. Full unfiltered `dotnet test` — reserved for a final check before handing back to the user or
   calling a fix done, not for in-progress iteration.

