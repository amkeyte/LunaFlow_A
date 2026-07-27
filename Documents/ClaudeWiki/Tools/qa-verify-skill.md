# Skill — `qa-verify`

**Location:** `.claude/skills/qa-verify/SKILL.md`

## Purpose

Fixes [QA (Lothar)](../Roles/qa.md)'s verification routine and report format so a "pass" means
the same thing regardless of which session runs it. Without this, a fresh QA session re-derives
its own checklist and report shape every time it's spun up cold — inconsistent rigor across
sessions is itself a rubber-stamping risk, which is the exact failure mode this role exists to
prevent.

## What it encodes

- Fixed check order: pin down the actual acceptance criteria → build → automated suite → manual
  spot-check only for what the suite doesn't cover → cross-check every criterion individually →
  scan for known red flags (`NotImplementedException`, undisclosed prior QA reports).
- A fixed report template, written to `Documents/QA/<phase-or-ticket-slug>-verdict.md`.
- The reconciliation rule for superseding a stale report: move it to `Documents/QA/Archive/` with
  a banner, never silently overwrite or delete it.
- The block-vs-ticket rule: any FAIL or UNPROVEN criterion blocks the phase and gets a handoff
  ticket filed; QA doesn't fix code or talk itself into "pass with follow-ups."

**Added 2026-07-26** (see ticket [0029](../Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md)),
**pending application to the executable skill file** — `.claude/skills/qa-verify/SKILL.md` is
write-protected from an agent session; Arryn needs to apply these two additions directly:

- **Out-of-scope check (Step 1), distinct from UNPROVEN.** For each criterion, ask whether it
  references content/mechanisms/phases that don't exist *anywhere in the codebase yet* — not "not
  built for this ticket," but genuinely absent (e.g. a Done-When clause about two ROMs correlating
  a fact when only one ROM exists). UNPROVEN means the work hasn't landed or been tested yet — a
  Dev/Test gap. Out-of-scope means nobody could evaluate this criterion until a later phase's
  content exists — a **Done-When bar problem**, routed to the PM as a scope correction, not left to
  sit scoring UNPROVEN indefinitely. Report in a new `## Scope flags` section. This exists because
  Phase 2.6.5 shipped with exactly this unrecognized — a Done-When clause referencing Phase 2.7
  content that didn't exist at close.
- **Vestigial-concept scan (Step 6, red flags).** While reading the touched public surface (via
  `Documents/ApiReference/`), flag any type/member that's placeholder-marked ("future work," "not
  currently in use"), has zero consumers, or whose definition contradicts a current design doc or
  `glossary.md` entry. Not a FAIL on its own — a finding routed to PM/Architect for a ruling (keep,
  relocate, retype, delete), reported in a new `## Vestigial concepts flagged` section. This exists
  because `IGameSystem` sat in `LunaFlow.Rules` — public, unused, contradicting `architecture.md`'s
  own definition of "game system" — until Amara happened across it while scoping unrelated design
  work, not through any systematic check. Ruling and deletion ask: [ticket
  0030](../Processes/Handoffs/0030-doc-reconcile-post-2-6-5.md) item 12 (Amara's ruling) → [ticket
  0031](../Processes/Handoffs/0031-code-currency-vs-2-6-5-design.md) item 4 (Baraka's ask).

## Workspace

`Documents/QA/` holds the verdict artifacts this skill produces — `index.md` lists the current
authoritative verdict per phase, one file per verdict, same shape as
[`Handoffs/`](../Processes/Handoffs/index.md).

## Related pages

- [QA / Verification role](../Roles/qa.md)
- [Handoff ticket process](../Processes/handoff-tickets.md)
- [ClaudeWiki index](../index.md)
