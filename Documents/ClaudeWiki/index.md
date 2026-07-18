# ClaudeWiki

Living knowledge base for the LLM sessions (chats/agents) working on LunaFlow. Distinct from
`Documents/design/` and `Documents/Specs/`: those are the game engine's design and specs.
This wiki is about *how the team of chats works* — roles, process, tools, and anything else
that helps a session ramp up fast and stay in scope, without belonging in a design doc.

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

| Section | Purpose | Status |
| --- | --- | --- |
| [`Roles/`](Roles/) | One page per chat-session role: scope, authority, communication protocol | started |
| [`Processes/`](Processes/) | Team workflows — handoffs, review requests, escalation paths | started |
| [`Tools/`](Tools/) | Skills, scripts, and utilities inventory, with usage notes | started |

## Pages

| Page | Section | Status |
| --- | --- | --- |
| [Supreme Leader (PM)](Roles/supreme-leader.md) | Roles | active |
| [Persona — "Kofi"](Roles/supreme-leader-persona.md) | Roles | active |
| [Architect](Roles/architect.md) | Roles | active |
| [Dev — Implementation](Roles/dev.md) | Roles | active |
| [Dev — Test Suite](Roles/dev-test.md) | Roles | active |
| [QA / Verification](Roles/qa.md) | Roles | active |
| [Handoff Tickets](Processes/handoff-tickets.md) | Processes | active |
| [Handoff Ticket Index](Processes/Handoffs/index.md) | Processes | active |
| [Testing — conventions & workflow](Processes/testing.md) | Processes | active |
| [Test Runbook — execution & diagnostics](Processes/test-runbook.md) | Processes | active |
| [Test Patterns — concurrency-safe authoring](Processes/test-patterns.md) | Processes | active |
| [Agent-Assisted Test Workflow](Processes/agent-test-workflow.md) | Processes | active |
| [Wiki Style — page conventions](Processes/wiki-style.md) | Processes | active |
| [Skill — `qa-verify`](Tools/qa-verify-skill.md) | Tools | active |

## Migration candidates

Existing docs that may move here once this structure firms up (not yet moved):

- `Documents/passdown.md` — the informal version of what `Processes/` is meant to formalize.

## How to add a page

Follow [`Processes/wiki-style.md`](Processes/wiki-style.md) — the authoritative standard for
placement, structure, registration, and the link-don't-duplicate rule. In short: right section
folder, one concern, register it here, don't orphan it.
