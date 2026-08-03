# Interfaces

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

## Shape

Planning names three interface-layer projects, split from `LunaFlow.Engine` so that no particular transport is baked into the engine core:

- `LunaFlow.Interface` — shared messaging contracts (DTOs) used by every concrete interface, so a WebSocket client and a CLI session speak the same command/event shapes.
- `LunaFlow.Interface.CLI` — console host and dev tools. `src/MyLunaFlowServer` is today's stand-in for this project; it isn't yet split out as `LunaFlow.Interface.CLI`, and it references `LunaFlow.Engine` directly rather than through a shared contracts layer.
- `LunaFlow.Interface.Web` — an ASP.NET Core WebSocket server, for a browser or remote client to connect to a running session.

## How this connects to the engine

[`engine-core.md`](engine-core.md) already defines the seam this is meant to plug into: `IGameEngineTranslator`. The engine (`GameEngineCore`) is a singleton; a translator is an adapter between one specific transport and that singleton, submitting `IGameCommand`s and relaying `MessagePosted`/`TurnTick` events back out. `ConsoleEngine` is the first, partial implementation of this pattern. A `LunaFlow.Interface.Web` translator would follow the same shape — accept WebSocket messages, turn them into commands via the shared `LunaFlow.Interface` DTOs and the command pipeline described in `engine-core.md`, and push engine events back down the socket.

Because the engine is a singleton shared across translators, multiple interfaces (a GM's web client and a debug CLI, say) can attach to the *same* running session rather than each owning an independent engine instance.

## Open design questions

Essentially everything below the "these are the three projects and roughly what they're for" level is undecided:

- What `LunaFlow.Interface`'s DTOs actually look like — command/event shapes, versioning, serialization format.
- Session/connection lifecycle for `LunaFlow.Interface.Web`: auth, reconnect behavior, how a WebSocket disconnect maps to translator cleanup.
- Whether `LunaFlow.Interface.CLI` is meant to be a full second translator alongside `LunaFlow.Interface.Web`, or primarily a dev/debug tool now that a web interface exists.
- Where the GM assistant's "Suggestion Display Engine" ([`ai-gm-assistant.md`](ai-gm-assistant.md)) lives relative to this layer — most likely as a client-side concern built on top of `LunaFlow.Interface.Web`, but that hasn't been decided.
