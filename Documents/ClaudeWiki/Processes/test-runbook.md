# Test Runbook — how to run, filter, and diagnose

The single home for *running* the `LunaFlow.EngineTests` suite: how to invoke it, the tag/filter
convention, execution order, and diagnostics. For the conventions you must follow when *writing*
tests see [`testing.md`](testing.md); for the concurrency-safe authoring patterns see
[`test-patterns.md`](test-patterns.md); for the human/agent split (who runs what, and when) see
[`agent-test-workflow.md`](agent-test-workflow.md).

## Who runs it (the short version)

Per [`agent-test-workflow.md`](agent-test-workflow.md): an agent may run a **narrow one-off**
`--filter` for a quick mid-work yes/no, but **doneness verification and troubleshooting are
user-run** — Arryn runs the pass, the agent triages the `.trx`. Read that page before driving a fix
or sign-off cycle.

## How to run

```bash
dotnet build LunaFlow_A.sln
dotnet test tests/LunaFlow.EngineTests                                            # everything
dotnet test tests/LunaFlow.EngineTests --filter "TestCategory=Rules_GeneratorServices"  # by namespace
dotnet test tests/LunaFlow.EngineTests --filter "FullyQualifiedName~GameEngineCoreTests"  # one class
dotnet test tests/LunaFlow.EngineTests --filter "Name~TurnCounter"              # by feature name
```

Pin the trx path with `--settings LunaFlow_A.runsettings` (see
[`agent-test-workflow.md`](agent-test-workflow.md)) so results land at
`tests/LunaFlow.EngineTests/TestResults/latest.trx`.

## Tag & filter convention

The point of tags is a **focused `.trx`**: run a subset, produce a small report an agent can triage
without wading through the whole suite.

**⚠ MSTest filter syntax is `TestCategory=`, not `Category=`.** This is an MSTest project —
`Category=` is NUnit's property and silently matches *nothing* here. Every filter must use
`TestCategory=...`. (An earlier round of docs used `Category=` throughout, which is why filtering
"didn't work"; corrected everywhere now.)

Combine with `&` (AND) / `|` (OR):

```bash
dotnet test tests/LunaFlow.EngineTests --filter "TestCategory=Rules_GeneratorServices"                     # one namespace
dotnet test tests/LunaFlow.EngineTests --filter "TestCategory=Phase2"                                      # one roadmap phase
dotnet test tests/LunaFlow.EngineTests --filter "TestCategory=Rules_GeneratorServices&TestCategory=Phase2" # pipeline tests in Phase 2
```

The scheme (**namespace + phase**; decided with Nadia — keep it lean, add surgically):

- **Primary — namespace** (which code namespace the tests cover), mirrored from the source so the
  vocabulary can't drift into subjective "component" granularity:
  - `Rules_GeneratorServices` — `LunaFlow.Rules.GeneratorServices` (parser, generator, compiler, export)
  - `Engine_Game` — `LunaFlow.Engine.Game.GameEngineCore` (turn loop, threading, dispatch, lifecycle)
  - `Engine` — broad engine integration (e.g. the Nine Rooms fixture)
- **Secondary — phase** (the roadmap phase the test's subject belongs to): `Phase1`, `Phase1.5`,
  `Phase2`. Roadmap numbering *only* — see the naming note below. This is *what the test covers*, not
  an execution tier: engine-core lifecycle/dispatch tests are `Phase1` even though they run in Tier 2.
- **Surgical — temporary** (an investigation): a self-expiring tag with initials + date, e.g.
  `[TestCategory("DebugParser-2026-07-19")]`. Grep and remove it when the investigation closes. No
  permanent cruft — surgicals never stay in committed code.

One namespace tag + one phase tag per test, applied class-level. No component/`B1–B5`/phantom tags
(`Engine`/`RuleRepository` were once documented but never a convention — dropped).

**Naming note (roadmap phase, not risk tier).** "Phase" as a *tag* means only the roadmap phase.
The execution-order tiers below use words, not numbers, precisely so they can't collide with a
`TestCategory=PhaseN` filter.

## Execution order

Run highest-risk first, so a low-level break surfaces before it cascades.

**Tier 1 — Turn loop & concurrency (highest risk).** Core state machine and thread safety; bugs
here cascade into everything.

- `GameEngineCoreeTurnLoopTests` — turn counter, `TurnTick` events, gate behavior
- `GameEngineCoreThreadSafetyTests` — race conditions, concurrent operations

**Tier 2 — Lifecycle & command dispatch (medium risk).** Depends on Tier 1 being solid.

- `GameEngineCoreTests` — Start/Stop/Restart, options, state initialization
- `GameEngineCoreCommandDispatchTests` — queue ordering, completion signals, exception handling

**Tier 3 — Content & pipeline fixtures.**

- `NineRoomsEngineFixtureTests` — engine loop end-to-end via Nine Rooms (Phase 1.5)
- `RulePipelineGeneratorServiceTests` — the rule-pipeline B1–B5 suite (Phase 2)

**Tier 4 — integration (future).** Broader integration classes (session management, parser/dispatcher
integration) aren't built yet; add them here as they land rather than listing aspirational names.

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
logic error; (4) if it's a consistent failure, trace to the corresponding source. A test taking
>30s almost always means a deadlock or starvation, not slow logic.

## Related pages

- [Testing — conventions & where-to-start](testing.md)
- [Test patterns — authoring concurrency-safe tests](test-patterns.md)
- [Agent-assisted test workflow (who runs what)](agent-test-workflow.md)
- [Tool — `.trx` Triage Engine](../Tools/trx-triage-tool.md) — automatic failure classification and prioritization
- [Wiki style](wiki-style.md)
