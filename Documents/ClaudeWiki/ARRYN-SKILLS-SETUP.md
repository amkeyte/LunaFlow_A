# Documentation Skills Setup — For Arryn

**TL;DR:** Two companion skills to scaffold new docs and audit for drift. No installation required; scripts live in `Documents/ClaudeWiki/Tools/`. Run them manually or on schedule.

---

## What You Have

### 1. **doc-scaffold** (`doc-scaffold.ps1`)
Generates new handoff tickets and specs with correct structure, preambles, and conventions baked in. No more copy-paste templates.

**One command to create a ticket:**
```powershell
pwsh ./doc-scaffold.ps1 -Type ticket -Id 0025 -Title "My ticket title"
```

**One command to create a spec:**
```powershell
pwsh ./doc-scaffold.ps1 -Type spec -Name "world-module" -DesignDoc "scripting-content.md"
```

**Payoff:** Tickets and specs always have the right structure. Preambles are correct (one-liner + link, no duplication). Conventions are baked in; can't mess up the format.

### 2. **doc-lint** (`doc-lint.sh`)
Audits your docs against conventions. Catches:
- Status fields living outside the roadmap (should only be in `Roadmap/roadmap.md`)
- Orphaned docs not linked from any index
- Incomplete deprecation notices (marked "DEPRECATED" but missing link to current pattern)
- Broken links and typos

**One command to run full audit:**
```bash
bash ./doc-lint.sh
```

**Payoff:** No silent rot. Status drift is caught immediately. Orphaned docs are flagged before they become a problem.

---

## Where to Find Them

Both scripts are in `Documents/ClaudeWiki/Tools/` as skeleton `.md` files with embedded instructions:
- `doc-scaffold-SKILL.md` — describes what it generates, parameters, workflow examples
- `doc-lint-SKILL.md` — describes what it checks, how to interpret output, PM checklist

**These are not yet implemented.** They're designs/specs for you to have built out as bash/PowerShell scripts or Python if you prefer.

---

## Implementation (What You Need to Do)

Choose your tool chain. Here are two paths:

### Path A: PowerShell (If You Like Windows Scripts)

**For doc-scaffold.ps1:**
```powershell
# Basic structure (pseudo-code; flesh out the details)
param(
    [string]$Type,       # "ticket" or "spec"
    [string]$Id,         # For tickets: "0025"
    [string]$Title,      # For tickets: "My ticket title"
    [string]$Name,       # For specs: "world-module"
    [string]$DesignDoc,  # For specs: "scripting-content.md"
    [string]$OpenedBy,   # Optional: ticket metadata
    [string]$For         # Optional: ticket metadata
)

# 1. Validate inputs (ticket ID unique, spec name doesn't exist, design doc exists)
# 2. Create file with correct path
# 3. Populate with template (insert title, links, sections)
# 4. Report success + guidance
```

**For doc-lint.sh (adapt to PowerShell if needed):**
```powershell
# Check for status drift, orphaned docs, broken deprecation links, broken links
# Report findings in text/json/csv format
# Support --fix mode for safe auto-corrections
```

### Path B: Bash/Python (If You Like Unix)

Write the scripts in bash (or Python for complex logic):
- `doc-scaffold.sh` — generate templates using `cat` + variable substitution + file ops
- `doc-lint.sh` — use `grep`, `find`, `sed` for pattern matching; Python subprocess for complex validation

---

## Quick Implementation Roadmap

**Week 1:** Build doc-scaffold.ps1 (or .sh)
- [ ] Accept parameters for ticket or spec
- [ ] Generate correct file paths and preambles
- [ ] Test with one ticket and one spec generation

**Week 2:** Build doc-lint.sh (or equiv)
- [ ] Scan for status drift (regex: `^Status: (done|draft)` outside Roadmap/)
- [ ] Find orphaned docs (files not linked from indexes)
- [ ] Check deprecation links (find "DEPRECATED", verify link in next 10 lines)
- [ ] Test against your existing docs

**Week 3:** Polish + integrate
- [ ] Add --fix mode to doc-lint
- [ ] Create PM maintenance checklist in wiki-style.md
- [ ] Document in the tool pages (you already have the SKILLs written)
- [ ] Consider adding to a pre-commit hook or scheduled task

**Minimal viable:** Get doc-scaffold working first (high immediate payoff). doc-lint is useful but less urgent.

---

## How the PM Will Use Them

### When creating a new ticket:
```
PM: "I need a ticket for Baraka to implement the world-module spec."
→ PM runs: pwsh doc-scaffold.ps1 -Type ticket -Id 0026 -Title "Implement world module spec" -For "Baraka"
→ File appears with Decision/Context/Verification/Follow-up sections pre-structured
→ PM fills in the Ask, hands off to Baraka
```

### When creating a new spec:
```
PM: "Juno designed the world-module layer. I need a buildable spec."
→ PM runs: pwsh doc-scaffold.ps1 -Type spec -Name "world-module" -DesignDoc "scripting-content.md"
→ File appears with Purpose/Shape/AcceptanceCriteria/OpenQuestions sections
→ PM fills in the specifics, hands off to Baraka to build against
```

### Before closing each phase (15-minute maintenance):
```
PM: "Time to close Phase N. Checking for doc drift..."
→ PM runs: bash doc-lint.sh
→ Script reports: "Status drift: 0 | Orphaned: 0 | Deprecation: 0 | Links: OK"
→ PM can confidently mark the phase done
```

**If doc-lint finds issues:**
```
→ PM runs: bash doc-lint.sh --fix
→ Script auto-removes status fields outside roadmap, suggests link templates
→ PM manually reviews and commits
```

---

## Integration Points

### Optionally wire into CI/pre-commit:

If you want drift prevention at git time, add doc-lint to a pre-commit hook:

```bash
# .git/hooks/pre-commit
bash ./Documents/ClaudeWiki/Tools/doc-lint.sh --check status-drift --check orphaned
if [ $? -ne 0 ]; then
  echo "Docs have drift issues. Run: bash doc-lint.sh --fix"
  exit 1
fi
```

This prevents commits that introduce orphaned docs or status field drift.

### Or run on schedule:

If you prefer a gentler "nag," run doc-lint once a week:
```bash
# cron job (adjust as needed)
0 0 * * 1  cd /path/to/LunaFlow_A && bash ./Documents/ClaudeWiki/Tools/doc-lint.sh > ./doc-audit.txt
# Email doc-audit.txt to PM if not empty
```

---

## Testing Your Implementation

Before rolling out, test against your real docs:

**Test doc-scaffold:**
```powershell
# Create a test ticket
pwsh doc-scaffold.ps1 -Type ticket -Id 9999 -Title "TEST ONLY"
# Verify: Documents/ClaudeWiki/Processes/Handoffs/9999-test-only.md exists
#         Has Decision/Context/Verification/Follow-up sections
#         Status field is "open"
# Delete the file
rm Documents/ClaudeWiki/Processes/Handoffs/9999-test-only.md
```

**Test doc-lint:**
```bash
# Run audit (baseline)
bash doc-lint.sh > baseline-report.txt

# Introduce a deliberate error (add Status: draft to a design doc)
echo "Status: draft" >> Documents/design/test-doc.md

# Run audit again
bash doc-lint.sh > error-report.txt

# Verify: error-report.txt mentions the test-doc.md status drift
# Clean up
git checkout Documents/design/test-doc.md
```

---

## Maintenance & Evolution

**Once deployed**, you own these scripts. Keep them in sync with your conventions:

- If you add a new convention to `wiki-style.md`, update doc-lint to check for it.
- If you rename sections (e.g., "Context" → "Background"), update doc-scaffold templates.
- If agents report a common mistake you want to prevent, add a doc-lint check.

**Review annually** or after major doc restructuring. The goal is to make it impossible to create docs that drift from your conventions.

---

## Quick Ref: Script Locations

| Tool | Location | Purpose |
| --- | --- | --- |
| doc-scaffold.ps1 | `Documents/ClaudeWiki/Tools/` | Generate new tickets/specs |
| doc-lint.sh | `Documents/ClaudeWiki/Tools/` | Audit for drift & orphans |
| SKILL descriptions | `doc-scaffold-SKILL.md`, `doc-lint-SKILL.md` | Documentation for the tools |
| This guide | `Documents/ClaudeWiki/ARRYN-SKILLS-SETUP.md` | Setup & implementation instructions |

---

## Questions?

- **"Should I use PowerShell or bash?"** — PowerShell is fine for Windows; bash is more portable. Pick whichever you're comfortable maintaining.
- **"Can I integrate with GitHub Actions?"** — Yes. doc-lint can be a workflow that runs on PR, flagging docs drift before merge.
- **"Do I need to implement --fix right away?"** — No. Start with read-only auditing (doc-lint reports issues). Auto-fix can come later if it saves time.
- **"What if I change doc conventions?"** — Update wiki-style.md and the scripts together. They should stay in sync.

---

## Next Steps

1. **Review** the two SKILL.md files to confirm the designs match your needs.
2. **Pick your implementation** (PowerShell or bash).
3. **Build doc-scaffold.ps1** first (high ROI; week 1-2 effort).
4. **Build doc-lint.sh** second (week 2-3 effort).
5. **Wire into workflow** — PM runs doc-scaffold when creating tickets/specs. Runs doc-lint at phase closes.
6. **Test against real docs** before committing to repo.
7. **Document in the project** — Add a "Running the doc tools" section to wiki-style.md or CLAUDE.md once live.
