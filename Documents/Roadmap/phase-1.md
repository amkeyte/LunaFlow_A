# Phase 1 — Engine core consolidation

**Status:** done

See [`design/engine-core.md`](design/engine-core.md) for detail.

## Overview

Consolidate two independent "start the game" paths into a single queue/turn-gate model.

## Deliverables

- `EngineHost` and the obsolete `GameEngine` are deleted
- `ConsoleEngine` + `GameEngineCore` (the queue/turn-gate model) is the sole host, wired up in `MyLunaFlowServer/Program.cs`
- `DefaultCommandParser.Parse` and `ConsoleEngine`'s `ReadInput` (`Console.ReadLine()`) are implemented, so a CLI session accepts a command and gets a response
- `GameState`, `GameWorld`, and `Session` have minimal real shape — turn number, a session list, and per-connection id/translator/connect-time — just enough to hold "what turn is it" and "who's connected," not a full data model yet

## Done when

Running `MyLunaFlowServer` accepts a typed command and dispatches it to a handler, with no `NotImplementedException` in the path.

**Status:** met; verified against code (no `EngineHost`/`GameEngine` remain, no `NotImplementedException` anywhere under `src/`).

## Related phases

- Previous: [Phase 0 — Scaffolding](phase-0.md)
- Next: [Phase 1.1 — Engine host interface](phase-1-1.md)
