# Engine Core

**Status:** partially implemented, in a messy in-between state. See [`CLAUDE.md`](../../CLAUDE.md) for the exact current code layout. This document describes the intended shape of the engine host and loop, and calls out where today's code diverges from it.

## What the engine core owns

Per the role breakdown in [`architecture.md`](architecture.md), the engine core is the Developer's layer: session management, the turn loop, the command pipeline, and the guarantees every rule author can rely on without having to reimplement them. Rulebook authors should never need to know how the loop advances turns or how commands get parsed — they write rules and trust the engine to schedule them correctly.

## Current state vs. target

The code today has **two independent, non-interoperating engine hosts**, which is the most important thing to resolve before building anything new on top of this layer:

- `EngineHost` + `GameEngine` — a synchronous `while (true)` loop over `ReadInput` → `Parser.Parse` → `Dispatcher.Dispatch`. `GameEngine` is marked `[Obsolete]`. This is what `MyLunaFlowServer/Program.cs` calls today, but neither `ReadInput` nor `GameEngine.Initialize`/`Execute` are implemented, so it doesn't run.
- `ConsoleEngine` + `GameEngineCore` — a background-thread loop (`GameEngineCore.RunLoop`) driven by a command queue (`BlockingCollection<IGameCommand>`) and a manual turn gate (`FinishTurn()` / `TurnTick` event), with `GameEngineCore` as an internal singleton (`GameEngineCore.Instance`). This is closer to the target shape described below but is also incomplete (`ConsoleEngine.ReadInput` throws, its loop body is commented out).

The target is a single host built around the `GameEngineCore` model — an explicit command queue plus an explicit turn-advance signal, rather than a tight synchronous loop — because it needs to serve more than one interface (CLI today, WebSocket later, per [`interfaces.md`](interfaces.md)) without those interfaces blocking each other on `Console.ReadLine()`-style calls. Consolidating onto that model, deleting `EngineHost`/`GameEngine`, and wiring `MyLunaFlowServer` to `ConsoleEngine` is the next concrete step here, not a rewrite.

## Translators

`IGameEngineTranslator` is the intended seam between the singleton engine and whatever is driving it — a console session, a WebSocket connection, a test harness. The doc comment on the interface states the model directly: the engine is a singleton, and it starts running when the first translator touches its instance. `ConsoleEngine` is the first (partial) translator implementation; a WebSocket translator would be `LunaFlow.Interface.Web`'s equivalent, submitting `IGameCommand`s to the same `GameEngineCore.Instance` rather than owning a second engine.

## Command pipeline

Commands flow through three roles, already scaffolded in `LunaFlow.Engine.Commands`:

- `ICommandParser` turns raw input (a console line, a WebSocket message) into an `ICommand` (`Name` + `Arguments`). `DefaultCommandParser.Parse` is currently an unimplemented stub — this is the actual blocker for getting a CLI session running end to end, not the engine loop itself.
- `ICommandDispatcher` routes a parsed `ICommand` to a registered `ICommandHandler` by name. `DefaultCommandDispatcher` is implemented and works standalone.
- `ICommandHandler` implementations do the actual game-state mutation for one command name.

This pipeline is shared by both hosts today and should stay host-agnostic — it has no dependency on `Console` or sockets, which is what makes it reusable across interfaces.

## Turn loop and engine guarantees

`GameEngineCore.RunLoop` separates two concerns that a synchronous loop conflates: draining submitted commands (which can happen any time, from any connected client) and advancing a turn (which only happens when something calls `FinishTurn()` and at least one listener is subscribed via `TurnTick`). A loop with no `TurnTick` subscribers still processes commands but never advances turns — this is what lets admin/debug commands work against an otherwise idle engine.

Once rule evaluation is wired into the turn advance, the engine is expected to provide guarantees that individual rules shouldn't have to reimplement by hand:

- **Settling markers** — once a marker fact like an end-of-game condition is present, further rule evaluation for that session stops. Rule authors shouldn't have to add `.Not<ExitGameMarker>()` to every rule by hand; see [`rule-pipeline.md`](rule-pipeline.md) for how the generator is expected to inject this automatically.
- **One-time constraints** — a rule that should fire at most once per fact lifetime.
- **Evaluation phasing** — grouping rules into phases (e.g. via NRules agenda groups) so that, for example, all "reaction" rules resolve before the next command is accepted.

None of this is implemented yet; `RuleEngine` (the direct NRules wrapper in `LunaFlow.Engine`) exposes raw `Insert`/`Update`/`Retract`/`Fire`, with no phasing or marker-injection on top. That's the gap between it and what `GeneratorServices` is meant to produce.

## `GameState`, `GameWorld`, `Session`

These three types in `LunaFlow.Engine.Game` are currently empty placeholders and are the obvious next things to design once the single-host consolidation above is done:

- `Session` — one connected client's context: input history, local state, which translator it's attached to.
- `GameState` — a snapshot of engine state at a point in time, with an eye toward rollback/replay.
- `GameWorld` — persistent, cross-session world data: locations, entities, and other domain-level constructs that outlive a single `Session`.

Their current emptiness is a placeholder, not a design decision — no shape has been agreed for them yet beyond the doc comments already on the classes.
