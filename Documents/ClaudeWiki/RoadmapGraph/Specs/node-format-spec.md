# Spec: RM node file format

> **WIP — draft complete, part of an unratified proposal.** See
> [`../../index.md`](../../index.md) for this space's status. Nothing here is buildable against yet —
> this spec exists to be reviewed alongside [`../proposal.md`](../proposal.md) and
> [`graph-tooling-spec.md`](graph-tooling-spec.md), not implemented on its own.

**Companion to:** [`../proposal.md`](../proposal.md) §4 (core model) and §6 (tooling) —
read those first for rationale. **Consumed by:** [`graph-tooling-spec.md`](graph-tooling-spec.md).

## Purpose

Defines exactly what an RM node file contains, so the graph-tooling script has something precise to
parse and a human has something precise to write. One file per node, plain markdown with a small,
fixed set of machine-readable fields at the top — same shape discipline the existing handoff-ticket
template already holds authors to, extended with the fields a graph needs that a flat ticket list
never did (`DependsOn`, `Kind`, and a reversible status for convergence nodes).

## Location and naming

`Documents/Roadmap/Nodes/RM-NNNN-slug.md` — a new folder, parallel to `Documents/Roadmap/Proposals/`,
deliberately **separate from** `Documents/ClaudeWiki/Processes/Handoffs/`. Reasoning: the Handoffs
index is explicitly allowed to be archived and sharded once it grows too long to skim
(`index-archive.md`, `index-archive-2.md` already exist for exactly this reason) — that's fine for
routine findings-and-asks tickets, but fatal for something load-bearing enough to gate a convergence
node. RM nodes need a home that's small enough, and important enough, to never need that treatment.

`NNNN` is monotonic, assigned at creation, never reused — identical discipline to ticket numbers,
just a separate counter. `slug` is a short kebab-case description, same as ticket filenames today.

**No hand-maintained index file for this folder.** That's the entire point of §6's `render` query —
the crawlable view is generated, not maintained. A stale, hand-edited index is exactly the drift this
proposal exists to stop introducing.

## Required fields

Every node file opens with a fixed block, in this order:

```markdown
# RM-0042 — <short title>

**Kind:** work | convergence
**Status:** <see below, depends on Kind>
**Created:** <date>
**Owner:** <role — Kofi, Amara, Juno, Baraka, Nadia, or Lothar>
**DependsOn:** [RM-0012, RM-0031]
```

- **Kind** is set once at creation and never changes. A work node that turns out to be more
  significant than expected doesn't get promoted — a new convergence node is created that depends on
  it instead. (Mirrors the project's own rule against generalizing a mechanism from a single
  consumer — don't reclassify retroactively on a hunch, wait for a second real reason.)
- **DependsOn** is a flat list of RM-IDs. Empty list is valid (a node with no prerequisites is
  actionable from creation). Malleable while the node is `open`/`WIP`; changing it after the node is
  `resolved` requires a dated note in the node's own body, not a silent edit — same discipline
  ticket Resolution sections already follow.
- **Owner** is whichever role's judgment call closes the node — not necessarily who created it,
  mirroring the ticket system's opened-by/for split.
- **Ticket** (optional): `**Ticket:** [NNNN](path) — <its current ticket status>`, once work has
  actually started. Pure cross-reference — points at whichever handoff ticket is doing the day-to-day
  work, so a reader lands on the live conversation instead of hunting for it. Carries no weight in any
  query below; see the note under Status vocabulary for why.

## Status vocabulary

**Deliberately no "in progress" status.** A node's own status only answers one question — does the
deliverable exist yet — and stays `open` for as long as that's not yet true, whether that means
"nobody's touched it" or "it's in-review and nearly done." The finer-grained question, *is someone
actively on this right now*, is already answered by the linked ticket's own `open → in-review →
resolved` lifecycle — that mechanism already exists, already works, and doesn't need a parallel one
bolted onto the node. `frontier` (§6 of the proposal) will keep listing an in-review node as
actionable, and that's correct, not a gap: actionable means "dependencies are satisfied, this could be
picked up," never "unclaimed." Whether it's already claimed is one click away, same as it is today.

**Work nodes** — terminal once left `open`:

| Status | Meaning | Reversible? |
| --- | --- | --- |
| `open` | Not yet resolved. Actionable if `DependsOn` is fully satisfied (computed, see below). | — |
| `resolved` | Done. Historical fact, permanent. | No — corrections are a new node, never reopening this one. |
| `superseded` | Wrong, or replaced by a better answer. Must carry `**Superseded by:** RM-XXXX`. | No — same rule as `resolved`. |

**Convergence nodes** — the one place status is genuinely reversible:

| Status | Meaning | Reversible? |
| --- | --- | --- |
| `WIP` | The milestone isn't currently claimed solid. Initial state, and the state a `reached` node returns to if reopened. | — |
| `reached` | The milestone currently holds, backed by an independent QA verdict. | **Yes** — can revert to `WIP`. Requires QA authority per [`../proposal.md`](../proposal.md) §7. |

Convergence nodes additionally carry a `ReachedLog` — append-only, never edited or pruned once an
entry is written:

```markdown
## ReachedLog

- 2026-08-04 — reached, verdict [phase-2-10-verdict.md](../../../QA/phase-2-10-verdict.md)
- 2026-09-02 — reverted to WIP, see RM-0058 (gap found in Poisoned's cross-session persistence)
- 2026-09-20 — reached again, verdict [...]
```

This is the part that never gets erased even when current `Status` flips back to `WIP` — the record
that it *was* reached, when, and on what evidence, stays exactly as true as it always was. Only the
*current* claim changes.

## What's stored vs. what's computed

Stored (an author writes it, changes only by explicit edit): `Kind`, `Status`, `DependsOn`, `Owner`,
`Created`, `ReachedLog`, `Superseded by`.

**Never stored, always computed by the tooling:** whether a node is *actionable*. No field for it,
no human sets it — it's derived fresh every time as "status permits starting, and every `DependsOn`
entry is `resolved` or `reached`." Same reasoning `wiki-style.md` already applies to phase status:
don't let a claim of readiness be asserted when it can be derived from evidence instead. A stored
"actionable: true" field would just be a second place for the truth to drift away from, the exact
failure this whole proposal exists to stop introducing elsewhere.

## Body content

Below the required block, a node file reads like any other wiki page — purpose, context, whatever
narrative the work needs. No fixed template beyond the header block above; work nodes can borrow the
existing ticket template's Context/Ask/Response/Resolution shape wholesale where that fits (most
will, since most RM nodes *are* build tickets that also happen to be roadmap-load-bearing — see
[`../migration-plan.md`](../migration-plan.md) on how the two relate day to day).

## Example — a convergence node

```markdown
# RM-0007 — ROM Developer operational

**Kind:** convergence
**Status:** reached
**Created:** 2026-07-15
**Owner:** Kofi
**DependsOn:** [RM-0002, RM-0003, RM-0004, RM-0005]

The point at which a third party could author a new ROM against `Capability`/`IRom`/
`ComponentRegistry` without touching engine internals, with the toolbox proven across three real
ROMs (GridMap, D20System, Loot) plus a fourth relay case (Reputation).

## ReachedLog

- 2026-08-04 — reached, verdict phase-2-10-verdict.md
```

## Example — a work node citing a live design question

```markdown
# RM-0058 — Evaluate Roslyn removal from the rule-compilation pipeline

**Kind:** work
**Status:** open
**Created:** 2026-08-04
**Owner:** Amara
**DependsOn:** []

Roslyn compiles 1.d20's rule groups at ~14x the cost of the NRules Rete-build it feeds. Findings and
charted options in `rule-pipeline-compilation-cost.md`. See handoff ticket 0073 for the original
finding — this node exists because whichever option gets ruled on may retroactively affect anything
that cited "Roslyn compiles rule groups" as a stated fact (§5's citation convention is what makes
that list computable instead of remembered).
```

## Related pages

- [RoadmapGraph space index](../../index.md)
- [Full proposal](../proposal.md)
- [Graph tooling spec](graph-tooling-spec.md)
- [Migration plan](../migration-plan.md)
