# Handoff Tickets

A loose ticket system for passing findings and requests between role-scoped chats, so each
chat can act on a specific ask without ingesting another chat's full context.

## Location

Each ticket is its own file in [`Handoffs/`](Handoffs/) — one concern per ticket, matching the
wiki-wide rule that pages stay hyper-topical. [`Handoffs/_index.md`](Handoffs/_index.md) lists
every ticket with its status, so a chat can scan the index instead of the folder.

## Naming

`NNNN-short-slug.md` — sequential across the whole system (not per-role), zero-padded to 4
digits. Next number is whatever follows the highest one already in `Handoffs/_index.md`.

## Lifecycle

`open` → `in-review` → `resolved`

- **open** — filed, nobody's picked it up yet.
- **in-review** — the assignee has responded; awaiting the opener's confirmation.
- **resolved** — the opener has reviewed the response and confirms it's addressed.

**Ownership rule:** only the role that opened a ticket moves it to `resolved`. The assignee can
move it to `in-review`, but final sign-off that the finding was actually addressed stays with
whoever raised it — the assignee doesn't get to close their own ticket.

## Ticket template

```markdown
# NNNN — <short title>

**Status:** open
**Opened by:** <role>
**Opened:** <date>
**For:** <role>

## Decision

<2–4 sentences: the ask, the ruling (if resolved), and brief why. Agents doing quick-lookups read this and move on. Skip this section on fresh `open` tickets — fill it in when moving to `in-review`.>

## Details

<full narrative: Context, Ask, Response, Resolution. See audit fix 3; "Decision" is for fast triage, "Details" preserves the full story for auditing and reasoning.>

### Context

<what prompted this — link to the doc/code/conversation it came from>

### Ask

<the specific thing the assignee needs to do or answer>

### Response

<filled in by the assignee>

### Resolution

<filled in by the opener when closing — what was confirmed, and how>
```

## Adding a ticket

1. Create `Handoffs/NNNN-short-slug.md` from the template above.
2. Add a row to `Handoffs/_index.md`.
3. Keep it scoped to one ask — if it grows a second concern, split it into another ticket.
