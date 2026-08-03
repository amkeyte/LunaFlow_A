# doc-lint: Audit Docs for Drift & Consistency

**Built and verified 2026-07-26.** `doc-lint.py` is real, run against this repo's actual `Documents/`
tree. First run surfaced real findings — see [Known findings](#known-findings-as-of-2026-07-26)
below — and two script bugs (status-drift matching mid-sentence bold prose instead of just header
fields; the Decision-section check rejecting `"open (parked...)"`-style statuses), both fixed and
re-verified before this banner came down.

**Purpose:** Validate documentation against conventions, catch drift, flag orphaned docs.

**Use when:** Running PM maintenance windows, or as a pre-commit check to prevent rot.

---

## Quick Start

```bash
python3 Documents/ClaudeWiki/Tools/doc-lint.py
```

Scans `Documents/` and reports:
- **Status drift** — `.md` files outside `Roadmap/` that contain `Status: done`, `Status: draft`, etc.
- **Orphaned docs** — `.md` files not linked from any index or parent.
- **Missing Decision sections** — handoff tickets without a `## Decision` heading.
- **Stale deprecation** — Docs marked "DEPRECATED" but missing a link to the current pattern.
- **Broken links** — Links to non-existent files (design docs, spec files, etc.).

---

## What It Checks

### 1. Status Drift (Convention 1)
**Rule:** Per `wiki-style.md`'s "Where status lives," exactly three places legitimately carry a
`**Status:**` field: phase docs (`Roadmap/`), tickets (`Handoffs/`, their own open→in-review→
resolved lifecycle), and specs (`Specs/`, their own draft→in review→active lifecycle). Everywhere
else — design docs, role/process pages, tool pages — a `**Status:**` line is drift.

**Scans:** All `.md` files under `Documents/` **except** `Roadmap/`, `Handoffs/`, and `Specs/`.
**Flags:** Any file containing a `**Status:**` (or unbolded `Status:`) line.
**Output:**
```
Status drift detected:
  Documents/design/some-doc.md:3  →  "Status: draft"
  (This belongs in roadmap only; update roadmap, remove status field here, and refer to it.)
```
(Illustrative — as of 2026-07-26, `1-d20.md` and the other design docs have already had their
`Status:` fields removed per `documentation-audit.md`'s Fix 1. A real hit today would be a
regression, not an expected finding.)

### 2. Orphaned Documents (Convention 7)
**Rule:** Every `.md` file is linked from an index or parent page.

**Scans:** All `.md` files in `Documents/`.
**Builds:** List of linked files from all index pages (`index.md`, `README*.md`, section indexes).
**Flags:** Any file not in the linked list.
**Exempt:** `Documents/Roadmap/CodePlans/` — a CodePlan is Baraka's working notes for one specific
build ticket; the ticket links to the CodePlan, not the reverse, and it isn't a wiki-indexed
reference doc. Being unlinked is the normal shape for these, not drift.
**Output:**
```
Orphaned documents (not linked from any index):
  Documents/some-old-thing.md  →  Decide: delete or add to an index?
  Documents/ClaudeWiki/Roles/old-persona.md  →  Not in index.md; add it or move to archive.
```

### 3. Missing Decision Section (Convention 4)
**Rule:** Every handoff ticket opens with a `## Decision` section — 2–4 sentences: the ask, the
ruling (if resolved), brief why. This is the fix for `documentation-audit.md`'s Deficiency 3
("dense handoff tickets" — a 200-line ticket to convey "this was decided; verified") and the reason
this whole tool exists, not a generic lint rule.

**Scans:** All `.md` files in `Handoffs/` except `index.md`.
**Flags:** Any ticket with `**Status:**` other than `open` that has no `## Decision` heading —
matches `handoff-tickets.md`'s own template note ("skip this section on fresh `open` tickets — fill
it in when moving to `in-review`"). A fresh `open` ticket legitimately has nothing to decide yet; an
`in-review` or `resolved` ticket without one has skipped the step that makes it fast to triage.
**Output:**
```
Missing Decision section:
  Handoffs/0099-example.md  →  No "## Decision" heading found.
  (Agents doing quick lookups have to read the whole ticket instead of the 2–4 sentence summary.)
```

### 4. Deprecation Incomplete (Convention 3)
**Rule:** Any doc marked "DEPRECATED" must have an immediate link to the current pattern.

**Scans:** All `.md` files for "DEPRECATED" or "deprecated".
**Checks:** Same line plus next 10 lines for a markdown link `[text](path)`.
**Flags:** Missing link entirely.
**Output:**
```
Incomplete deprecation:
  Documents/SomeFolder/some-doc.md  →  "DEPRECATED" but no link to current pattern in next 10 lines.
  Fix: Add "**Current pattern:** see [X](link)." right after the deprecation notice.
```

### 5. Broken Links (Bonus)
**Scans:** All `[text](path)` links.
**Checks:** File exists at the target path (relative to the linking file).
**Flags:** Missing files, typos, wrong extensions. Skips external (`http://`/`https://`) links.
**Output:**
```
Broken links:
  Documents/ClaudeWiki/index.md:42  →  [Link](../nonexistent.md)  File not found.
  Documents/Specs/engine-host-spec.md:5  →  [design](../design/engne-core.md)  Typo? "engne"?
```

---

## Running It

### Full audit (all checks):
```bash
python3 Documents/ClaudeWiki/Tools/doc-lint.py
```

### Specific checks:
```bash
python3 Documents/ClaudeWiki/Tools/doc-lint.py --check status-drift
python3 Documents/ClaudeWiki/Tools/doc-lint.py --check orphaned
python3 Documents/ClaudeWiki/Tools/doc-lint.py --check decision
python3 Documents/ClaudeWiki/Tools/doc-lint.py --check deprecation
python3 Documents/ClaudeWiki/Tools/doc-lint.py --check links
```

### Format output:
```bash
python3 Documents/ClaudeWiki/Tools/doc-lint.py --format json   # Machine-readable
python3 Documents/ClaudeWiki/Tools/doc-lint.py --format text   # Default (readable summary)
```

---

## Output Examples

Illustrative only — hypothetical filenames/paths, not a claim about the current state of this
repo's docs (which have already had their audit-flagged issues fixed as of 2026-07-26).

### No issues found:
```
✓ Status drift check: OK (0 issues)
✓ Orphaned docs check: OK (0 issues)
✓ Decision section check: OK (0 tickets missing Decision)
✓ Deprecation check: OK (1 note: Nine Rooms marked historical, link present)
✓ Broken links: OK (0 issues)

Summary: All checks passed. Docs are clean.
```

### With issues (hypothetical):
```
✗ Status drift: 1 issue found
  Documents/design/some-new-doc.md:1  →  Status field detected (belongs in roadmap only)

✗ Orphaned: 1 issue found
  Documents/some-orphan.md  →  Not linked from any index

✗ Decision section: 1 issue found
  Handoffs/0099-example.md  →  No "## Decision" heading found (Convention 4)

✗ Deprecation: 1 issue found
  Documents/SomeFolder/some-doc.md  →  Has "deprecated" but no link to current pattern in next 10 lines

✗ Broken links: 1 issue found
  Documents/design/architecture.md:42  →  Link to ../desgin/glossary.md (typo: "desgin")

Summary: 5 issues.
```

---

## PM Maintenance Window Checklist

Run before closing a phase (per `wiki-style.md`'s Convention 8):

```bash
# 1. Run full audit
python3 Documents/ClaudeWiki/Tools/doc-lint.py

# 2. Fix any status drift (remove status fields outside roadmap)
# 3. Fix any orphaned docs (link them or archive them)
# 4. Fix any tickets missing a Decision section
# 5. Verify deprecation links (if anything new was deprecated)
# 6. Spot-check broken links (especially in new/updated docs)
```

No auto-fix mode in v1 — every finding needs editorial judgment (what to link an orphan from, what
the Decision summary actually says), so the tool reports, the PM fixes. Revisit if a category of
finding turns out to be safely automatable.

---

## Implementation

The script (`doc-lint.py`):

1. **Walks `Documents/` recursively** for all `.md` files.
2. **For each check:**
   - Runs a regex or pattern match (e.g., status field regex, Decision heading presence).
   - Builds a list of files/line numbers that match.
   - Cross-references against a "clean list" where relevant (e.g., roadmap files are exempt from
     the status-drift check).
3. **Reports findings** in human-readable (default) or JSON format.
4. **Exits with status code:**
   - `0` if all checks pass.
   - `1` if issues found (non-fatal — informational, doesn't block anything on its own).
   - `2` if the script errors (bad args, unreadable file, etc.).

No external dependencies beyond the Python 3 standard library (`pathlib`, `re`, `argparse`, `json`)
— keeps it runnable in any environment without a setup step.

---

## Typical Issues It Catches

| Issue | Example | Fix |
| --- | --- | --- |
| Status outside roadmap | A design doc gains a `Status: draft` line | Remove field; refer to roadmap |
| Orphaned doc | A new doc isn't linked from any index | Link from index or move to archive |
| Missing Decision section | A ticket has no `## Decision` heading | Add one — 2–4 sentences, ask/ruling/why |
| Incomplete deprecation | A doc has "DEPRECATED" but no "see X" link | Add link template |
| Typo in link | Link to `../desgin/glossary.md` | Fix to `../design/glossary.md` |
| Dead reference | Link to non-existent `Specs/foo-spec.md` | Fix or remove link |

---

## Known findings (as of 2026-07-26)

First real run against this repo's `Documents/` tree, for the record — not all fixed yet, this is
the backlog the tool surfaced, not a claim everything's clean:

| Check | Count | What it is |
| --- | --- | --- |
| Broken links | 84 (was 108 as of 2026-07-26, before [0035](../Processes/Handoffs/0035-broken-relative-links-across-documents-cleanup-pass.md)'s fix) | The dominant root cause — `Handoffs/*.md` linking `Roadmap/`/`design/`/`QA/`/`Specs/` with `../../` (2 levels) instead of the real `../../../` (3 levels) — is **fixed** in 12 tickets (24 links). Remaining 84 is a mix of: the same shape of bug in 3 very old archived tickets with an extra `Documents/` segment; ~20 similarly-shaped misses in `Roadmap/phase-*.md` (Kofi's pages, not Handoffs); stale paths in deprecated `NineRooms/` content; and a handful of intentional documentation examples on this page itself (`[design](../design/engne-core.md)` — a deliberate typo example) that a human filters on sight. Itemized in full in 0035. |
| Missing Decision section | 28 | Every ticket from before Convention 4 existed (roughly 0001–0029). Expected — the convention applies going forward, not retroactively. A real backlog if full retrofitting is ever wanted, not a defect. |
| Status drift | 0 (fixed 2026-08-02) | `NineRooms/nine-rooms-spec.md` and `NineRooms/nine-rooms.md` had `**Status:**` fields outside their legitimate home; both cut, the deprecation banners already carry the redirect. |
| Incomplete deprecation | 2 | Both are the literal string `DEPRECATED` appearing inside a filename reference (`README-DEPRECATED.md`) or this SKILL page's own rule description, not an actual unlinked deprecation banner. False positives from simple pattern-matching; understood, not worth over-fitting the regex for two hits. |
| Orphaned docs | 2 (was 3; `CodePlans/` exempted 2026-08-02) | `Handoffs/0019-phase-2-5-final-verdict.md` — a deliberate redirect stub for a numbering collision, intentionally left unlinked per `wiki-style.md`'s supersede-don't-delete rule. `Handoffs/0034-phase-2-7-test-refactor.md` — not in the index and carries a `pending` status the ticket lifecycle doesn't define; flagged for Kofi to look at, not fixed here. `Roadmap/CodePlans/2-7-supersceded.md` no longer flags at all — CodePlans is now exempt from this check (see above). |

The broken-links path bug is the one worth a real fix pass — everything else is either expected
(Decision backlog) or a disclosed, low-value false positive.

## Related

- [doc-tooling-setup.md](doc-tooling-setup.md) — why these tools exist, maintenance model
- [Documentation System Audit](../documentation-audit.md) — the audit that motivated the checks
- [Wiki Style Guidelines](../Processes/wiki-style.md) — conventions this enforces
- [doc-scaffold](doc-scaffold-SKILL.md) — companion: generates compliant docs
