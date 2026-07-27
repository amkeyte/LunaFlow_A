# 0037 — Implement the GridMap/D20System split (grid-rom-d20-split-spec.md)

**Status:** open
**Opened by:** Kofi (PM)
**Opened:** 2026-07-26
**For:** Baraka (Impl Dev)

## Decision

<Open — no ruling yet. Fill in on close per `handoff-tickets.md`'s convention.>

## Details

### Context

Phase 2.7's content design is done: [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md)
(Juno, via [0034](0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md)) and the
project-layout question it surfaced ([0036](0036-grid-rom-d20system-project-layout-decision.md),
ruled directly by Arryn) are both resolved. I checked the spec against source before opening this —
it's real and buildable, not a placeholder: capability declarations, all five 1.d20 commands mapped
with explicit cross-ROM read/write columns, ten checkable acceptance criteria, honestly-flagged open
questions. No design work remains; this is a build ticket.

The framework side is also specced, not just the content side —
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §4.1 gives `IInteropRom`'s
`Resolve`/`Resolve<T>` signatures in full; §2's gating question (does NRules match through
interfaces) is answered and closed ([0033](0033-spike-does-nrules-match-facts-through-interfaces.md)).
Both specs are stable. This ticket asks for both halves together — the broker's `Resolve` machinery
has no content-independent shape left to design, and building it apart from its first real consumer
is exactly what Amara ruled against doing back in [0029](0029-phase-2-6-5-status-and-qa-gap.md).

### Ask

**Build [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) in full**, plus the
`IInteropRom.Resolve`/`Resolve<T>` implementation it depends on, per
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §4.1. Project layout per
[0036](0036-grid-rom-d20system-project-layout-decision.md)'s scaffold
(`example/D20SystemExampleA`/`B`/`C` — three real `.csproj`s: `GridMap`, `D20System`,
`D20SystemInterop`).

The spec's own acceptance criteria are the acceptance criteria for this ticket — not restated here to
avoid a copy that drifts from the source. In short: three registered `IRom`s (`D20SystemInterop` also
implementing `IInteropRom`), `ComponentRegistry.Validate()` passing against all three,
`OneD20RuleBook.DependsOnRoms` naming all three, every existing `1-d20-spec.md` behavior unchanged,
the four capability contracts (`IPlayerLocation`/`IChallengeSolved`/`ICombatStatus`/`IPlayerMovement`)
crossing correctly with zero project reference between the two content peers (verified against the
assembly graph, per the spec's own bar — not just a passing test), `D20SystemContent.Configure` and
its static state gone, and the full suite green off an Arryn-run `latest.trx`.

**Explicitly not part of this ticket** (per the spec's own "Framework dependencies" section):
`DependsOnRoms` *validation* enforcement — `ComponentRegistry.Validate()` doesn't read
`DependsOnRoms` yet, and that stays tracked as separate, already-known live work on
[`phase-2-7.md`](../../../Roadmap/phase-2-7.md). Don't fold it in here; if building this split makes it
newly urgent, flag it rather than quietly implementing it as a side effect.

**One thing worth flagging back if it surprises you:** the spec's own "Open questions" section notes
that `IPlayerLocation`/`ICombatStatus`/`ICombatStatus` are read via handler-side `Query<T>` against
the shared NRules session (no broker call), reserving `IInteropRom.Resolve` for the one
service-shaped case (`IPlayerMovement`) — extending §2's polymorphic-matching finding from `Match<T>`
to `Query<T>` by the same reasoning. If that reading doesn't hold once you're actually building it,
that's a real design divergence for Amara, not something to route around silently.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
