# Nine Rooms (Test Rulebook)

**Status:** DEPRECATED as design guidance (2026-07-20) — disposable Phase 1.5 test content, kept
only for the engine-fixture regression coverage in
`tests/LunaFlow.EngineTests/Core/NineRoomsEngineFixtureTests.cs` (not scheduled for removal). This
doc still covers *why* Nine Rooms exists and why it's sequenced where it is, but **do not treat it
or `LunaFlow.Rom.NineRooms` as a current ROM/Rulebook design pattern** — it predates the
ROM/Rulebook split entirely (see [ROM vs. Rulebook layering](#rom-vs-rulebook-layering) below,
itself now superseded by Phase 2.6). For the current pattern, look at `LunaFlow.Rom.D20System` /
`LunaFlow.Rulebook.OneD20` instead, and
[Phase 2.6 — ROM/Rulebook boundary fortification](../Roadmap/phase-2-6.md) for why that split
looks the way it does.

For the actual fact shapes, rules, commands, and engine wiring, see
[`../Specs/nine-rooms-spec.md`](../Specs/nine-rooms-spec.md) — that's the buildable version of
this concept and the one that stays current as implementation decisions get made.

## Purpose

Phase 1 of [`roadmap.md`](roadmap.md) is "done when running `MyLunaFlowServer` accepts a typed
command and dispatches it to a handler, with no `NotImplementedException` in the path" — but
that bar is satisfiable with a single synthetic command and no real content. Nine Rooms exists
to prove the same plumbing (`ICommandParser` → `ICommandDispatcher` → `ICommandHandler` → NRules
facts → rule firing → turn settling) against something that's actually playable, deterministic,
and checkable by hand: a small 3×3 grid of rooms with riddle/math challenges, won by completing
a tic-tac-toe line rather than solving every room.

It deliberately does **not** touch [`rule-pipeline.md`](rule-pipeline.md)'s `GeneratorServices`.
Rules are hand-written `ARule` subclasses, loaded the same way
`tests/LunaFlow.EngineTests/Rules/RuleTests.cs` already loads its test rules
(`RuleRepository.Load(...).Compile().CreateSession()`). That loading contract already works
today per [`CLAUDE.md`](../../CLAUDE.md) — Nine Rooms only needs Phase 1's host/command work to
be real, not Phase 2's generator.

## ROM vs. Rulebook layering

**Superseded 2026-07-20:** this section's reasoning (why Nine Rooms specifically doesn't get its
own ROM module) still holds, but the broader premise it was written against — that no ROM/Rulebook
split had been proven yet — no longer does. Phase 2.6 proved the split for real against 1.d20
(`LunaFlow.Rom.D20System` / `LunaFlow.Rulebook.OneD20`). Read this section as historical context
for Nine Rooms' own layering only, not as current guidance on whether/how to draw that line for new
content.

`architecture.md` draws a real distinction between ROM (the game system — fact vocabulary and
mechanics, meant to be reusable across Rulebooks) and Rulebook (a specific compiled game built
on top of a ROM). Nine Rooms doesn't build on an existing ROM module, and — this is the
resolved version of what used to be an open question here — it doesn't get one written for it
either: everything it needs (`PlayerPosition`, `Room`, `RoomSolved`, the win-line logic) lives
entirely inside its own module (`LunaFlow.Rom.NineRooms`), with no shared ROM-level contract
extracted on its behalf. Grids and win-lines are specific to this one disposable test game, not
a generic capability worth abstracting from a single example. If a second grid-shaped game
actually gets built later, that's the point to look at what the two concrete implementations
share and factor out only that — not before.

## Non-goals for this test rulebook

- No scoring, timers, or attempt penalties.
- No persistence — a session is played start to finish in one run.
- No multiple simultaneous sessions (that's Phase 5 territory).
- No content authored outside C# — no JSON, no Lua.
- Not meant to be "the first real game" — it's disposable test content, not a rulebook worth
  extending later. A real rulebook is expected to live in its own repo per `architecture.md`.

## Roadmap placement

See [`roadmap.md`](roadmap.md) — inserted as **Phase 1.5**, after Phase 1's engine
consolidation and before Phase 2's rule-pipeline work, rather than as part of either phase:

- It can't come before Phase 1 is actually done — it needs a real `DefaultCommandParser`, a
  working dispatcher, and enough of `GameState`/`Session` to hold player position and turn
  state.
- It doesn't need to wait for Phase 2 — it bypasses `GeneratorServices` entirely and loads
  hand-written `ARule` classes exactly the way the existing test suite already does.
- It's a lighter-weight, hand-authored version of what Phase 3's "vertical slice" wants (a small
  rule set, driven by typed commands, running live in the host for multiple turns) — except
  Phase 3 specifically wants that content authored *through* `GeneratorServices`. Nine Rooms
  proves the same host/command/rule-firing loop earlier, without blocking on the pipeline, and
  can double as a manual sanity check once Phase 3's generated version exists.
