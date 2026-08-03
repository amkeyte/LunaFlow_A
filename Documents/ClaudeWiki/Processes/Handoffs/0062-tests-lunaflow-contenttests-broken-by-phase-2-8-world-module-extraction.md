# 0062 — tests/LunaFlow.ContentTests broken by Phase 2.8 World Module extraction

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-03
**For:** Nadia (Test Dev)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

Building [`world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md) per
ticket [0060](0060-implement-world-module-extraction-spec-md.md): `OneD20RulebookContent`/
`OneD20FlavorText`'s static surface is gone, replaced by `IOneD20WorldModule`/
`DefaultOneD20WorldModule`/`HollowKeepWorldModule`, and `OneD20SessionAssembler`'s constructor now
takes an `IOneD20WorldModule` instead of reading nothing. The spec's own acceptance criteria scope
"zero references" to `LunaFlow.Rulebook`, `example/`, and `src/MyLunaFlowServer` only — `tests/` was
explicitly out of scope, same precedent as ticket 0034 → CodePlan 0037 → 0038 → 0044 for Phase 2.7.
`dotnet build LunaFlow_A.sln` confirms: `src/` and `example/` build clean (0 warnings, 0 errors);
`tests/LunaFlow.ContentTests` fails with ~80 `CS0103`/`CS1503`/`CS7036` errors, all from direct
references to the now-deleted statics or the assembler's old single-arg constructor.

Five files are affected, all under `tests/LunaFlow.ContentTests/`:
- `Rulebook/OneD20/OneD20SessionAssemblerTests.cs` — constructs `OneD20SessionAssembler` with only a
  `Random` arg; needs an `IOneD20WorldModule` first, plus reads `OneD20RulebookContent.Challenges`/
  `.Monsters`/`.ItemIdByMonsterId` to assert against.
- `Rom/ComponentRegistryContentTests.cs` — builds `GridMapRom`/`D20SystemRom`/`LootRom` fixtures
  directly from `OneD20RulebookContent`'s statics.
- `Rom/Loot/InventoryCommandHandlerTests.cs` — constructs `InventoryCommandHandler` with
  `OneD20FlavorText.Text`, asserts against several of its literal string values.
- `Rom/D20System/D20SystemRuleFixtureTests.cs` and `Rom/D20System/D20SystemHandlerTests.cs` — the
  two heaviest, building full `GridMap`/`D20System` fixtures and asserting dozens of announcements
  against `OneD20FlavorText.Text`'s and `OneD20RulebookContent`'s values directly.

### Ask

Repoint all five files at `IOneD20WorldModule` — construct a `DefaultOneD20WorldModule` (or a small
test-local `IOneD20WorldModule` fixture, whichever fits each test's existing style better) and read
`Rooms`/`Challenges`/`Monsters`/`ItemIdByMonsterId`/`PlayerCharacter`/`PlayerAbilityScores`/
`FlavorText` from it instead of the deleted statics. `OneD20SessionAssemblerTests.cs`'s
`OneD20SessionAssembler(...)` calls need a leading `IOneD20WorldModule` argument added.

`DefaultOneD20WorldModule`/`HollowKeepWorldModule`/`IOneD20WorldModule`/`OneD20WorldModuleValidator`
live in `LunaFlow.Rulebook.OneD20`, same namespace the deleted statics were in — no new project
reference needed. Content is byte-for-byte identical to what `OneD20RulebookContent`/
`OneD20FlavorText` held (diffed against them before deletion), so no assertion values should need to
change, only what they're read from.

Once `tests/` compiles and the full suite is green (read from an Arryn-run `latest.trx`), that
closes `world-module-extraction-spec.md`'s own last acceptance-criteria bullet — flag back to Kofi
alongside closing this ticket, since it's also the thing 0060 is waiting on before it can fully close.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
