# Phase 2.11 — Cross-ROM Capability Mechanism Stress Test — Implementation Spec

**Status:** in review — draft complete, acceptance criteria stated; awaiting Amara/Baraka read (this
one especially, given the deliberately-adversarial items 6/8).
**Companion design doc:** [`../design/cartridge.md`](../design/cartridge.md) — read for rationale.
**Companion to:** [`cross-rom-capability-spec.md`](cross-rom-capability-spec.md) (the authoring
contract this content is written against) and
[`Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md)
(the reviewed proposal, per [ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md)).
**Supersedes:** nothing. Additive to [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md).

## Purpose

Turns the proposal's Phase 2.11 candidate items (6–8) into buildable content. Unlike Phase 2.10,
this content is **deliberately adversarial**: items 6 and 8 exist to be rejected by
`ComponentRegistry.Validate()`, not to ship into the playable session. That's the point — they
construct the exact "second real consumer" conditions `non-goals.md` names as the trigger for
revisiting capability arbitration (item 6) and the trigger that actually exercises the cycle check
that landed late and unit-test-only (item 8, ticket 0031). Per Amara's ruling on ticket 0067,
whatever these two items surface is to be treated as a real finding, not routed around quietly.
Item 7 is the one item in this phase that's real, shippable content — a genuine three-deep
capability relay, which nothing built so far has exercised past a flat pair-plus-sink.

## Scope

**In:**
- **Item 6 (clash):** two probe ROMs, each legitimately declaring `Provides` for the same new
  capability, validated in an **isolated** `ComponentRegistry` — not the live game's registry.
- **Item 7 (relay):** a new peer ROM (**Reputation**) that both `Requires` an existing capability
  and `Provides` a new one, consumed by an extension to Loot's existing award rule — this one
  *does* join the live Cartridge and ships as real content.
- **Item 8 (cycle):** two probe ROMs declaring a genuine mutual `Requires`/`Provides` pair,
  validated in an isolated `ComponentRegistry`, distinct from the missing-provider case item 6
  exercises.

**Out:**
- **Registering items 6's or 8's probe ROMs in the live game's Cartridge.** They are built
  specifically to fail `Validate()` — wiring them into `Program.cs`'s real registration list would
  just break the shipped game on every run, which is not the finding anyone wants. They live behind
  a separate, test-only registration path (see Mechanics).
- **Capability arbitration itself.** This phase probes whether the deferral in `non-goals.md` still
  holds; it does not build arbitration, even if the probe suggests it should exist. That's a
  follow-up ticket to the Architect if item 6 rules it necessary, not this spec's deliverable.
- Any change to GridMap, D20System's existing rules, or the win-line family.
- A fourth peer beyond Reputation — item 7 only needs one relay hop to prove the three-deep chain.

## Fact / marker vocabulary

### New contract types (Cartridge contract-surface assembly — not `LunaFlow.Rules`, not a ROM;
per `cross-rom-capability-spec.md` §4.4, alongside the existing `IChallengeSolved`/`IPlayerLocation`/
`IPlayerMovement` contracts)

```csharp
// Item 6 — the clash probe's shared capability. Two ROMs will legitimately both want to provide
// this: a "how threatening is the current encounter" signal is exactly the kind of thing more than
// one content system could plausibly own (a bounty-escalation model, a hazard-escalation model).
public interface IThreatLevel { int Level { get; } }

// Item 7 — the relay's provided capability.
public interface IReputationTier { string Tier { get; } }   // "Rookie" | "Veteran" | "Legend"

// Item 8 — the cycle probe's two mutually-dependent capabilities. Deliberately arbitrary; nothing
// about their content matters, only that each ROM's Requires points at the other's Provides.
public interface IAlphaSignal { }
public interface IBetaSignal { }
```

### Item 6 — clash probe ROMs (`LunaFlow.Rom.Probes`, test-only project — see Mechanics for why it's
never referenced from `Program.cs`'s real composition)

```csharp
public sealed class BountyThreatProbe : IRom
{
    public IReadOnlyList<Capability> Provides { get; } =
        new[] { Capability.Provided<IThreatLevel, BountyThreatFact>("ThreatLevel") };
    public IReadOnlyList<Capability> Requires { get; } = Array.Empty<Capability>();
}

public sealed class HazardThreatProbe : IRom
{
    public IReadOnlyList<Capability> Provides { get; } =
        new[] { Capability.Provided<IThreatLevel, HazardThreatFact>("ThreatLevel") };
    public IReadOnlyList<Capability> Requires { get; } = Array.Empty<Capability>();
}

// Minimal fact shapes — enough to satisfy Capability.Provided<,>'s generic constraint, no rules
// or handlers behind them. This content is never fired; it exists to be registered and rejected.
internal sealed record BountyThreatFact(int Level) : IThreatLevel;
internal sealed record HazardThreatFact(int Level) : IThreatLevel;
```

Both declare the same `Capability.Name` ("ThreatLevel") against the same `ContractType`
(`IThreatLevel`) — the exact shape `cross-rom-capability-spec.md` §3.2 rule 5 names as a hard
load-time error, never an arbitration.

### Item 7 — Reputation ROM (joins the live Cartridge as a real fourth peer)

```csharp
public sealed class ReputationRom : IRom
{
    public IReadOnlyList<Capability> Provides { get; } =
        new[] { Capability.Provided<IReputationTier, ReputationTierReached>("ReputationTier") };
    public IReadOnlyList<Capability> Requires { get; } =
        new[] { new Capability("ChallengeSolved", typeof(IChallengeSolved)) };
}

// Session-scoped, accumulates.
public sealed record ReputationPoints(int Total);   // replace-not-update, same convention as
                                                      // CombatRound's fields
public sealed record ReputationTierReached(string Tier) : IReputationTier;   // inserted once per
                                                      // tier threshold crossed, never retracted
```

Thresholds (session-tunable content, not framework): `Total >= 3 → "Veteran"`,
`Total >= 6 → "Legend"`. "Rookie" is the implicit default (no fact — absence of any
`ReputationTierReached` fact means Rookie); this avoids seeding a fact for the starting state.

### Loot ROM extension (item 7's consuming end)

```csharp
// Loot's Provides/Requires grows one entry:
Requires: [ new("ChallengeSolved", typeof(IChallengeSolved)),
            new("ReputationTier",  typeof(IReputationTier)) ]   // NEW
```

New fact, Loot-owned:

```csharp
public sealed record BonusItemAwarded(string ItemId, int RoomId);   // accumulates, same
                                                                       // convention as ItemAwarded
```

### Item 8 — cycle probe ROMs (same `LunaFlow.Rom.Probes` project as item 6)

```csharp
public sealed class CycleProbeAlpha : IRom
{
    public IReadOnlyList<Capability> Provides { get; } =
        new[] { Capability.Provided<IAlphaSignal, AlphaFact>("AlphaSignal") };
    public IReadOnlyList<Capability> Requires { get; } =
        new[] { new Capability("BetaSignal", typeof(IBetaSignal)) };
}

public sealed class CycleProbeBeta : IRom
{
    public IReadOnlyList<Capability> Provides { get; } =
        new[] { Capability.Provided<IBetaSignal, BetaFact>("BetaSignal") };
    public IReadOnlyList<Capability> Requires { get; } =
        new[] { new Capability("AlphaSignal", typeof(IAlphaSignal)) };
}

internal sealed record AlphaFact : IAlphaSignal;
internal sealed record BetaFact : IBetaSignal;
```

Each provider is genuinely present for the other's requirement — this is not the missing-provider
case item 6 exercises. `Validate()` has to walk the dependency graph and find the mutual edge, which
is what the ticket-0031 cycle check specifically added and nothing content-driven has exercised.

## Mechanics / rules

### Item 6 and item 8 — isolated registration, not `Program.cs`

Both probes' ROMs are registered and validated in a **standalone `ComponentRegistry` instance**,
constructed and torn down inside a test fixture (Nadia's territory), never inside `Program.cs`'s
real composition root. This is the concrete answer to "how do we ship content designed to fail
without shipping a broken game": the probe is the test, not a feature. `LunaFlow.Rom.Probes` is
referenced only by the test project, never by `MyLunaFlowServer`.

### Item 7 — the relay rule (new, D20System→Reputation)

```
When().Match<IChallengeSolved>(() => solved)
      .Match<ReputationPoints>(() => pts, or default ReputationPoints(0) if none exists)
Then: replace ReputationPoints(pts.Total + 1);
      if crossing 3 or 6, additionally insert ReputationTierReached(tier)
```

Fires once per solved challenge (skill-check or fight, same `IChallengeSolved` D20System already
provides). No new D20System-side change — Reputation matches the same contract type Loot's existing
award rule already matches, proving the "one provider, multiple independent requirers" pattern
extends past two.

### Item 7 — Loot's bonus-drop rule (extends the existing award rule, additive, doesn't replace it)

```
When().Match<ItemAwarded>(() => awarded)
      .Match<IReputationTier>(() => tier, t => t.Tier == "Veteran" || t.Tier == "Legend")
      .Not<BonusItemAwarded>(b => b.RoomId == awarded.RoomId)
Then: insert BonusItemAwarded(<a fixed bonus item id>, awarded.RoomId); announce bonus flavor
```

This is the three-deep chain: D20System provides `IChallengeSolved` → Reputation requires it and
provides `IReputationTier` → Loot requires `IReputationTier` (in addition to its existing
`IChallengeSolved` requirement). `ComponentRegistry`'s two-phase collect-then-validate has to resolve
both of Loot's requirements against two different providers in the same pass — the first time it's
asked to.

**Caught in self-review, easy to miss:** `OneD20RuleBook.DependsOnRoms` (declared, per
`loot-rom-content-expansion-spec.md`, and load-time validated since Phase 2.7's
`ComponentRegistry.ValidateDependsOn`) grows a fourth entry: `["GridMap", "D20System", "Loot",
"Reputation"]`. Registering `ReputationRom` with `ComponentRegistry` without also adding it here
would pass `ComponentRegistry.Validate()` (which doesn't read `DependsOnRoms`) but fail the
Rulebook's own declared-dependency check — a real, distinct failure mode from anything items 6–8
are testing, so it's worth stating explicitly rather than leaving implicit.

## Commands & handlers

No new player-facing commands. Item 7's content is entirely rule-driven (no handler reads
`ReputationPoints`/`ReputationTierReached` directly); `inventory` (existing, Loot-owned) lists
`BonusItemAwarded` items alongside `ItemAwarded` ones with no change to its own logic beyond reading
one more fact type off the same `Query<T>()` pattern it already uses.

## Acceptance criteria

- [ ] **Item 6:** registering `BountyThreatProbe` and `HazardThreatProbe` together in an isolated
      `ComponentRegistry` and calling `Validate()` throws, naming both components and the
      `"ThreatLevel"` capability in the error — verified against the exception's content, not just
      that *an* exception was thrown.
- [ ] **Item 6:** neither probe ROM is referenced from `MyLunaFlowServer`/`Program.cs` — assembly
      graph check, same discipline as every other peer-boundary criterion in this spec family.
- [ ] **Item 7:** `ReputationRom` registers and validates inside the live Cartridge alongside
      GridMap/D20System/Loot — four ROMs, `Validate()` passes.
- [ ] **Item 7:** `OneD20RuleBook.DependsOnRoms` names all four ROMs including `"Reputation"` —
      verified as a distinct check from `ComponentRegistry.Validate()` passing, since that check
      doesn't read `DependsOnRoms` at all.
- [ ] **Item 7:** a full session in which the PC solves 3+ challenges shows `ReputationTierReached
      ("Veteran")` inserted exactly once, not re-inserted on subsequent solves.
- [ ] **Item 7:** a fight room's `ItemAwarded` produces a `BonusItemAwarded` for the same room only
      once Veteran or higher is reached — verified by comparing an early-session fight (no bonus)
      against a late-session fight (bonus) in the same transcript.
- [ ] **Item 7:** skill-check rooms never produce `BonusItemAwarded`, same as they never produce
      `ItemAwarded` — the existing Loot constraint is unaffected by this extension.
- [ ] **Item 8:** registering `CycleProbeAlpha` and `CycleProbeBeta` together in an isolated
      `ComponentRegistry` and calling `Validate()` throws with an error that identifies a
      **dependency cycle**, distinguishable from item 6's clash error and from a plain
      missing-requirement error — the message content is the criterion, not just that validation
      failed.
- [ ] **Item 8:** neither probe ROM is referenced from `MyLunaFlowServer`/`Program.cs`.
- [ ] No existing acceptance criterion from `1-d20-spec.md`, `loot-rom-content-expansion-spec.md`,
      or `phase-2-10-content-coverage-spec.md` regresses.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

**None anticipated for item 7** — it's built entirely against the shipped `Capability`/`IRom`/
`ComponentRegistry` toolbox, same as `loot-rom-content-expansion-spec.md`'s Loot ROM.

**Items 6 and 8 are explicitly testing whether that stays true.** If either probe's failure mode
turns out to be unclear, unhelpful, or wrong in some way the current implementation doesn't already
predict (an ambiguous error message, a clash that's silently accepted instead of rejected, a cycle
that validates when it shouldn't), that's a real ticket to the Architect — per Amara's ruling on
ticket 0067, to be filed and taken seriously, not adjusted around in this spec's own content.

## Open questions / edge cases

- **What "Legend" tier is actually for, content-wise** — this spec only wires a bonus item at
  Veteran-or-above; a distinct Legend-tier effect is left unspecified, deliberately, since proving
  the relay mechanism doesn't need a third distinct payoff.
- **Whether `LunaFlow.Rom.Probes` should exist as a real project or the probe ROMs should live
  directly inside the test project that consumes them** — implementation detail, Baraka's/Nadia's
  call; either way, the "never referenced from `Program.cs`" criterion is what's binding, not the
  project layout.
- **If item 6 or item 8 surfaces a real finding** (see Framework dependencies above), whatever
  ticket results is out of this spec's scope to pre-answer — this spec's job is to make the finding
  reachable and verifiable, not to fix what it finds.

## Related documents

- [`cross-rom-capability-spec.md`](cross-rom-capability-spec.md) — the authoring contract this
  spec's `Capability.Provided<,>`/`Requires` usage is written against, including §3.2 rule 5 (clash
  is a hard error) and the cycle-check history behind item 8
- [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md) — the existing
  `IChallengeSolved` multi-consumer precedent (GridMap + Loot) item 7 extends to three
- [`../design/cartridge.md`](../design/cartridge.md) — the published-unit mechanism this whole spec
  stress-tests
- [`../design/non-goals.md`](../design/non-goals.md) — the capability-arbitration deferral item 6
  is designed to press against
- [Ticket 0031](../ClaudeWiki/Processes/Handoffs/0031-code-currency-vs-2-6-5-design.md) — the cycle
  check's late landing, unit-test-only until item 8
- [`../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md) —
  the reviewed proposal this spec implements Phase 2.11 of
- [Ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) —
  the proposal's review/approval, including Amara's "take whatever these items surface seriously" ruling
- [`content-spec-template.md`](../ClaudeWiki/Processes/content-spec-template.md) — the template this follows
