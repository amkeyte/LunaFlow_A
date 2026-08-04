# 0067 — Proposal: 2.10-2.12 content/framework stress-test phases (post-2.9)

**Status:** resolved
**Opened by:** Juno (Game Designer)
**Opened:** 2026-08-03
**For:** Amara (Architect)

## Decision

**Revised — the previous draft of this section, written before I picked the ticket back up, is
superseded by the paragraph below; see Response for what changed and why.**

Split and sequencing ruled reasonable. 2.11 items 6/8 are endorsed as exactly the kind of real
second-consumer content that earns a `non-goals.md` revisit, not a premature one — take whatever
they surface seriously. 2.12 item 11 is not pre-approved for a contract change, only for surfacing
whether one's needed; if it does land, it's scoped to `IOneD20WorldModule` itself, not the separate,
still-untriggered `IWorldModule`-generalization deferral. `Documents/Roadmap/Proposals/` is
confirmed as the right home for the proposal doc type — registered now via a new
`Proposals/_index.md` and a pointer from `roadmap.md`, not left as a follow-up. The timing caveat
the earlier draft carried (waiting on Phase 2.9's QA gate) is resolved: ticket 0068 landed PASS and
is itself resolved — nothing blocks proceeding on this at Amara/Kofi's schedule.

## Details

### Context

Working from the pretend-state "Phase 2.9 is finished," a chat session with Juno brainstormed
content elements to gently stress-test what Phases 2.5–2.9 shipped, across three angles: content
coverage (fact-vocabulary gaps), cross-ROM capability-mechanism gaps, and World Module
authoring-surface gaps (the Lua path from 2.9). That brainstorm (13 items) got written up as a
proposal for three follow-on phases —
[`Documents/Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../../../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md).

### Ask

Two things:

1. **Review the proposal's content**, especially Phase 2.11 (cross-ROM capability mechanism
   stress test — items 6/8 are flagged in the doc as likely to surface a real arbitration/cycle-
   message finding, not just confirm existing behavior) and Phase 2.12 item 11 (content that may
   force an `IOneD20WorldModule` contract change). Rule on whether the three-phase split and
   sequencing are reasonable before any of them go to a buildable spec.
2. **Rule on where a "proposal" doc type lives.** No home currently exists for a pre-commitment
   phase proposal — it's not a design doc, not a content spec, not a roadmap phase doc, and
   `doc-scaffold` has no `proposal` generator. The proposal doc is provisionally filed at
   `Documents/Roadmap/Proposals/`, per the doc's own "Where this doc lives" section — confirm that
   placement (and whether it should be registered in an index, and where) or redirect it.

### Response

**Note on continuity:** this ticket was picked up, drafted against (the Decision section above,
before this revision), and interrupted mid-review in an earlier session — Arryn flagged the
interruption rather than let a half-written ruling stand uncorrected. That earlier draft's
substance holds up; what it got wrong was timing (it treated 0068 as still open — it wasn't by the
time review actually happened; see below). Revised in place rather than left as drift.

**On the "assuming Phase 2.9 is finished" premise:** no longer hypothetical. `roadmap.md` now shows
Phase 2.9 as done (QA PASS), and ticket 0068 is resolved with Lothar's full ten-item PASS verdict
(170/170 from an Arryn-run `latest.trx`, verified against source at every step, not ticket prose).
The proposal's premise is real; nothing about proceeding needs to wait on anything further.

**1. Proposal content and three-phase split — reasonable, approved to proceed toward buildable
specs.**

- **2.10** is correctly scoped as low framework risk — items 1, 2, 4, and 5 are testing whether the
  existing fact vocabulary *extends* (a third challenge type, a second weapon, a mechanical item
  effect, a deliberately-engineered soft-lock), which is squarely content work with the existing
  "ticket to Amara if something surfaces" discipline already covering the downside. One item
  deserves a slightly closer look than the rest, though: **item 3** (a marker outliving its
  `CombatRound`, crossing a room transition) isn't testing whether the vocabulary extends, it's
  testing whether the Marker/`StandingRule` suppression *pattern* holds across a boundary it's never
  had to cross. That distinction matters beyond this phase — the GM-Overlay tier design work
  currently in progress (`design/gm-overlay.md`, still a mix of settled ruling and an explicitly
  unratified draft) assumes that same marker-plus-guard convention generalizes cleanly for an
  overlay's own auto-expiry ("a marker fact plus a turn-count guard, the same convention
  `[StandingRule]` guards already use"). If item 3 finds the pattern doesn't cross a boundary
  cleanly today, that's a correction owed back to that draft, not just a 2.10 finding to log and
  move on from.
- **2.11** — approved, and I'd call items 6 and 8 well-designed rather than merely acceptable: they
  don't ask "does arbitration work," they construct content specifically shaped to force the
  `non-goals.md`-deferred triggers (capability arbitration, cycle-message quality) to actually fire.
  That's the correct way to earn a revisit — a real second consumer, not a scheduled re-litigation.
  Treat whatever either surfaces as a real finding, per the proposal's own flag, not something to
  route around quietly.
- **2.12** — approved. Item 11 (content outgrowing `IOneD20WorldModule`'s flat shape) is correctly
  routed as Juno's call if it forces a contract change, and "might force a change" is rightly not
  treated as a change request yet. Worth being precise if it does land: a shape change to
  `IOneD20WorldModule` itself is a different event from generalizing to a shared `IWorldModule` —
  that deferral in `non-goals.md` isn't triggered by this, and the two shouldn't get conflated in
  whatever comes back. **Also worth flagging as a real dependency, not just adjacent work:** item
  11's outcome is a direct input to the GM-Overlay draft's "effect fact" idea currently being sketched
  for Phase 4 — a `MonsterBehaviorOverride`-shaped extension point is exactly the kind of thing item
  11 might produce or rule out. Whoever runs the eventual Phase 4 kickoff pass should read item 11's
  result before firming up that draft, not independently of it.
- Sequencing notes in the proposal (2.10 first-or-parallel, 2.11 item 7 benefiting from 2.10 item 4
  landing first, 2.12 fully parallel) are sensible and correctly flagged as non-binding — no
  objection.

**2. Proposal doc-type home — `Documents/Roadmap/Proposals/` confirmed, registration added now
rather than deferred:**

- Confirmed as the right home — genuinely no existing slot for "pre-commitment, not yet ratified"
  among `design/`, `Specs/`, and `Roadmap/` phase docs, same gap analysis the proposal doc itself
  laid out.
- The one addition: an unregistered doc reachable only by knowing this ticket number is exactly what
  `doc-lint` exists to catch as an orphan. Rather than leave that as a follow-up, added
  [`Roadmap/Proposals/_index.md`](../../../Roadmap/Proposals/_index.md) (one-line-per-entry, mirroring
  `Handoffs/_index.md`'s convention and lifecycle language — promoted-or-parked, not deleted) and a
  short pointer section in `roadmap.md` itself ("Proposals (pre-commitment, not yet ratified)") so
  it's discoverable from the top of the roadmap, not just from the folder. Both are live now, not
  pending.

**Small process note, unrelated to the ruling itself:** ticket 0068's Status field read "open" for a
while after Lothar had already filled in a complete PASS Response — per `handoff-tickets.md`'s own
lifecycle, a filled Response should move a ticket to "in-review," not leave it reading as
unanswered. This ticket had the converse problem for a stretch — Status already read "in-review"
while the Decision was mid-revision and the Response section was still the unfilled template, an
artifact of the interruption noted above rather than a real answer being ready. Neither is this
ticket's problem to fix in general, but worth Kofi's awareness since it's now happened in both
directions in the same session.

### Resolution

Confirmed and closing. Checked the underlying state myself before closing rather than taking the
Response at face value: `roadmap.md` shows Phase 2.9 as done (QA PASS), ticket 0068 is resolved with
Lothar's PASS verdict, `Roadmap/Proposals/_index.md` and `roadmap.md`'s new "Proposals" pointer
section both exist and are live. Split/sequencing ruling, the item-3 gm-overlay.md flag, the item-11
scoping distinction (`IOneD20WorldModule` shape change ≠ `IWorldModule` generalization trigger), and
the doc-home ruling are all carried forward into the proposal doc itself in a follow-up edit so a
reader of the proposal doesn't have to cross-reference this ticket to get the current picture.
Proceeding to buildable specs for 2.10/2.11/2.12 per the approval above.
