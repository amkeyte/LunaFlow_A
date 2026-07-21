# Phase 4 — World content & scripting

**Status:** pending

See [`design/scripting-content.md`](design/scripting-content.md).

## Overview

Extend the rule pipeline to support authored scripts, not just hand-written C#.

Only worth starting once Phase 2's IR is stable, since this phase's whole point is feeding the same IR from a different parser. Also gated on [Phase 2.5](phase-2-5.md) delivering a working `IMechanic` — scripted content can't reference a dice mechanic that doesn't exist in the engine yet. And on [Phase 2.6](phase-2-6.md) proving a real ROM/Rulebook boundary — a World-Builder script can't be fenced out of "Rulebook- or ROM-level logic" (`architecture.md`'s role table) that isn't distinguishable in code yet.

## Deliverables

- Commit to a script format (the doc's open question — YAML-ish DSL vs. something else)
- Build the `LunaFlow.Scripting` project:
  - Script parser as a second `IRuleParser`
  - `RuleRuntimeLoader`/`RuleAdapter` as a runtime `IRuleCompiler` target
- Resolve the hot-reload question at least at "reload requires a session restart" level, even if a live-reload version comes later

## Done when

A rule authored in the script format (not C#) fires correctly in a running session.

## Related phases

- Previous: [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md) (which follows
  [Phase 2.5](phase-2-5.md), itself superseding [Phase 3](phase-3.md))
- Parallel: [Phase 5 — Interface expansion](phase-5.md) (can start once Phase 1 is done)
- Next: [Phase 6 — GM AI assistant](phase-6.md)
