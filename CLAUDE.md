# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working conventions

- The user develops in VS2022 on their own machine. **Do not run `git` commands** (commit, add, checkout, etc.) from an agent session against this repo — the user runs git themselves to avoid file-lock conflicts with VS2022. Read-only inspection (`git status`, `git diff`, `git log`) is fine if needed, but leave any state-changing git operation to the user.

## Commands

This is a .NET 9 solution (`LunaFlow_A.sln`) with five projects under `src/`, a test project under `tests/`, and a `Documents` project (docs + API-reference generation).

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
- `src/LunaFlow.Rules` — the rule-authoring surface: base types for rules (`ARule`), reference/identity model (`ILunaFlowReference`/`IHasLunaFlowReference`), the `IMechanic` marker (Phase 2.5), the `IRuleBook`/`IRuleGroup` tier interfaces, and `GeneratorServices`, the **implemented** rule-authoring pipeline (see Rule model below).
- `src/LunaFlow.Rom` — the ROM tier (the reusable game-system toolbox; "direct system access" lives here — a ROM may reference `LunaFlow.Engine` directly). Contains `D20System` (the d20 vertical slice from Phase 2.5, retrofitted into a real ROM in Phase 2.6: the five command handlers `move`/`look`/`check`/`fight`/`flee`, `D20ResolutionMechanic`, the fact shapes, and the pipeline-authored declarative rule group) and `NineRooms` (earlier disposable test content). References `LunaFlow.Engine` and `LunaFlow.Rules`.
- `src/LunaFlow.Rulebook` — the Rulebook tier (a specific game's Scenario **data**, built on a ROM). `LunaFlow.Rulebook.OneD20` holds the 9 rooms' skill/DC/flavor assignments, the PC and monster stat blocks, and flavor text — pure data plus an `IRuleBook` declaration. References `LunaFlow.Rom` only; **no Rulebook source touches a `LunaFlow.Engine` primitive** (`Engine.dll` is present transitively via ROM, which is correct — ROM is the conduit). This boundary is the subject of Phase 2.6; see `Documents/Roadmap/phase-2-6.md`.
- `src/MyLunaFlowServer` — console entry point (`Program.cs`) that wires up and runs the engine. As the composition root it references all four content/framework projects (`LunaFlow.Engine`, `LunaFlow.Rules`, `LunaFlow.Rom`, `LunaFlow.Rulebook`) and may touch `LunaFlow.Engine` freely — composition-root code is exempt from the Rulebook boundary above.
- `tests/LunaFlow.EngineTests` — MSTest project. Engine/rule fixture tests build and fire NRules sessions directly against local test rule classes (`tests/LunaFlow.EngineTests/Rules`); separate suites exercise `GeneratorServices` (Phase 2 B1–B5) and the d20 ROM handlers/rules (`Rom/D20System/`).

### Engine host (consolidated)

`LunaFlow.Engine` used to have two independent, non-interoperating "start the game" paths. That's resolved: `EngineHost` and `GameEngine` are gone from the codebase, and `ConsoleEngine` + `GameEngineCore` is the sole host, wired up in `MyLunaFlowServer/Program.cs`.

- `GameEngineCore` (in `Game/GameEngineCore.cs`) is an internal singleton (`GameEngineCore.Instance`) that runs a background thread loop (`RunLoop`) driven by a `BlockingCollection<IGameCommand>` command queue and a manual-reset turn gate (`FinishTurn`/`TurnTick`).
- `ConsoleEngine.Run()` reads a line via `Console.ReadLine()`, parses it with `DefaultCommandParser.Parse` (implemented — splits input into `Name` + `Arguments` on whitespace, first token lowercased), and submits a `DispatchGameCommand` to `GameEngineCore`, blocking on completion before reading the next line.
- `DefaultCommandDispatcher` routes a parsed `ICommand` to a registered `ICommandHandler` by name; `quit`/`exit`/`echo`/`help` are registered and functional in `Program.cs`.
- `GameState` (`TurnNumber`), `GameWorld` (session list), and `Session` (id/translator name/connect time) hold minimal real shape — enough for "what turn is it" and "who's connected," not a full data model yet.

No `NotImplementedException` remains anywhere in this path. What's *not* built yet is the engine-wiring surface `Documents/Specs/nine-rooms-spec.md` calls for on top of this host (a `GameEngineCore.Rules` property, `RuleEngine.Query<T>()`, a public `Post`, unconditional `Fire()` after dispatch) — that's Phase 1.5 work, not a gap in the host consolidation itself.

`Game/GameState.cs`, `Game/GameWorld.cs`, and `Game/Session.cs` are internal classes with minimal real shape — `GameState.TurnNumber`, `GameWorld`'s `Session` list (`AddSession`/`RemoveSession`), and `Session`'s `Id`/`TranslatorName`/`ConnectedAt` — enough for "what turn is it" and "who's connected," not a full data model yet.

### Rule model (`LunaFlow.Rules`)

- Rules derive from `ARule` (itself `NRules.Fluent.Dsl.Rule` aliased as `NRule` via a global using in `Globals.cs`), and additionally carry an `ILunaFlowReference` for hierarchical identification (`FullPath`/`Path`, parent/subreference links) — the intent is every rule, mechanic, and fact traceable to a dotted reference path like `MyRuleBook.Chapter(A).Subsection(s)`.
- `IGameSystem` → `IRuleBook` → (rules/groups) is the intended top-level organizational hierarchy. `IRuleBook` now carries real shape (`IHasLunaFlowReference` + `IReadOnlyList<string> DependsOnRoms`, added in Phase 2.6); `IRuleGroup` is still an empty marker interface.
- `GeneratorServices` is the **implemented** rule-authoring pipeline (Phase 2). `RuleExport` (the author-facing entry point) wires `ReflectionRuleParser` → `RuleCodeGenerator` → `RoslynRuleCompiler` through `RulePipeline`; `RuleExport.Compile(Type)` takes a `[RuleGroup]`-attributed class and emits a loadable NRules `Assembly` (via an IR stage: `RuleIRGroup` → `RuleCodeUnit`, Roslyn-compiled in memory). One group per parse call — `CompileAll(Assembly)` yields one assembly per group. The authoritative design detail lives in `Documents/design/rule-pipeline.md`; treat that (not this summary) as the spec.
- By contrast, `LunaFlow.Engine.RuleEngine` is a working, direct wrapper over NRules (`LoadRulesFromAssembly`/`LoadRulesFromTypes`, `Insert`/`Update`/`Retract`/`Fire`) used as-is by the test suite — it doesn't go through `GeneratorServices` at all.

### `planning/`

`planning/LunaFlow.Rules.content.md` and `planning/chats/*.txt` are raw design-discussion transcripts (largely aspirational/exploratory), not documentation of current behavior. They describe a much larger target architecture (Lua/MoonSharp scripting, an Ollama-backed GM AI assistant, an ASP.NET Core WebSocket interface, a many-project layout under `LunaFlow.ROM`/`.Scripting`/`.Interface.Web`/etc.) that has not been built. Useful for understanding *why* something like `GeneratorServices` or `ILunaFlowReference` is shaped the way it is, but don't treat them as a spec for what exists today.
