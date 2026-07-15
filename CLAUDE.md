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

Tests use MSTest and run in parallel at method scope (`MSTestSettings.cs` sets `Parallelize(Scope = ExecutionScope.MethodLevel)`), so tests must not share mutable static state.

Versioning is managed by Nerdbank.GitVersioning (`nbgv`, declared in `.config/dotnet-tools.json`, driven by `version.json`). Run `dotnet tool restore` once if `nbgv` isn't available locally.

## Architecture

### Projects

- `src/LunaFlow.Engine` — the runtime: command handling, the game loop/host, and a thin wrapper around NRules (`RuleEngine`).
- `src/LunaFlow.Rules` — the rule-authoring surface: base types for rules (`ARule`), reference/identity model (`ILunaFlowReference`), and `GeneratorServices`, a not-yet-implemented pipeline for compiling author-written rule declarations into NRules `Rule` classes.
- `src/MyLunaFlowServer` — console entry point (`Program.cs`) that wires up and runs the engine. Only references `LunaFlow.Engine`, not `LunaFlow.Rules`.
- `tests/LunaFlow.EngineTests` — MSTest project. Its rule tests build and fire NRules sessions directly against local test rule classes (`tests/LunaFlow.EngineTests/Rules`); they don't exercise `GeneratorServices`.

### Two parallel engine hosts (in-progress state, be aware when editing)

The engine currently has two independent, non-interoperating "start the game" paths living side by side in `LunaFlow.Engine`:

- `EngineHost` + `GameEngine` — the original path. `GameEngine` is marked `[Obsolete]`. This is what `MyLunaFlowServer/Program.cs` currently calls, but `GameEngine.Initialize`/`Execute` are placeholder stubs and `EngineHost.ReadInput` throws `NotImplementedException`, so running the server does not reach a working loop.
- `ConsoleEngine` + `GameEngineCore` — the newer path. `GameEngineCore` (in `Game/GameEngineCore.cs`) is an internal singleton (`GameEngineCore.Instance`) that runs a background thread loop (`RunLoop`) driven by a `BlockingCollection<IGameCommand>` command queue and a manual-reset turn gate (`FinishTurn`/`TurnTick`), rather than the simple synchronous while-loop in `EngineHost`. `ConsoleEngine` wraps this singleton but its own run loop is also incomplete (`ReadInput` throws, the main loop body is commented out).

When touching engine startup/hosting code, check which of these two paths is actually intended before extending it — don't assume `EngineHost` is current just because `Program.cs` uses it today.

Supporting command types (`ICommand`, `ICommandParser`/`DefaultCommandParser`, `ICommandDispatcher`/`DefaultCommandDispatcher`, `ICommandHandler`) are shared by both hosts. `DefaultCommandParser.Parse` is currently an unimplemented stub.

`Game/GameState.cs`, `Game/GameWorld.cs`, and `Game/Session.cs` are empty placeholder classes (internal, no members yet).

### Rule model (`LunaFlow.Rules`)

- Rules derive from `ARule` (itself `NRules.Fluent.Dsl.Rule` aliased as `NRule` via a global using in `Globals.cs`), and additionally carry an `ILunaFlowReference` for hierarchical identification (`FullPath`/`Path`, parent/subreference links) — the intent is every rule, mechanic, and fact traceable to a dotted reference path like `MyRuleBook.Chapter(A).Subsection(s)`.
- `IGameSystem` → `IRuleBook` → (rules/groups) is the intended top-level organizational hierarchy, though `IRuleBook`/`IRuleGroup` are currently empty marker interfaces.
- `GeneratorServices` defines the shape of a rule-authoring pipeline — `RulePipeline(IRuleParser, IRuleIRGenerator, IRuleCompiler).Compile(RuleParseContext)` — meant to take author-written `[RuleGroup]`/`[RuleTemplate]`-attributed `RulePrototype` declarations and emit compiled NRules rule classes (via an IR stage: `RuleIRGroup` → `RuleCodeUnit`). All types in this namespace are interfaces/DTOs only; no concrete `IRuleParser`/`IRuleIRGenerator`/`IRuleCompiler` implementation exists yet, and `RuleExport` (the intended author-facing entry point) is an empty stub.
- By contrast, `LunaFlow.Engine.RuleEngine` is a working, direct wrapper over NRules (`LoadRulesFromAssembly`/`LoadRulesFromTypes`, `Insert`/`Update`/`Retract`/`Fire`) used as-is by the test suite — it doesn't go through `GeneratorServices` at all.

### `planning/`

`planning/LunaFlow.Rules.content.md` and `planning/chats/*.txt` are raw design-discussion transcripts (largely aspirational/exploratory), not documentation of current behavior. They describe a much larger target architecture (Lua/MoonSharp scripting, an Ollama-backed GM AI assistant, an ASP.NET Core WebSocket interface, a many-project layout under `LunaFlow.ROM`/`.Scripting`/`.Interface.Web`/etc.) that has not been built. Useful for understanding *why* something like `GeneratorServices` or `ILunaFlowReference` is shaped the way it is, but don't treat them as a spec for what exists today.
