# Phase 2.9 — Lua World Module authoring

**Status:** done (QA PASS). Built per [`Specs/lua-world-module-spec.md`](../Specs/lua-world-module-spec.md)
against the design in [`design/world-module.md`](../design/world-module.md#two-authoring-paths-into-one-contract)
and [`design/scripting-content.md`](../design/scripting-content.md#refinement--the-world-module-tier-collapses-this-split-to-lua-only).
QA-verified 2026-08-03, ticket [0068](../ClaudeWiki/Processes/Handoffs/0068-phase-2-9-qa-gate.md),
verdict [phase-2-9-verdict.md](../QA/phase-2-9-verdict.md).

## Overview

A second, independently-authored way to produce an `IOneD20WorldModule` — Lua via MoonSharp, hosted
in a new `LunaFlow.Scripting` project — alongside the hand-written C# path Phase 2.8 proved. Lua
table literals are a structural superset of JSON, so this collapses `scripting-content.md`'s
JSON/Lua split into "Lua only" for World Module content specifically; see the design doc's
refinement note for what that does and doesn't affect.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — already
  delivered as the spec above.
- Implementation: [Dev (Baraka)](../ClaudeWiki/Roles/dev.md).
- No content-design role needed — the proof script is a verbatim transcription of already-shipped
  content, not new game content.

## Deliverables

- `LunaFlow.Scripting` project with a sandboxed MoonSharp execution primitive.
- `LuaOneD20WorldModule` marshaling a Lua table into `IOneD20WorldModule`'s shape.
- A demonstration script reproducing `DefaultOneD20WorldModule`'s content, diff-verified.
- Two-layer fail-fast: marshal errors, then the existing, unchanged `OneD20WorldModuleValidator`.
- A demonstrated sandbox-escape rejection.
- **No new framework dependency** — no `ComponentRegistry`/`EngineTier` change.

## Done when

A full session runs correctly against a Lua-authored World Module, with zero changes to
`IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, or any ROM — same
acceptance bar Phase 2.8 held, now proven against a third, differently-authored implementation.

## Related phases

- Previous: [Phase 2.8 — World Module extraction](phase-2-8.md)
- Next: [Phase 4 — World content & scripting](phase-4.md), which inherits `LunaFlow.Scripting`
  already stood up and MoonSharp-wired rather than creating it itself.

## Related documents

- [`design/world-module.md`](../design/world-module.md#two-authoring-paths-into-one-contract) — the
  two-authoring-paths section this phase builds
- [`design/scripting-content.md`](../design/scripting-content.md#refinement--the-world-module-tier-collapses-this-split-to-lua-only) —
  the refinement note on how this collapses the JSON/Lua split for World Module content specifically
- [`Specs/lua-world-module-spec.md`](../Specs/lua-world-module-spec.md) — the buildable spec
- [Phase 2.8 — World Module extraction](phase-2-8.md) — the contract and validator this phase
  proves a second authoring path against, unchanged
