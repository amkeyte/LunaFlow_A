# Migration plan: from `roadmap.md` to the dependency graph

> **WIP — draft complete, part of an unratified proposal.** See [`../index.md`](../index.md) for this
> space's status. Nothing below has been executed. This plan only becomes real if
> [`proposal.md`](proposal.md) is approved.

**Companion to:** [`proposal.md`](proposal.md) §8 (relationship to what exists today) and §9 (open
questions this plan resolves).

## Principle: no big-bang rewrite, no history loss

Everything in `Documents/Roadmap/` and `Documents/ClaudeWiki/Processes/Handoffs/` stays exactly as it
is throughout this migration until the specific step that says otherwise. At every stage, the old
system keeps working — this is an addition running alongside the current process until it's proven,
not a cutover done on faith.

## Stage 0 — build against a copy, touch nothing live

Implement [`Specs/graph-tooling-spec.md`](Specs/graph-tooling-spec.md) against a synthetic test
graph (small, hand-built, the kind of thing the spec's own acceptance criteria already describe).
Zero contact with real `Documents/Roadmap/` or `Documents/ClaudeWiki/` content at this stage. This is
normal Dev/Test-Dev work, no different from building any other tool — nothing roadmap-process-facing
changes yet.

## Stage 1 — pilot against one live case

Once Stage 0 passes its own acceptance criteria, create real node files for exactly one live,
already-in-flight question: **ticket 0073**, the Roslyn-removal evaluation. This is the case that
motivated the whole proposal, so it's the right first real test rather than a synthetic one.

- Write `RM-0001` (or whatever the real next ID is) as a work node wrapping 0073's question, citing
  the existing ticket rather than duplicating it.
- Do **not** yet create convergence nodes for "ROM Developer operational" etc. — that's a bigger
  backfill, Stage 2's job, and shouldn't block proving the tool works on one real node first.
- Run `frontier`/`validate` against this single-node graph. Confirm the output is sane before trusting
  it with anything bigger.

This stage answers the real question: does the model hold up against real, messy, in-flight work, or
only against a clean synthetic example? If it breaks here, that's cheap to find out and cheap to fix
— one node, not the whole roadmap's history riding on it.

## Stage 2 — backfill existing history, once

Convert the currently-`done` phases (`phase-0.md` through `phase-2-9.md`, all QA-PASS'd) into
`resolved` work nodes and the natural convergence points among them (e.g., whatever "ROM Developer
operational" actually should mean, decided in this pass, not invented ad hoc) into `reached`
convergence nodes — **as a one-time backfill, not a rewrite.** Every backfilled node's `ReachedLog` /
resolution cites the *existing* QA verdict and ticket that already proved it, exactly as it's already
documented in `Documents/QA/` and `Documents/ClaudeWiki/Processes/Handoffs/`. Nothing about *what
happened* changes — only that it now also has a graph-shaped record alongside the prose record it
already has.

Currently in-flight and pending items get converted as-is: `phase-2-10.md` → `resolved` (it's done),
`phase-2-11.md` → `open` (build ticket 0076 already cut), the `2.10–2.12` proposal's still-pending
2.12 → whatever state matches its real status at the time this stage runs.

**What doesn't get backfilled:** superseded/archived material (`phase-3.md`, the two Handoffs archive
pages). Dead ends don't need a graph node; the existing "superseded, see X" note already does that
job, and duplicating it into the graph is exactly the "two copies drift" problem this project already
avoids elsewhere.

## Stage 3 — run both systems in parallel

`roadmap.md`'s phase table keeps being the source of truth. The generated index
(`render`'s output) runs alongside it, visible, but informational — anyone can compare the two and
flag a mismatch. This stage has no fixed length; it ends when the generated index has been trusted
enough, long enough, that nobody's finding daylight between the two.

## Stage 4 — cut over

`roadmap.md`'s phase-sequence table is replaced with a short pointer to the generated index, the same
way `Documents/ApiReference/` already replaced hand-written type documentation — generated,
regenerated on demand, not hand-maintained. The existing phase docs (`phase-2-10.md` etc.) aren't
deleted; they become the "body" a node file points at, the same relationship a ticket file already
has to its one-line row in `Handoffs/_index.md` today.

## Stage 5 — retire the manual table

Once Stage 4 has held for a while with nobody needing the old table, mark the phase-sequence table's
last hand-maintained version as historical per the usual supersession convention — banner, pointer to
the generated index, not deleted.

## What never changes, at any stage

- `Documents/ClaudeWiki/Processes/Handoffs/` — untouched. Regular tickets keep working exactly as
  `handoff-tickets.md` describes, forever, independent of whether this proposal is adopted.
- Every existing QA verdict, ticket resolution, and phase doc — none of it is rewritten. The graph
  only ever adds a structured pointer to what already exists; it never becomes the sole copy of a
  fact that used to live only in prose.
- Authority — who can mark what, per [`proposal.md`](proposal.md) §7 — is unchanged by migration
  staging; it's a proposal-approval question, not a rollout question.

## Rollback

Trivial through Stage 3: stop generating the index, keep using `roadmap.md` exactly as today, delete
the `Nodes/` folder if desired — nothing outside it was ever touched. Rollback gets genuinely costly
only after Stage 4's cutover, which is exactly why Stage 3 has no fixed timebox — it ends on
confidence, not on a calendar.

## Related pages

- [RoadmapGraph space index](index.md)
- [Full proposal](proposal.md)
- [Node format spec](Specs/node-format-spec.md)
- [Graph tooling spec](Specs/graph-tooling-spec.md)
- [Ticket 0073](../Processes/Handoffs/0073-rule-compilation-cost-and-runtime-mode.md) — Stage 1's pilot case
