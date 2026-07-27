# Phase 2.6.5 — ROM identity & cross-ROM interop

**Status:** done (2026-07-23) on the shipped identity/declaration/validation surface — see restated
Done-When below. Design complete (2026-07-21, Arryn + Amara); implementation complete (Baraka,
ticket 0027); test suite green (Nadia, ticket 0028). Build 0 errors, 113/113 tests green, zero
regressions. **No independent QA verdict was produced for this phase** (Phases 1.5, 2, 2.5, and 2.6
all got one); disclosed and ruled on 2026-07-26 as a one-time exception rather than run retroactively
— the shipped surface is small and infrastructure-only, and [ticket
0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md) will verify the
cross-ROM mediation half for real once Phase 2.7 ships actual content to check it against. Going
forward, no phase is marked done without a QA verdict or a disclosed exception exactly like this one
— see [`wiki-style.md`'s Where status lives](../ClaudeWiki/Processes/wiki-style.md#where-status-lives).

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

A second realization during design: `LunaFlow.Rom` and `LunaFlow.Rulebook` are the architectural
layers, not example-content holders that happen to also be usable for more — the fact that today
they hold only D20System/NineRooms and OneD20 is incidental, not their scope. That shaped where
everything below actually lives.

Numbered 2.6.5 rather than folded into 2.7: this is infrastructure Phase 2.7 depends on to close
ticket 0023, not a follow-on to it. 2.7's actual content split (Grid ROM + trimmed D20System) is
blocked on this phase's mechanism existing. Phase 2.6 itself was administratively closed
(2026-07-21) rather than held open pending this phase — see `phase-2-6.md`'s Status note.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — same "content
  forces framework work" relationship Phase 2.5 had with `IMechanic` and Phase 2.6 had with the
  ROM/Rulebook split itself.
- No content design in this phase — Juno's Grid/D20System split content still lands in 2.7, once
  this phase's mechanism exists for it to build on.

## Deliverables

**In `LunaFlow.Rom`:**
- **`IRom`** — real type identity for ROMs, replacing the current folder-and-namespace-only
  convention. Carries publishing metadata (name, credits — **not** version; see ticket
  [0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md), `non-goals.md` already parks
  ROM versioning and nothing in this phase reads one) plus declared `Provides`/`Requires` capability
  lists (via `IHasDependencies`, below).
- **`IInteropRom : IRom`** — explicit marker for a ROM acting as a capability broker: aggregates
  advertised capabilities across loaded ROMs, resolves a peer's `Requires` against them. Kept as
  its own named interface rather than folded into plain `IRom` — any `IRom` could technically play
  this role, but a named interface is a discoverability win for content authors, same reasoning
  ticket 0015 used to keep `IMechanic` separate from `IHasLunaFlowReference` despite being
  structurally identical to it.

**In `LunaFlow.Rulebook`:**
- **`IRuleBook`** (and its direct associations, e.g. `DependsOnRoms`) relocates here from
  `LunaFlow.Rules` — it only lived in Rules because `LunaFlow.Rulebook` didn't exist yet when it
  was conceived. **`IRuleGroup` stays in `LunaFlow.Rules`** — unlike `IRuleBook`, it's used by
  ROM-tier rule authoring too (`D20SystemRuleGroup` already lives in `LunaFlow.Rom`), so it belongs
  with the tier-agnostic NRules-authoring surface, not with one tier's identity type. Don't conflate
  the two moves.
- Implementing `IHasDependencies` (an `LunaFlow.Engine.Components` type, below) from
  `LunaFlow.Rulebook` code is **not** a violation of Phase 2.6's "a Rulebook never touches
  `LunaFlow.Engine` primitives directly" rule. That rule is about *authored content* — what a
  Rulebook Author writes into `OneD20RuleBookContent` — not about the tier's own infrastructure
  type (`IRuleBook` itself) participating in the engine's module system. Worth recording next to
  the original rule in `engine-core.md` so a future session doesn't over-read it as a blanket ban.

**In `LunaFlow.Engine.Components`** (new namespace; not `LunaFlow.Rules` — Rules stays scoped to
NRules authoring, this is core/meta, "literally standing up the modular system that IS the game";
not `.Modules` — collides with `EngineTier.WorldModule`, an already-named, unrelated concept):
- **`Capability`** — `record Capability(string Name, Type ContractType)`. `Name` is enforced-unique
  per capability; `ContractType` required, not optional. Deliberately small, with room to grow more
  metadata (version constraints, description) later without a redesign.
- **`IHasDependencies`** — `{ IReadOnlyList<Capability> Provides { get; } IReadOnlyList<Capability>
  Requires { get; } }`. `IRom` and `IRuleBook` each implement it from their own projects (Rom is
  already Engine-visible; Rulebook's participation is the case the note above covers).
- **`EngineTier`** — tags `IRom`/`IRuleBook` with their tier (Rom < Rulebook < WorldModule <
  GmOverlay < Player) and is real, load-bearing: the validator uses it to decide *how* a declared
  dependency should resolve — same-tier means capability-mediated (`IInteropRom`), downward means a
  direct reference is expected. Peers (same tier) may depend on each other, but only through
  `Capability`/`IInteropRom` — never a direct project reference. This is the mechanism that actually
  closes the case ticket 0023 raised: Grid ROM and D20System stay peers, mediated by a capability,
  zero assembly reference between them. **Not in this phase** (see below): the explicit
  upward-dependency-is-an-error enforcement pass — the 2.7 proof never declares an upward
  dependency, so that check has no failing case to validate against yet.
- **The registry/validator** — explicit, hardcoded registration only (modules listed in
  `Program.cs`, same as today's `RuleEngineOptions.FromAssembly(...)` calls; no reflection-based
  discovery). Runs **two-phase**: collect every registered module's `Provides`/`Requires` first
  (registration order doesn't matter for this phase), then validate the complete graph as a unit —
  clash detection (two providers for the same capability), missing-requirement detection, and a
  cycle check, all against the whole picture rather than incrementally. This is a deliberate,
  hardcoded design choice, not a placeholder — see "Explicitly not this phase" below for why an
  NRules-based version was considered and dropped.
- **Clash handling** — v1 is a hard load-time error when two ROMs advertise the same capability.
  Arbitration between competing providers is named and explicitly deferred; no design commitment
  made toward it.
**Deprecation:**
- **`D20SystemContent.Configure(...)`** — the static, mutable ROM content holder from Phase 2.6
  (ticket 0022 close notes), carried into 2.7 as a watch item — is **deprecated outright as of this
  phase**, not conditionally re-examined as originally planned. `IInteropRom` supersedes it: Scenario
  data reaching ROM content becomes a capability-mediated concern instead of a static field the
  composition root pokes before rule compilation. `phase-2-7.md`'s watch-item bullet is struck
  through with a pointer here, not deleted, so the original reasoning stays legible.

**Coupling discipline, recorded explicitly (`engine-core.md`):** compiled ROM/Rulebook tiers get
the strict discipline above (capability-mediated only, no direct peer reference) because that tier
is where it's actually enforceable; script-authored tiers (World Module/GM Overlay/Player, once
Phase 4 exists) are expected to reference each other directly — "scripts are wild west by design."
The unified system still tracks/promotes those dependencies for bookkeeping, but doesn't gate the
coupling underneath. Deliberate, not an oversight — matches the trust gradient `architecture.md`'s
role table already describes, and the C# tier is where enforcement is actually possible; below
that, it's best-practice-made-easy, not compiler-enforced.

**Explicitly not this phase** (discussed and set aside during design, not forgotten):

- ~~**The upward-dependency-is-an-error enforcement pass** ([ticket 0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md)) —
  `EngineTier` itself is real and load-bearing (see above), but the explicit check that rejects a
  component depending on a higher tier has no failing case in 2.7's content to validate against: Grid
  ROM/D20System are peers, and the one downward edge (Rulebook→Rom) is trivially satisfied. *Revisit
  once WorldModule (Phase 2.8) is a real tier and something actually declares a dependency across
  that boundary — not preemptively.*~~ **Ratified as shipped-early, 2026-07-26** (Amara, ticket
  [0030](../ClaudeWiki/Processes/Handoffs/0030-doc-reconcile-post-2-6-5.md)): this deferral did not
  hold — `ComponentRegistry.Validate()`'s third pass rejects a component whose required capability is
  provided by a higher tier, with a named-components error message, and
  [0028](../ClaudeWiki/Processes/Handoffs/0028-phase-2-6-5-test-suite.md) covers it. **The code is
  ratified, not tolerated:** the check is cheap, correct, and matches what `engine-core.md` has
  claimed since before any of it was real, so removing it to match a deferral would be worse than
  the drift. Recorded here rather than silently corrected because doc and code disagreed in *two*
  directions at once — this pass shipped though deferred, while the specified cycle check did not
  ship at all (fixed since, ticket
  [0031](../ClaudeWiki/Processes/Handoffs/0031-code-currency-vs-2-6-5-design.md)). The original
  reasoning stays struck-through rather than deleted so the judgement that proved wrong is still
  legible.
- **`IScript`** ([ticket 0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md), confirmed
  by Arryn) — cut entirely, not even as a placeholder. `Capability.ContractType` is already a bare
  `Type`; Phase 4 introducing a real scripting type later (expected — "sure to become a new project
  around phase 4 or so," per Arryn) is a new value in an already-open slot, not a shape migration, so
  a reserved marker today doesn't actually prevent the "nub to rebuild around" risk it was proposed
  to guard against. Intent isn't lost, just not built now — see [`phase-4.md`](phase-4.md)'s
  Deliverables for the forward pointer.
- A Roslyn-based dynamic type resolution / interop compiler — real infrastructure, but solves a
  problem (independently-loaded, dynamically-versioned ROMs) this project doesn't have yet.
- An NRules-based dependency-validation pass (model `Provides`/`Requires` as facts, let a rule
  detect mismatches) — fits the project's general instinct to push declarative logic into NRules,
  but the concrete worry (a client's dependency registering before its provider, or a circular
  dependency) is more directly and simply handled by the hardcoded two-phase collect-then-validate
  approach above. Revisit if/when arbitration logic gets real — that's naturally rule-shaped in a
  way "present or not" isn't.
- Arbitration between multiple capability providers for the same capability — v1 errors, doesn't
  arbitrate.
- Rolling `IRuleBook.DependsOnRoms` into the unified `IHasDependencies` shape — it likely survives
  as a distinct case (a direct, downward Rulebook→Rom reference doesn't need capability-mediation
  the way peer-ROM dependencies do), but this is parked, not decided. Pin to revisit once the
  unified system exists for real, if it turns out to make sense.
- Script-to-script dependency tracking shape ("promote but don't gate") — moot until Phase 4
  actually introduces scripting; nothing to design against yet now that `IScript` itself is cut.

## Done when

**Restated 2026-07-26** (see ticket [0029](../ClaudeWiki/Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md)): the original bar below included a cross-ROM mediation clause that could not be evaluated at close — Phase 2.7's content didn't exist yet, and `IInteropRom` shipped as an empty marker interface, not a working broker. That clause is struck through and moves to [`phase-2-7.md`](phase-2-7.md), where it belongs and can actually be verified against real content.

`IRom`, `IInteropRom`, `Capability`, and `IHasDependencies` exist in their respective projects and
are load-time validated by the two-phase registry (a missing required capability, or a duplicate
provider, both fail fast with a clear error; tier-ordering violations are load-time errors too).
~~Grid ROM and D20System (Phase 2.7's split) correlate `PlayerPosition` through a capability with zero
project reference between them — the mechanism the now-administratively-rescinded ticket 0023
originally asked for.~~ `D20SystemContent.Configure` is removed from the codebase, migrated to the new
mechanism. Scope trimmed per [ticket 0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md):
no upward-tier-violation enforcement pass, no `IScript`, no `version` on `IRom` — all three deferred
or cut because 2.7's proof doesn't exercise them.

**What actually shipped and is verified:** ROM/Rulebook identity (`IRom`, `IInteropRom` as marker), capability declaration (`Capability`/`IHasDependencies`/`EngineTier`), and load-time validation (`ComponentRegistry`'s two-phase collect-then-validate — clash detection, missing-requirement detection, tier-ordering; **no cycle check yet**, tracked as a defect against Phase 2.7's peer-ROM shape in [ticket 0031](../ClaudeWiki/Processes/Handoffs/0031-code-currency-vs-2-6-5-design.md)). **What did not ship:** the capability broker itself — `IInteropRom`'s aggregate/resolve behavior. That work moves to Phase 2.7, sized against Juno's real Grid ROM/D20System split rather than designed on zero examples.

## Related phases

- Previous: [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md) — administratively
  closed 2026-07-21; this phase supersedes part of what it shipped (`Configure`).
- Next: [Phase 2.7 — Multi-ROM composition](phase-2-7.md), blocked on this phase's mechanism. 2.7's
  own Done-When bar ("neither ROM references the other's assembly") will need rewording to match the
  real rule (no mutual/direct reference; one-directional client→`IInteropRom` references are
  expected and governed) — tracked as follow-up, not resolved in this doc.

## Related documents

- [`design/engine-core.md`](../design/engine-core.md) — the Component model section this phase
  formalizes and mostly replaces (the `AEngineComponent`/`ARom` sketch); also needs the
  authored-content-vs-infrastructure clarification (Deliverables, `LunaFlow.Rulebook` section above)
  recorded next to the existing 2.6 Rulebook/Engine rule
- [`design/architecture.md`](../design/architecture.md) — the target project layout's description of
  what an external Rulebook consumes is stale once `IRuleBook` lives in `LunaFlow.Rulebook` — it
  currently only lists `LunaFlow.Rules`/`Engine`/`Rom`; needs `LunaFlow.Rulebook` added
- [`design/non-goals.md`](../design/non-goals.md) — "Not building the general ROM/Rulebook
  component model yet" is partially triggered by this phase (identity and dependency
  declaration/validation, not the `IEngineEventSubscriber`/turn-dispatch-ordering half, which stays
  genuinely deferred). "Not building config-driven ROM/Rulebook packaging yet" is **not** triggered
  — registration stays hardcoded in `Program.cs` — correcting an earlier draft of this doc, which
  claimed both were triggered.
- [`phase-2-7.md`](phase-2-7.md) — the Done-When bar this phase's mechanism will require rewording;
  also carries the now-struck-through `Configure` watch item this phase resolves by deprecation
- [Handoff ticket 0023](../ClaudeWiki/Processes/Handoffs/0023-cross-rom-fact-mediation.md) — the
  concrete case that surfaced this gap; administratively rescinded by Kofi rather than closed
  through the normal Response/Resolution flow, since this phase superseded it
- [Handoff ticket 0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md) — Kofi's scope
  pass against this doc's Deliverables; trimmed the upward-tier-enforcement pass, `IScript`, and
  `IRom.version` per the ruling recorded there
