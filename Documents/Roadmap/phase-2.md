# Phase 2 — Rule pipeline MVP

**Status:** done — QA-signed-off 2026-07-19 ([`Documents/QA/phase-2-verdict.md`](../QA/phase-2-verdict.md)).
Build green, 71/71 suite (8 rule-pipeline tests), full B1–B5 automated with no manual-only gap. The
compile-time C# rule-authoring path (parser → generator → in-memory Roslyn compiler → `RuleExport`)
is implemented and behaviorally verified, including B5 parity (generated vs. hand-written rules fire
identically). Delivered via tickets 0007 (design) → 0008 (impl) → 0009 (doc reconcile) → 0010 (B1–B5
suite) → 0011 (verdict).

See [`design/rule-pipeline.md`](../design/rule-pipeline.md).

## Overview

Implement a pipeline that compiles author-written rule declarations into executable `ARule` classes.

This is the highest-value design doc to implement next because the test suite already depends on the *output* shape (an `ARule`-derived class loadable via `RuleRepository`) — the pipeline just needs to produce that shape instead of a human writing it by hand.

## Deliverables

- Implement `IRuleParser` for the compile-time path: reflect over `[RuleGroup]`/`[RuleTemplate]` classes into `RuleIRGroup`
- Implement `IRuleIRGenerator`: `RuleIRGroup` → `RuleCodeUnit`, emitting `ARule`-derived source
- Implement `IRuleCompiler`: in-memory Roslyn compilation to a loadable `Assembly` — required for
  B4 (see [handoff 0007](../ClaudeWiki/Processes/Handoffs/0007-rule-pipeline-design-decisions.md)'s
  D2; a `.cs`-to-disk-only compiler can't satisfy a same-run "loads and fires" test). Needs a new
  `Microsoft.CodeAnalysis.CSharp` package reference on `LunaFlow.Rules`.
- Implement `[StandingRule]` marker injection (the `.Not<T>()` guard), the mechanism that makes the "settling markers" guarantee in `engine-core.md` real
- Wire `RuleExport` as the actual entry point authors call

## Done when

A hand-written `[RuleGroup]` class compiles through the pipeline into a class that loads and fires correctly through the existing `RuleRepository` path — provable as a test alongside the existing `RuleTests.cs`.

## Benchmarks (agreed acceptance milestones)

Staged so progress is measurable, not guessed:

- **B1 — Parser:** a sample `[RuleGroup]` yields a correct `RuleIRGroup`, including `[StandingRule]` types captured into `RuleIR.StandingRules` (unit test on IR shape).
- **B2 — Generator:** a `RuleIR` emits source that compiles.
- **B3 — Marker injection:** the generated `When()` contains the injected `.Not<T>()` guard from `[StandingRule]`.
- **B4 — End-to-end (the done-bar):** a hand-written `[RuleGroup]` → pipeline → generated rule that loads via `RuleRepository` and fires in a session, as a test beside `RuleTests.cs`.
- **B5 — Parity:** a generated rule and an equivalent hand-written rule fire identically on the same facts — the regression anchor proving the generator doesn't subtly diverge.

Note: "benchmarks" here means acceptance milestones, not performance. Generation is dev/build-time; no perf target applies.

## Explicitly out of scope

- The general ROM/Rulebook component model (`engine-core.md`'s `IEngineEventSubscriber`/`AEngineComponent`/tiered registry)
- Config-driven package loading

Both are named non-goals until a second real ROM or Rulebook makes the hardcoded `RuleEngineOptions.FromAssembly(...)` call in `Program.cs` actually insufficient. See `design/non-goals.md`.

## Related phases

- Previous: [Phase 1.5 — Nine Rooms test rulebook](phase-1-5.md)
- Next: [Phase 3 — Vertical slice](phase-3.md)
