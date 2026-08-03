# LunaFlow Design Docs

This folder is the formalized system design for LunaFlow, distilled from earlier ChatGPT brainstorming sessions (originally in `planning/`, now archived at `planning/archive/` for historical reference).

**These are target-state design documents, not documentation of current behavior.** For what's actually implemented today, see [`CLAUDE.md`](../CLAUDE.md) at the repo root — it describes the real state of the code, including the parts that are unfinished or duplicated. Each doc here states its own implementation status up front and links back to the relevant part of `CLAUDE.md` where the two overlap.

## Reading order

1. [`roadmap.md`](Roadmap/roadmap.md) — build order for everything below, in dependency-ordered phases with a "done" bar for each. Start here if you're picking up work, not just reading design.
2. [`architecture.md`](design/architecture.md) — the layered architecture, the role/access model that drives it, and the target project layout.
3. [`engine-core.md`](design/engine-core.md) — the game loop, engine hosting, and command pipeline. Covers the current dual-host situation in the code and what consolidating it should look like.
4. [`nine-rooms.md`](NineRooms/nine-rooms.md) — a small, deterministic test rulebook (Roadmap Phase 1.5) for proving the engine core works end to end against real content, without waiting on the rule pipeline.
5. [`rule-pipeline.md`](design/rule-pipeline.md) — the `GeneratorServices` rule-authoring pipeline: how C# rule declarations become NRules rule classes. The most fleshed-out design here.
6. [`scripting-content.md`](design/scripting-content.md) — how world-builders author content in JSON/Lua without a C# compile step, and how that content becomes rules through the same pipeline as (5).
7. [`ai-gm-assistant.md`](design/ai-gm-assistant.md) — the multi-pass AI assistant that helps a GM turn natural-language requests into rule-consistent outcomes.
8. [`interfaces.md`](design/interfaces.md) — CLI and WebSocket interface projects, and the translator seam they plug into on the engine. The thinnest of these docs — mostly open questions.

## Reference

- [`glossary.md`](design/glossary.md) — recurring domain terms, including a couple that collide with NRules/.NET terms (notably "Session" — see the glossary entry before assuming which one a doc means).
- [`non-goals.md`](design/non-goals.md) — what LunaFlow isn't trying to do yet, and what would change that. Worth a skim before a design conversation expands scope.

## How to use these docs

Update the doc, not just the code, when a design decision changes — these are meant to stay authoritative as the project moves rather than being a one-time snapshot. If an implementation ends up diverging from what's written here, prefer updating the doc over letting it go stale; a wrong design doc is worse than no design doc.

Each doc's "Open design questions" section is the actual backlog of undecided things in that area — that's the place to add new questions as they come up, rather than letting them live only in conversation.
