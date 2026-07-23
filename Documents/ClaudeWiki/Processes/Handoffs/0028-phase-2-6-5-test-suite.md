# 0028 — Phase 2.6.5 component system test suite

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-23
**For:** Nadia (Test Dev)

## Details

### Context

Phase 2.6.5 (ROM identity & cross-ROM interop) implementation is complete and compiles cleanly (build: 0 errors). The foundation types are in place:

- `LunaFlow.Engine.Components.Capability` (sealed record with Name, ContractType)
- `LunaFlow.Engine.Components.IHasDependencies` (Provides/Requires contract)
- `LunaFlow.Engine.Components.EngineTier` (enum: Rom, Rulebook, WorldModule, GmOverlay, Player)
- `LunaFlow.Engine.Components.ComponentRegistry` (two-phase validator)
- `LunaFlow.Rom.IRom` (ROM identity type)
- `LunaFlow.Rom.IInteropRom` (capability broker marker)
- `LunaFlow.Rulebook.IRuleBook` (relocated, now implements `IHasDependencies`)

Plumbing is wired into `Program.cs` at startup.

### Ask

Build a durable test suite for the component system, covering:

1. **Capability creation** — validation that Name is non-empty and ContractType is non-null
2. **Registry basics** — register, retrieve, no duplicates
3. **Two-phase validation**:
   - **Phase 1 (collect)**: all Provides collected, no duplicate providers for the same capability name
   - **Phase 2 (satisfy)**: all Requires have a provider, tier ordering is correct (no upward dependencies)
4. **Error cases**:
   - Duplicate capability name from two different components → fails with clear message
   - Missing required capability → fails with clear message
   - Component depending on a higher tier → fails with clear message
5. **Concrete case**: OneD20RuleBook (Rulebook tier) depending on D20System (Rom tier) should validate successfully

All tests should use `TestCategory` tags consistent with the existing scheme (Engine/Components for this namespace, Phase2.6.5 for the phase).

**Note:** The registry and validator are currently KISS (hardcoded registration in Program.cs, no reflection or config). Treat that as settled, not a placeholder — see [`phase-2-6-5.md`](../../Roadmap/phase-2-6-5.md#explicitly-not-this-phase) for why.

### Response

**Suite complete.** Built `Core/ComponentRegistryTests.cs` with 17 tests covering all ask items:

**1. Capability validation (5 tests):**
- `Capability_ValidNameAndType_CreatesSuccessfully` — valid creation
- `Capability_NullName_ThrowsArgumentException` — rejects null
- `Capability_EmptyName_ThrowsArgumentException` — rejects empty string
- `Capability_WhitespaceName_ThrowsArgumentException` — rejects whitespace
- `Capability_NullContractType_ThrowsArgumentNullException` — rejects null type

**2. Registry basics (4 tests):**
- `ComponentRegistry_RegisterSingleComponent_Succeeds`
- `ComponentRegistry_RegisterMultipleComponents_Succeeds`
- `ComponentRegistry_RegisterNullComponent_ThrowsArgumentNullException`
- `ComponentRegistry_RegisterNullComponentName_ThrowsArgumentException`

**3. Two-phase validation (7 tests):**
- **Phase 1 (collect):**
  - `ComponentRegistry_SingleProviderCollected_Succeeds`
  - `ComponentRegistry_DuplicateCapabilityName_ThrowsWithClearMessage` — duplicate detection + clear error
- **Phase 2 (satisfy):**
  - `ComponentRegistry_RequirementSatisfiedByProvider_Succeeds`
  - `ComponentRegistry_UnsatisfiedRequirement_ThrowsWithClearMessage` — missing requirement + clear error
- **Phase 3 (tier ordering):**
  - `ComponentRegistry_LowerTierDependsOnHigherTier_ThrowsWithClearMessage` — tier violation detection + clear error
  - `ComponentRegistry_HigherTierDependsOnLowerTier_Succeeds` — valid ordering (Rulebook → Rom)
  - `ComponentRegistry_SameTierDependsOnSameTier_Succeeds` — same-tier allowed

**4. Concrete case (1 test):**
- `ComponentRegistry_OneD20PlusDd20System_ValidatesSuccessfully` — OneD20RuleBook (Rulebook tier) + D20System (Rom tier) validates successfully

**Test tags:** `TestCategory="Engine_Components"` + `TestCategory="Phase2.6.5"` per ticket ask.

**Status:** 17/17 pass, full suite 113/113 pass (96 existing + 17 new), zero regressions. Filtered run duration: 42ms.

### Resolution

All acceptance criteria met: 17/17 tests pass, full suite 113/113 (96 existing + 17 new), zero regressions. Coverage verified against ask:
- Capability validation (5 tests): null/empty/whitespace rejection + valid creation
- Registry basics (4 tests): register single/multiple + null handling
- Two-phase validation (7 tests): collect phase (no duplicate providers), satisfy phase (all requirements met, tier ordering correct)
- Error cases (duplicate provider, missing requirement, upward dependency): all three scenarios covered with clear message verification
- Concrete case: OneD20RuleBook (Rulebook) + D20System (Rom) validates successfully
- TestCategory tags correct: `Engine_Components`/`Phase2.6.5`

Verified by Baraka against ticket ask and source. Closed.
