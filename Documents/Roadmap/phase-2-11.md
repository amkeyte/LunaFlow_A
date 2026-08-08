# Phase 2.11 — Cross-ROM capability mechanism stress test

**Status:** pending. Built per [`Specs/phase-2-11-capability-stress-test-spec.md`](../Specs/phase-2-11-capability-stress-test-spec.md)
against the reviewed proposal in
[`Proposals/2-10-through-2-12-content-framework-stress-test.md`](Proposals/2-10-through-2-12-content-framework-stress-test.md),
approved via ticket [0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md).

## Overview

The second of three deliberate stress-test phases on what Phases 2.5–2.9 shipped. Unlike 2.10, this
one is framework-heavy by design and deliberately adversarial in two of its three items: items 6 and 8
exist to be *rejected* by `ComponentRegistry.Validate()`, constructing the exact "second real
consumer" conditions `non-goals.md` names as the trigger for revisiting capability arbitration (item
6, two legitimate providers of the same capability) and the trigger that finally exercises the cycle
check that landed late and unit-test-only (item 8, ticket 0031, a genuine mutual dependency). Item 7
is the one item that ships as real, playable content: a new peer ROM (Reputation) that both requires
an existing capability and provides a new one — the first capability *relay*, proving the two-phase
collect-then-validate registry resolves a three-deep chain, not just a flat pair-plus-sink.

**Explicitly not in scope:** capability arbitration itself (this phase probes whether the deferral in
`non-goals.md` still holds; it does not build arbitration even if the probe suggests it should exist),
any change to GridMap/D20System's existing rules or the win-line family, and registering items 6's or
8's probe ROMs in the live game's Cartridge — they're built to fail `Validate()` on purpose and live
behind an isolated, test-only registration path, never `Program.cs`'s real composition.

**Amara's standing flag (ticket 0067):** whatever items 6 and 8 surface — confirmation the current
behavior is fine, or a real ticket for arbitration/cycle-message quality — gets treated as a real
finding, not routed around quietly. The "no framework work anticipated" default that held for every
content-only phase since 2.7.1 doesn't apply here by default.

## Ownership

- Content design: [Game Designer (Juno)](../ClaudeWiki/Roles/game-designer.md) — spec already
  drafted and reviewed (ticket 0067).
- Implementation: [Dev (Baraka)](../ClaudeWiki/Roles/dev.md), against the spec.
- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md) — flagged by ticket
  0067 as likely to get a real ask from items 6/8, not just a shape review.

## Deliverables

- **Item 6 (clash probe):** `BountyThreatProbe`/`HazardThreatProbe`, both legitimately declaring
  `Provides` for a new `IThreatLevel` capability, registered and validated inside a standalone
  `ComponentRegistry` instance (test-only, never `Program.cs`) — confirms `Validate()` throws naming
  both components and the capability, not just that some exception fires.
- **Item 7 (relay, ships as real content):** a fourth peer ROM, **Reputation**, requiring
  `IChallengeSolved` and providing a new `IReputationTier` capability; `ReputationPoints` accumulate
  per solved challenge, crossing a threshold inserts `ReputationTierReached`. Loot's existing award
  rule grows a second, additive rule requiring `IReputationTier` to grant a bonus item at
  Veteran-or-above — the first capability required from two independent providers *and* the first
  three-deep chain (D20System → Reputation → Loot).
- **Item 8 (cycle probe):** `CycleProbeAlpha`/`CycleProbeBeta`, a genuine mutual `Requires`/`Provides`
  pair, registered and validated inside the same kind of isolated `ComponentRegistry` instance —
  confirms `Validate()` throws with an error identifying a dependency cycle, distinguishable from item
  6's clash error.
- `OneD20RuleBook.DependsOnRoms` grows a fourth entry (`"Reputation"`), validated as a distinct check
  from `ComponentRegistry.Validate()` passing.

## Done when

- All eleven acceptance criteria in `phase-2-11-capability-stress-test-spec.md` are met, verified
  against the exception content for items 6/8 (not just "an exception was thrown") and against a live
  transcript for item 7's threshold/bonus-item behavior.
- Neither probe ROM (items 6/8) is referenced from `MyLunaFlowServer`/`Program.cs` — an assembly-graph
  check, not just a passing test.
- No existing acceptance criterion from `1-d20-spec.md`, `loot-rom-content-expansion-spec.md`, or
  `phase-2-10-content-coverage-spec.md` regresses.
- Full suite green, read from an Arryn-run `latest.trx`.
- Any real finding from items 6/8 (an ambiguous error message, a clash silently accepted, a cycle that
  validates when it shouldn't) is ticketed to the Architect and treated seriously — this phase's
  Done-When bar does not quietly close around an unresolved framework question.

## Related phases

- Previous: [Phase 2.10 — Content coverage stress test](phase-2-10.md)
- Sibling (same proposal, independent scope): Phase 2.12 (Lua World Module authoring-surface stress
  test) — still proposal-stage, not yet ratified into this sequence table.
- Next: none scheduled against this phase specifically. Depending on what items 6/8 surface, may
  produce a follow-up ticket to Amara before Phase 4 kickoff.

## Related documents

- [`Specs/phase-2-11-capability-stress-test-spec.md`](../Specs/phase-2-11-capability-stress-test-spec.md) —
  the buildable spec
- [`Proposals/2-10-through-2-12-content-framework-stress-test.md`](Proposals/2-10-through-2-12-content-framework-stress-test.md) —
  the reviewed proposal this phase is drawn from
- [Ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) —
  the proposal's review/approval, including the "take whatever items 6/8 surface seriously" ruling
- [`design/cartridge.md`](../design/cartridge.md) — the published-unit mechanism this phase stress-tests
- [`design/non-goals.md`](../design/non-goals.md) — the capability-arbitration deferral item 6 presses
  against
- [Ticket 0031](../ClaudeWiki/Processes/Handoffs/0031-code-currency-vs-2-6-5-design.md) — the cycle
  check's late landing, unit-test-only until item 8
