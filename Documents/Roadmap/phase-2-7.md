# Phase 2.7 — Multi-ROM composition

**Status:** done — QA-verified 2026-07-30 (ticket
[0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md), verdict
[`phase-2-7-verdict.md`](../QA/phase-2-7-verdict.md), **PASS**). All Done-When criteria below
confirmed against source/fault-injection, not ticket prose; one flagged non-blocking process item
(solo-run `latest.trx`, see 0025's Resolution).

**Scope grew 2026-07-26** (see ticket [0029](../ClaudeWiki/Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md)): this phase was originally scoped as *composing against* an existing cross-ROM mediation mechanism from Phase 2.6.5. That mechanism doesn't exist yet — 2.6.5 shipped `IInteropRom` as an empty marker interface, not a working capability broker. Amara (Architect) ruled against carving out a separate phase for the broker: its surface is dictated by what a consuming ROM's rule actually needs, so it has to be designed against Juno's real Grid ROM/D20System split, not in the abstract. **The broker's design and implementation are now part of this phase's scope**, not a prerequisite delivered by 2.6.5. This makes 2.7 materially bigger than originally written; absorbed as a schedule consequence, not re-litigated as a phase-boundary question — see `roadmap.md`'s framing ("dependency order, not a calendar").

**Scope re-adjusted 2026-07-27 (Cartridge pivot, tickets [0039](../ClaudeWiki/Processes/Handoffs/0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md)/[0040](../ClaudeWiki/Processes/Handoffs/0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md)) — ruled by Kofi (PM), no re-sequencing.**
`IInteropRom.Resolve`/`Resolve<T>` is cut entirely — the broker-behavior build the previous paragraph
added back to this phase's scope is now gone, replaced by plain constructor injection in `Program.cs`
(`FleeCommandHandler` takes `IPlayerMovement` directly). That's real scope coming *out*. Against it:
retyping `example/D20SystemExampleC`'s interop project down to bare contract interfaces (not a new
`src/` project — briefly proposed, corrected same day: the interfaces are content, same rule that
already keeps `GridMap`/`D20System` in `example/`), and the previously-unscoped Rulebook-decoupling
work (`LunaFlow.Rulebook` losing its ROM project references — see `roadmap.md`'s standing
constraint). Net effect reads as a wash, if anything smaller than first estimated — same reasoning
applies: **absorbed within this phase, not split into a 2.7.5.** Baraka's ticket
([0037](../ClaudeWiki/Processes/Handoffs/0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md))
is amended in place to redirect the in-flight work, not reopened as a new phase.

## Overview

Phase 2.6 fortifies the ROM/Rulebook boundary with exactly one ROM. `architecture.md` has claimed
pluggability since before any of this was real — "a Rulebook depends on whichever ROM modules it
needs, not one fixed model" — but that claim has never been tested against more than one. This phase
is the first real test: prove a single Rulebook can depend on and coordinate two independently-loaded
ROMs at once.

**Which specific ROMs split apart is deliberately not named here.** That's content-design territory —
Juno's call once she's designing against this phase's real content, not something architecture
prescribes ahead of it. This doc states the mechanism the split has to support, not the split itself.

The design conversation that shaped this phase (2026-07-20) surfaced the mechanism: cross-ROM
interaction should happen entirely through facts and declarative guards, never through one ROM
calling or referencing another directly. A ROM inserts a fact because that's its own concern; another
ROM's rule or handler guards against that fact's presence because that's *its* own concern. Neither
needs to know the other exists. This is the existing Marker/`[StandingRule]` pattern
(`ExitGameMarker`, `.Not<T>()` auto-injection) generalized from same-ROM rule-suppression to the
general answer for cross-ROM coordination.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md).
- Which ROMs actually split apart, and what content exercises the split: [Game Designer
  (Juno)](../ClaudeWiki/Roles/game-designer.md), against real content — same relationship 2.6 has to
  1.d20, not architecture inventing an example in advance.

## Deliverables

- ~~**A shared, ROM-agnostic marker vocabulary in `LunaFlow.Rules`** (alongside `IHasLunaFlowReference`,
  `IMechanic`) that any ROM can produce (insert a fact) or consume (guard against it in a rule or
  handler) without referencing the other ROM's assembly. Start narrow — a small, concrete family of
  markers proven against real cross-ROM cases as they appear, not a fully general "any fact can
  suppress any action" system on the first pass.~~ **Struck 2026-07-26 — superseded by Phase 2.6.5**
  (Arryn's ruling: where the two conflict, 2.6.5 is the authority; ticket
  [0030](../ClaudeWiki/Processes/Handoffs/0030-doc-reconcile-post-2-6-5.md)). This bullet predates
  2.6.5, which considered and **rejected** exactly this placement: content facts, and even abstracted
  domain-flavoured interfaces (`IRoomLocated`, `ISpatialPosition` were both proposed), do not belong
  in `LunaFlow.Rules` — "that project is NRules infrastructure, not a content home, however abstract
  the content gets."

  **The pattern survives; the location doesn't.** Cross-ROM coordination through facts and
  declarative guards — one ROM inserts, another guards via `.Not<T>`, neither knowing the other
  exists — is still the intended interaction model, and it's the generalization of the existing
  Marker/`[StandingRule]` pattern this phase's Overview describes. What 2.6.5 changed is the answer
  to *where the shared type lives*: it comes through the capability mechanism, not through a shared
  vocabulary project. That makes it part of the broker deliverable below, not a separate one.
- **`IRuleBook` grows a declared list of which ROM(s) it depends on** (extends 2.6's minimal shape).
  Loading validates every declared ROM is actually present and errors out clearly on a mismatch —
  fail fast, not silent degradation. **Half-shipped as of 2.6.5, verified 2026-07-26:** the
  *declaration* exists (`IRuleBook.DependsOnRoms`, `IReadOnlyList<string>`, in `LunaFlow.Rulebook`).
  The *validation* was missing as of Lothar's first 2.7 QA pass (ticket
  [0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md)) — filed as
  [0047](../ClaudeWiki/Processes/Handoffs/0047-dependsonroms-validation-not-enforced.md) — and is now
  built (`ComponentRegistry.ValidateDependsOn`) and independently fault-injection-verified by Lothar.
- **Two real, independently-loadable ROMs**, proven by one Rulebook depending on and coordinating
  both, with their only interaction being fact/marker-mediated (no project reference between them).
- **The capability broker itself** — `IInteropRom`'s aggregate/resolve behavior, which Phase 2.6.5
  shipped as an empty marker interface only (see [`phase-2-6-5.md`](phase-2-6-5.md)'s restated
  Done-When). Sized against this phase's real Grid ROM/D20System split, not designed ahead of a
  consumer. Architect's (Amara's) call, per [ticket 0029](../ClaudeWiki/Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md) — no separate 2.6.6 phase for it.
  **Specced 2026-07-26:** [`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md).
  Draft, and gated on one verification spike (does NRules match polymorphically through *interfaces*,
  not just base classes) that has to be run before anything is built on it. The authoring contract is
  stable enough for content design to proceed against in the meantime.
- ~~**Retrofit Nine Rooms onto whichever navigation-shaped ROM this phase produces**, so the split has
  two real consumers proving it, not one content module divided on spec. (Without this, the split is
  tidiness, not fortification — same reasoning 2.6 applied to itself.)~~ **Removed 2026-07-26, on
  Arryn's direct instruction** (ticket [0029](../ClaudeWiki/Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md)):
  Nine Rooms is deprecated and retained for regression-test purposes only; retrofitting it onto the
  new ROM is out of scope. The split's two real consumers are D20System and whatever content Juno
  designs against the Grid ROM directly.
- **A bootstrap contract probe** ([`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md) §5).
  The component model claims a mismatch is a hard failure at bootstrap; that is currently **not true**
  — the CLR resolves types lazily, so a binding failure surfaces at first use, possibly inside a rule
  firing mid-session. The probe exercises each declared contract at startup so the runtime's own
  failure happens at a controlled point, is reported against the offending module, and exits non-zero.
  Cheap, and valuable today: a stale build artefact or a partially rebuilt solution produces exactly
  this failure. Added 2026-07-27 with [`../design/cartridge.md`](../design/cartridge.md).
- **Keep each ROM's actually-consumed public surface small and deliberate** — same instinct as
  `IEngineHost`/`IMechanic` staying thin. A narrow surface breaks less often, independent of whatever
  versioning eventually sits on top of it.
- **A Rulebook must not depend on a specific ROM assembly** (standing constraint, see `roadmap.md`) —
  `LunaFlow.Rulebook`'s two ROM project references (`LunaFlow.Rom`, `example/D20SystemExampleA`) and
  `using LunaFlow.Rom.D20System;` in `OneD20RulebookContent.cs` come out; Rulebook content is
  expressed in Rulebook-owned types that `Program.cs` translates against the Cartridge contract
  surface (`example/D20SystemExampleC`'s interfaces — content, so it stays in `example/`, not a
  `src/` project). Added 2026-07-27 with the Cartridge design ([`design/cartridge.md`](../design/cartridge.md),
  ticket [0039](../ClaudeWiki/Processes/Handoffs/0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md)) —
  real, previously-unscoped work, not a rewording.

**Explicitly deferred, with a named trigger condition:**

- **Full `AEngineComponent`/`EngineTier`/`DependsOn`-ordering machinery.** Nothing in this phase's
  actual content needs startup or lifecycle ordering between ROMs — only fact-level interaction and
  load-time presence-checking, both much lighter. Build the heavier model only once a concrete case
  needs one ROM initialized before another (a Combat ROM genuinely depending on a Physics ROM, per
  `engine-core.md`'s own original example) — still hypothetical, not proven by this phase.
- **ROM versioning/compatibility.** No independent-development trigger exists yet — one team, one
  solution, plain project references; a breaking change surfaces as a same-repo compile error, not a
  version-mismatch problem. When ROMs and Rulebooks are actually maintained independently (different
  people, different release timing) or a second Rulebook needs to pin an older ROM version, the answer
  is standard NuGet/SemVer versioning, using `nbgv` (already wired into this solution) — not a
  bespoke system. Considered and explicitly parked, not forgotten.

## Carried-in watch item from 2.6

- **`D20SystemContent.Configure(...)` — the static, mutable ROM content holder.** ~~Phase 2.6's
  retrofit routes Scenario data into ROM through a static holder the composition root populates
  before rules compile, because NRules-compiled rule code has no constructor/DI seam (ticket
  [0022](../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md), close notes). Re-examine it
  here; if the two-ROM composition proves it clean, leave it, otherwise it's a ticket to Amara at
  that point.~~ **Resolved by deprecation, not re-examination (2.6.5):** the contingent evaluation
  this bullet describes never happens — `Configure` is deprecated outright, superseded by
  `IInteropRom` (see [Phase 2.6.5](phase-2-6-5.md)). Left struck-through rather than deleted so the
  original watch item stays legible.

## Done when

**Updated 2026-07-26** (tickets [0029](../ClaudeWiki/Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md), [0030](../ClaudeWiki/Processes/Handoffs/0030-doc-reconcile-post-2-6-5.md)): Nine Rooms clause struck per Arryn's direct instruction (Kofi); reference rule stated precisely and the marker-vocabulary residue removed (Amara). One clause below still can't be written until the broker's shape lands — flagged inline rather than left vague.

One Rulebook genuinely depends on and coordinates two independently-loadable ROMs.

**The reference rule, stated precisely** (the 2.6.5 follow-up this doc previously owed): neither ROM
references the other's assembly — no mutual reference, no direct peer reference in either direction.
A **one-directional reference from a ROM to the broker** (`IInteropRom`) is expected and governed,
not a violation; that's the whole mechanism. The bar is the peer-to-peer edge, not every edge.

Their interaction is capability-mediated through the broker this phase builds. (The "and/or
fact/marker-mediated" alternative that stood here is gone with the marker-vocabulary deliverable
above — facts are still how ROMs coordinate at runtime, but the shared type reaches them through the
capability mechanism, so it isn't a second, parallel route.)

Removing either ROM from the load list produces a clear, immediate error rather than a silent failure
or a partial game — confirmed by Lothar's independent fault injection (0047): commenting out a ROM's
registration produces the expected validation error, not a silent partial game. ~~Nine Rooms runs
against the shared navigation-shaped ROM alongside whatever new content proves the split.~~

`LunaFlow.Rulebook` names no ROM assembly — `LunaFlow.Rom`/`example/D20SystemExampleA` project
references and `using LunaFlow.Rom.D20System;` are gone, replaced by Rulebook-owned types translated
against the Cartridge contract surface (`example/D20SystemExampleC`) in `Program.cs`. Added
2026-07-27 as part of the standing constraint in `roadmap.md`.

~~**Still to be written here:** the acceptance clause for the broker itself — what "the broker works"
means in verifiable terms. Deliberately left open until its surface is specced against real content
rather than guessed at now; that spec is the next Architect deliverable, and this clause lands with
it.~~ **Moot as of 2026-07-27** (Cartridge pivot, above) — there is no broker to write an acceptance
clause for. `IInteropRom.Resolve` is cut; the one service-shaped dependency (`IPlayerMovement`) is
satisfied by `Program.cs` injecting the concrete implementation directly, which the existing
"neither ROM references the other's assembly" and bootstrap-probe clauses already cover.

## Related phases

- Previous: [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md), via
  [Phase 2.6.5 — ROM identity & cross-ROM interop](phase-2-6-5.md) — the Grid/D20System split landed
  against 2.6.5's `Capability`/`IRom` mechanism (the `IInteropRom.Resolve` broker was cut in the
  Cartridge pivot, above); the Done-When bar's reference rule is worded to match the shipped
  mechanism and QA-verified against it (ticket 0025).
- Next: [Phase 2.8 — World Module extraction](phase-2-8.md)

## Related documents

- [`design/architecture.md`](../design/architecture.md) — the pluggability claim this phase tests
  for the first time
- [`design/engine-core.md`](../design/engine-core.md) — the Marker/Standing-Rule pattern this
  generalizes; the "Two interfaces, not one" / component-model section this phase deliberately
  doesn't build
- [`design/non-goals.md`](../design/non-goals.md) — why full dependency-ordering and ROM versioning
  both stay out here
- [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md) — the `IRuleBook` shape and
  boundary this phase extends
