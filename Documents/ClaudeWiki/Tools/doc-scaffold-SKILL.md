# doc-scaffold: Generate Handoff Tickets & Specs

**Built and verified 2026-07-26.** `doc-scaffold.py` is real, tested against a throwaway ticket and
spec generation (structure confirmed matching the real templates, duplicate-ID and missing-design-doc
validation both confirmed working), then cleaned up. Commands below are live.

**Purpose:** Scaffold new handoff tickets and specs with correct structure, preambles, and conventions.

**Use when:** Creating a new handoff ticket or buildable spec. Saves time, ensures structure consistency.

---

## Quick Start

### Generate a new handoff ticket:

```bash
python3 Documents/ClaudeWiki/Tools/doc-scaffold.py ticket --id 0033 --title "My ticket title"
```

Creates `Documents/ClaudeWiki/Processes/Handoffs/0033-my-ticket-title.md` with:
- Status field (defaulting to "open")
- `## Decision` section header (ready to fill in — Convention 4)
- `## Details` wrapper with `### Context` / `### Ask` / `### Response` / `### Resolution`
- Proper heading hierarchy per `handoff-tickets.md`'s real template

### Generate a new spec:

```bash
python3 Documents/ClaudeWiki/Tools/doc-scaffold.py spec --name engine-host --design-doc engine-core.md
```

Creates `Documents/Specs/engine-host-spec.md` with:
- One-liner preamble linking to the design doc, per `Specs/README.md`'s template
- `## Purpose` / `## Scope` sections
- Reference to `Specs/README.md` for the full pattern

---

## What's Generated

### Handoff Ticket Template

```markdown
# [ID] — [Title]

**Status:** open
**Opened by:** [OpenedBy or "TBD"]
**Opened:** [today's date]
**For:** [For or "TBD"]

## Decision

[2–4 sentences: what was asked, what was ruled, brief why. Fill in once there's a ruling — leave as
a placeholder note on a fresh `open` ticket.]

## Details

### Context

[Background, alternatives considered, rationale.]

### Ask

[The specific thing the assignee needs to do or answer.]

### Response

[Filled in by the assignee.]

### Resolution

[Filled in by the opener when closing.]
```

**Notes:**
- ID is auto-formatted to 4 digits (`0033`, not `33`); script checks `Handoffs/index.md` and the
  folder itself for the next free number and warns on collision.
- Filename is auto-slugified from title (lowercase, spaces → hyphens, punctuation stripped).
- Status defaults to `open`.
- **Does not** auto-add a row to `Handoffs/index.md` — that's a deliberate manual step (the index
  row is a summary written after the ticket has real content, not generated from a title alone).

### Spec Template

```markdown
# [SpecName] — Implementation Spec

**Status:** draft
**Companion design doc:** [`../design/[DesignDoc]`](../design/[DesignDoc]) — read for rationale.

## Purpose

[What this spec turns into buildable form. One paragraph, link don't restate.]

## Scope

[In / Out, or brief bullets of what's covered.]
```

**Notes:**
- Preamble matches [`Specs/README.md`](../../Specs/README.md)'s template exactly — one-liner +
  links, no restated design-doc content.
- Status defaults to `draft` per the spec lifecycle (`draft` → `in review` → `active`).
- Ends with a `## Related pages` footer linking back to the design doc.

---

## Implementation

The script (`doc-scaffold.py`) does:

1. **Validate inputs** — ticket ID not already in use (checked against the `Handoffs/` folder
   listing, not just `index.md`, since the two can drift); spec name doesn't collide with an
   existing file in `Specs/`.
2. **Create file with correct path** — `Handoffs/NNNN-slug.md` or `Specs/spec-name-spec.md`.
3. **Populate template** — insert provided metadata (title, design-doc link, dates).
4. **Preserve conventions** — correct heading levels, preamble format, section order per
   `wiki-style.md` and `handoff-tickets.md`.
5. **Report success** — print the file path and a reminder of what's still manual (index row for
   tickets, filling in Purpose/Scope for specs).

No external dependencies beyond the Python 3 standard library.

---

## Parameters

### For tickets:
- `ticket` — subcommand.
- `--id NNNN` — Ticket ID (e.g., `0033`). Required.
- `--title "Text"` — Human-readable title. Required.
- Optional: `--opened-by "Name"`, `--for "Name"` (defaults to `TBD`).

### For specs:
- `spec` — subcommand.
- `--name "name"` — Spec name (e.g., `engine-host`). Required.
- `--design-doc "filename"` — Design doc it's paired with (e.g., `engine-core.md`). Required —
  script checks the file exists in `Documents/design/` and errors if not (a spec with no real
  design-doc companion is exactly the drift this tool exists to prevent).

---

## Workflow Example

**Creating a new ticket:**

```bash
python3 Documents/ClaudeWiki/Tools/doc-scaffold.py ticket --id 0033 \
  --title "ROM identity & cross-ROM interop" --opened-by Kofi --for Amara
# → Documents/ClaudeWiki/Processes/Handoffs/0033-rom-identity-cross-rom-interop.md
# Ready to edit: fill in Decision (2–4 sentences), Context, Ask
# Reminder: add a row to Handoffs/index.md once there's real content
```

**Creating a new spec:**

```bash
python3 Documents/ClaudeWiki/Tools/doc-scaffold.py spec --name world-module \
  --design-doc scripting-content.md
# → Documents/Specs/world-module-spec.md
# Ready to edit: expand Purpose, add Scope
```

---

## Related

- [doc-tooling-setup.md](doc-tooling-setup.md) — why these tools exist, maintenance model
- [Handoff Ticket Process](../Processes/handoff-tickets.md) — the real template this generates
- [Wiki Style Guidelines](../Processes/wiki-style.md) — conventions this enforces
- [Documentation System Audit](../documentation-audit.md) — why these templates exist
- [doc-lint](doc-lint-SKILL.md) — companion: validates generated (and hand-written) docs
