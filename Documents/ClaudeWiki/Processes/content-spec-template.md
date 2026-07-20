# Content Spec Template — what a buildable spec contains

**Status:** active. Registered in the [wiki index](../index.md) and cross-linked from
[`game-designer.md`](../Roles/game-designer.md) and [`wiki-style.md`](wiki-style.md) — wired in
by Kofi, 2026-07-20. Owned by the [Game Designer](../Roles/game-designer.md) role. Defines the
required shape of a
content-design spec (ROM mechanics, a Rulebook, a scripting-format surface) handed to
[Baraka](../Roles/dev.md) for implementation, so "precise enough to build blind" means the same
thing every time instead of being re-derived per spec.

## Why this exists

`Roles/game-designer.md` names `nine-rooms-spec.md` as the bar for spec quality, but that spec
predates this role and has no reusable structure behind it — no required sections, no acceptance
checklist, no status vocabulary. Handoff Tickets solved the equivalent problem for tickets with a
literal template embedded in `handoff-tickets.md`; this page is the same move for content specs.

## When to use this

Any time the Game Designer hands Baraka a spec for content that will ship (not disposable test
content the way Nine Rooms was). One spec can span multiple files if it's large — Nine Rooms
splits into a top-level spec plus `-facts.md`/`-gameplay.md` companions — but every file in the
set should be reachable from a single entry doc structured per below.

## Flavor content: a localization-style split

Any content spec with player-facing prose (room descriptions, flavor text, narration strings)
splits it into a companion `<name>-flavor.md`, keyed by id — the same shape as a localization
resource table, not a runtime one. The technical spec never inlines the actual words; it references
a key (`DescriptionKey: "room-3-desc"`) and states the field's type. The flavor file is pure prose:
key, text, nothing mechanical — no DCs, no fact shapes, no rule logic.

This isn't a runtime i18n system — nothing loads these strings dynamically by locale. It's a
spec-authoring split, done for the same reason the fact/gameplay split exists: Baraka implementing
`Description: string` doesn't need to read the words to get the field right, and a narrative review
pass doesn't need to wade through fact shapes and DC math to judge whether the prose lands. Each
file stays authoritative for disjoint content, so there's nothing for the two to drift on.

Skip this split for specs with only incidental prose (a line or two, not worth a second file) —
judgment call, not a hard rule.

## Required sections

Every content spec's entry doc has, in order:

1. **Header** — title, **Status** (`draft` / `in review` / `active` / `superseded`), and what it
   supersedes or is a companion to (link the concept doc it's turning into something buildable, if
   one exists — e.g. a `Documents/design/` page).
2. **Purpose** — one paragraph: what concept this turns into something buildable, and what it
   deliberately does *not* implement (interfaces/boundaries vs. internals — see Nine Rooms' "This
   spec defines interfaces and boundaries... does not implement those internals" pattern).
3. **Scope** — explicit in/out list. What mechanics, facts, or commands this spec covers, and what
   adjacent things it's deliberately leaving for a later spec or another role.
4. **Fact / marker vocabulary** — exact shapes (records, fields, types) for every fact this content
   inserts, queries, or retracts. Baraka should not have to guess a field name or type. Any prose
   field (a description, a name) is typed here but its content lives in the flavor companion, keyed
   by id — see [Flavor content](#flavor-content-a-localization-style-split).
5. **Mechanics / rules** — the rule list: trigger conditions, `Then()` behavior, ordering or
   suppression relationships between rules (e.g. a `Downed` marker suppressing an attack rule).
   State player-facing behavior, not just the NRules mechanics.
6. **Commands & handlers** (if the content adds player commands) — command name, arguments,
   handler responsibility, what facts it reads/writes.
7. **Acceptance criteria** — a concrete, testable checklist. This is what QA (Lothar) verifies
   against and what Baraka treats as "done." Written as pass/fail statements, not prose goals.
8. **Framework dependencies** — capabilities this spec needs from the engine that don't exist yet.
   If any are found, they become tickets to the [Architect](../Roles/architect.md) (per the
   abstract/concrete seam in `Roles/game-designer.md`) — link the ticket once filed, don't leave
   the dependency implicit.
9. **Open questions / edge cases** — anything left for Baraka to flag rather than silently decide,
   or explicitly deferred with a reason.
10. **Related documents** — links to companion spec files, the concept doc, and any filed tickets.

## Markdown skeleton

```markdown
# <Content Name> — Implementation Spec

**Status:** draft
**Companion to:** <concept doc, if any>
**Supersedes:** <prior spec, if any — otherwise "nothing">

## Purpose

<what this turns into something buildable; what it does not implement>

## Scope

**In:** <...>
**Out:** <...>

## Fact / marker vocabulary

<exact shapes>

## Mechanics / rules

<rule list, trigger conditions, Then() behavior, ordering>

## Commands & handlers

<if applicable>

## Acceptance criteria

- [ ] <pass/fail statement>
- [ ] <pass/fail statement>

## Framework dependencies

<capabilities needed from the engine that don't exist yet; link tickets once filed>

## Open questions / edge cases

<...>

## Related documents

- <links>
```

## Status vocabulary for content specs

Reuses the wiki-wide vocabulary from `Processes/wiki-style.md`: **draft → in review → active**,
with **superseded** for anything replaced (mark in place, don't delete — same rule as everywhere
else in the wiki). A spec earns `active` when Baraka has implemented against it and QA has
verified the acceptance criteria pass, not merely when the Game Designer considers it finished.

## Related pages

- [ClaudeWiki index](../index.md)
- [Game Designer role](../Roles/game-designer.md)
- [Handoff ticket process](handoff-tickets.md) — the template this page's approach is modeled on
- [Wiki Style](wiki-style.md)
