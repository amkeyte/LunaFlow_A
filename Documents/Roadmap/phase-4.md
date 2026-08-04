# Phase 4 — World content & scripting

**Status:** pending

See [`design/scripting-content.md`](../design/scripting-content.md).

## Overview

Extend the rule pipeline to support authored scripts, not just hand-written C#.

Only worth starting once Phase 2's IR is stable, since this phase's whole point is feeding the same IR from a different parser. Also gated on [Phase 2.5](phase-2-5.md) delivering a working `IMechanic` — scripted content can't reference a dice mechanic that doesn't exist in the engine yet. This phase is about scripted *behavior* compiled into the live rule engine — a different mechanism from scripted *data* authoring a World Module, which [Phase 2.9](phase-2-9.md) proves (Lua via MoonSharp), not Phase 2.8 directly. The multi-World-Module-loaded-at-once question ("addable alongside other World Modules rather than exclusive of them") is a separate, still-deferred concern (`non-goals.md`) that neither this phase nor 2.9 needs — reconsidered and reaffirmed at the 2.8/2.9 gate, not re-litigated here.

**Not yet buildable as a spec, but narrower than before.** [`design/gm-overlay.md`](../design/gm-overlay.md)
(2026-08-03) rules the tier question: the script-driven rule adapter and its `StartCombat`-shaped
content belong to the GM-Overlay tier (`EngineTier.GmOverlay`, already shipped as an enum value, not
yet a registered component), not World Module. Still open before a buildable spec exists: committing
to the rule-DSL script format (declarative `when`/`then` stays the top-level shape per
`gm-overlay.md`'s ruling, but YAML vs. another concrete syntax isn't chosen), the inner expression
grammar for condition/action bodies (custom vs. MoonSharp-backed), and the concrete shape/cost of the
reload primitive. Hot-reload's **mechanism** is now settled — `gm-overlay.md`'s ["How reload actually
works"](../design/gm-overlay.md#how-reload-actually-works-verified-against-nrules-102-source), checked
against NRules 1.0.2 source, found no API to mutate a running session's rule set, so reload can only
mean recompile-a-new-factory-and-replay-facts; the "reload requires a session restart" floor below
turns out to likely be the ceiling too, not a conservative placeholder for something better later.

A kickoff design pass (same shape
[0057](../ClaudeWiki/Processes/Handoffs/0057-phase-2-8-design-package-ready-for-kickoff.md) was for
2.8) is expected once Phase 2.9 is underway — see ticket
[0064](../ClaudeWiki/Processes/Handoffs/0064-phase-2-9-ready-and-phase-4-scope-correction.md). No
Phase 2.10; if that pass surfaces a GM-Overlay-tier build substantial enough to separate from the
rule-DSL pipeline, that's the trigger for a "4.1," not decided preemptively.

## Deliverables

- Commit to a script format (the doc's open question — YAML-ish DSL vs. something else)
- Extend the `LunaFlow.Scripting` project (stood up by [Phase 2.9](phase-2-9.md) for Lua World
  Module authoring — this phase inherits it rather than creating it):
  - Script parser as a second `IRuleParser`
  - `RuleRuntimeLoader`/`RuleAdapter` as a runtime `IRuleCompiler` target
- Build the reload primitive (recompile a session factory from the active rule set — Rulebook plus
  active GM Overlays — create a new session, replay current facts into it, swap `GameEngineCore`'s
  session reference) — the mechanism [`gm-overlay.md`](../design/gm-overlay.md#how-reload-actually-works-verified-against-nrules-102-source)
  rules against NRules source; "session restart" is this operation, not a fallback for it, since
  NRules offers no cheaper alternative to recompile-and-replay
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
