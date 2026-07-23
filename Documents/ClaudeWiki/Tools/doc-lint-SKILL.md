# doc-lint: Audit Docs for Drift & Consistency

**Purpose:** Validate documentation against conventions, catch drift, flag orphaned docs.

**Use when:** Running PM maintenance windows, or as a pre-commit check to prevent rot.

---

## Quick Start

```bash
$ bash ./doc-lint.sh
```

Scans `Documents/` and reports:
- **Status drift** — `.md` files outside `Roadmap/` that contain `Status: done`, `Status: draft`, etc.
- **Orphaned docs** — `.md` files not linked from any index or parent.
- **Stale deprecation** — Docs marked "DEPRECATED" but missing a link to the current pattern.
- **Broken links** — Links to non-existent files (design docs, spec files, etc.).

---

## What It Checks

### 1. Status Drift (Convention #1)
**Rule:** Only `Roadmap/roadmap.md` should contain `Status: done`, `Status: resolved`, etc.

**Scans:** All `.md` files outside `Roadmap/`.  
**Flags:** Any file containing regex `^Status:\s+(done|resolved|draft|target|in review)`  
**Output:**
```
Status drift detected:
  Documents/design/1-d20.md:1  →  "Status: draft"
  Documents/Specs/engine-host-spec.md:3  →  "Status: concrete companion"
  (This belongs in roadmap only; update roadmap, remove status field here, and refer to it.)
```

### 2. Orphaned Documents (Convention #7)
**Rule:** Every `.md` file is linked from an index or parent page.

**Scans:** All `.md` files in `Documents/`.  
**Builds:** List of linked files from all index pages (`*/index.md`, `*/README.md`, etc.).  
**Flags:** Any file not in the linked list.  
**Output:**
```
Orphaned documents (not linked from any index):
  Documents/some-old-thing.md  →  Decide: delete or add to an index?
  Documents/ClaudeWiki/Roles/old-persona.md  →  Not in index.md; add it or move to archive.
```

### 3. Deprecation Incomplete (Convention #3)
**Rule:** Any doc marked "DEPRECATED" must have an immediate link to the current pattern.

**Scans:** All `.md` files for "DEPRECATED" or "deprecated".  
**Checks:** Next 10 lines for a link pattern like `[Current pattern](` or `see [X](link)`.  
**Flags:** Missing or broken link.  
**Output:**
```
Incomplete deprecation:
  Documents/NineRooms/nine-rooms.md  →  "DEPRECATED" but no link to current pattern in next 10 lines.
  Fix: Add "**Current pattern:** see [D20 System](link)." right after the deprecation notice.
```

### 4. Broken Links (Bonus)
**Scans:** All `[text](path)` links.  
**Checks:** File exists at the target path (relative or absolute).  
**Flags:** Missing files, typos, wrong extensions.  
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
bash ./doc-lint.sh
```

### Specific checks:
```bash
bash ./doc-lint.sh --check status-drift
bash ./doc-lint.sh --check orphaned
bash ./doc-lint.sh --check deprecation
bash ./doc-lint.sh --check links
```

### Format output:
```bash
bash ./doc-lint.sh --format json    # Machine-readable
bash ./doc-lint.sh --format csv     # Spreadsheet-friendly
bash ./doc-lint.sh --format text    # Default (readable summary)
```

---

## Output Examples

### No issues found:
```
✓ Status drift check: OK (0 issues)
✓ Orphaned docs check: OK (0 issues)
✓ Deprecation check: OK (1 warning: Nine Rooms marked historical)
✓ Broken links: OK (0 issues)

Summary: All checks passed. Docs are clean.
```

### With issues:
```
✗ Status drift: 3 issues found
  Documents/design/1-d20.md:1  →  Status field detected (belongs in roadmap only)
  [... more ...]

✗ Orphaned: 2 issues found
  Documents/passdown.md  →  Not linked from any index
  [... more ...]

✗ Deprecation: 1 issue found
  Documents/NineRooms/nine-rooms-gameplay.md  →  Has "deprecated" but no link to current pattern
  [... more ...]

✗ Broken links: 5 issues found
  Documents/design/architecture.md:42  →  Link to ../desgin/glossary.md (typo: "desgin")
  [... more ...]

Summary: 11 issues. Run with --fix to auto-correct what's automatable.
```

---

## Auto-Fix Mode

```bash
bash ./doc-lint.sh --fix
```

Auto-fixes what it can:
- **Status drift:** Removes `Status: [field]` lines, comments them out for review.
- **Broken links:** Flags typos (high confidence) for manual fix; doesn't auto-correct.
- **Missing deprecation link:** Suggests template; doesn't insert (needs editorial judgment).
- **Orphaned docs:** Asks "delete or link?" — doesn't decide for you.

---

## PM Maintenance Window Checklist

Run before closing a phase:

```bash
# 1. Run full audit
bash ./doc-lint.sh

# 2. Fix any status drift (remove status fields outside roadmap)
# 3. Fix any orphaned docs (link them or archive them)
# 4. Verify deprecation links (if anything new was deprecated)
# 5. Spot-check broken links (especially in new/updated docs)

# 6. If all clean, commit
git add -A && git commit -m "Docs sync: Phase N complete"
```

---

## Implementation

The script (`doc-lint.sh`):

1. **Scans `Documents/` recursively** for all `.md` files.
2. **For each check:**
   - Runs a regex or pattern match (e.g., status field regex).
   - Builds a list of files/line numbers that match.
   - Cross-references against a "clean list" (expected entries, e.g., roadmap status fields).
3. **Reports findings** in human-readable or machine-readable format.
4. **Optionally auto-fixes** what's safe to fix (removals, suggestions).
5. **Exits with status code:**
   - `0` if all checks pass.
   - `1` if issues found (non-fatal; doesn't block commit).
   - `2` if script errors (bad args, etc.).

---

## Typical Issues It Catches

| Issue | Example | Fix |
| --- | --- | --- |
| Status outside roadmap | `Documents/design/1-d20.md` has `Status: draft` | Remove field; refer to roadmap |
| Orphaned doc | `Documents/passdown.md` not linked | Link from index or move to archive |
| Incomplete deprecation | `nine-rooms-gameplay.md` has "DEPRECATED" but no "see X" link | Add link template |
| Typo in link | Link to `../desgin/glossary.md` | Fix to `../design/glossary.md` |
| Dead reference | Link to non-existent `Specs/foo-spec.md` | Fix or remove link |

---

## Related

- [Documentation System Audit](../documentation-audit.md) — why these checks exist
- [Wiki Style Guidelines](../Processes/wiki-style.md) — conventions this enforces
- [doc-scaffold](doc-scaffold-SKILL.md) — companion: generates compliant docs
