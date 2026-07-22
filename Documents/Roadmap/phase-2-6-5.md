# Phase 2.6.5 — ROM identity & cross-ROM interop

**Status:** designing — mechanism agreed in a design conversation between Arryn and Amara
(2026-07-21); several shape questions still open (see below). No code written yet.

## Overview

Ticket [0023](../ClaudeWiki/Processes/Handoffs/0023-cross-rom-fact-mediation.md) surfaced a gap
underneath Phase 2.7's Grid ROM/D20System split: **a ROM is currently just a folder and a
namespace** (`LunaFlow.Rom.D20System`, `LunaFlow.Rom.NineRooms`) — there's no `IRom`/`ARom` type,
so "does ROM A depend on ROM B" has never had anything real to check against. D20System's rule
needs to correlate against a fact Grid ROM owns (`PlayerPosition`), but 2.7's own Done-When bar
requires zero project reference between the two ROMs, and content facts — or even abstracted,
domain-flavored interfaces (`IRoomLocated`, `ISpatialPosition` were both proposed and rejected
during design) — don't belong in `LunaFlow.Rules`. That project is NRules infrastructure, not a
content home, however abstract the content gets. A Roslyn-driven dynamic type registry was also
considered and set aside: it solves independently-loaded, dynamically-versioned ROMs, a problem
this project doesn't have yet (one team, one solution — see `non-goals.md`'s existing versioning
stance).

The actual answer needs three things none of the codebase has today: real ROM identity, a governed
way for peer ROMs to depend on each other without referencing each other's assemblies, and
enforcement of the layer ordering `architecture.md` has claimed since before any of this was real.
This phase builds all three as a system, not a one-off fix for Grid ROM/D20System — the same gap
resurfaces at every tier once Phase 4 scripting exists. `non-goals.md` currently defers both "the
general ROM/Rulebook component model" and "config-driven ROM/Rulebook packaging" until a second
real ROM needs them; this phase is that trigger firing, not a preemptive build.

Numbered 2.6.5 rather than folded into 2.7: this is infrastructure Phase 2.7 depends on to close
ticket 0023, not a follow-on to it. 2.7's actual content split (Grid ROM + trimmed D20System) is
blocked on this phase's mechanism existing.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — same "content
  forces framework work" relationship Phase 2.5 had with `IMechanic` and Phase 2.6 had with the
  ROM/Rulebook split itself.
- No content design in this phase — Juno's Grid/D20System split content still lands in 2.7, once
  this phase's mechanism exists for it to build on.

## Deliverables

- **`IRom`** — real type identity for ROMs. Carries publishing metadata (name, version, credits)
  plus declared `Provides`/`Requires` capability lists.
- **`Capability`** — `record Capability(string Name, Type ContractType)`. `Name` is enforced-unique
  per capability; `ContractType` required, not optional. Deliberately small, with room to grow more
  metadata (version constraints, description) later without a redesign.
- **`IInteropRom : IRom`** — explicit marker for a ROM acting as a capability broker: aggregates
  advertised capabilities across loaded ROMs, resolves a peer's `Requires` against them. Kept as
  its own named interface rather than folded into plain `IRom` — any `IRom` could technically play
  this role, but a named interface is a discoverability win for content authors, same reasoning
  ticket 0015 used to keep `IMechanic` separate from `IHasLunaFlowReference` despite being
  structurally identical to it.
- **Tier-ordering enforcement** — dependencies must flow downward through `EngineTier` (Rom <
  Rulebook < WorldModule < GmOverlay < Player); a component declaring a dependency on a higher tier
  is a load-time error, not a warning. Formalizes the rule `engine-core.md`'s `AEngineComponent`
  sketch already proposed but never enforced. Peers (same tier) may depend on each other, but only
  through `Capability`/`IInteropRom` — never a direct project reference. This is the mechanism that
  actually closes ticket 0023: Grid ROM and D20System stay peers, mediated by a capability, zero
  assembly reference between them.
- **Clash handling** — v1 is a hard load-time error when two ROMs advertise the same capability.
  Arbitration between competing providers is named and explicitly deferred; no design commitment
  made toward it.
- **`IScript`** — placeholder `Capability.ContractType` value reserved for Phase 4 scripting. Built
  no further than the marker itself — no behavior, no parallel component-manifest type — so Phase 4
  has a slot to land in without this system's shape being relitigated or rebuilt around a stub
  later. Not used anywhere until Phase 4 exists.
- **Record the coupling-discipline split in `engine-core.md`**: compiled ROM/Rulebook tiers get the
  strict discipline (capability-mediated only, no direct peer reference) because that tier is where
  it's actually enforceable; script-authored tiers (World Module/GM Overlay/Player, once Phase 4
  exists) are expected to reference each other directly — the unified system still tracks/promotes
  those dependencies for bookkeeping, but doesn't gate the coupling underneath. Deliberate, not an
  oversight — matches the trust gradient `architecture.md`'s role table already describes.

**Explicitly not this phase** (discussed and set aside during design, not forgotten):

- A Roslyn-based dynamic type resolution / interop compiler — real infrastructure, but solves a
  problem (independently-loaded, dynamically-versioned ROMs) this project doesn't have yet.
- Arbitration between multiple capability providers for the same capability — v1 errors, doesn't
  arbitrate.
- Any real Phase 4 scripting behavior behind `IScript` — placeholder only, no scripting engine work
  happens in this phase.

## Open design questions

Tracked here so nothing from the design conversation gets lost before implementation starts:

- Where `IRom`/`IInteropRom`/`Capability`/`IScript` physically live — leaning `LunaFlow.Rules`,
  consistent with `IMechanic`/`IHasLunaFlowReference`/`IRuleBook`, not yet confirmed.
- Whether `IRuleBook.DependsOnRoms` (already shipped, Phase 2.6/2.7) gets absorbed into this system
  or stays a separate, complementary mechanism at the Rulebook level.
- Discovery mechanism for capability providers — explicit hardcoded registration (matching
  `non-goals.md`'s current stance on ROM loading) vs. reflection-based scanning.
- The exact shape of "promote but don't gate" dependency tracking for script-to-script
  relationships once Phase 4 exists.

## Done when

`IRom`, `IInteropRom`, and `Capability` exist and are load-time validated (a missing required
capability, or a duplicate provider, both fail fast with a clear error). Tier-ordering violations
are load-time errors. Grid ROM and D20System (Phase 2.7's split) correlate `PlayerPosition` through
a capability with zero project reference between them, closing ticket 0023. `IScript` exists as an
unused, unimplemented placeholder type.

## Related phases

- Previous: [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md)
- Next: [Phase 2.7 — Multi-ROM composition](phase-2-7.md), blocked on this phase's mechanism —
  ticket 0023 gets its real Response once this lands. 2.7's own Done-When bar ("neither ROM
  references the other's assembly") will need rewording to match the real rule (no mutual/direct
  reference; one-directional client→`IInteropRom` references are expected and governed) — tracked
  as follow-up, not resolved in this doc.

## Related documents

- [`design/engine-core.md`](../design/engine-core.md) — the Component model section this phase
  formalizes and mostly replaces (the `AEngineComponent`/`ARom` sketch)
- [`design/non-goals.md`](../design/non-goals.md) — "Not building the general ROM/Rulebook
  component model yet" and "Not building config-driven ROM/Rulebook packaging yet," both actively
  triggered by this phase, need reconciling once it lands
- [`phase-2-7.md`](phase-2-7.md) — the Done-When bar this phase's mechanism will require rewording
- [Handoff ticket 0023](../ClaudeWiki/Processes/Handoffs/0023-cross-rom-fact-mediation.md) — the
  concrete case that surfaced this gap
