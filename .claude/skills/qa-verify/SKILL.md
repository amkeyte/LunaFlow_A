---
name: qa-verify
description: Run the QA (Lothar) verification routine against a phase or handoff ticket's done-bar — fixed check order and report template, so a "pass" means the same thing regardless of which session produced it. Use when asked to verify a done claim, sign off a phase, or produce a QA verdict for LunaFlow.
---

# QA verification routine

This is Lothar's checklist, not Lothar's opinion. It exists so every QA pass — regardless of
which session runs it — checks the same things in the same order and reports them the same way.
Skipping a step because "it obviously passes" is exactly the rubber-stamping this role exists to
prevent.

## 1. Pin down what's being claimed

Read the actual acceptance criteria before touching code — from the handoff ticket's `## Ask`,
the relevant `Documents/Specs/*.md`, or the roadmap phase entry. List each criterion explicitly.
Do not invent criteria that sound reasonable, and do not drop one because it's inconvenient to
check. If a criterion is genuinely ambiguous, that's a ticket to the Architect, not a guess.

## 2. Build

```
dotnet build LunaFlow_A.sln
```

Paste the real result. "Should build" is not evidence. If the toolchain isn't available in this
session, say so explicitly rather than assuming a build would pass — an unbuildable claim is a
FAIL by default, not a skip.

## 3. Automated suite

```
dotnet test tests/LunaFlow.EngineTests
```

Record the actual pass/fail counts. A suite that doesn't exist yet, or doesn't cover a claimed
criterion, means that criterion is unproven — say so, don't infer it from adjacent green tests.

## 4. Manual/smoke pass — only for what step 3 doesn't cover

Run the specific scenario (e.g. `dotnet run --project src/MyLunaFlowServer` and a real
playthrough), capture the actual transcript, and diff it against the expected transcript line by
line. Note explicitly which criteria are suite-covered vs. manually spot-checked, and why the gap
exists.

## 5. Cross-check every criterion individually

Go back to the list from step 1. Each criterion gets its own verdict citing which of steps 2–4
proved it. No criterion inherits a pass from a neighboring one ("the other seven win-lines fired
correctly so this one probably does too" is not evidence).

## 6. Scan for known red flags

- `grep`/search for `NotImplementedException` in the relevant path if the done-bar claims none
  remain.
- Check whether a prior QA report already exists for this phase in `Documents/QA/` — if so, it
  must be reconciled (see below), not left to disagree silently with the new verdict.

## Report format

One file per verdict: `Documents/QA/<phase-or-ticket-slug>-verdict.md`.

```markdown
# <Phase/Ticket> — QA Verdict

**Verdict:** PASS | FAIL
**Date:** <date>
**Ticket:** <link to handoff ticket, if any>
**Build:** <toolchain available? build result, verbatim>
**Suite:** <dotnet test result, verbatim>

## Criteria

| Criterion | Evidence | Result |
| --- | --- | --- |
| <from spec/ticket, verbatim> | <suite test name, or manual transcript excerpt, or "unproven — no toolchain"> | PASS/FAIL/UNPROVEN |

## Deviations from spec

<implementation choices that diverge from the written spec but aren't blocking — informational,
not a pass/fail input>

## Superseded reports

<link to any prior report in Documents/QA/ this replaces, and why>

## Tickets filed

<links to any handoff tickets opened as a result of a FAIL>
```

Then:

1. Update `Documents/QA/index.md` to point at this file as the current verdict for the phase.
2. If superseding a prior report, move it to `Documents/QA/Archive/` with a one-line banner at
   the top ("Superseded by `<new file>` on `<date>` — see there for the current verdict.")
   prepended. Don't delete it; it's the audit trail.
3. Fill in the `## Response` section of the originating handoff ticket (if any) and leave
   `## Resolution` for the opener — QA doesn't close its own ticket.

## When to block vs. file a ticket

- Any FAIL or UNPROVEN criterion → the phase does not get marked done. File a handoff ticket:
  defect → [Dev](../../../Documents/ClaudeWiki/Roles/dev.md), spec mismatch →
  [Architect](../../../Documents/ClaudeWiki/Roles/architect.md).
- QA does not fix the code, does not edit the spec, and does not talk itself into "pass with
  follow-ups." A follow-up on a failing criterion is a FAIL.
