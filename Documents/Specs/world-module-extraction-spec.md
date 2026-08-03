# World Module Extraction — Implementation Spec

**Status:** draft
**Companion design doc:** [`../design/world-module.md`](../design/world-module.md) — the tier's
shape and rationale; read that first, this spec is its buildable translation.
**Supersedes:** nothing. Restructures where `OneD20RulebookContent`/`OneD20FlavorText`'s existing,
shipped content lives; no fact shape, capability, or player-visible behavior changes.

## Purpose

Turns [`phase-2-8.md`](../Roadmap/phase-2-8.md) into buildable work: split OneD20's Rulebook-tier
content into what's genuinely *structure* (stays in `LunaFlow.Rulebook.OneD20`) and what's *instance
data* (moves into a new `IOneD20WorldModule` implementation), then prove the split is real by
building a second, independently-authored implementation that runs the same session unchanged.

**This spec does not implement:** any new framework capability, any new `ComponentRegistry`/
`EngineTier` machinery, or any change to GridMap/D20System/Loot's fact shapes, capabilities, or
commands. Everything here is a Rulebook-tier and composition-root restructuring.

## Scope

**In:**
- `IOneD20WorldModule` (`LunaFlow.Rulebook.OneD20`) — the shape contract.
- `DefaultOneD20WorldModule` — today's shipped content (`1-d20-flavor.md`,
  `loot-rom-content-expansion-flavor.md`, the 9-room/9-challenge/5-monster tables), reimplemented
  against the contract instead of as static fields.
- `OneD20SessionAssembler` decoupled from `OneD20RulebookContent`'s statics — takes an
  `IOneD20WorldModule` instead of reading fixed fields.
- A validation pass at the World Module boundary (see Mechanics).
- A second, independently-authored `IOneD20WorldModule` implementation, proving the swap — content
  is Juno's call (a reskin is enough to prove the type-level claim; new balance numbers are not
  required), wiring/validation is this spec's.

**Out:**
- Any change to `GridRoomData`/`RoomChallenge`/`MonsterTemplate`/`PlayerCharacter`/`CombatantStats`
  (ROM-owned fact/content shapes) — these are what `Program.cs` translates *into*, unchanged.
- Simultaneous multi-World-Module loading — additive composition is a recorded principle
  (`architecture.md`), not proven here. One World Module feeds one Rulebook in this pass.
- `ComponentRegistry` registration for World Modules — see `world-module.md`'s "deliberately not
  built yet" section.
- Any GM Overlay or scripting-tier work — out of this phase entirely.

## Type vocabulary

### Rulebook-owned shapes (structure — unchanged in field list, reclassified in ownership only)

```csharp
public enum OneD20ChallengeType { SkillCheck, Fight }
public sealed record OneD20RoomData(int Id, int Row, int Col, OneD20ChallengeType ChallengeType, string DescriptionKey);
public sealed record OneD20ChallengeTemplate(string Key, string Skill, string SkillAbility, int DC, string SuccessKey, string FailureKey, string PromptKey);
public sealed record OneD20CombatantStats(int Defense, int AttackBonus, string DamageDie, int DamageBonus, int MaxHp);
public sealed record OneD20MonsterTemplate(string Id, string Name, string DescriptionKey, OneD20CombatantStats Stats);
public sealed record OneD20SessionAssignment(
    IReadOnlyDictionary<int,string> ChallengeKeyByRoom,
    IReadOnlyDictionary<int,string> MonsterIdByRoom,
    IReadOnlyDictionary<int,string> ItemIdByRoom);
```

These already exist in `OneD20ScenarioData.cs`/`OneD20SessionAssembler.cs` today and do not change
shape. What changes is that a World Module populates them instead of `OneD20RulebookContent` holding
the populated instances itself.

### The World Module contract (new)

```csharp
public interface IOneD20WorldModule
{
    IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; }
    int StartRoomId { get; }
    IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; }
    IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters { get; }
    IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; }
    OneD20CombatantStats PlayerCharacter { get; }
    IReadOnlyDictionary<string, int> PlayerAbilityScores { get; }
    IReadOnlyDictionary<string, string> FlavorText { get; }
}
```

Every property here is exactly today's `OneD20RulebookContent`/`OneD20FlavorText` static surface,
moved to instance members. `DefaultOneD20WorldModule : IOneD20WorldModule` carries today's shipped
9 rooms / 9 challenges / 5 monsters / loot table / PC stats / flavor text, verbatim — this is a
relocation, not a rewrite of content.

## Mechanics / rules

### Validation (new — Rulebook-tier, not the World Module's job)

`Program.cs` constructs an `IOneD20WorldModule` and passes it to a new validation step before
anything else touches it (`OneD20WorldModuleValidator.Validate(IOneD20WorldModule)` or equivalent,
Rulebook-tier). Checks, all fail-fast with a message naming what's wrong:

- `Rooms` has exactly the 9 keys `1..9` GridMap's fixed grid expects.
- Every `OneD20RoomData.ChallengeType == Fight` room has no corresponding entry required in
  `Challenges` (fight rooms draw from `Monsters`, not `Challenges`); every `SkillCheck` room's
  position is fillable from `Challenges`.
- `Challenges.Count >= 6` and `Monsters.Count >= 3` — the grid's fixed slot counts
  (`phase-2-7-1.md`'s 6 skill-check / 3 fight positions). A World Module supplying fewer can't fill
  the session; this is the concrete failure mode a careless second implementation would hit.
- Every `OneD20ChallengeTemplate`/`OneD20MonsterTemplate`'s flavor keys (`PromptKey`/`SuccessKey`/
  `FailureKey`/`DescriptionKey`) exist in `FlavorText` — a missing key fails at load, not as a blank
  announcement mid-session.
- `PlayerAbilityScores` names all six abilities `OneD20ResolutionMechanic`/`fight`/`check` read
  (`Str`/`Dex`/`Con`/`Int`/`Wis`/`Cha`).
- `FlavorText` contains, verbatim and unprefixed, the **fixed vocabulary** every command handler
  reads by literal string rather than through a template's `*Key` field: `pc-hit`, `pc-miss`,
  `monster-hit`, `monster-miss`, `monster-defeated`, `monster-fled`, `pc-flee`, `pc-lost`,
  `win-announce`, `inventory-empty`, `inventory-header`. **Added 2026-08-03 (Amara, doc pass):**
  this was missing from the original Mechanics list — every other `FlavorText` key is data-driven
  (a template's field names the key, so a World Module may rename it freely), but these eleven are
  hardcoded literal lookups inside `FightCommandHandler`/`FleeCommandHandler`/
  `InventoryCommandHandler`/`Program.cs` — shared handler code no World Module swap changes. A
  module that renames or omits one of these (e.g. namespacing it under a theme prefix, the way a
  room or monster key correctly can be) passes every other check and then throws
  `KeyNotFoundException` at first use — mid-session, not at load, which is exactly the failure mode
  this validation pass exists to rule out. Found auditing Hollow Keep's flavor content against this
  spec; see ticket 0061.

A World Module failing any check is a load-time error, same "hard fail at bootstrap, never a
degradation during play" property the Cartridge probe (`cross-rom-capability-spec.md` §5) upholds
one tier down — this spec's version is a plain validation pass, not the binding-exception-family
probe, since nothing here crosses an assembly-loading boundary the CLR resolves lazily.

### `OneD20SessionAssembler` (changed: decoupled from static content)

`Assemble()` (or its constructor) takes an `IOneD20WorldModule` and reads `Rooms`/`Challenges`/
`Monsters`/`ItemIdByMonsterId` from it instead of `OneD20RulebookContent`'s static fields. The
shuffle-and-slot algorithm itself — choose N of M, zip to fixed positions — is unchanged; only where
its inputs come from changes. This is the one real coupling ticket 0055's inventory surfaced: today
the N-of-M draw is implicitly sized off whatever `.Count` the static dictionaries happen to have,
which silently assumed there would only ever be one `IOneD20WorldModule`-shaped thing. Decoupling it
is what makes the second-implementation proof below mean something.

### Program.cs (changed: one new input, same translation path)

`Program.cs` constructs a concrete `IOneD20WorldModule` (which one is a compile-time choice, same as
every other composition-root wiring today — no loader, see `world-module.md`'s deferred list),
validates it, passes it into `OneD20SessionAssembler`, and translates the resulting assignment into
`GridRoomData`/`RoomChallenge`/`MonsterTemplate`/`PlayerCharacter`/`MonsterLootTable` exactly as it
does today (`ConstructGridMap`/`ConstructD20System`/`ConstructLoot`) — those methods change their
data *source* (`worldModule.Rooms` instead of `OneD20RulebookContent.Rooms`), not their translation
logic.

## Acceptance criteria

- [ ] `IOneD20WorldModule` exists in `LunaFlow.Rulebook.OneD20`; `DefaultOneD20WorldModule`
      implements it and reproduces today's exact shipped content (9 rooms, 9 challenges, 5 monsters,
      loot table, PC stats, flavor text) — a diff against `OneD20RulebookContent`/`OneD20FlavorText`'s
      current values, not just a shape match.
- [ ] `OneD20RulebookContent`/`OneD20FlavorText`'s static surface is gone; nothing in
      `LunaFlow.Rulebook`, `example/`, or `src/MyLunaFlowServer` reads them as statics.
- [ ] `OneD20SessionAssembler` takes an `IOneD20WorldModule`; grepping its source for
      `OneD20RulebookContent`/`OneD20FlavorText` by name returns zero hits.
- [ ] The validation pass rejects a deliberately broken `IOneD20WorldModule` (missing room, a
      `Challenges` pool of 5, a flavor key gone) with a clear, load-time, non-zero-exit error naming
      what's wrong — demonstrated, not asserted, same bar `cross-rom-capability-spec.md`'s probe
      criteria hold.
- [ ] The validation pass rejects a module missing any of the eleven fixed-vocabulary `FlavorText`
      keys (Mechanics section, above) — this is a load-time check, not something discovered by
      running a full session, since a session run might not exercise every one of the eleven in a
      single pass (e.g. never triggering `pc-lost` or `monster-fled`).
- [ ] **A second, independently-authored `IOneD20WorldModule` implementation runs a full session
      correctly** — move/look/check/fight/flee/inventory all work, win-line topology unaffected,
      loot awards correctly — **with zero changes to `LunaFlow.Rulebook`'s structural code, GridMap,
      D20System, or Loot.** This is Phase 2.8's actual Done-When bar; everything above is what makes
      it checkable rather than assumed.
- [ ] `DefaultOneD20WorldModule` is a real, complete `IOneD20WorldModule` — the "second"
      implementation above must not be the one exercising all properties for the first time.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

None anticipated — see `world-module.md`'s "deliberately not built yet" for what's consciously
excluded (`ComponentRegistry` registration, a generalized `IWorldModule`) and why. If implementation
surfaces a real gap this spec's toolbox can't express, that's a ticket to the Architect, per the same
discipline `loot-rom-content-expansion-spec.md` held.

## Open questions / edge cases

- **Where `DefaultOneD20WorldModule`'s content actually lives** (one file vs. split by concern —
  rooms/challenges/monsters/flavor as today's separate files vs. consolidated) is Baraka's
  translation call, not specced here — same "state behavior, not file layout" instinct
  `content-spec-template.md` asks for.
- **What the second World Module's content should be** is Juno's call (Scope, above) — this spec
  only requires that *a* valid second implementation exists and is exercised; it doesn't pick the
  reskin's theme.
- **Whether validation lives as a method on the Rulebook's assembly type or a standalone validator
  class** — an implementation-shape choice, not a behavior the spec needs to pin.

## Related documents

- [`../design/world-module.md`](../design/world-module.md) — the tier's shape and rationale this
  spec builds against
- [`../design/architecture.md`](../design/architecture.md) — the four-tier diagram and the
  additive-composition principle recorded there per this phase's Deliverables
- [`../design/non-goals.md`](../design/non-goals.md) — why `IOneD20WorldModule` isn't generalized
  and why World Modules aren't `ComponentRegistry`-registered yet
- [`../Roadmap/phase-2-8.md`](../Roadmap/phase-2-8.md) — the phase this spec belongs to
- [`grid-rom-d20-split-spec.md`](grid-rom-d20-split-spec.md) /
  [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md) — the content this
  extraction relocates, unchanged in shape
- [Ticket 0055](../ClaudeWiki/Processes/Handoffs/0055-loot-spec-mechanism-deviations.md) — surfaced
  the `OneD20SessionAssembler`/static-content coupling this spec fixes
- [Specs README](README.md)
