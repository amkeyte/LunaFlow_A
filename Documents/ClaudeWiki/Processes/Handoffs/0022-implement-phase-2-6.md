# 0022 — Implement Phase 2.6 (ROM/Rulebook boundary fortification)

**Status:** open
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-20
**For:** Dev — Implementation (Baraka)
**Depends on:** [Phase 2.6](../../../Roadmap/phase-2-6.md) (Amara, design complete),
[0021](0021-phase-2-6-spec-alignment.md) (Juno's split confirmation, item 1 — see note below, the
specific design it confirmed has since changed).

**Rewritten 2026-07-20, before any response landed — not a patch.** The original version of this
ticket was built off an earlier draft of `phase-2-6.md` that Amara has since revised after further
design discussion with Arryn. The old draft split by "is the logic generic" (`check`/`fight`
System-tier, `move`/`look`/`flee` Scenario-tier, with a new fact-API layer for the former to route
through). The current `phase-2-6.md` calls that distinction wrong and replaces it with a sharper
one: **direct system access, not genericness, is the dividing line.** All five command handlers
move to ROM. No fact-API layer is needed. If you started work off the old version of this ticket,
stop and re-read `phase-2-6.md` fresh before continuing — don't try to reconcile the two versions
yourself.

## Context

Bigger picture, so this doesn't read as a rules change for its own sake: talking through Phase 4
surfaced that scripted content can't be authored against a C# ecosystem that hasn't actually drawn
the boundaries it claims to have. This phase (and 2.7, 2.8 after it) aren't new architecture so
much as finally building the bones `architecture.md` has described since before any real content
existed — poorly captured until now, not changed. Phase 4 needs that foundation solid before a
World-Builder script has anything real to be scoped against.

1.d20 shipped flat: `D20ResolutionMechanic`, room content, the PC, and the rule group all live
undivided in `LunaFlow.Rom.OneD20`, and handlers call `LunaFlowEngine.Host.Rules` directly. Full
design and reasoning in [`phase-2-6.md`](../../../Roadmap/phase-2-6.md) — read that first, this
ticket doesn't restate the "why," only the "what."

**The dividing line:** a Rulebook never touches `LunaFlow.Engine` primitives directly — no
`ICommandHandler`, no `LunaFlowEngine.Host`, no `RuleEngine` — regardless of whether its specific
logic happens to be reusable. Writing a command handler at all *is* direct system access. All five
of 1.d20's handlers (`move`, `look`, `check`, `fight`, `flee`) are ROM-tier under this principle.

**Note on ticket 0021:** Juno reviewed and confirmed the *previous* System/Scenario split against
her intent in `1-d20.md`. That review is now against a superseded design — not wrong, just
overtaken. This is an architecture-authority correction, not a content-design one, so it likely
doesn't need her formal re-approval, but I'm looping her in separately so she's not carrying a
stale picture. Not blocking you — noted here for the record.

## Ask

1. **Move all five command handlers into the ROM project.** Working namespace
   `LunaFlow.Rom.D20System`, replacing today's flat `LunaFlow.Rom.OneD20`. Parameterize them to
   read Scenario-supplied data (room definitions, combatant stats, flavor-key lookups) rather than
   anything Department-7-specific — `check`/`fight` mostly already do this via `OneD20Content`;
   `move`/`look` need the same treatment relative to grid content. `Room`, `CombatantStats`,
   `CombatRound` — the fact *shapes* — move with the handlers as ROM vocabulary; Scenario only ever
   supplies instances of them.
2. **One ROM project, not two.** Grid/win-line navigation and d20 resolution both land in the same
   `LunaFlow.Rom` assembly for now — don't split them into separately-pluggable modules yet, no
   second consumer wants one without the other. Revisit only if that changes.
3. **Wire up `src/LunaFlow.Rulebook/`.** The project already exists on disk
   (`LunaFlow.Rulebook.csproj`, empty net9.0 class library) but **is not in `LunaFlow_A.sln`** —
   confirmed by grep, zero matches. Add it to the solution as part of this work, don't assume it's
   already wired. Scenario content becomes `LunaFlow.Rulebook.OneD20` there: the 9 rooms' specific
   skill/DC/flavor assignments, the monster's stats, the pregenerated PC's sheet, flavor text — data
   only. References `LunaFlow.Rom` (for the fact shapes and ROM types it instantiates) but never
   `LunaFlow.Engine`, directly or transitively.
4. **No separate fact-API-mediation layer.** Once handlers live entirely in ROM, Scenario has no
   code path that touches facts at all — this falls out of item 1, it isn't its own piece of work.
   If you find yourself building one, stop — that's a sign item 1 isn't actually done.
5. **`IRuleBook` grows minimal real shape** — likely just `IHasLunaFlowReference` plus a declared
   list of which ROM(s) it depends on (static data, not full dependency-ordering machinery — that
   stays deferred per `non-goals.md`). Same "smallest interface that does the job" discipline
   ticket 0015 applied to `IMechanic`.
6. **Reconcile the declarative rule group** (`OneD20RuleGroup.cs`) against the new split — it moves
   to the ROM project too, still pipeline-authored, still subject to Phase 2.5's `GeneratorServices`
   condition. Code only. `1-d20-spec.md` itself is **not your call to edit** — separate ticket to
   Juno, filed once this lands. If the retrofit changes fact shapes enough that the spec will
   clearly be wrong, note it in your response so that ticket is accurate, but don't touch the doc.

**Explicitly not asked of you, don't build it:**

- A generic command-authoring mechanism for Scenarios that want a command no ROM provides. Per
  `phase-2-6.md`: that's the Rulebook author's problem to solve by writing a small ROM, not an
  engine concern.
- Correcting `IEngineHost`'s doc comment or recording the "no Rulebook touches engine primitives"
  rule in `architecture.md`/`engine-core.md` — those are Amara's docs to correct, not yours to edit.
- Host-wiring changes beyond what's needed to run 1.d20 — `Program.cs` referencing `LunaFlow.Engine`
  freely is fine, it's composition-root code, not Rulebook content.

## Done when

1.d20's existing acceptance criteria and the full test suite (96/96 as of
[0020](0020-phase-2-5-final-verdict.md), including 0017's 25 Phase2.5 tests) still pass unchanged.
`LunaFlow.Rulebook` contains **zero** references to `LunaFlow.Engine` — no `ICommandHandler`, no
`LunaFlowEngine.Host`, no `RuleEngine` — and is pure data plus an `IRuleBook` declaration. All
command handlers and declarative rules live in `LunaFlow.Rom` instead.

If any test needs to change to keep passing, that's a red flag worth explaining, not a routine
update — this is a structural retrofit, not a behavior change.

**If anything in `phase-2-6.md` turns out unbuildable or ambiguous**, that's a ticket to Amara, same
channel as always — not a silent workaround.

Repo convention (`CLAUDE.md`): no state-changing git commands.

## Response

_(Baraka to fill in.)_

## Resolution

_(Opener to fill in on close.)_
