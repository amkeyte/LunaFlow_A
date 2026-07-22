# Phase 2.6 — ROM/Rulebook boundary fortification

**Status:** implementation-complete, QA-deferred — retrofit landed and verified against source
(ticket [0022](../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md)); build 7/7, regression
96/96 unchanged (2026-07-20). Formal Lothar final-verdict is **deferred to post-2.7 per Arryn** —
a one-ROM boundary proves less than the two-ROM shape 2.7 introduces, so the QA spend is held until
the boundary firms. Not "done" in the fully-signed sense phases 1.5/2/2.5 carry.

## Overview

`architecture.md` and `1-d20.md` have described a ROM/Rulebook split since before either Nine Rooms
or 1.d20 existed — ROM as the reusable, domain-agnostic toolbox; Rulebook as the specific game built
on top of it. Neither piece of content shipped so far has actually drawn that line. Nine Rooms was
explicitly exempted (`nine-rooms.md`: "disposable test content that doesn't get a reusable module
boundary"). 1.d20 was supposed to be the first real test — `1-d20.md`'s own "Scope split: System vs.
Scenario" section names it explicitly — but shipped flat, undivided in `LunaFlow.Rom.OneD20`.

**The dividing line, sharpened during this phase's own design discussion (2026-07-20): direct system
access, not "is the logic generic."** A Rulebook never touches `LunaFlow.Engine` primitives directly
— no `ICommandHandler`, no `LunaFlowEngine.Host`, no `RuleEngine` — regardless of whether its specific
logic happens to be reusable elsewhere. Writing a command handler at all *is* direct system access,
by definition. This resolves earlier framing in this phase's own drafting that treated `check`/`fight`
as System-tier because their resolution math was generic while leaving `move`/`look` as Scenario-tier
— wrong distinction. All five of 1.d20's command handlers (`move`, `look`, `check`, `fight`, `flee`)
are ROM-tier under this principle, not because their logic is reusable (though it is), but because
authoring an `ICommandHandler` requires system access a Rulebook shouldn't have. `architecture.md`'s
role table already implied this and was never enforced: the Rulebook Author row lists "read/consume
access to whichever ROM modules the Rulebook depends on" and never mentions Core Library at all.

One doc correction this surfaces, not urgent but worth recording: `IEngineHost`'s own doc comment
currently reads "the engine surface exposed to content assemblies (**ROM today, Rulebook once that
tier exists**)" — written before this principle was sharpened, and now wrong on purpose rather than
just not-yet-true. Rulebook should never reach `IEngineHost` directly, full stop, not "not yet."

`non-goals.md` defers the general `AEngineComponent`/`ARom`/`EngineTier` component model "until a
second real ROM... needs" it — that condition hasn't actually fired, because nothing built so far was
ever *structured as* a ROM in the first place. This phase doesn't build that general model; it
retrofits the one real candidate for a ROM that already exists (1.d20, nearly all of it, per the
principle above) into an actual ROM, so the boundary is proven once against real content before Phase
4 needs to police it against World-Builder scripts, and before Juno drafts 2.d20 against a split that
doesn't yet exist in code.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — same "content
  forces framework work" relationship Phase 2.5 had with `IMechanic`.
- No new content design — this is a structural retrofit of 1.d20, not new Scenario work. Juno's
  2.d20 draft is the next phase to actually exercise the resulting split with fresh content.

## Deliverables

- **Move all five command handlers to a ROM project** (`move`, `look`, `check`, `fight`, `flee`),
  parameterized to read Scenario-supplied data (room definitions, combatant stats, flavor-key
  lookups) rather than anything Department-7-specific — `check`/`fight` already do this via
  `OneD20Content`; `move`/`look` need the same treatment relative to grid content. `Room`,
  `CombatantStats`, `CombatRound` — the fact *shapes* — move with them as ROM vocabulary; Scenario
  only ever supplies instances.
- **One ROM project for now, not two.** Grid/win-line navigation and d20 resolution are genuinely
  different toolboxes, but there's no second real consumer yet wanting one without the other —
  splitting them into separately-pluggable modules now would be the same premature-generalization
  mistake `non-goals.md` names elsewhere. Everything lands in one ROM assembly, still inside the
  existing `LunaFlow.Rom` project (working namespace `LunaFlow.Rom.D20System`, replacing today's flat
  `LunaFlow.Rom.OneD20`); revisit the split if a future System ever wants grid navigation without d20
  math or vice versa.
- **Rulebook side lands in the new `LunaFlow.Rulebook` project** (`src/LunaFlow.Rulebook/`, scaffolded
  and ready — empty net9.0 class library, no project references wired yet). Scenario content becomes
  `LunaFlow.Rulebook.OneD20` there: the 9 rooms' specific skill/DC/flavor assignments, the monster's
  stats, the pregenerated PC's sheet, flavor text — data only, referencing `LunaFlow.Rom` (for the
  fact shapes and ROM types it instantiates) but never `LunaFlow.Engine` directly.
- **No fact-API-mediation layer needed as a separate deliverable.** Once command handlers live
  entirely in ROM, Scenario has no code path that touches facts directly in the first place —
  `engine-core.md`'s "ROM as conduit to NRules" is satisfied as a consequence of the handler move,
  not as its own separate piece of plumbing.
- **`IRuleBook` grows minimal real shape** — enough to distinguish Rulebook content from ROM content
  in the type system, mirroring the "smallest interface that does the job" call `IMechanic` got in
  ticket 0015. Likely just `IHasLunaFlowReference` plus a declared list of which ROM(s) it depends
  on (static data, not the full `AEngineComponent.DependsOn` dependency-ordering machinery — that
  stays deferred; this phase still produces exactly one real ROM, not two).
- **Correct `IEngineHost`'s doc comment** per the Overview note above.
- **Record the "no Rulebook touches engine primitives directly" rule** in `architecture.md` and/or
  `engine-core.md` — currently implied by the role table but never stated as an explicit rule.
- Reconcile `1-d20-spec.md` and the declarative rule group against the new split — the challenge-
  prompt/win-line/`GameEndRule` rules move to the ROM project too (still pipeline-authored, still
  subject to Phase 2.5's load-bearing condition, just relocated).

**Explicitly not a problem, not tracked further:**

- **A Scenario needing a command no ROM provides isn't a gap to design around.** A Rulebook author in
  that position just authors a small, one-command ROM — that's what a ROM is for, and it's already
  where direct system access legitimately lives. No generic command-authoring mechanism needed on top
  of what this phase already builds; if such a ROM's command wants to be more generic, that's the ROM
  author's own design problem to solve (e.g. a string-parameterized command), not an engine concern.
  Considered and dropped, not deferred — nothing here to revisit later.
- Host-wiring (registering the ROM's handlers with the dispatcher, feeding them Scenario's data) is
  composition-root code, not Rulebook-authored content — it can reference `LunaFlow.Engine` freely
  without violating the principle above, same as `Program.cs` already does.

## Done when

1.d20's existing acceptance criteria and test suite still pass unchanged, but `LunaFlow.Rulebook`
contains zero references to `LunaFlow.Engine` — no `ICommandHandler`, no `LunaFlowEngine.Host`, no
`RuleEngine` — and is pure data plus an `IRuleBook` declaration. All command handlers and declarative
rules live in `LunaFlow.Rom` instead.

## Related phases

- Previous: [Phase 2.5 — d20 vertical slice & mechanic infrastructure](phase-2-5.md)
- Next: [Phase 2.6.5 — ROM identity & cross-ROM interop](phase-2-6-5.md) — the ROM-identity/capability
  mechanism ticket 0023 surfaced a need for while drafting [Phase 2.7](phase-2-7.md)'s Grid/D20System
  split; 2.7 itself follows once 2.6.5 lands. [Phase 4](phase-4.md) follows once 2.7/2.8 land, still
  separately gated on Phase 2.5's `IMechanic` delivery.

## Related documents

- [`design/1-d20.md`](../design/1-d20.md) — the "Scope split: System vs. Scenario" section this
  phase finally executes
- [`design/architecture.md`](../design/architecture.md) — ROM/Rulebook role definitions; the
  Rulebook Author access-scope row this phase's principle was already implied by
- [`design/engine-core.md`](../design/engine-core.md) — "ROM as the conduit to NRules"
- [`design/non-goals.md`](../design/non-goals.md) — why the full component model, and the
  Grid/D20System module split, both stay deferred here
- [`Specs/1-d20/1-d20-spec.md`](../Specs/1-d20/1-d20-spec.md) — the spec this phase retrofits
