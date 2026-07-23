# Documentation System Audit

**Scope:** Read-only audit focused on streamlining for minimal agent token usage.  
**Date:** 2026-07-22  
**Purpose:** Identify deficiencies in the doc system and provide fixes + conventions for the PM to work through incrementally.

---

## Executive Summary

The LunaFlow documentation system is **intentionally structured and well-linked**, with strong adherence to the "one concern per file" principle. However, **three patterns create token inefficiency** that agents encounter repeatedly when loading context:

1. **Distributed status tracking** — Phase status lives in 4+ places (roadmap, handoff summary column, individual tickets, QA index, design doc status fields), requiring agents to cross-reference.
2. **Over-explanation in specs** — Each spec file restates the design-doc relationship and context before diving into buildable content.
3. **Dense handoff tickets** — Complete resolution narratives make tickets valuable for audit trails but token-heavy for agents looking only for decisions/blockers.

**Good news:** All deficiencies are fixable without restructuring the core system. The fixes are modular and can be implemented in phases without blocking each other.

---

## Deficiencies (Eight Total)

### 1. **Status Distributed Across Four Locations** (CRITICAL)

**Problem:** "Is Phase 2.5 done?" requires consulting roadmap, handoff index, QA index, and individual ticket headers. No single source of truth.

**Impact:** Agents must cross-reference multiple sources. Stale status in one place isn't obvious. Can't confidently answer phase status from any single read.

**Fix:** Roadmap is the source of truth. All other docs link to it, never restate status.

---

### 2. **Redundant Context in Spec Preambles** (MEDIUM)

**Problem:** Every spec repeats "this is concrete, the design doc explains why" intro. Agents read same explanation in each spec.

**Impact:** Specs are 15–20% longer than buildable content requires. Agents waste tokens on repeated preambles.

**Fix:** Factor preambles into shared template in `Specs/README.md`. Each spec opens directly to buildable content.

**Token savings:** ~100–150 lines per spec read.

---

### 3. **Handoff Tickets Contain Full Resolution Narratives** (MEDIUM)

**Problem:** Tickets record entire decision journey (ask → discussion → ruling → proof → verification). A 200-line ticket to convey "this was decided; verified."

**Impact:** Agents doing quick lookups must parse through the whole file. Test Dev loading testing tickets must read past irrelevant details.

**Fix:** Add "Decision" section (2–4 sentences) at the top. Full narrative moves to "Details" section.

**Token savings:** ~40–80 lines per quick-lookup ticket.

---

### 4. **Test Documentation Requires Four Separate Files** (MEDIUM)

**Problem:** Testing knowledge split intentionally across `testing.md`, `test-runbook.md`, `test-patterns.md`, `agent-test-workflow.md`. Agents need all four to avoid anti-patterns and know how to run.

**Impact:** New test authoring = 4-file context load (~500 lines total). No shortcut path.

**Fix:** Create `testing-quick-start.md` that hits 80/20 essentials. Link to detailed docs for deep dives.

**Token savings:** ~300 lines when an agent writes one file instead of four.

---

### 5. **Deprecated Content Still Carries Full Weight** (MEDIUM)

**Problem:** Nine Rooms marked deprecated in one place but still has 4 supporting files, full API reference, continued references in tickets. Agents waste tokens confirming it's not their pattern.

**Impact:** Agents encounter Nine Rooms warnings mid-doc and re-read to confirm it's historical. Confusion on which patterns to follow.

**Fix:** Create `NineRooms/README-DEPRECATED.md`. Mark all references as historical. Link to current pattern (D20 System).

**Token savings:** ~50 lines saved when agents don't re-read stale guidance.

---

### 6. **Architecture Over-Explains Upfront** (LOW-MEDIUM)

**Problem:** `architecture.md` is 300+ lines front-loading philosophy. Agent needing "who can edit what?" must skim to find the role/access table.

**Impact:** Agents must skim rather than scan. New roles risk being added in narrative before the matrix, causing sync drift.

**Fix:** Move role/access table and layer diagram to "Quick Reference" section at top. Move philosophy to "Design Rationale" below.

**Token savings:** ~80 lines when agents only need role/tier info.

---

### 7. **Design Doc Status Fields Drift from Roadmap** (LOW)

**Problem:** Design docs have header `Status: [draft|target|in review]` fields. Roadmap is the real source of truth. They drift.

**Impact:** Agents can't trust design doc status without checking roadmap. Maintenance burden that adds no value.

**Fix:** Remove status fields. Add one-liner: "Implementation status: see Roadmap — phase X."

---

### 8. **No Quick-Reference Path for Orientation** (MEDIUM)

**Problem:** No quick-reference guide for common agent questions: "What's the active phase?" "Is feature X done?" "What's my role?" "Show me the public API." Agents must navigate full structure.

**Impact:** Onboarding agents waste context. Context thrashing when agents need specific pieces of information.

**Fix:** Create `Documents/README-QUICK-START.md` with FAQ and navigation shortcuts.

**Token savings:** Prevents multi-file context loads for simple questions.

---

## Fixes (Prioritized)

| Priority | Fix | Deficiency | Effort | Payoff |
| --- | --- | --- | --- | --- |
| **1** | Establish status cascade (roadmap = source of truth) | 1 | Low | CRITICAL — blocks downstream sync |
| **2** | Factor spec preambles into template | 2 | Low | ~100+ tokens per spec read |
| **3** | Add "Decision" section to tickets | 3 | Low | ~40–80 tokens per quick-lookup |
| **4** | Create testing-quick-start.md | 4 | Low | ~300 tokens when writing tests |
| **5** | Consolidate Nine Rooms deprecation | 5 | Low | ~50 tokens; clarity gain |
| **6** | Create agent quick-reference README | 8 | Low | Orientation efficiency |
| **7** | Restructure architecture.md | 6 | Medium | Clarity, lower token load |
| **8** | Remove design doc status fields | 7 | Low | Sync improvement |
| **9** | Formalize conventions into wiki-style.md | All | Medium | Long-term maintenance discipline |

---

## Conventions to Prevent Future Rot

### Convention 1: "Status Lives in One Place"
- Roadmap is the source of truth. Design docs, tickets, specs refer to it; never restate.
- Add to `CLAUDE.md`: "Status updates: only in `Roadmap/roadmap.md`."
- Add to `wiki-style.md`: "Before marking a phase done, verify no stale status fields elsewhere."

### Convention 2: "Link, Don't Restate"
- Preambles link and say *where* to look, not *what* you'll find.
- If preamble >80 words, trim it. Move explanation to main body or linked doc.
- Code-review checklist: "Does this duplicate content from a linked file? If yes, remove it."

### Convention 3: "Deprecation Implies Redirection"
- Any deprecated doc has immediate link to current pattern.
- Template: `> **DEPRECATED** (date) — [X] is historical. **Current:** see [Y](link).`

### Convention 4: "Decision-First Tickets"
- Every ticket opens with "Decision" section (2–4 sentences): ask, ruling, brief why.
- Full narrative in "Details" below.

### Convention 5: "Quick-Start for Multi-File Concerns"
- If a concern splits into 3+ files but agents need the whole picture to start, provide a quick-start.
- Example: testing-quick-start ties together testing.md + test-runbook.md + test-patterns.md + agent-test-workflow.md.

### Convention 6: "Current Pattern Marker"
- Mark files that represent current, recommended approach at the top.
- Deprecated/historical content has equally clear marker + redirect.

### Convention 7: "No Orphaned Documents"
- Every `.md` file in `Documents/` is linked from an index.
- Before adding a new doc, check if it fits an existing category.
- Lint: `grep` for .md files not linked from any index.

### Convention 8: "Maintenance Windows"
- PM does 15-minute sync after each phase closes:
  - Roadmap status current?
  - No stale status fields in design docs?
  - Handoff index, QA index reflect roadmap?
  - Deprecation links all present?
- Add checklist to `wiki-style.md` under "PM responsibilities."

---

## Implementation Checklist for PM

Work through these in priority order. Each is independent; you can pick and choose.

- [ ] **Fix 1:** Update roadmap entry doc in CLAUDE.md: "Status updates only in roadmap."
- [ ] **Fix 1:** Create index entries in Handoffs and QA that just mirror roadmap status (no duplication).
- [ ] **Fix 2:** Create `Specs/README.md` with preamble template.
- [ ] **Fix 2:** Trim all spec preambles to one-liner + link to template.
- [ ] **Fix 3:** Add "Decision" section to all existing tickets (or just new ones going forward).
- [ ] **Fix 4:** Create `Processes/testing-quick-start.md` (~150 lines).
- [ ] **Fix 4:** Update ClaudeWiki index to link quick-start first.
- [ ] **Fix 5:** Create `NineRooms/README-DEPRECATED.md`.
- [ ] **Fix 5:** Update all Nine Rooms links to add "— **historical**" comment.
- [ ] **Fix 6:** Restructure `architecture.md`: move role/tier table to top.
- [ ] **Fix 7:** Remove `Status:` lines from all design docs. Add one-liner referring to roadmap.
- [ ] **Fix 8:** Create `Documents/README-QUICK-START.md` (~250 lines).
- [ ] **Fix 8:** Link from CLAUDE.md, Documents/README.md, ClaudeWiki index.
- [ ] **Conventions:** Add all 8 conventions to `Processes/wiki-style.md` + create PM maintenance checklist.

---

## Related

- [Handoff Ticket Process](Processes/handoff-tickets.md)
- [Wiki Style Guidelines](Processes/wiki-style.md)
- [ClaudeWiki Index](index.md)
- [Tools Index](Tools/) — companion docs: doc-scaffold & doc-lint skills
