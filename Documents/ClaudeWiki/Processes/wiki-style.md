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
2. **A purpose/status line up top** — one or two sentences on what the page is and (for
   role/ticket/phase pages) its status. A reader should know in the first three lines whether this
   is the page they want.
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

## Status and lifecycle vocabulary

- Role/phase pages: **active / pending / done**.
- Tickets: **open → in-review → resolved** (per [`handoff-tickets.md`](handoff-tickets.md)).
- Content specs: **draft → in review → active** (per
  [`content-spec-template.md`](content-spec-template.md)) — `active` means Baraka has implemented
  against it and QA has verified the acceptance criteria, not just that the Game Designer is done.
- Use **superseded** for a decision replaced by a newer one, in any of the above.
- **Date material changes.** When you reconcile or reverse something, add a dated one-liner
  ("Reconciled 2026-07-17: …") rather than silently overwriting — the trail is how the next chat
  trusts the page.

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

## Checklist for any wiki edit

1. Right section folder, kebab-case filename, one concern.
2. H1 + purpose/status line + `## Related pages` footer.
3. Registered in the index (and any section index).
4. No duplicated content — linked to the owner instead.
5. Status/date conventions applied; superseded content marked, not deleted.

## Related pages

- [ClaudeWiki index](../index.md)
- [Handoff ticket process](handoff-tickets.md)
- [Testing — conventions & workflow](testing.md)
