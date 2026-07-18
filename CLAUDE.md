# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working conventions

- The user develops in VS2022 on their own machine. **Do not run `git` commands** (commit, add, checkout, etc.) from an agent session against this repo — the user runs git themselves to avoid file-lock conflicts with VS2022. Read-only inspection (`git status`, `git diff`, `git log`) is fine if needed, but leave any state-changing git operation to the user.

## Commands

This is a .NET 9 solution (`LunaFlow_A.sln`) with four projects under `src/` and `tests/`.

```
dotnet build LunaFlow_A.sln                                   # build everything
dotnet test tests/LunaFlow.EngineTests                        # run all tests
dotnet test --filter "FullyQualifiedName~RuleTests.Test2"     # run a single test
dotnet run --project src/MyLunaFlowServer                     # run the console host
```

Tests use MSTest. Execution model and conventions — parallelism, the `[DoNotParallelize]`-for-singleton rule, how to capture rule/handler output, and anti-patterns — live in `Documents/ClaudeWiki/Processes/testing.md` (which points on to `tests/LunaFlow.EngineTests/TEST_EXECUTION_STRATEGY.md`). Read that before writing tests rather than inferring the rules from a single line here.

Versioning is managed by Nerdbank.GitVersioning (`nbgv`, declared in `.config/dotnet-tools.json`, driven by `version.json`). Run `dotnet tool restore` once if `nbgv` isn't available locally.

## Architecture

### Projects

- `src/LunaFlow.Engine` — the runtime: command handling, the game loop/host, and a thin wrapper around NRules (`RuleEngine`).
- `src/LunaFlow.Rules` — the rule-authoring surface: base types for rules (`ARule`), reference/identity model (`ILunaFlowReference`), and `GeneratorServices`, a not-yet-implemented pipeline for compiling author-written rule declarations into NRules `Rule` classes.
- `src/MyLunaFlowServer` — console entry point (`Program.cs`) that wires up and runs the engine. Only references `LunaFlow.Engine`, not `LunaFlow.Rules`.
- `tests/LunaFlow.EngineTests` — MSTest project. Its rule tests build and fire NRules sessions directly against local test rule classes (`tests/LunaFlow.EngineTests/Rules`); they don't exercise `GeneratorServices`.

### Engine host (consolidated)

`LunaFlow.Engine` used to have two independent, non-interoperating "start the game" paths. That's resolved: `EngineHost` and `GameEngine` are gone from the codebase, and `ConsoleEngine` + `GameEngineCore` is the sole host, wired up in `MyLunaFlowServer/Program.cs`.

- `GameEngineCore` (in `Game/GameEngineCore.cs`) is an internal singleton (`GameEngineCore.Instance`) that runs a background thread loop (`RunLoop`) driven by a `BlockingCollection<IGameCommand>` command queue and a manual-reset turn gate (`FinishTurn`/`TurnTick`).
- `ConsoleEngine.Run()` reads a line via `Console.ReadLine()`, parses it with `DefaultCommandParser.Parse` (implemented — splits input into `Name` + `Arguments` on whitespace, first token lowercased), and submits a `DispatchGameCommand` to `GameEngineCore`, blocking on completion before reading the next line.
- `DefaultCommandDispatcher` routes a parsed `ICommand` to a registered `ICommandHandler` by name; `quit`/`exit`/`echo`/`help` are registered and functional in `Program.cs`.
- `GameState` (`TurnNumber`), `GameWorld` (session list), and `Session` (id/translator name/connect time) hold minimal real shape — enough for "what turn is it" and "who's connected," not a full data model yet.

No `NotImplementedException` remains anywhere in this path. What's *not* built yet is the engine-wiring surface `Documents/Specs/nine-rooms-spec.md` calls for on top of this host (a `GameEngineCore.Rules` property, `RuleEngine.Query<T>()`, a public `Post`, unconditional `Fire()` after dispatch) — that's Phase 1.5 work, not a gap in the host consolidation itself.

`Game/GameState.cs`, `Game/GameWorld.cs`, and `Game/Session.cs` are empty placeholder classes (internal, no members yet).

### Rule model (`LunaFlow.Rules`)

- Rules derive from `ARule` (itself `NRules.Fluent.Dsl.Rule` aliased as `NRule` via a global using in `Globals.cs`), and additionally carry an `ILunaFlowReference` for hierarchical identification (`FullPath`/`Path`, parent/subreference links) — the intent is every rule, mechanic, and fact traceable to a dotted reference path like `MyRuleBook.Chapter(A).Subsection(s)`.
- `IGameSystem` → `IRuleBook` → (rules/groups) is the intended top-level organizational hierarchy, though `IRuleBook`/`IRuleGroup` are currently empty marker interfaces.
- `GeneratorServices` defines the shape of a rule-authoring pipeline — `RulePipeline(IRuleParser, IRuleIRGenerator, IRuleCompiler).Compile(RuleParseContext)` — meant to take author-written `[RuleGroup]`/`[RuleTemplate]`-attributed `RulePrototype` declarations and emit compiled NRules rule classes (via an IR stage: `RuleIRGroup` → `RuleCodeUnit`). All types in this namespace are interfaces/DTOs only; no concrete `IRuleParser`/`IRuleIRGenerator`/`IRuleCompiler` implementation exists yet, and `RuleExport` (the intended author-facing entry point) is an empty stub.
- By contrast, `LunaFlow.Engine.RuleEngine` is a working, direct wrapper over NRules (`LoadRulesFromAssembly`/`LoadRulesFromTypes`, `Insert`/`Update`/`Retract`/`Fire`) used as-is by the test suite — it doesn't go through `GeneratorServices` at all.

### `planning/`

`planning/LunaFlow.Rules.content.md` and `planning/chats/*.txt` are raw design-discussion transcripts (largely aspirational/exploratory), not documentation of current behavior. They describe a much larger target architecture (Lua/MoonSharp scripting, an Ollama-backed GM AI assistant, an ASP.NET Core WebSocket interface, a many-project layout under `LunaFlow.ROM`/`.Scripting`/`.Interface.Web`/etc.) that has not been built. Useful for understanding *why* something like `GeneratorServices` or `ILunaFlowReference` is shaped the way it is, but don't treat them as a spec for what exists today.
