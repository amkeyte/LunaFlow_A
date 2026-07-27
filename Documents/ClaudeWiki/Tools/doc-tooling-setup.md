# Doc Tooling — Setup & Implementation Notes

> **STATUS UPDATE 2026-07-26: doc-scaffold.py and doc-lint.py are now built and tested.** This page
> originally described unbuilt specs (moved here from a root-level `ARRYN-SKILLS-SETUP.md`, fixed
> for naming/registration per `wiki-style.md`). Implementation is complete — see
> [`doc-scaffold-SKILL.md`](doc-scaffold-SKILL.md) and [`doc-lint-SKILL.md`](doc-lint-SKILL.md) for
> real, current usage. This page now covers *why* they exist and the maintenance model; the SKILL
> pages are the source of truth for how to run them.

**TL;DR:** Two companion Python scripts to scaffold new docs and audit for drift. Live in
`Documents/ClaudeWiki/Tools/`. Run manually or wire into a maintenance window.

---

## What You Have

### 1. **doc-scaffold** (`doc-scaffold.py`)
Generates new handoff tickets and specs with correct structure, preambles, and conventions baked
in — no more copy-paste templates, no risk of a hand-authored ticket skipping the Decision section.

### 2. **doc-lint** (`doc-lint.py`)
Audits docs against `wiki-style.md`'s conventions. Catches:
- Status fields living outside the roadmap
- Orphaned docs not linked from any index
- Incomplete deprecation notices
- Broken links
- **Tickets missing a `## Decision` section** — the specific finding from
  [`documentation-audit.md`](../documentation-audit.md)'s Deficiency 3 ("dense handoff tickets,"
  the token-bloat problem). This is the check that exists *because* of the audit, not a generic
  lint rule — a ticket without Decision-first structure is exactly the "200 lines to convey 'this
  was decided'" pattern the audit flagged.

See both `SKILL.md` files in this folder for exact usage, parameters, and output format.

---

## Why these exist

[`documentation-audit.md`](../documentation-audit.md) found doc-system token inefficiency across
eight deficiencies. Several of the fixes are one-time (trim a preamble, add a Decision section to
an existing ticket) but two are *ongoing discipline* — status only in the roadmap, tickets
Decision-first — that erode the moment nobody's checking. These scripts are that checking,
automated, so it doesn't depend on whoever's running the PM maintenance window that week
remembering to look.

## How the PM uses them

**Creating a new ticket:**
```bash
python3 Documents/ClaudeWiki/Tools/doc-scaffold.py ticket --id 0033 --title "My ticket title" --for Baraka
```
File appears with Decision/Details(Context/Ask/Response/Resolution) sections pre-structured, per
the real [handoff ticket template](../Processes/handoff-tickets.md). PM fills in the Ask, hands off.

**Creating a new spec:**
```bash
python3 Documents/ClaudeWiki/Tools/doc-scaffold.py spec --name world-module --design-doc scripting-content.md
```

**Before closing a phase (part of the 15-minute maintenance window in `wiki-style.md`):**
```bash
python3 Documents/ClaudeWiki/Tools/doc-lint.py
```
Reports status drift, orphaned docs, incomplete deprecations, broken links, and Decision-less
tickets. Clean report → confidently mark the phase done. Issues → fix before close.

---

## Maintenance & evolution

Both scripts are hand-maintained, not vendored — when a convention in `wiki-style.md` changes,
update the matching check/template in the same sitting. If a new convention needs a doc-lint check,
add it there rather than letting drift go uncaught. Keep scripts and conventions in sync;
`wiki-style.md` is the spec, the scripts are the enforcement.

## Related

- [doc-scaffold](doc-scaffold-SKILL.md) — real usage, parameters, examples
- [doc-lint](doc-lint-SKILL.md) — real usage, checks, examples
- [Documentation System Audit](../documentation-audit.md) — why these tools exist
- [Wiki Style Guidelines](../Processes/wiki-style.md) — the conventions they enforce
- [ClaudeWiki index](../index.md)
