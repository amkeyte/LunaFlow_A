# Phase 1.5 — Nine Rooms test rulebook

**Status:** pending

See [`Specs/nine-rooms-spec.md`](Specs/nine-rooms-spec.md), [`Specs/nine-rooms-facts.md`](Specs/nine-rooms-facts.md), and [`Specs/nine-rooms-gameplay.md`](Specs/nine-rooms-gameplay.md).

## Overview

A small, deterministic, hand-authored rulebook (a 3×3 room grid with riddle/math challenges) that exercises Phase 1's plumbing against real content instead of a single synthetic command.

Sits here rather than in Phase 1 or Phase 2 because it needs Phase 1's host/command work to be real but deliberately bypasses `GeneratorServices` — rules are hand-written `ARule` classes loaded the same way the existing test suite already loads its rules, so it doesn't block on Phase 2.

## Deliverables

- Hardcoded C# room content (no JSON/scripting — that's Phase 4)
- Hand-written rules for challenge prompts, answer checking, and win detection via `RoomSolved` facts
- Playable end to end through `ConsoleEngine` + `GameEngineCore`, reached via Phase 1.1's `IEngineHost`/`Engine.Host` surface
- Nine-Rooms-specific engine wiring: an unconditional `Fire()` after every dispatched command, and the three project references from Phase 1.1

## Done when

A full game — move between rooms, answer challenges, complete a tic-tac-toe line, see the game settle on a `WinMarker` — is playable via typed commands through the running console host.

## Note on compilation

Expect a brief non-compiling stretch at first commit, not the whole phase. With Phase 1.1 landing `IEngineHost`/`Query<T>`/`Announce` ahead of time, Nine Rooms content should compile against real engine surface from the start — the remaining gap is just wiring up the three project references, which is normal first-commit setup.

## Related phases

- Previous: [Phase 1.1 — Engine host interface](phase-1-1.md)
- Next: [Phase 2 — Rule pipeline MVP](phase-2.md)
