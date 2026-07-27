# LunaFlow — Agent Quick-Start

Fast answers to common agent questions. For deep dives, follow the links.

---

## FAQ

**Q: I need to understand what's in the codebase. Where do I start?**
A: **Always read the API Reference first** — it's generated from source after every build, so it's current and accurate. See [`ApiReference/`](ApiReference/) (open after `dotnet build`). The public surface is usually what you need. Only read `/src` directly if you need implementation details or the API Reference doesn't answer your question.

**Q: What's the active roadmap phase?**
A: See [`Roadmap/roadmap.md`](Roadmap/roadmap.md) for current status — this page doesn't restate it (phase status changes; restating it here just gives it a second place to go stale).

**Q: Is feature X done?**
A: Roadmap is the source of truth. No status in design docs, role pages, or prose — only in `Roadmap/roadmap.md`. See [ClaudeWiki — where status lives](ClaudeWiki/Processes/wiki-style.md#where-status-lives).

**Q: What's my role?**
A: See [`ClaudeWiki/Roles/`](ClaudeWiki/Roles/) — one page per role (Supreme Leader, Architect, Game Designer, Dev — Implementation, Dev — Test Suite, QA). Each page names your authority, tools, and communication protocol.

**Q: How do I write tests?**
A: Start here: [`ClaudeWiki/Processes/testing-quick-start.md`](ClaudeWiki/Processes/testing-quick-start.md) (~10 min). Full guidance in `testing.md`, `test-runbook.md`, `test-patterns.md`, `agent-test-workflow.md`.

**Q: How do I add a design doc or spec?**
A: See [`ClaudeWiki/Processes/wiki-style.md`](ClaudeWiki/Processes/wiki-style.md) — one concern per file, register in index, no status fields (link to roadmap instead).

**Q: What's the engine architecture?**
A: [`design/architecture.md`](design/architecture.md) — tiers (Engine, Rules, ROM, Rulebook), roles, access boundaries. Start with "Quick Reference" section if you just need the tier diagram.

**Q: Is feature X built yet?**
A: Check the roadmap phase's spec or design doc. If it's not in the roadmap's "done" phases, it's either pending or superseded. Specs live in [`Specs/`](Specs/).

**Q: How do I communicate findings to other roles?**
A: [`ClaudeWiki/Processes/handoff-tickets.md`](ClaudeWiki/Processes/handoff-tickets.md) — one file per finding, one concern. Quick decision summary at the top; full narrative in Details section.

**Q: What's not in scope?**
A: [`design/non-goals.md`](design/non-goals.md) — deliberately deferred items and the conditions that would make them worth revisiting.

---

## Navigation map

| Need | Go to |
| --- | --- |
| **Understand the code surface** | **[`ApiReference/`](ApiReference/)** — generated, current, accurate (open after `dotnet build`). Read this first, not `/src`. |
| **Roadmap, phases, status** | [`Roadmap/roadmap.md`](Roadmap/roadmap.md) → phase doc → linked spec/design |
| **Architecture, design decisions** | [`design/`](design/) — architecture.md, engine-core.md, rule-pipeline.md, etc. |
| **Buildable specs** | [`Specs/`](Specs/) — concrete fact shapes, rules, acceptance criteria |
| **Testing** | [`ClaudeWiki/Processes/testing-quick-start.md`](ClaudeWiki/Processes/testing-quick-start.md) |
| **QA verdicts** | [`QA/`](QA/) — phase verdicts, test results, defect reports |
| **Handoff tickets** | [`ClaudeWiki/Processes/Handoffs/`](ClaudeWiki/Processes/Handoffs/) |
| **Role definitions** | [`ClaudeWiki/Roles/`](ClaudeWiki/Roles/) — one per role, authority & communication |
| **Deprecated content** | Marked with "DEPRECATED" banner + link to current pattern. Start with the link. |

---

## First time here?

1. Read [`../CLAUDE.md`](../CLAUDE.md) — repo conventions, build/test commands, architecture at a glance.
2. Read your role page in [`ClaudeWiki/Roles/`](ClaudeWiki/Roles/).
3. Read [`Roadmap/roadmap.md`](Roadmap/roadmap.md) to see where the project stands.
4. Skim [`design/architecture.md`](design/architecture.md) if you need to understand the overall structure.
5. Follow links from there — everything is linked.

---

## Key conventions

- **Status lives in one place:** [`Roadmap/roadmap.md`](Roadmap/roadmap.md). All other docs link to it; never restate.
- **One concern per file.** Each page is loadable in isolation; related pages link to each other.
- **Link, don't duplicate.** If you're restating content from another page, link instead.
- **Deprecated = marked + redirected.** Any deprecated doc has a clear "see [current pattern](link)" at the top.

---

## Getting help

- **"What should I read?"** Start with this page's FAQ or map above.
- **"How do I X?"** Check the "Authoritative sources" table on the specific page (e.g., `testing.md`'s table shows what owns testing conventions, how-to-run, patterns, workflow).
- **"Is this a bug or design choice?"** Check [`design/non-goals.md`](design/non-goals.md) and the relevant design doc.
- **"Where's the code for X?"** See [`../CLAUDE.md`](../CLAUDE.md) for architecture overview, then grep the source.

---

## Related

- [CLAUDE.md](../CLAUDE.md) — repo conventions, build/test, architecture summary
- [ClaudeWiki index](ClaudeWiki/index.md) — full documentation map
- [Roadmap](Roadmap/roadmap.md) — phase sequence and status
