# Implementation Specs

Buildable, concrete specs for the systems LunaFlow implements. Each spec is a companion to a design doc, stating *what to build* rather than *why*.

## Preamble template

Every spec opens with this header structure (use as-is; trim to one-liner + links):

```markdown
# [Feature Name] — Implementation Spec

**Status:** [draft | in review | active] — [brief state summary, 1 line]  
**Companion design doc:** [`../../design/[X].md`] — the concept/rationale.  
**Related specs:** [`[Y].md`], [`[Z].md`] (if applicable)

## Purpose

[One paragraph: what this spec turns into buildable form. Reference the design doc for "why." Keep this focused on scope, not rationale.]

## Scope

[In / Out list, or brief bullets of what's covered.]
```

### Status values

- **draft** — under active design; not ready to implement against.
- **in review** — draft complete; awaiting feedback or acceptance-criteria sign-off.
- **active** — implemented and QA-verified against acceptance criteria.

### Preamble discipline

- **Link, don't explain.** The preamble is a map, not an overview. "Read the design doc for why" is better than re-explaining the rationale.
- **Keep it under 100 words.** If the preamble grows beyond that, move explanatory content into the first section body.
- **Status lives on the spec,** not in prose. No "will be implemented in Phase 2" in a role or process page. The spec itself is where lifecycle tracking lives.

## Related pages

- [ClaudeWiki — Documentation Audit](../ClaudeWiki/documentation-audit.md) — the reasoning behind this template
- [Wiki Style — Where status lives](../ClaudeWiki/Processes/wiki-style.md#where-status-lives)
