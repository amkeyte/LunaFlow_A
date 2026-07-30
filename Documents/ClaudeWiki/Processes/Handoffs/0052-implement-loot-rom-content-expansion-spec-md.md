# 0052 — Implement loot-rom-content-expansion-spec.md

**Status:** open
**Opened by:** Kofi (PM)
**Opened:** 2026-07-30
**For:** Baraka (Impl Dev)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

Phase 2.7.1 ([`phase-2-7-1.md`](../../../Roadmap/phase-2-7-1.md)) is content-only, wired into
`roadmap.md`. Juno's package — spec and flavor companion — is reviewed and ready
([0051](0051-phase-2-7-1-content-package-ready-for-kickoff.md)). Same pattern as 0034 → CodePlan
0037 for Phase 2.7.

### Ask

Build [`loot-rom-content-expansion-spec.md`](../../../Specs/loot-rom-content-expansion-spec.md) in
full, flavor text from [`loot-rom-content-expansion-flavor.md`](../../../Specs/loot-rom-content-expansion-flavor.md).
Four items:

1. **Loot ROM** (`example/D20SystemExampleD`, `LunaFlow.Rom.Loot`) — `Requires: [ChallengeSolved]`,
   `Provides: []`. First pure-sink ROM; zero project reference to GridMap/D20System.
2. **Monster roster** — `MonsterTemplate` keyed record (5 entries), `RoomChallenge.MonsterId` field,
   `fight`/challenge-prompt read the assigned monster per session.
3. **9-pool skill checks + session-start randomization** — Rulebook-tier content-assembly code
   (not `Program.cs`), injectable `Random` per ticket 0019's seam. `MonsterLootTable` (static) +
   `LootAssignment` facts (session-derived) feed the one generic award rule.
4. **`inventory`/`inv` command** — Loot-owned, reads only its own facts.

`OneD20RuleBook.DependsOnRoms` grows to `["GridMap", "D20System", "Loot"]`. Acceptance criteria are
the spec's own checklist — work each one, don't summarize around it. If content needs something the
toolbox (`Capability`/`IRom`/`ComponentRegistry`/Cartridge surface) doesn't offer, that's a ticket to
Amara, not a workaround.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
