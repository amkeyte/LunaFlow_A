# Phase 2 — Rule pipeline MVP

**Status:** pending

See [`design/rule-pipeline.md`](design/rule-pipeline.md).

## Overview

Implement a pipeline that compiles author-written rule declarations into executable `ARule` classes.

This is the highest-value design doc to implement next because the test suite already depends on the *output* shape (an `ARule`-derived class loadable via `RuleRepository`) — the pipeline just needs to produce that shape instead of a human writing it by hand.

## Deliverables

- Implement `IRuleParser` for the compile-time path: reflect over `[RuleGroup]`/`[RuleTemplate]` classes into `RuleIRGroup`
- Implement `IRuleIRGenerator`: `RuleIRGroup` → `RuleCodeUnit`, emitting `ARule`-derived source
- Implement `IRuleCompiler`: at minimum, emit `.cs`; a Roslyn-based `.dll` emit can come after
- Implement `[StandingRule]` marker injection (the `.Not<T>()` guard), the mechanism that makes the "settling markers" guarantee in `engine-core.md` real
- Wire `RuleExport` as the actual entry point authors call

## Done when

A hand-written `[RuleGroup]` class compiles through the pipeline into a class that loads and fires correctly through the existing `RuleRepository` path — provable as a test alongside the existing `RuleTests.cs`.

## Explicitly out of scope

- The general ROM/Rulebook component model (`engine-core.md`'s `IEngineEventSubscriber`/`AEngineComponent`/tiered registry)
- Config-driven package loading

Both are named non-goals until a second real ROM or Rulebook makes the hardcoded `RuleEngineOptions.FromAssembly(...)` call in `Program.cs` actually insufficient. See `design/non-goals.md`.

## Related phases

- Previous: [Phase 1.5 — Nine Rooms test rulebook](phase-1-5.md)
- Next: [Phase 3 — Vertical slice](phase-3.md)
