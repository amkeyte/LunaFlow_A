# Phase 2.11.1 — Rulebook content extraction

**Status:** pending — spec complete, not yet built. Ticket [0082](../ClaudeWiki/Processes/Handoffs/0082-implement-phase-2-11-1-rulebook-content-extraction-spec-md.md)
filed to Baraka. Built per
[`Specs/rulebook-content-extraction-spec.md`](../Specs/rulebook-content-extraction-spec.md).

## Overview

`src/LunaFlow.Rulebook` never got the treatment `src/LunaFlow.Rom` already went through in Phase 2.7:
shedding real content out to a dedicated `example/` project, leaving only tier infrastructure behind.
Today it holds one infrastructure file (`IRuleBook.cs`) and the entire OneD20 Rulebook's content side
by side. This phase brings the Rulebook tier into line with the ROM tier's existing discipline — the
same content/infrastructure separation `D20System`, `GridMap`, `Loot`, and `Reputation` already prove
out, extended one tier up.

Numbered as a sub-phase off Phase 2.11 rather than folded into Phase 2.12: it's a zero-behavior-change
structural move, not new content or a new mechanism, and doesn't block or depend on 2.12's Lua
authoring-surface work.

**Explicitly not this phase:** any change to `IRuleBook`'s shape, any move of Rulebook content into the
D20 Cartridge's implementation (considered and rejected — see the spec's Purpose section), and the
larger question of Rulebook eventually becoming a Cartridge-held tier itself. That's a named future
direction, not scoped here — no second Rulebook exists yet to design it against.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — spec authored
  directly, no Game Designer content involved (this isn't new content).
- Implementation: [Dev (Baraka)](../ClaudeWiki/Roles/dev.md), against the spec.

## Deliverables

- New project `example/OneD20/OneD20.csproj`, holding everything currently under
  `src/LunaFlow.Rulebook/OneD20/` — twelve source files plus the embedded `DefaultOneD20WorldModule.lua`
  resource.
- `src/LunaFlow.Rulebook` reduced to `IRuleBook.cs`, matching `src/LunaFlow.Rom`'s infra-plus-nothing
  shape (Nine Rooms aside, per that project's own disclosed exception).
- `MyLunaFlowServer` and `LunaFlow.ContentTests` repointed to the new project; zero source changes,
  since every moved type keeps its `LunaFlow.Rulebook.OneD20` namespace.
- `architecture.md`'s target project layout updated to list the new project.

## Done when

All acceptance criteria in `rulebook-content-extraction-spec.md` are met: the new project builds and
holds exactly the moved content, `src/LunaFlow.Rulebook` holds only `IRuleBook.cs`, no namespace or
consumer source changed, full suite green read from an Arryn-run `latest.trx`, and
`architecture.md`'s layout section updated.

## Related phases

- Previous: [Phase 2.11 — Cross-ROM capability mechanism stress test](phase-2-11.md)
- Sibling: Phase 2.12 (Lua World Module authoring-surface stress test) — independent scope, still
  proposal-stage and not yet ratified into this sequence table as of this phase's creation (see
  [ticket 0081](../ClaudeWiki/Processes/Handoffs/0081-phase-2-12-spec-ready-for-ratification-read.md));
  this phase doesn't block it.
- Next: none scheduled against this phase specifically.

## Related documents

- [`Specs/rulebook-content-extraction-spec.md`](../Specs/rulebook-content-extraction-spec.md) — the
  buildable spec
- [`design/architecture.md`](../design/architecture.md#target-project-layout) — the target layout this
  phase brings `LunaFlow.Rulebook` into line with
- [`design/cartridge.md`](../design/cartridge.md) — why the extracted content lands in its own
  `example/` project rather than inside the D20 Cartridge's implementation
