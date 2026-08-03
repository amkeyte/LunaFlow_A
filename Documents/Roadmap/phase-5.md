# Phase 5 — Interface expansion

**Status:** pending

See [`design/interfaces.md`](../design/interfaces.md).

## Overview

Extract the CLI interface into a proper project layer and build a WebSocket interface on top of the same engine.

Can start in parallel with Phase 4 once Phase 1 is done, but a WebSocket interface is more useful once there's actually a rulebook worth connecting to (Phase 3+).

## Deliverables

- Extract `LunaFlow.Interface` (shared DTOs) and `LunaFlow.Interface.CLI` (today's `MyLunaFlowServer`) as real projects
- Build `LunaFlow.Interface.Web` as a second `IGameEngineTranslator` implementation against the same `GameEngineCore` singleton

## Done when

A CLI session and a WebSocket client can attach to the same running session simultaneously.

## Related phases

- Previous: [Phase 1 — Engine core consolidation](phase-1.md) (can start here)
- Parallel: [Phase 4 — World content & scripting](phase-4.md)
- Next: [Phase 6 — GM AI assistant](phase-6.md)
