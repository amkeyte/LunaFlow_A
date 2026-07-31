# 0053 — Split content tests into their own test project

**Status:** open
**Opened by:** Kofi (PM)
**Opened:** 2026-07-30
**For:** Nadia (Test Dev)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

**Not blocking, not tied to a roadmap phase — backlog item, raised by Arryn.** All tests today live
in one project, `tests/LunaFlow.EngineTests`, with framework tests (`Core/`, `GeneratorServices/`)
and content/ROM tests (`Rom/`) side by side. That `Rom/` folder is already growing — Phase 2.7 added
GridMap/D20System suites, Phase 2.7.1 (ticket
[0052](0052-implement-loot-rom-content-expansion-spec-md.md)) adds Loot on top — and every future
content phase adds more, in the same project as the framework's own tests.

### Ask

Scope out (don't build yet, unless it's genuinely low-risk to do now) splitting content/ROM tests
into their own test project, separate from framework tests. Goal: being able to run "just the
content tests" or "just the framework tests" without a `--filter` on one shared project, and keeping
content-test growth from bloating the project that proves the engine/rule-pipeline itself. Consider:
whether this is a straight namespace/folder move to a new `tests/LunaFlow.ContentTests`-shaped
project, whether `Core`/`GeneratorServices` truly have zero content dependency today (worth checking
before assuming a clean split), and how this interacts with the `[DoNotParallelize]`-for-singleton
convention in `testing.md`. Report back with a plan; this doesn't need to ship before Phase 2.7.1 or
anything else in flight.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
