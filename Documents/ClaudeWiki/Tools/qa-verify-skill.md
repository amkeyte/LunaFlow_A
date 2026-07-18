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

## Workspace

`Documents/QA/` holds the verdict artifacts this skill produces — `index.md` lists the current
authoritative verdict per phase, one file per verdict, same shape as
[`Handoffs/`](../Processes/Handoffs/index.md).

## Related pages

- [QA / Verification role](../Roles/qa.md)
- [Handoff ticket process](../Processes/handoff-tickets.md)
- [ClaudeWiki index](../index.md)
