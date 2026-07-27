# ClaudeWiki

Living knowledge base for the LLM sessions (chats/agents) working on LunaFlow. Distinct from
`Documents/design/` and `Documents/Specs/`: those are the game engine's design and specs.
This wiki is about *how the team of chats works* — roles, process, tools, and anything else
that helps a session ramp up fast and stay in scope, without belonging in a design doc.

**New here?** Start with [`../README-QUICK-START.md`](../README-QUICK-START.md) — FAQ, navigation, and key conventions.

## Design principles

- **Small and hyper-topical.** One concern per file. A session should be able to load exactly
  the page(s) it needs, not a monolith.
- **LLM-first audience.** Written for a fresh agent session to read cold and act on, not as
  prose for a human to enjoy. Terse, structured, current.
- **Traceable from here.** Every page gets linked from this index (or from a section index it
  rolls up to) so nothing is orphaned.
- **Living, not archival.** Update pages in place as decisions change. If a page goes stale,
  fix it or mark it stale — don't let it silently rot the way `planning/archive/` was allowed to.

## Structure

| Section | Purpose |
| --- | --- |
| [`Roles/`](Roles/) | One page per chat-session role: scope, authority, communication protocol |
| [`Processes/`](Processes/) | Team workflows — handoffs, review requests, escalation paths |
| [`Tools/`](Tools/) | Skills, scripts, and utilities inventory, with usage notes |

## Pages

| Page | Section |
| --- | --- |
| [Supreme Leader (PM)](Roles/supreme-leader.md) | Roles |
| [Persona — "Kofi"](Roles/supreme-leader-persona.md) | Roles |
| [Architect](Roles/architect.md) | Roles |
| [Game Designer](Roles/game-designer.md) | Roles |
| [Dev — Implementation](Roles/dev.md) | Roles |
| [Dev — Test Suite](Roles/dev-test.md) | Roles |
| [QA / Verification](Roles/qa.md) | Roles |
| [Handoff Tickets](Processes/handoff-tickets.md) | Processes |
| [Handoff Ticket Index](Processes/Handoffs/index.md) | Processes |
| [Testing — quick-start](Processes/testing-quick-start.md) | Processes |
| [Testing — conventions & workflow](Processes/testing.md) | Processes |
| [Test Runbook — execution & diagnostics](Processes/test-runbook.md) | Processes |
| [Test Patterns — concurrency-safe authoring](Processes/test-patterns.md) | Processes |
| [Agent-Assisted Test Workflow](Processes/agent-test-workflow.md) | Processes |
| [Wiki Style — page conventions](Processes/wiki-style.md) | Processes |
| [Content Spec Template — buildable-spec shape](Processes/content-spec-template.md) | Processes |
| [API Doc Generation — `DefaultDocumentation` wiring & MAX_PATH](Processes/api-doc-generation.md) | Processes |
| [Documentation System Audit — deficiencies, fixes, conventions](documentation-audit.md) | System |
| [Skill — `qa-verify`](Tools/qa-verify-skill.md) | Tools |
| [Tool — `.trx` Triage Engine](Tools/trx-triage-tool.md) | Tools |
| [Doc Tooling — setup & implementation notes](Tools/doc-tooling-setup.md) | Tools |
| [Skill — `doc-scaffold`](Tools/doc-scaffold-SKILL.md) | Tools |
| [Skill — `doc-lint`](Tools/doc-lint-SKILL.md) | Tools |

## Migration candidates

Existing docs that may move here once this structure firms up (not yet moved):

- `Documents/passdown.md` — the informal version of what `Processes/` is meant to formalize.

## How to add a page

Follow [`Processes/wiki-style.md`](Processes/wiki-style.md) — the authoritative standard for
placement, structure, registration, and the link-don't-duplicate rule. In short: right section
folder, one concern, register it here, don't orphan it.
