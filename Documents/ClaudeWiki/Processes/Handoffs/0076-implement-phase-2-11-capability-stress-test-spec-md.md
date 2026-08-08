# 0076 — Implement phase-2-11-capability-stress-test-spec.md

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-04
**For:** Baraka (Impl Dev)

## Decision

Built in full — all 11 acceptance criteria met, verified via a checked-in runnable spike
(`example/Phase2_11Spike`, kept per Arryn's direction so Lothar can independently re-run it) plus a
real `dotnet run --project src/MyLunaFlowServer` bootstrap. Items 6/8 surfaced no framework gap —
`ComponentRegistry.Validate()`'s existing clash/cycle errors are already clear, correctly-scoped, and
distinguishable; no ticket to Amara needed on that front. Item 7 did surface a real content-modeling
bug the spike caught before it shipped (see Response) — fixed within content authoring, no framework
change needed. Two pre-existing `LunaFlow.ContentTests` tests now fail as expected fallout from the
content-shape change (`DependsOnRoms`/`Loot.Requires` growing) — ticketed to Nadia as 0079, same
pattern as 0070. Full suite otherwise green locally; Arryn-run `latest.trx` still needed to close.

## Details

### Context

Phase 2.11 ([`phase-2-11.md`](../../../Roadmap/phase-2-11.md)) is ratified into `roadmap.md`, between
2.10 and Phase 3. Juno's spec is drafted and reviewed via ticket
[0067](0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md). This is the second
of three sibling stress-test phases (2.10 done/QA PASS, 2.11 this ticket, 2.12 still proposal-stage).
**Different shape from 2.10: this one is framework-heavy and deliberately adversarial in two of its
three items.** Per Amara's ruling on 0067, whatever items 6/8 surface is a real finding, not something
to route around quietly — don't apply 2.10's "no framework work anticipated" default here by default.

### Ask

Build [`phase-2-11-capability-stress-test-spec.md`](../../../Specs/phase-2-11-capability-stress-test-spec.md)
in full. Three items, two of which are deliberately meant to fail:

1. **Item 6 (clash probe) — meant to fail, never ships.** `BountyThreatProbe`/`HazardThreatProbe`
   (new `LunaFlow.Rom.Probes` project or equivalent, Nadia's/your call on layout), both legitimately
   declaring `Provides` for a new `IThreatLevel` capability. Registered and validated inside a
   **standalone `ComponentRegistry` instance**, constructed and torn down inside a test fixture —
   never inside `Program.cs`'s real composition. Confirm `Validate()` throws naming both components
   and `"ThreatLevel"` in the error, not just that some exception fires. Neither probe ROM may be
   referenced from `MyLunaFlowServer`/`Program.cs` — verify via the assembly graph, same discipline
   every peer-boundary criterion in this spec family holds to.
2. **Item 7 (relay) — real, shippable content.** A fourth peer ROM, **Reputation**
   (`IReputationTier` contract: "Rookie"/"Veteran"/"Legend", Rookie is the implicit no-fact default),
   requiring `IChallengeSolved` and providing `IReputationTier`. `ReputationPoints` accumulate
   (replace-not-update) per solved challenge; crossing 3 or 6 inserts `ReputationTierReached` once,
   never re-inserted. Loot's existing award rule grows a second, additive rule: `ItemAwarded` +
   Veteran-or-above `IReputationTier` + no existing `BonusItemAwarded` for that room →
   `BonusItemAwarded`. `OneD20RuleBook.DependsOnRoms` grows a fourth entry (`"Reputation"`) — this is
   a distinct check from `ComponentRegistry.Validate()` passing, verify both.
3. **Item 8 (cycle probe) — meant to fail, never ships.** `CycleProbeAlpha`/`CycleProbeBeta`, same
   probes project as item 6, a genuine mutual `Requires`/`Provides` pair. Registered/validated in the
   same kind of isolated `ComponentRegistry` instance. Confirm `Validate()` throws with an error
   identifying a **dependency cycle**, distinguishable from item 6's clash error and from a plain
   missing-requirement error — the message content is the criterion.

Acceptance criteria are the spec's own checklist (11 items) — work each one, don't summarize around
it. **Framework dependencies section is explicit: none anticipated for item 7, but items 6/8 are
testing whether that stays true for the toolbox generally.** If either probe's failure mode is
unclear, unhelpful, or wrong in a way the current implementation doesn't already predict — an
ambiguous error message, a clash silently accepted instead of rejected, a cycle that validates when
it shouldn't — that's a real ticket to Amara, to be taken seriously per her own ruling on 0067, not
adjusted around in this spec's content.

Two open questions the spec leaves to you: whether `LunaFlow.Rom.Probes` is a real project or the
probes live directly in the consuming test project (either way, "never referenced from `Program.cs`"
is the binding criterion, not the layout), and Legend tier has no distinct payoff specified —
deliberate, proving the relay doesn't need a third distinct effect.

### Response

**Layout questions resolved before I built anything:** ticket 0077/0078 landed same-day — Reputation
goes into a shared implementation project with GridMap/Loot (`example/OneD20SharedRoms`), not a
standalone `ExampleE`; consolidation tracked separately as 0078 (in-review). `LunaFlow.Rom.Probes` is
a real, standalone project (`example/LunaFlow.Rom.Probes`), referenced only by
`example/Phase2_11Spike` — never by `MyLunaFlowServer.csproj` or `LunaFlow.ContentTests.csproj`.

**Item 7 (relay, real content):**
- `ReputationRom` (`LunaFlow.Rom.Reputation`): `Provides` `IReputationTier` (paired with
  `ReputationTierReached` via `Capability.Provided<,>`), `Requires` `IChallengeSolved`. New contract
  `IReputationTier` added to the Cartridge surface (`example/D20SystemExampleC`).
- `ReputationPoints` accumulates replace-not-update; `ReputationTierReached` inserted once at 3
  ("Veteran") and 6 ("Legend"), guarded `Not<ReputationTierReached>` so neither re-inserts.
  **Correctness note not in the spec's fact vocabulary:** added a private `ReputationCounted(RoomId)`
  marker guarding the accrual rule — without it, `ReputationPoints`' replace-not-update convention
  means every solve retracts+reinserts it, and NRules' Rete join re-pairs the fresh fact against
  *every already-solved room's* `IChallengeSolved`, re-firing the accrual rule once per prior solve
  instead of once for the new one. Documented in `Facts.cs`.
- **Loot's award rule is a split, not a literal second rule, and this is a deliberate deviation from
  the spec's Mechanics-section sketch, found by running the checked-in spike, not inferred.** I first
  built it exactly as sketched — `AwardItem` (unchanged) plus a separate `BonusItem` rule matching
  existing `ItemAwarded` + `IReputationTier` + `Not<BonusItemAwarded>`. The spike's room-1-then-room-5
  scenario caught it live: room 1's fight happens *before* Veteran is reached (correctly no bonus at
  that point) — but once `ReputationTierReached("Veteran")` is inserted later, NRules' Rete join
  re-evaluates `BonusItem` against *every already-existing* `ItemAwarded`, including room 1's, and
  retroactively grants it a bonus too. That fails the acceptance criterion's actual intent (bonus
  gated by *reaching* the tier, not backfilled onto fights that predate it). Fixed by splitting the
  award rule into two mutually-exclusive variants keyed off `Match`/`Not<IReputationTier>` at the same
  moment `ItemAwarded` is created (`AwardItem` below Veteran, `AwardItemWithBonus` at Veteran+) —
  bonus-or-not is now decided atomically, once, with no separate rule left to re-fire retroactively.
  Also spike-verified the adjacent edge case (a third fight solved once *both* Veteran and Legend
  facts coexist) doesn't double-insert — NRules retracts the losing activation once the winning one's
  `Then()` inserts `ItemAwarded`, confirmed empirically, not just read off the source. Full reasoning
  is in `LootRuleGroup.cs`'s doc comments.
- `OneD20RuleBook.DependsOnRoms` grows `"Reputation"` (fourth entry) — confirmed as a distinct check
  from `ComponentRegistry.Validate()` by exercising both: a real `dotnet run` bootstrap passes both,
  and the spike-adjacent `dotnet test` run shows `ValidateDependsOn` failing on its own when a test
  doesn't register Reputation (see the two ContentTests failures below).
- Verified via the spike's "live transcript" (fact-state dump per solve, since capturing
  `LunaFlowEngine.Host.Announce` text needs `GameEngineCore` internals not worth exposing to a
  standalone project — see the spike's own doc comment): early fight (room 1, pre-Veteran) → no
  bonus, permanently; late fight (room 5, post-Veteran) → bonus; skill-check rooms never produce
  `ItemAwarded`/`BonusItemAwarded`; Veteran/Legend each inserted exactly once across 8 solves.

**Items 6/8 (probes, meant to fail):**
- `LunaFlow.Rom.Probes`: `BountyThreatProbe`/`HazardThreatProbe` (both `Provides` `IThreatLevel` as
  `"ThreatLevel"`) and `CycleProbeAlpha`/`CycleProbeBeta` (mutual `Requires`/`Provides` via
  `IAlphaSignal`/`IBetaSignal`). Three new Cartridge contracts (`IThreatLevel`, `IAlphaSignal`,
  `IBetaSignal`) — placed in the contract surface per the spec's own snippet, and confirmed they
  don't trip §5.4's `ValidateInterfacesOnly` (all three are interfaces; the real bootstrap run below
  still passes with them present).
- Isolated-registry results, from the spike (verbatim):
  - Item 6: `Duplicate capability 'ThreatLevel': provided by both 'HazardThreatProbe' and
    'BountyThreatProbe'.` — names both components and the capability, not just "an exception."
  - Item 8: `Circular dependency detected: 'CycleProbeBeta' depends on capability 'AlphaSignal'
    provided by 'CycleProbeAlpha', which (transitively) depends on capabilities provided by
    'CycleProbeBeta'.` — identifies a cycle by name, textually disjoint from item 6's message and
    from the plain `"Unsatisfied requirement..."` missing-provider message; spike asserts all three
    are mutually distinguishable, not eyeballed.
  - **No real finding for Amara.** Both failure modes are already clear, name the right things, and
    reject correctly — `ComponentRegistry` doesn't need a change. Recording this as the outcome per
    Amara's ruling that "confirmation the current behavior is fine" is itself a legitimate finding to
    report, not a silent pass-through.
- Assembly-graph check: `MyLunaFlowServer.csproj` and `LunaFlow.ContentTests.csproj` reference lists
  (inspected directly) name `D20SystemExampleA`, `D20SystemExampleC`, `OneD20SharedRoms` only —
  `LunaFlow.Rom.Probes` appears nowhere in either. `example/LunaFlow.Rom.Probes.csproj` documents why
  in a header comment.

**The persisted spike (`example/Phase2_11Spike`), per your direction to keep it available for
Lothar:** a checked-in, runnable console project (`dotnet run --project example/Phase2_11Spike`),
referenced by nothing else, not part of `MyLunaFlowServer`'s composition. Runs item 7's scenario
against a bare `RuleEngine` (same technique `LootRuleGroupFixtureTests` already uses — deterministic,
no RNG/command-dispatch involved) and items 6/8's isolated-`ComponentRegistry` scenarios, printing a
PASS/FAIL line per acceptance-criterion-shaped check plus the raw fact-state transcript, and exits
non-zero on any failure. Current run: all checks pass (pasted above). This is not a substitute for
Nadia's maintained suite — ticketed as 0079 below — it's the demonstrable evidence this ticket's own
"Confirm..." language asked for, kept re-runnable rather than trusted from a pasted transcript.

**Build/test status:**
- `dotnet build LunaFlow_A.sln` — clean, 0 errors.
- `dotnet run --project src/MyLunaFlowServer` (piped `quit`) — bootstrap succeeds: "Component registry
  validated successfully", "ROMs registered: 4 (GridMap, D20System, Loot, Reputation)", 24 rules
  loaded (GridMap 9, D20System 9, Loot 2, Reputation 4).
- `dotnet test LunaFlow_A.sln` — `LunaFlow.EngineTests`: 93/93 green (untouched by this phase).
  `LunaFlow.ContentTests`: 75/77 — the 2 failures are exactly the expected `DependsOnRoms`/
  `Requires`-count fallout named above, not a regression in any *other* existing acceptance criterion
  (`1-d20-spec.md`, `loot-rom-content-expansion-spec.md`, `phase-2-10-content-coverage-spec.md`
  content otherwise unaffected — confirmed by the rest of the suite staying green). Ticketed to Nadia
  as [0079](0079-phase-2-11-maintained-test-coverage.md).
- Full-suite-green criterion's Arryn-run `latest.trx` bar is still open — Kofi/Arryn, over to you for
  that pass once 0079 lands.

### Resolution

<Filled in by the opener when closing.>
