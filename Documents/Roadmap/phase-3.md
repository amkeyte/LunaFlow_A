# Phase 3 — Vertical slice

**Status:** superseded by [Phase 2.5](phase-2-5.md) — kept for reference, not deleted. 1.d20 is
expected to prove this phase's done-bar as a side effect of proving the d20 mechanic
infrastructure (see Phase 2.5's supersession note). If that turns out not to hold, revive this
phase rather than reinventing it.

## Overview

The first point where Phases 1 and 2 actually meet: a minimal example rulebook, authored through `GeneratorServices`, running live in the `ConsoleEngine` loop, driven by typed commands.

## Deliverables

- A small `LunaFlow.Examples`-style rule set (a handful of rules, not a real game)
- Commands that insert/retract facts and observe rule output through the CLI

## Done when

You can type a command into the running console host and see a rule-driven response come back, sourced from a generated rule rather than a hand-authored test rule.

## Related phases

- Previous: [Phase 2 — Rule pipeline MVP](phase-2.md)
- Superseded by: [Phase 2.5 — d20 vertical slice & mechanic infrastructure](phase-2-5.md)
- Next (if revived): [Phase 4 — World content & scripting](phase-4.md)
