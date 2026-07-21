# Phase 2.6 — ROM/Rulebook boundary fortification

**Status:** pending

## Overview

`architecture.md` and `1-d20.md` have described a ROM/Rulebook split since before either Nine Rooms
or 1.d20 existed — ROM as the reusable, domain-agnostic toolbox; Rulebook as the specific game built
on top of it, touching ROM only through defined entry points. Neither piece of content shipped so far
has actually drawn that line. Nine Rooms was explicitly exempted (`nine-rooms.md`: "disposable test
content that doesn't get a reusable module boundary"). 1.d20 was supposed to be the first real test —
`1-d20.md`'s own "Scope split: System vs. Scenario" section names it explicitly — but shipped flat:
`D20ResolutionMechanic`, room content, the pregenerated PC, and the rule group all live undivided in
`LunaFlow.Rom.OneD20`, and `CheckCommandHandler`/`FightCommandHandler` call
`LunaFlowEngine.Host.Rules` directly, the same unmediated pattern `engine-core.md`'s "ROM as the
conduit to NRules" section describes as temporary, pre-real-content behavior.

`non-goals.md` defers the general `AEngineComponent`/`ARom`/`EngineTier` component model "until a
second real ROM... needs" it — that condition hasn't actually fired, because nothing built so far
was ever *structured as* a ROM in the first place. This phase doesn't build that general model; it
retrofits the one real candidate for a ROM that already exists (1.d20's System half) into an actual
ROM, so the boundary is proven once against real content before Phase 4 needs to police it against
World-Builder scripts, and before Juno drafts 2.d20 against a split that doesn't yet exist in code.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — same "content
  forces framework work" relationship Phase 2.5 had with `IMechanic`.
- No new content design — this is a structural retrofit of 1.d20, not new Scenario work. Juno's
  2.d20 draft is the next phase to actually exercise the resulting split with fresh content.

## Deliverables

- **Split `LunaFlow.Rom.OneD20`** into System (ROM-tier: six ability scores + modifier formula, the
  universal `d20 + modifier vs. target number` resolution, Defense, saving throws, skill checks,
  attack resolution, the `Strong` archetype's math, `D20ResolutionMechanic`) and Scenario
  (Rulebook-tier: the 9 rooms, DCs, flavor text, the monster stat block, the pregenerated PC) — real
  namespace/project separation, not a prose distinction.
- **A minimal System-owned fact API.** Scenario handlers (`CheckCommandHandler`,
  `FightCommandHandler`, etc.) stop calling `LunaFlowEngine.Host.Rules` directly for facts System
  owns; they go through System's own API instead. First real proof of `engine-core.md`'s "ROM as
  conduit to NRules" pattern — scoped to what 1.d20 actually needs, not a general-purpose ROM
  contract.
- **`IRuleBook` grows minimal real shape** — enough to distinguish Rulebook content from ROM content
  in the type system, mirroring the "smallest interface that does the job" call `IMechanic` got in
  ticket 0015. Explicitly **not** `AEngineComponent`/`ARom`/`EngineTier` — that stays deferred per
  `non-goals.md`; this phase still produces exactly one real ROM, not two.
- Reconcile `1-d20-spec.md` and the declarative rule group against the new split.

## Done when

1.d20's existing acceptance criteria and test suite still pass unchanged, but its rules, facts, and
mechanic are structurally split System vs. Scenario, and Scenario code never calls `RuleEngine`/NRules
directly for anything System owns.

## Related phases

- Previous: [Phase 2.5 — d20 vertical slice & mechanic infrastructure](phase-2-5.md)
- Next: [Phase 4 — World content & scripting](phase-4.md) — inherits a proven ROM/Rulebook boundary
  instead of designing against an unproven one; also still gated on Phase 2.5's `IMechanic` delivery,
  independently of this phase.

## Related documents

- [`design/1-d20.md`](../design/1-d20.md) — the "Scope split: System vs. Scenario" section this
  phase finally executes
- [`design/architecture.md`](../design/architecture.md) — ROM/Rulebook role definitions
- [`design/engine-core.md`](../design/engine-core.md) — "ROM as the conduit to NRules"
- [`design/non-goals.md`](../design/non-goals.md) — why the full component model stays deferred here
- [`Specs/1-d20/1-d20-spec.md`](../Specs/1-d20/1-d20-spec.md) — the spec this phase retrofits
