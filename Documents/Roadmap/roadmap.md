# Roadmap

This orders the design docs into a build sequence. It's **dependency order, not a calendar** — there are no dates because this is a solo/small-team project and dates would just go stale. Each phase names a rough "done" bar so it's obvious when to move to the next one.

## Standing constraints

Requirements that hold across every phase from the point they're adopted, not scoped to one phase's
Done-When bar. Recorded here (not just in the phase that triggered them) so a later phase doesn't
have to rediscover them.

- **A Rulebook must not depend on a specific ROM assembly; a ROM must be replaceable without
  recompiling what sits above it.** Adopted 2026-07-27 per [`design/cartridge.md`](../design/cartridge.md)
  and ticket [0039](../ClaudeWiki/Processes/Handoffs/0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md) —
  a Rulebook binds to a Cartridge's contract surface, never to a ROM's implementation assembly.
  Fixed as of [Phase 2.7](phase-2-7.md) (QA-verified 2026-07-30, ticket
  [0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md)):
  `LunaFlow.Rulebook.csproj` no longer references `LunaFlow.Rom`/`example/D20SystemExampleA`, and
  Rulebook content is translated against the Cartridge contract surface in `Program.cs`.

## Phase sequence

| Phase | Status | Link |
| --- | --- | --- |
| Phase 0 | done | [Scaffolding](phase-0.md) |
| Phase 1 | done | [Engine core consolidation](phase-1.md) |
| Phase 1.1 | done | [Engine host interface](phase-1-1.md) |
| Phase 1.5 | done | [Nine Rooms test rulebook](phase-1-5.md) |
| Phase 2 | done | [Rule pipeline MVP](phase-2.md) |
| Phase 2.5 | done | [d20 vertical slice & mechanic infrastructure](phase-2-5.md) |
| Phase 2.6 | done (QA PASS) | [ROM/Rulebook boundary fortification](phase-2-6.md) |
| Phase 2.6.5 | done | [ROM identity & cross-ROM interop](phase-2-6-5.md) |
| Phase 2.7 | done (QA PASS) | [Multi-ROM composition](phase-2-7.md) |
| Phase 2.7.1 | done (QA PASS) | [Content expansion: Loot ROM, monster roster, room randomization](phase-2-7-1.md) |
| Phase 2.8 | done (QA PASS) | [World Module extraction](phase-2-8.md) |
| Phase 2.9 | done (QA PASS) | [Lua World Module authoring](phase-2-9.md) |
| Phase 2.10 | pending | [Content coverage stress test](phase-2-10.md) — build ticket cut to Baraka |
| Phase 3 | superseded (confirmed) | [Vertical slice](phase-3.md) — superseded by Phase 2.5; the load-bearing condition held, verified by QA |
| Phase 4 | pending | [World content & scripting](phase-4.md) — kickoff design pass needed before a buildable spec exists; see [0064](../ClaudeWiki/Processes/Handoffs/0064-phase-2-9-ready-and-phase-4-scope-correction.md) |
| Phase 5 | pending | [Interface expansion](phase-5.md) (can start at Phase 1) |
| Phase 6 | pending | [GM AI assistant](phase-6.md) |

## Proposals (pre-commitment, not yet ratified)

Pitches for follow-on phases that haven't been reviewed into the phase sequence above yet — see
[`Proposals/_index.md`](Proposals/_index.md). Nothing here is committed; a proposal only lands above
once it's been through Amara/Kofi review and gets its own `phase-N.md`.

## Unscheduled work

- [Player-contributed cosmetic rules](phase-unscheduled.md) — pick up once Phase 3 proves the core loop is fun to extend
