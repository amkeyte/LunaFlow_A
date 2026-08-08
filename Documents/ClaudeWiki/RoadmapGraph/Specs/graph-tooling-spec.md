# Spec: the graph-tooling script

> **WIP — draft complete, part of an unratified proposal.** See
> [`../../index.md`](../../index.md) for this space's status. Not built yet — this is the buildable
> spec a Dev would implement against once the proposal is approved, not existing tooling.

**Companion to:** [`../proposal.md`](../proposal.md) §6 and [`node-format-spec.md`](node-format-spec.md)
(the input this script reads). **Produces:** [`../Mockups/sample-generated-index.md`](../Mockups/sample-generated-index.md)-shaped
output and the JSON [`../Mockups/sample-visualization.html`](../Mockups/sample-visualization.html) consumes.

## Purpose

One script, one in-memory graph, several read-only queries against it. Loads every file under
`Documents/Roadmap/Nodes/`, parses the header block `node-format-spec.md` defines, builds a directed
graph (nodes = RM-IDs, edges = `DependsOn`), and answers questions against it. Never writes to a node
file. Never makes a judgment call — it computes lists for a human to judge, same boundary QA's own
tooling already holds (`qa.md`: "develops... scripts to keep verification lean... script output
becomes the evidence for verdicts, not raw code reads").

## Scope

**In:** parsing node files, building the graph, cycle detection, the five queries below, two output
renderers (`render` → markdown, `export-json` → structured data).

**Out:** editing node files, deciding whether a downstream node survives a deprecation, generating
the HTML visualization itself (that's a future consumer of `export-json`'s output — see
[`../proposal.md`](../proposal.md) §9's open questions), anything touching `Documents/ClaudeWiki/Processes/Handoffs/`
(a different ledger, untouched by this tool).

## Input

Every `Documents/Roadmap/Nodes/RM-*.md` file. Parse failure on any single file (malformed header,
unknown `Kind`, `DependsOn` referencing a nonexistent ID) is a hard error naming the offending file —
same fail-loud discipline `ContractSurfaceValidator` already holds itself to; a script that silently
skips a malformed node and produces a frontier missing an entry is worse than one that refuses to run.

## Queries

### `validate`

Walks the full graph for cycles (`DependsOn` chains that revisit a node still on the walk stack).
Throws naming every node in the cycle, not just that one exists — same bar
`ComponentRegistry`'s own cycle detector already holds. Every other query implicitly runs this first
and refuses to produce output against a graph that fails it.

### `frontier`

Every node where: (work node, `open`) or (convergence node, `WIP`), **and** every `DependsOn` entry
resolves to a node that's `resolved` or `reached`. This is the actionable menu — see
[`../Mockups/sample-generated-index.md`](../Mockups/sample-generated-index.md) for what it looks like
rendered.

### `downstream <RM-ID>`

**Full transitive closure**, not just direct dependents — if RM-0030 depends on RM-0022 which depends
on RM-0012, then `downstream RM-0012` must include RM-0030 even though it never mentions RM-0012
directly. This is the deprecation/revert-impact query: point it at a node about to become
`superseded`, or a convergence node about to revert to `WIP`, and get back everything that might need
re-judging — in dependency order (closest to farthest), since a human reviewing the list may
themselves deprecate something partway through, and anything further downstream needs to already be
in view when that happens, not discovered in a second pass.

Output is a worklist, not a verdict: for each node in the closure, list it with its current status
and let the human decide "stands on its own" / "needs amendment" / "also superseded/reverts" — the
tool doesn't guess which.

### `blocking <RM-ID>`

The reverse of frontier for one node: every `DependsOn` entry that is *not yet* `resolved`/`reached`,
walked transitively — "why can't I start this yet," answered without hand-tracing the chain.

### `render`

Generates the crawlable markdown index: `frontier`'s output at the top (names only — no ticket
prose, this is a map, not a summary), then the full dependency structure below, indented/nested by
depth so it can be read top-to-bottom like an index rather than requiring the reader to already know
the graph. Exact shape: [`../Mockups/sample-generated-index.md`](../Mockups/sample-generated-index.md).

### `export-json`

The same graph — nodes with their stored fields, edges, computed actionable/blocked status — as
structured data. Exists specifically so the future HTML visualization
([`../Mockups/sample-visualization.html`](../Mockups/sample-visualization.html) mocks what it'd show)
is a second renderer over this same export, not a second parser reimplementing everything above.
Shape (illustrative, not final):

```json
{
  "nodes": [
    { "id": "RM-0007", "kind": "convergence", "status": "reached", "name": "ROM Developer operational" },
    { "id": "RM-0058", "kind": "work", "status": "open", "name": "Evaluate Roslyn removal" }
  ],
  "edges": [
    { "from": "RM-0058", "to": "RM-0012" }
  ]
}
```

`"from"` depends on `"to"` — same direction as `DependsOn` in the node file, so the JSON doesn't
introduce a second convention to keep straight against the markdown source.

## Acceptance criteria

- [ ] `validate` catches a synthetic cycle (three nodes, A→B→C→A) and names all three in the error.
- [ ] `frontier` against a small synthetic graph returns exactly the nodes with all deps satisfied —
      verified against a hand-computed expected set, not just "it returned something."
- [ ] `downstream` against a four-deep synthetic chain (A→B→C→D) returns B, C, and D when queried on
      A — the transitive case, not just direct dependents.
- [ ] `render`'s output matches the mockup's structure: frontier section first, names only, no ticket
      body text leaking into the index.
- [ ] `export-json`'s node/edge shape round-trips against a hand-built synthetic graph without loss.
- [ ] A malformed node file (bad `Kind`, or a `DependsOn` entry pointing at a nonexistent RM-ID)
      produces a named, specific error, not a silent skip or a generic crash.
- [ ] Read-only against `Documents/Roadmap/Nodes/` — confirmed via a run that diffs the folder
      before/after and finds zero changes.

## Open implementation questions

- Language/runtime: not pinned here. Python fits the project's existing QA-tooling precedent
  (`scripts/qa/`); this could live at `scripts/roadmap/` alongside it as a sibling convention. Dev's
  call at build time, not a design constraint.
- Whether `render` overwrites a single generated file or is invoked on demand — a rollout-sequencing
  question, addressed in [`../migration-plan.md`](../migration-plan.md), not here.

## Related pages

- [RoadmapGraph space index](../../index.md)
- [Full proposal](../proposal.md)
- [Node format spec](node-format-spec.md)
- [Sample generated index (mockup)](../Mockups/sample-generated-index.md)
- [Sample visualization (mockup)](../Mockups/sample-visualization.html)
- [Migration plan](../migration-plan.md)
