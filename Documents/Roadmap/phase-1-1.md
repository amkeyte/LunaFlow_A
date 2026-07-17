# Phase 1.1 — Engine host interface (`IEngineHost`)

**Status:** done — verified against code (`IEngineHost.cs`, `Engine.cs`, `RuleEngine.Query<T>()`, `GameEngineCore : IEngineHost`) and QA sign-off.

See [`Specs/engine-host-spec.md`](Specs/engine-host-spec.md).

## Overview

Build a narrow, purpose-built public surface for content code, instead of widening `GameEngineCore` itself.

`GameEngineCore` is `internal sealed`, so content assemblies (like `LunaFlow.Rom`) can't name it at all, regardless of member accessibility. This phase exposes just what content code needs (`Rules` and `Announce`), while keeping turn-ending/lifecycle control (`FinishTurn`/`Stop`/`RequestStop`) hidden.

Pulled out ahead of Phase 1.5 rather than folded into it, since Nine Rooms' content can't compile against the engine at all until this lands.

## Deliverables

- `RuleEngine.Query<T>()` passthrough
- `IEngineHost` interface (`Rules` property, `Announce` method)
- `GameEngineCore` implements `IEngineHost` via explicit interface implementation
- The public `LunaFlow.Engine.Engine.Host` static gateway (`Engine.Host.Rules`, `Engine.Host.Announce(...)`)
- `nine-rooms-spec.md` updated to reference `IEngineHost`/`Engine.Host` instead of re-deriving

## Done when

- Content code can call `Engine.Host.Rules.Query<T>()` and `Engine.Host.Announce(string)` without naming `GameEngineCore`
- `GameEngineCore` stays `internal sealed` with lifecycle control unreachable from outside `LunaFlow.Engine`
- `ConsoleEngine` is unaffected
- The three `ProjectReference` additions land together at Nine Rooms' first commit in Phase 1.5:
  - `LunaFlow.Rom` → `LunaFlow.Rules`
  - `LunaFlow.Rom` → `LunaFlow.Engine`
  - `MyLunaFlowServer` → `LunaFlow.Rom`

(These references land in Phase 1.5, not here, since `LunaFlow.Rom` has no real content yet.)

## Related phases

- Previous: [Phase 1 — Engine core consolidation](phase-1.md)
- Next: [Phase 1.5 — Nine Rooms test rulebook](phase-1-5.md)
