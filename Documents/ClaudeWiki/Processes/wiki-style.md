# Wiki Style — how to write and edit ClaudeWiki pages

The house standard for ClaudeWiki. Read this before adding or editing a page. It exists so any
chat editing the wiki writes to the same conventions cold, instead of re-deriving them or drifting.
This page is the authoritative standard; where other pages once stated a convention inline, they
now defer here.

## Audience and voice

- **LLM-first.** Written for a fresh agent session to read cold and act on — not prose for a human
  to enjoy. Terse, structured, current.
- **Imperative and concrete.** "Add a row to the index," not "one might consider indexing."
- **Current, not historical.** State what's true now. Record superseded decisions as *superseded*
  (see below), don't leave them reading as live.

## One concern per file

Each page covers one topic. If a page grows a second concern, split it. This is what keeps pages
loadable in isolation — a chat pulls exactly the page it needs, not a monolith.

## File placement and naming

- Put the page in the right section: `Roles/` (one per chat-session role), `Processes/` (team
  workflows), `Tools/` (skills, scripts, utilities).
- Filenames are kebab-case `.md` (`wiki-style.md`, `dev-test.md`).
- Handoff tickets are `NNNN-short-slug.md`, sequential across the whole system — see
  [`handoff-tickets.md`](handoff-tickets.md) for that template and lifecycle (not restated here).

## Page structure

Every page has, in order:

1. **`# H1 title`** — matches what the index calls it.
2. **A purpose line up top** — one or two sentences on what the page is, so a reader knows in the
   first three lines whether this is the page they want. Descriptive pages (roles, processes, tools)
   do **not** carry a `**Status:**` header — see [Where status lives](#where-status-lives). Tickets,
   content specs, and roadmap phases do, because that's where a real lifecycle is tracked.
3. **Body** — sectioned with `##` headings.
4. **`## Related pages`** footer — relative links to the index and adjacent pages, so navigation
   never dead-ends.

## Register it — never orphan a page

Adding a page is not done until it's linked from somewhere discoverable:

- Add a row to the [index](../index.md) Pages table.
- If the page belongs to a set with its own index (e.g. `Handoffs/index.md`), add it there too.
- Cross-link from related pages' "Related pages" footers.

An unlinked page is invisible — the exact failure that made `testing.md` necessary.

## Link, don't duplicate — single source of truth

One fact has one authoritative owner. Other pages **link** to it; they don't restate it, because
two copies drift and then contradict. When a topic spans code and wiki, name the owner explicitly —
an "authoritative sources" table (see [`testing.md`](testing.md)) is the pattern: one row per
concern, one owning doc.

If you catch yourself copying a paragraph from another page, link instead.

## Where status lives

Progress and state are tracked in exactly three places, and nowhere else:

- **Ticket status** — **open → in-review → resolved**, in [`Handoffs/`](Handoffs/) (per
  [`handoff-tickets.md`](handoff-tickets.md)).
- **Phase status** — **pending → done** (and **superseded**), in the roadmap
  (`Documents/Roadmap/`).
- **Content-spec status** — **draft → in review → active**, on the spec itself (per
  [`content-spec-template.md`](content-spec-template.md); `active` means Baraka has implemented
  against it and QA has verified the acceptance criteria, not just that the Game Designer is done).

Role, process, and tool pages describe **stable shape** — what a role *is*, how a process *works* —
and carry **no** `**Status:**` header and no embedded progress claims ("implemented in Phase 2,"
"fixed post-2.6," "very little exists yet," "not yet wired"). Those rot the moment code moves, then
either get hand-maintained forever or quietly start lying. To find out whether something is built,
read the roadmap and the tickets — not a role page. **Design docs (`Documents/design/`) follow the
same rule**: they describe intended shape; the roadmap says what's real. A design doc should not open
with a "how much of this exists yet" status line.

Use **superseded** for a decision replaced by a newer one, on the tickets/specs/phases where the
lifecycle actually lives. Date material reversals there too ("Reconciled 2026-07-17: …") rather than
silently overwriting — the trail is how the next chat trusts the record. Don't reproduce that trail
as running commentary in a descriptive page.

## Supersede, don't delete

Never silently delete a decision, verdict, or report that others may have acted on. Mark it
superseded in place, or move it to an `Archive/` subfolder with a banner explaining what replaced
it (the convention QA already uses for `Documents/QA/`). Deletion erases the reasoning; supersession
preserves it.

## Formatting

- CommonMark. Put a blank line before any list and after any heading, or it won't render.
- Prefer prose and short sections over deep bullet nests. Bullets for genuine lists, not for
  every sentence.
- Keep tables narrow enough to scan; long detail goes in prose beneath.

## Documentation conventions (binding)

These conventions prevent common rot patterns. All violations worth fixing incrementally.

### Convention 1: "Status lives in one place"

- Roadmap is the source of truth. Design docs, tickets, specs refer to it; never restate status inline.
- Every design doc opens with: `Implementation status: see [Roadmap](../../Roadmap/roadmap.md).`
- Tickets and specs carry their own status (per [ticket template](#ticket-template) and [spec preamble](../Specs/README.md)).
- If you catch a status claim in prose ("implemented in Phase 2"), it's rotten — delete it and link to the roadmap instead.

### Convention 2: "Link, don't duplicate"

- One fact has one authoritative owner. Other pages link to it; they don't restate it, because two copies drift.
- If preamble > 80 words, trim it. Move explanation to main body or linked doc.
- When templating or scaffolding (e.g., spec preambles), use templates in dedicated files (`Specs/README.md`), not repeated prose.

### Convention 3: "Deprecation implies redirection"

- Any deprecated doc has immediate link to current pattern at the very top.
- Template: `> **DEPRECATED** (date) — [X] is historical. **Current:** see [Y](link).`
- Example: [`NineRooms/README-DEPRECATED.md`](NineRooms/README-DEPRECATED.md).

### Convention 4: "Decision-first tickets"

- Every ticket opens with "Decision" section (2–4 sentences): ask, ruling, brief why.
- Full narrative in "Details" below (preserves audit trail, doesn't block quick-lookups).
- See [ticket template](#ticket-template) for exact structure.

### Convention 5: "Quick-start for multi-file concerns"

- If a concern splits into 3+ files but agents need the whole picture to start, provide a quick-start.
- Example: [`testing-quick-start.md`](testing-quick-start.md) ties together `testing.md` + `test-runbook.md` + `test-patterns.md` + `agent-test-workflow.md`.

### Convention 6: "Current pattern marker"

- Mark files representing current, recommended approach at the top.
- Deprecated/historical content has equally clear marker + redirect.
- Example: [`1-d20-spec.md`](../Specs/1-d20/1-d20-spec.md) (current) vs. [`NineRooms/README-DEPRECATED.md`](NineRooms/README-DEPRECATED.md) (historical).

### Convention 7: "No orphaned documents"

- Every `.md` file in `Documents/` is linked from an index.
- Before adding a new doc, check if it fits an existing category.
- Lint: `grep` for .md files not linked from any index.

### Convention 8: "Maintenance windows"

- PM does 15-minute sync after each phase closes:
  1. Roadmap status current? (Phase marked done in `roadmap.md` and specific phase doc)
  2. No stale `Status:` fields in design docs? (Should link to roadmap, not restate)
  3. Handoff index, QA index align with roadmap phase status? (Link, don't duplicate)
  4. Deprecation links present on any historical/superseded content?
- Updates to this checklist land here, not scattered across tickets.

## Checklist for any wiki edit

1. Right section folder, kebab-case filename, one concern.
2. H1 + purpose line + `## Related pages` footer. No `**Status:**` header on a role/process/tool
   page — see [Where status lives](#where-status-lives).
3. Registered in the index (and any section index).
4. No duplicated content — linked to the owner instead.
5. No embedded progress/state claims; superseded decisions marked (not deleted) on the ticket/spec/
   phase that owns the lifecycle.

## Related pages

- [ClaudeWiki index](../index.md)
- [Handoff ticket process](handoff-tickets.md)
- [Testing — conventions & workflow](testing.md)
