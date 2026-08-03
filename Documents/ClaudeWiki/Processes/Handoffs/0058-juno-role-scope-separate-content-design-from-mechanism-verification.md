# 0058 — Juno role scope - separate content design from mechanism verification

**Status:** open
**Opened by:** Juno (Game Designer)
**Opened:** 2026-08-02
**For:** Kofi (PM)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

Arryn raised, in conversation rather than against a specific deliverable, that Juno's work has been
running tech-heavy — doubling as content design and de facto mechanism verification — and asked for
my own read on whether/when to shift the verification half back to Amara so Juno concentrates on
"making the game."

Looking back at this arc concretely, two different kinds of work have been happening under one hat.
Real content design: the Loot ROM's design, the five-monster roster, monster-to-loot pairing,
randomization intent (what varies vs. what stays fixed to protect win-line topology), flavor-text
genericization. Separately, mechanism verification that isn't content design at all: reading
`ComponentRegistry.Validate()` source directly to confirm an empty `Provides` list (the Loot ROM's
pure-sink shape) doesn't break anything, tracking the Cartridge pivot's ripple through
`grid-rom-d20-split-spec.md`, catching my own `src/` vs `example/` placement error, reasoning about
tier-ordering/cycle-detection safety. The second kind only landed on me because the Cartridge
mechanism has been in active flux for this whole arc — named, then had its broker cut, then grew a
fifth capability mid-build. Designing content responsibly against a moving mechanism means
understanding it well enough to notice when it moves; that's different from designing content
against a mechanism that's already stable.

Worth noting since it happened in this same session: ticket 0055 (Baraka → Amara, two mechanism
deviations building the Loot spec) is a clean example of the boundary working as intended even
under the current setup — both deviations got caught, ticketed, and ratified without a design
change, and confirmed the spec's underlying design was sound. That's the system already working;
this ticket isn't about a failure, it's about where the next increment of specialization should
land.

### Ask

Not asking for a ruling on this turn — this is a heads-up / discussion-log ticket, since the actual
handoff is a future one (rebalancing Juno's and Amara's scopes), not something to action today.
Flagging a concrete trigger condition for when Kofi should consider making that cut explicit:

**Once a phase's own "Framework dependencies" section reads "none anticipated" for two phases
running, with no framework-shaped surprises surfacing during build** (Phase 2.7.1 is the first of
those two, contingent on how 0055-style deviations shake out during the rest of its build/QA), that's
the signal the Cartridge/capability mechanism has stopped changing shape often enough that content
design and mechanism verification can cleanly separate again. Past that point, the cleaner split is:
Juno designs against the published, stable contract and treats any doubt about whether the mechanism
supports something as a ticket to Amara — full stop, even for the "I'm 95% sure this works" cases —
rather than personally reading Engine source to self-verify, the way this arc required.

No action needed now beyond logging the trigger condition somewhere Kofi will see it at the next
phase gate (this ticket, or wherever Kofi tracks standing watch-items).

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
