# Phase 4 — World content & scripting

**Status:** pending

See [`design/scripting-content.md`](../design/scripting-content.md).

## Overview

Extend the rule pipeline to support authored scripts, not just hand-written C#.

Only worth starting once Phase 2's IR is stable, since this phase's whole point is feeding the same IR from a different parser. Also gated on [Phase 2.5](phase-2-5.md) delivering a working `IMechanic` — scripted content can't reference a dice mechanic that doesn't exist in the engine yet. And on [Phase 2.8](phase-2-8.md) proving a real World Module tier — a World-Builder script is meant to author *that* tier specifically, which only makes sense once it's proven genuinely separable from Rulebook and ROM (and, per that phase, addable alongside other World Modules rather than exclusive of them), not just conventionally organized that way.

## Deliverables

- Commit to a script format (the doc's open question — YAML-ish DSL vs. something else)
- Build the `LunaFlow.Scripting` project:
  - Script parser as a second `IRuleParser`
  - `RuleRuntimeLoader`/`RuleAdapter` as a runtime `IRuleCompiler` target
- Resolve the hot-reload question at least at "reload requires a session restart" level, even if a live-reload version comes later
- Introduce the real scripting-participation type for the [Phase 2.6.5](phase-2-6-5.md) capability
  system (working name `IScript`, deliberately cut from 2.6.5 as premature — see [ticket
  0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md)) and use it as a
  `Capability.ContractType` value. No shape change needed on the capability system itself —
  `ContractType` has been a bare `Type` since 2.6.5, so this is a new value dropped into an
  already-open slot, not a migration.

## Done when

A rule authored in the script format (not C#) fires correctly in a running session.

## Related phases

- Previous: [Phase 2.8 — World Module extraction](phase-2-8.md) (which follows
  [Phase 2.7](phase-2-7.md), [Phase 2.6](phase-2-6.md), and [Phase 2.5](phase-2-5.md), the last of
  which superseded [Phase 3](phase-3.md))
- Parallel: [Phase 5 — Interface expansion](phase-5.md) (can start once Phase 1 is done)
- Next: [Phase 6 — GM AI assistant](phase-6.md)
