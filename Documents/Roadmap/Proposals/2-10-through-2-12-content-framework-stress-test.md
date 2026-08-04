# Proposal — Phases 2.10–2.12: content/framework stress test (post-2.9)

**Status:** proposal — not yet a roadmap phase. Opened for review via
[ticket 0067](../../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md).
Not linked from `roadmap.md` or any index yet — see [Where this doc lives](#where-this-doc-lives-open-question).

**Author:** Juno (Game Designer). **For review by:** Amara (Architect), Kofi (PM).

## Purpose

Assuming Phase 2.9 (Lua World Module authoring) is finished, this proposes three follow-on phases
that deliberately stress-test what 2.5–2.9 built, rather than expanding content for its own sake.
Each candidate item below was chosen because it exercises a fact vocabulary, capability-mechanism
path, or authoring surface that's currently either untouched or only proven in the narrowest case
that made a Done-When bar pass. The goal is to find gaps on purpose, in a controlled way, before
Phase 4 (scripting) inherits whatever these tiers can and can't express today.

This is a **proposal**, not a committed roadmap phase — no Done-When bar here is binding until
Amara and Kofi have looked at it. Numbering (2.10/2.11/2.12) is a suggestion for sequencing, not a
claim on the roadmap.

## Why now, and why split into three

The full candidate list (13 items, discussed in chat 2026-08-03) is too much for one phase to hold
without losing the "done bar" discipline every phase before it kept. It splits cleanly along the
three angles the brainstorm was organized by, and that split also happens to sort by *who's most on
the hook*:

- **2.10** is almost entirely Juno/Baraka — content and fact-vocabulary pressure, low framework
  risk, most likely to ship clean.
- **2.11** is deliberately adversarial toward the capability mechanism — several items are expected
  to either surface a real ticket to Amara or confirm a v1 limitation (arbitration, clash) is still
  fine to live with. Framework-heavy by design.
- **2.12** pressure-tests the Lua World Module path 2.9 only proved narrowly (a diff-verified
  transcription of existing content). Content-plus-authoring-tooling, Baraka-adjacent.

Each phase's items are independent of each other within the phase — nothing here requires all of
them to ship together the way, say, Phase 2.6.5's identity/validation/registry triad did.

## Phase 2.10 — Content coverage stress test

**Ownership:** Juno (content design), Baraka (implementation). No framework work anticipated —
if any item surfaces one, it's a ticket to Amara, same discipline Phase 2.7.1 held to.

**Candidate items:**

1. **A saving-throw room.** Every shipped room is skill-check-or-fight; System kept Fortitude/
   Reflex/Will as a resolution mechanic but nothing built has ever used one. Tests whether `Room`/
   `OneD20ChallengeTemplate`'s shape can express a third challenge type, or whether it's implicitly
   hardwired to "skill vs. DC."
2. **A second weapon or a ranged attack.** `1-d20.md` names this as "a content addition for later,
   not an engine change" — never cashed in. Tests whether `CombatantStats`/attack resolution
   actually generalizes past melee.
3. **A marker that outlives its `CombatRound`.** Every status effect today dies with the fight
   that created it. A persistent effect (e.g. `Poisoned`) crossing a room transition tests the
   Marker/Standing-Rule suppression pattern across a boundary it's never had to cross.
4. **An item with a mechanical effect**, not just an inventory line. `ItemAwarded` is Loot-only
   today — nothing reads it back. Forces the first Loot→D20System correlation in the direction
   opposite what's shipped.
5. **A room engineered to hit the soft-lock gap on purpose.** `1-d20-spec.md`'s open questions
   already flag "no soft-lock detection" as unsolved. Deliberately stacking `RoomFailed`s toward
   zero reachable win lines turns that from a flagged risk into a demonstrated, transcript-able
   dead end — useful either way it lands.

## Phase 2.11 — Cross-ROM capability mechanism stress test

**Ownership:** Juno (content framing), Amara (expected to rule on at least one item — flagged
below). Framework-heavy; treat any item that fails cleanly as a real finding, not a bug to route
around quietly.

**Candidate items:**

6. **Two legitimate providers of the same capability.** v1 hard-errors on any clash; arbitration
   is named and explicitly deferred (`phase-2-6-5.md`). Content where two ROMs plausibly both want
   to provide something tests whether that deferral still holds under real pressure.
7. **A capability relay, not just a sink.** Loot is `Requires`-only today — nothing both consumes
   and produces. A ROM that requires one capability and provides a different one downstream tests
   whether the two-phase collect-then-validate registry handles a three-deep chain, not just a flat
   pair-plus-sink (Phase 2.7.1's proof).
8. **A real mutual dependency (cycle).** The cycle check landed late (ticket 0031) and is
   unit-test-covered only — never content-driven. Two ROMs authored to genuinely need each other
   is the first time the cycle error surfaces to someone actually trying to ship content.

**Flag for Amara specifically:** items 6 and 8 are likely to either (a) confirm current behavior is
fine and this phase just documents it, or (b) generate a real ticket for arbitration/cycle-message
quality. Worth a quick read before scoping, since the "no framework work anticipated" default that
worked for 2.7.1 probably doesn't hold here.

## Phase 2.12 — World Module authoring surface stress test (Lua path)

**Ownership:** Juno (content), Baraka (implementation, `LunaFlow.Scripting`/`LunaFlow.Rulebook.OneD20`).
Builds on 2.9's infrastructure without requiring changes to it, unless item 12 below says otherwise.

**Candidate items:**

9. **A Lua World Module with no C# equivalent to diff against.** 2.9's proof is explicitly a
   verbatim transcription of shipped content. A genuinely new scenario authored directly in Lua
   tests whether the surface is usable for real content design, not just a marshaling demo.
10. **A Lua script using real computation** (loops/conditionals), not a flat `return {...}` table
    literal. `world-module.md` claims this is "the same execution path, just more of it exercised"
    but nothing built exercises the claim — e.g. procedurally generating a monster roster from a
    small parameter table.
11. **Content that outgrows `IOneD20WorldModule`'s fixed shape.** A monster with something beyond
    flat `CombatantStats` (a special attack, an on-hit effect) either forces a Rulebook-tier
    contract change or reveals the ceiling of what a World Module can express without one — worth
    knowing before Phase 4 inherits this same contract shape.
12. **A World Module deliberately built to fail validation, through the Lua path specifically** —
    the two-stage (marshal-error, then `OneD20WorldModuleValidator`) failure mode `world-module.md`
    describes is unit-tested but never exercised by someone actually trying to author bad content.

**Flag:** item 11 is the one most likely to bounce back as a design question rather than a build
task — if it forces a contract change, that's Juno's call to make (Rulebook-tier design), not
Baraka's to guess at, per the usual seam.

## Sequencing notes

- 2.10 has no hard dependency on 2.11/2.12 and could ship first or in parallel — it's the lowest-risk,
  most content-only of the three.
- 2.11's item 7 (capability relay) benefits from 2.10's item 4 (Loot→D20System correlation) landing
  first — they're different mechanisms (fact correlation vs. capability declaration) but similar
  enough in spirit that doing them out of order risks confusing which gap each one is actually
  demonstrating.
- 2.12 has no dependency on the other two and could run fully in parallel.

None of this is a hard ordering constraint — flagged for whoever schedules these, not a ruling.

## Where this doc lives (open question)

There's no existing home for a "proposal" doc type. It's not a design doc (`Documents/design/` —
stable shape, no status, no progress claims), not a content spec (`content-spec-template.md` —
buildable, with fact vocabulary and acceptance criteria; this is pre-spec), and not a roadmap phase
doc (`Documents/Roadmap/` — carries a committed Done-When bar and moves `pending → done` behind a
QA verdict; this hasn't been ratified as a phase yet). `doc-scaffold` only generates tickets and
specs — no `proposal` doc type exists there either, confirming the gap rather than just missing a
convention that already existed.

Parked here rather than decided unilaterally, since a new wiki doc-type is a process convention
call, not a content-design one. Proposed placement, for Amara/Kofi to rule on: a `Documents/Roadmap/
Proposals/` subfolder, holding pre-commitment phase proposals that either get promoted into a real
`phase-N.md` (content moves, doc superseded in place per `wiki-style.md`'s supersession rule) or
get rejected/parked (marked as such, not deleted). This doc is filed at that path pending the
ruling; move it if a different home is decided.

## Related documents

- [Ticket 0067](../../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) — the review request this proposal is attached to
- [`../../design/1-d20.md`](../../design/1-d20.md), [`../../Specs/1-d20/1-d20-spec.md`](../../Specs/1-d20/1-d20-spec.md) — the content this stress-tests
- [`../phase-2-7-1.md`](../phase-2-7-1.md) — Loot ROM, the capability-sink/multi-consumer precedent items 6–8 build on
- [`../phase-2-8.md`](../phase-2-8.md), [`../../design/world-module.md`](../../design/world-module.md) — the World Module tier items 9–12 stress-test
- [`../phase-2-9.md`](../phase-2-9.md) — the Lua authoring path this assumes is finished
- [`../../design/non-goals.md`](../../design/non-goals.md) — capability arbitration and versioning are both named, deferred non-goals items 6/8 press against
- [`../../ClaudeWiki/Processes/wiki-style.md`](../../ClaudeWiki/Processes/wiki-style.md) — status/placement conventions this doc's own placement is pending a ruling against
