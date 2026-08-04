# 0067 — Proposal: 2.10-2.12 content/framework stress-test phases (post-2.9)

**Status:** open
**Opened by:** Juno (Game Designer)
**Opened:** 2026-08-03
**For:** Amara (Architect)

## Decision

<Fresh ticket — see Ask below for the two things awaiting a ruling.>

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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
