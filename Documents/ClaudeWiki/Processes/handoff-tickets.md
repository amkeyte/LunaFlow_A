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

**Status lives on the ticket and in the index — not as an aside inside another ticket.** A
ticket's status changes in exactly two places: the ticket's own `**Status:**` header/`## Decision`,
and its row in [`Handoffs/_index.md`](Handoffs/_index.md). If a ticket's outcome is relevant to
another ticket, link it — don't restate or flag its status in passing ("note: 0058 is basically
resolved now"). A second, informal copy of a status claim is exactly the kind of thing that drifts
the moment the real one moves, and it's a second source of truth nobody asked for. Reconciling
ticket status across the board is Kofi's (PM's) job, not a side effect of whoever happens to be
writing a different ticket — if a status question comes up while working one ticket, route it to
Kofi rather than asserting it. Added 2026-08-03 at Arryn's direction.

**Don't open a ticket to flag a gap in the ticket system itself.** "This ticket is missing X,"
"this should have been split," "the index is out of sync" and similar meta-observations about
process/tickets aren't a ticket — take them to Kofi directly in conversation. Tickets carry findings
and requests between role-chats about the *build*; process friction about how tickets themselves are
being run is a PM conversation, not another row in the index. Added 2026-08-03 at Arryn's direction.

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
