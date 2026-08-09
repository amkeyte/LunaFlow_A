# Proposal — Phases 2.10–2.12: content/framework stress test (post-2.9)

**Status:** fully done. **2.10** — see [`../phase-2-10.md`](../phase-2-10.md), QA PASS, ticket
[0074](../../ClaudeWiki/Processes/Handoffs/0074-phase-2-10-qa-gate.md). **2.11** — see
[`../phase-2-11.md`](../phase-2-11.md), QA PASS, ticket
[0080](../../ClaudeWiki/Processes/Handoffs/0080-phase-2-11-qa-gate.md). **2.12** — see
[`../phase-2-12.md`](../phase-2-12.md), QA PASS, ticket
[0086](../../ClaudeWiki/Processes/Handoffs/0086-phase-2-12-qa-gate.md); item 11's Enrage finding
(cheap contract extension, doesn't prove a runtime-composable "effect fact") routed to Amara for
`gm-overlay.md` as ticket [0087](../../ClaudeWiki/Processes/Handoffs/0087-phase-2-12-item-11-finding-for-gm-overlay-draft.md).
All three phases of this proposal are now real, completed roadmap phases; this doc is historical from
here. Registered in [`Proposals/_index.md`](_index.md) and pointed to from `roadmap.md`'s "Proposals"
section.

**Author:** Juno (Game Designer). **Reviewed by:** Amara (Architect), via ticket 0067.

## Purpose

Phase 2.9 (Lua World Module authoring) is done (QA PASS, ticket 0068) — this proposes three
follow-on phases that deliberately stress-test what 2.5–2.9 built, rather than expanding content
for its own sake. Each candidate item below was chosen because it exercises a fact vocabulary,
capability-mechanism path, or authoring surface that's currently either untouched or only proven in
the narrowest case that made a Done-When bar pass. The goal is to find gaps on purpose, in a
controlled way, before Phase 4 (scripting) inherits whatever these tiers can and can't express
today.

This is a **reviewed proposal**, approved in principle — not yet a committed roadmap phase with a
binding Done-When bar. That's the next step, once buildable specs exist for each (see Related
documents). Numbering (2.10/2.11/2.12) is a sequencing suggestion, not yet a claim on the roadmap.

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
   Marker/Standing-Rule suppression pattern across a boundary it's never had to cross. **Flagged by
   Amara (ticket 0067) as more than a 2.10 finding:** `design/gm-overlay.md`'s draft assumes this
   same marker-plus-guard convention generalizes cleanly for an overlay's own auto-expiry ("a marker
   fact plus a turn-count guard, the same convention `[StandingRule]` guards already use"). If this
   item finds the pattern doesn't cross a boundary cleanly, that's a correction owed back to that
   draft, not just a result to log.
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
    small parameter table. **Secondary relevance flagged by Amara:** a minor but genuine data point
    for whether GM-Overlay's inner expression language should reuse MoonSharp — not the same
    mechanism (World Module's Lua runs once at construction, GM-Overlay's would evaluate
    repeatedly), but if MoonSharp handles procedural logic awkwardly even in this simpler one-shot
    case, that's worth knowing before assuming it's fine for the harder reactive case.
11. **Content that outgrows `IOneD20WorldModule`'s fixed shape.** A monster with something beyond
    flat `CombatantStats` (a special attack, an on-hit effect) either forces a Rulebook-tier
    contract change or reveals the ceiling of what a World Module can express without one — worth
    knowing before Phase 4 inherits this same contract shape. **Ruled a real sequencing dependency
    by Amara, not just adjacent work:** this item's outcome is a direct input to `gm-overlay.md`'s
    still-draft "effect fact" idea for Phase 4 — a `MonsterBehaviorOverride`-shaped extension point
    is exactly what this item might produce or rule out. Whoever runs the Phase 4 kickoff pass
    should read this item's result first. If a contract change does land, it's scoped to
    `IOneD20WorldModule` itself — it does **not** trigger `non-goals.md`'s separate, still-untriggered
    `IWorldModule`-generalization deferral; the two aren't the same event and shouldn't be conflated.
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
- 2.12 has no dependency on the other two and could run fully in parallel — **except** that Phase
  4's kickoff pass should not finalize the GM-Overlay "effect fact" draft until 2.12 item 11's
  result is in (Amara's ruling, ticket 0067). This doesn't reorder 2.10–2.12 relative to each other,
  it constrains what comes *after* them.

None of this is a hard ordering constraint among 2.10/2.11/2.12 themselves — flagged for whoever
schedules these, not a ruling. The Phase 4 dependency above is a ruling, not a flag.

## Where this doc lives — resolved

`Documents/Roadmap/Proposals/` is ratified (ticket 0067) as the home for this doc type: pre-
commitment content that isn't a design doc (`Documents/design/` — stable shape, no status), a
content spec (`content-spec-template.md` — buildable, this is pre-spec), or a roadmap phase doc
(`Documents/Roadmap/` — carries a committed Done-When bar). Registered in
[`Proposals/_index.md`](_index.md) and pointed to from `roadmap.md`'s own "Proposals" section, so
this doc no longer depends on the ticket number to be discoverable. Lifecycle: a proposal here gets
**promoted** (content moves into real `phase-N.md` docs, this doc marked superseded in place) or
**rejected/parked** (marked as such, not deleted) — see `Proposals/_index.md` for the full statement.

## Related documents

- [Ticket 0067](../../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) — the review request this proposal is attached to
- [`../../design/1-d20.md`](../../design/1-d20.md), [`../../Specs/1-d20/1-d20-spec.md`](../../Specs/1-d20/1-d20-spec.md) — the content this stress-tests
- [`../phase-2-7-1.md`](../phase-2-7-1.md) — Loot ROM, the capability-sink/multi-consumer precedent items 6–8 build on
- [`../phase-2-8.md`](../phase-2-8.md), [`../../design/world-module.md`](../../design/world-module.md) — the World Module tier items 9–12 stress-test
- [`../phase-2-9.md`](../phase-2-9.md) — the Lua authoring path this proceeds from, done (QA PASS)
- [`../../design/non-goals.md`](../../design/non-goals.md) — capability arbitration and versioning are both named, deferred non-goals items 6/8 press against
- [`../../design/cartridge.md`](../../design/cartridge.md) — the capability/clash/cycle mechanism items 6–8 stress-test
- [`../../design/gm-overlay.md`](../../design/gm-overlay.md) — the still-draft Phase 4 tier that items 3 and 11 (and, secondarily, 10) feed back into
- [`_index.md`](_index.md) — this proposal's entry in the Proposals index
- [`../../Specs/phase-2-10-content-coverage-spec.md`](../../Specs/phase-2-10-content-coverage-spec.md) — buildable spec for Phase 2.10
- [`../../Specs/phase-2-11-capability-stress-test-spec.md`](../../Specs/phase-2-11-capability-stress-test-spec.md) — buildable spec for Phase 2.11
- [`../../Specs/phase-2-12-lua-worldmodule-stress-test-spec.md`](../../Specs/phase-2-12-lua-worldmodule-stress-test-spec.md) — buildable spec for Phase 2.12
