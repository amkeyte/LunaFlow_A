# Phase 0 — Scaffolding

**Status:** done

## Overview

Foundation work: establish projects, prove NRules integration, and formalize design.

## Deliverables

- Solution and four projects exist (`LunaFlow.Engine`, `LunaFlow.Rules`, `MyLunaFlowServer`, `LunaFlow.EngineTests`).
- NRules is wired in directly two ways:
  - `LunaFlow.Engine.RuleEngine` (a working wrapper)
  - Hand-written test rules that prove the `RuleRepository.Load(...).Compile().CreateSession()` path works end to end
- `LunaFlow.Rules.GeneratorServices` exists as interfaces/DTOs only — no implementation.
- Design formalized into `docs/design/`.

## Next phase

[Phase 1 — Engine core consolidation](phase-1.md)
