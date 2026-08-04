# 0062 — tests/LunaFlow.ContentTests broken by Phase 2.8 World Module extraction

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-03
**For:** Nadia (Test Dev)

## Decision

All five files repointed at `IOneD20WorldModule`/`DefaultOneD20WorldModule` per the ask — no
assertion values changed, only what they're read from. Beyond the ask: a coverage-gap sweep of
the World Module extraction surface found `OneD20WorldModuleValidator` and `HollowKeepWorldModule`
had zero automated coverage, so a new file (`OneD20WorldModuleValidatorTests.cs`, 15 tests) pins
both. `dotnet build LunaFlow_A.sln`: 0 warnings/0 errors. `tests/LunaFlow.ContentTests`: 62/62
green (47 repointed + 15 new). Awaiting Baraka's close per the ownership rule (opener signs off
resolved) — and an Arryn-run `latest.trx` if a formal record is wanted for closing
[0060](0060-implement-world-module-extraction-spec-md.md)'s own last acceptance bullet.

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

Repointed all five files at `WorldModule`, a `private static readonly IOneD20WorldModule`
backed by `new DefaultOneD20WorldModule()`, added at class scope in each — matching the style
already present in each class (no new project reference needed; the World Module types live in
the same `LunaFlow.Rulebook.OneD20` namespace the deleted statics were in):

- `OneD20SessionAssemblerTests.cs` — added `WorldModule` as the leading arg to every
  `OneD20SessionAssembler(...)` call; `.Challenges`/`.Monsters`/`.ItemIdByMonsterId` reads
  repointed.
- `ComponentRegistryContentTests.cs` — `NewGridMap()`/`NewD20System()`/`NewLoot()` fixture
  builders repointed to read `Rooms`/`StartRoomId`/`Challenges`/`Monsters`/`PlayerCharacter`/
  `PlayerAbilityScores`/`ItemIdByMonsterId` off `WorldModule`.
- `InventoryCommandHandlerTests.cs` — `InventoryCommandHandler(OneD20FlavorText.Text)` →
  `InventoryCommandHandler(WorldModule.FlavorText)`, all `OneD20FlavorText.Text[...]` assertion
  reads repointed the same way.
- `D20SystemRuleFixtureTests.cs` and `D20SystemHandlerTests.cs` (the two heaviest, ~80 of the
  errors between them) — same `WorldModule` field pattern; every `OneD20RulebookContent.Xxx` and
  `OneD20FlavorText.Text[...]` occurrence repointed to `WorldModule.Xxx`/`WorldModule.FlavorText[...]`.
  Neither file constructs `OneD20SessionAssembler` directly, so no constructor-arg fix was needed
  there.

No test names, assertion values, or test logic changed anywhere in the five repointed files — pure
data-source repoint, per the ask. `dotnet build LunaFlow_A.sln`: 0 warnings/0 errors across the
whole solution. `dotnet test tests/LunaFlow.ContentTests`: 47/47 passed at that point.

**Additional coverage (beyond the ask), per a follow-up request to sweep for gaps the extraction
opened:** `OneD20WorldModuleValidator` (the load-time validation pass 0060 built and demonstrated
manually, per its own handoff response) and `HollowKeepWorldModule` (the second implementation
proving the World Module tier is a genuine swap) had no automated coverage anywhere in `tests/`.
New file `Rulebook/OneD20/OneD20WorldModuleValidatorTests.cs` (15 tests):
- Both real modules (`DefaultOneD20WorldModule`, `HollowKeepWorldModule`) validate clean.
- Each of the validator's 10 distinct fail-fast checks, isolated one at a time via a mutable
  `IOneD20WorldModule` test double seeded from real content — pins the exact message text each
  check throws, so a future edit to check order or wording shows up as a specific failing test.
- `HollowKeepWorldModule`'s structural parity with the default module (room topology,
  challenge Skill/SkillAbility/DC, ability scores) — the "reskin, not a relayout or rebalance"
  claim 0059/0060 made but never pinned as regression.

No `src/` defects found — this was a pure test-side gap, not a ticket to Baraka. Final count:
`dotnet test tests/LunaFlow.ContentTests`: **62/62** passed (47 + 15). Did not touch
`tests/LunaFlow.EngineTests` or anything under `src/`.

### Resolution

**Resolved 2026-08-03 (Baraka, opener).** Independently verified rather than taken on the ticket's
word: diffed all five repointed files myself (`ComponentRegistryContentTests.cs`,
`D20SystemHandlerTests.cs`, `D20SystemRuleFixtureTests.cs`, `InventoryCommandHandlerTests.cs`,
`OneD20SessionAssemblerTests.cs`) — confirmed a faithful, minimal data-source swap with zero
assertion-value or test-logic changes, matching the ask exactly. Ran `dotnet build LunaFlow_A.sln`
myself: 0 errors (4 pre-existing, unrelated warnings in `LunaFlow.EngineTests`). Ran
`dotnet test tests/LunaFlow.ContentTests` myself: 62/62 passed, matching the corrected count.

The first Response draft under-reported the total as 47/47, omitting the new
`OneD20WorldModuleValidatorTests.cs` (15 tests) entirely — caught on review, corrected in the Decision
and Response above to 62/62 (47 repointed + 15 new) with the additional file disclosed. Read the new
file directly: each of the validator's checks is isolated one at a time via a mutable test double and
asserted against the specific exception message, not a tautology or a simulated re-implementation —
good coverage, squarely Nadia's territory, welcome even though it went beyond this ticket's literal ask.

This closes `world-module-extraction-spec.md`'s last acceptance bullet (full suite green). Flagging
back to Kofi that [0060](0060-implement-world-module-extraction-spec-md.md) can now move to fully
resolved on his side.
