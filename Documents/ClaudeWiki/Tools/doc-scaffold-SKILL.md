# doc-scaffold: Generate Handoff Tickets & Specs

**Purpose:** Scaffold new handoff tickets and specs with correct structure, preambles, and conventions.

**Use when:** Creating a new handoff ticket or buildable spec. Saves time, ensures structure consistency.

---

## Quick Start

### Generate a new handoff ticket:

```
$ dotnet tool restore  # if needed
$ pwsh ./doc-scaffold.ps1 -Type ticket -Id 0025 -Title "My ticket title"
```

Creates `Documents/ClaudeWiki/Processes/Handoffs/0025-my-ticket-title.md` with:
- Status field (defaulting to "open")
- Decision section header (ready to fill in)
- Context, Verification, Follow-up sections
- Proper heading hierarchy and conventions

### Generate a new spec:

```
$ pwsh ./doc-scaffold.ps1 -Type spec -Name engine-host -DesignDoc "engine-core.md"
```

Creates `Documents/Specs/engine-host-spec.md` with:
- One-liner preamble linking to design doc
- Purpose, Shape, AcceptanceCriteria, OpenQuestions sections
- Reference to `Specs/README.md` for pattern guidance
- Proper heading hierarchy

---

## What's Generated

### Handoff Ticket Template

```markdown
# [ID] — [Title]

**Status:** open

## Decision
[2–4 sentences: what was asked, what was ruled, brief why, any immediate follow-up.]

## Context
[Background, alternatives considered, rationale.]

## Verification
[How it was/will be verified; what you checked.]

## Follow-up
[Related tickets, next steps, or "none".]
```

**Notes:**
- ID is auto-formatted to 4 digits (0025, not 25).
- Filename is auto-slugified from title.
- Status defaults to "open" (editor changes to "resolved" when done).

### Spec Template

```markdown
# [SpecName] Specification

**Concrete companion to [`../design/[DesignDoc]`](../design/[DesignDoc]).**
See [Specs README](README.md) for the design-to-spec pattern.

## Purpose
[What this spec covers; why it exists as separate from the design doc.]

## Shape
[The structural design: tiers, components, interfaces, data shapes.]

## Acceptance Criteria
[Buildable, verifiable done-bar. Numbered list or table.]

## Open Questions
[Undecided items; prerequisites; known gaps.]
```

**Notes:**
- Preamble is exactly one line linking to design doc.
- No restating of design-doc content; focus on buildable specifics.
- Ends with a "Related" footer linking back to design doc and any related specs.

---

## Implementation

The script (`doc-scaffold.ps1`) does:

1. **Validate inputs** — Check that ticket ID is unique, spec name doesn't already exist.
2. **Create file with correct path** — `Handoffs/NNNN-slug.md` or `Specs/spec-name-spec.md`.
3. **Populate template** — Insert provided metadata (title, design-doc link, etc.).
4. **Preserve conventions** — Correct heading levels, preamble format, section order per `wiki-style.md`.
5. **Report success** — Print the file path and a note: "Edit and fill in [sections]."

---

## Parameters

### For tickets:
- `-Type ticket` — Create a handoff ticket.
- `-Id NNNN` — Ticket ID (e.g., 0025). Required.
- `-Title "Text"` — Human-readable title. Required.
- Optional: `-OpenedBy "Name"`, `-For "Name"` (metadata; defaults to "TBD").

### For specs:
- `-Type spec` — Create a spec.
- `-Name "name"` — Spec name (e.g., "engine-host"). Required.
- `-DesignDoc "filename"` — Design doc it's paired with (e.g., "engine-core.md"). Required.
- Optional: `-Purpose "Brief text"` (fills in first line of Purpose section; user expands).

---

## Workflow Example

**Creating a new ticket:**

```powershell
pwsh ./doc-scaffold.ps1 -Type ticket -Id 0025 -Title "ROM identity & cross-ROM interop" -OpenedBy "Kofi" -For "Amara"
# → Documents/ClaudeWiki/Processes/Handoffs/0025-rom-identity-cross-rom-interop.md
# Ready to edit: fill in Decision (2–4 sentences), Context, Verification, Follow-up
```

**Creating a new spec:**

```powershell
pwsh ./doc-scaffold.ps1 -Type spec -Name "world-module" -DesignDoc "scripting-content.md" `
  -Purpose "Content authored in JSON/Lua without a C# compile step"
# → Documents/Specs/world-module-spec.md
# Ready to edit: expand Purpose, add Shape, AcceptanceCriteria, OpenQuestions
```

---

## Related

- [Handoff Ticket Process](../Processes/handoff-tickets.md)
- [Wiki Style Guidelines](../Processes/wiki-style.md) — conventions this enforces
- [Documentation System Audit](../documentation-audit.md) — why these templates exist
- [doc-lint](doc-lint-SKILL.md) — companion: validates generated docs
