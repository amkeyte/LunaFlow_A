# Roadmap

This orders the design docs into a build sequence. It's dependency order, not a calendar — there are no dates because this is a solo/small-team project and dates would just go stale. Each phase names a rough "done" bar so it's obvious when to move to the next one.

## Phase 0 — Scaffolding (done)

- Solution and four projects exist (`LunaFlow.Engine`, `LunaFlow.Rules`, `MyLunaFlowServer`, `LunaFlow.EngineTests`).
- NRules is wired in directly two ways: `LunaFlow.Engine.RuleEngine` (a working wrapper) and hand-written test rules that prove the `RuleRepository.Load(...).Compile().CreateSession()` path works end to end.
- `LunaFlow.Rules.GeneratorServices` exists as interfaces/DTOs only — no implementation.
- Design formalized into `docs/design/`.

## Phase 1 — Engine core consolidation

Blocking everything else: right now there are two non-functional engine hosts, and nothing runs end to end. See [`engine-core.md`](engine-core.md) for the detail.

- Pick one host. The recommendation in `engine-core.md` is `ConsoleEngine` + `GameEngineCore` (the queue/turn-gate model) over `EngineHost` + the obsolete `GameEngine`.
- Delete the losing path rather than leaving it to rot alongside the winner.
- Implement `DefaultCommandParser.Parse` and whichever `ReadInput` the surviving host needs, so a CLI session can accept a command and get a response.
- Give `GameState`, `GameWorld`, and `Session` minimal real shape — just enough to hold "what turn is it" and "who's connected," not a full data model yet.

**Done when:** running `MyLunaFlowServer` accepts a typed command and dispatches it to a handler, with no `NotImplementedException` in the path.

## Phase 2 — Rule pipeline MVP

See [`rule-pipeline.md`](rule-pipeline.md). This is the highest-value design doc to implement next because the test suite already depends on the *output* shape (an `ARule`-derived class loadable via `RuleRepository`) — the pipeline just needs to produce that shape instead of a human writing it by hand.

- Implement `IRuleParser` for the compile-time path: reflect over `[RuleGroup]`/`[RuleTemplate]` classes into `RuleIRGroup`.
- Implement `IRuleIRGenerator`: `RuleIRGroup` → `RuleCodeUnit`, emitting `ARule`-derived source.
- Implement `IRuleCompiler`: at minimum, emit `.cs`; a Roslyn-based `.dll` emit can come after.
- Implement `[StandingRule]` marker injection (the `.Not<T>()` guard), since this is the mechanism that makes the "settling markers" guarantee in `engine-core.md` real instead of aspirational.
- Wire `RuleExport` as the actual entry point authors call.

**Done when:** a hand-written `[RuleGroup]` class compiles through the pipeline into a class that loads and fires correctly through the existing `RuleRepository` path — provable as a test alongside the existing `RuleTests.cs`.

## Phase 3 — Vertical slice

The first point where Phases 1 and 2 actually meet: a minimal example rulebook, authored through `GeneratorServices`, running live in the `ConsoleEngine` loop, driven by typed commands, for a couple of turns.

- A small `LunaFlow.Examples`-style rule set (a handful of rules, not a real game).
- Commands that insert/retract facts and observe rule output through the CLI.

**Done when:** you can type a command into the running console host and see a rule-driven response come back, sourced from a generated rule rather than a hand-authored test rule.

## Phase 4 — World content & scripting

See [`scripting-content.md`](scripting-content.md). Only worth starting once Phase 2's IR is stable, since this phase's whole point is feeding the same IR from a different parser.

- Commit to a script format (the doc's open question — YAML-ish DSL vs. something else).
- Build the `LunaFlow.Scripting` project: script parser as a second `IRuleParser`, `RuleRuntimeLoader`/`RuleAdapter` as a runtime `IRuleCompiler` target.
- Resolve the hot-reload question at least at "reload requires a session restart" level, even if a live-reload version comes later.

**Done when:** a rule authored in the script format (not C#) fires correctly in a running session.

## Phase 5 — Interface expansion

See [`interfaces.md`](interfaces.md). Can start in parallel with Phase 4 once Phase 1 is done, but a WebSocket interface is more useful once there's actually a rulebook worth connecting to (Phase 3+).

- Extract `LunaFlow.Interface` (shared DTOs) and `LunaFlow.Interface.CLI` (today's `MyLunaFlowServer`) as real projects.
- Build `LunaFlow.Interface.Web` as a second `IGameEngineTranslator` implementation against the same `GameEngineCore` singleton.

**Done when:** a CLI session and a WebSocket client can attach to the same running session simultaneously.

## Phase 6 — GM AI assistant

See [`ai-gm-assistant.md`](ai-gm-assistant.md). Deliberately last: the Fact Resolver stage needs a real, queryable `GameState`/`GameWorld` (Phase 1+) and the Suggestion Display Engine needs a real interface to show a preview in (Phase 5).

- `LunaFlow.AI` project, Ollama adapter, the two-pass prompt flow.

**Done when:** a GM can type a natural-language action, see the assistant's resolved facts and suggested outcome, and accept or discard it.

## Explicitly not sequenced here

Player-contributed cosmetic rules (the bottom layer in `architecture.md`'s diagram) aren't given a phase because nothing in the current design work has gone further than naming them. Pick this up once Phase 3 proves the core loop is fun to extend at all — building a sandboxed authoring surface for a game that doesn't exist yet isn't a good use of time.
