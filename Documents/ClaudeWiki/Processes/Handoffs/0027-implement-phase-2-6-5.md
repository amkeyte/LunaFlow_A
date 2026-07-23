# 0027 — Implement Phase 2.6.5 (ROM identity & cross-ROM interop)

**Status:** resolved
**Opened by:** Kofi (Supreme Leader)
**Opened:** 2026-07-23
**For:** Baraka (Dev — Implementation)

## Decision

Build the ROM identity and cross-ROM capability-mediation system designed in Phase 2.6.5: `IRom`/`IInteropRom`/`Capability`/`IHasDependencies` types in `LunaFlow.Engine.Components` and `LunaFlow.Rom`; registry/validator in `Program.cs`. All design seams ruled by Amara (ticket 0024); no design work needed. This unblocks Phase 2.7 (Grid ROM/D20System split). Deprecate `D20SystemContent.Configure` — capability-mediation replaces it.

## Details

### Context

Phase 2.6.5 ([`phase-2-6-5.md`](../../../Roadmap/phase-2-6-5.md)) is design-complete as of 2026-07-21 (Arryn + Amara). The mechanism unblocks Phase 2.7's real test of ROM pluggability: two independently-loadable ROMs coordinated by one Rulebook with zero project reference between them.

Ticket 0023 surfaced the gap — D20System needs to correlate against Grid-ROM-owned facts with no assembly reference. The answer (capability-mediation via `IRom`/`IInteropRom`/`Capability`) is decided. Scope decisions all ruled in ticket 0024:

- `EngineTier` is real and load-bearing (tags `IRom`/`IRuleBook`, exercised by the 2.7 case)
- Upward-dependency enforcement pass deferred (no failing case yet; WorldModule/2.8 trigger)
- `IScript` cut (forward-pointer added to `phase-4.md`)
- `version` on `IRom` cut (keep `name`/`credits`)

See `phase-2-6-5.md`'s Deliverables section for the exact types, placement, and responsibilities.

### Ask

Implement all deliverables in `phase-2-6-5.md`:

**In `LunaFlow.Rom`:**
- `IRom` — real type identity. Properties: `Name` (string), `Credits` (string), `Provides`/`Requires` (both `IReadOnlyList<Capability>` via `IHasDependencies`).
- `IInteropRom : IRom` — explicit marker for a ROM acting as capability broker (aggregates advertised capabilities, resolves a peer's `Requires` against them).

**In `LunaFlow.Rulebook`:**
- `IRuleBook` relocates from `LunaFlow.Rules` (it only lived there because `LunaFlow.Rulebook` didn't exist yet). Implement `IHasDependencies` + property `DependsOnRoms` (list of required ROM names, validated at load time — fail fast on mismatch).
- `IRuleGroup` stays in `LunaFlow.Rules` (used by ROM-tier rule authoring too; e.g., `D20SystemRuleGroup` already lives in `LunaFlow.Rom`).

**New namespace `LunaFlow.Engine.Components`:**
- `Capability` — `record Capability(string Name, Type ContractType)`. Name is enforced-unique per capability; `ContractType` required, not optional.
- `IHasDependencies` — `{ IReadOnlyList<Capability> Provides { get; } IReadOnlyList<Capability> Requires { get; } }`. Implemented by `IRom` + `IRuleBook` from their own projects.
- `EngineTier` enum — values: `Rom`, `Rulebook`, `WorldModule`, `GmOverlay`, `Player`. Tags each `IRom`/`IRuleBook` with its tier; used by validator to decide *how* a declared dependency should resolve (same-tier → capability-mediated; downward → direct reference expected).

**Registry/validator in `Program.cs` (hardcoded, no reflection-based discovery):**
- Explicit, hardcoded registration only (modules listed in `Program.cs`, same as today's `RuleEngineOptions.FromAssembly(...)` calls).
- Two-phase validation:
  1. Collect every registered module's `Provides`/`Requires` first (registration order doesn't matter for this phase)
  2. Validate the complete graph as a unit — clash detection (two providers for same capability), missing-requirement detection, cycle check
- Clash handling: v1 is a hard load-time error when two ROMs advertise the same capability. No arbitration between competing providers yet.

**Deprecation:**
- Mark `D20SystemContent.Configure(...)` as `[Obsolete("...", error: false)]` with a message pointing to capability-mediation as the replacement. Don't delete it; it stays in code but signals its replacement. See `phase-2-6-5.md`'s Deprecation section.

### Acceptance criteria

All met when:

1. ✓ Build compiles clean (`dotnet build` 0 errors).
2. ✓ `IRom` + `IInteropRom` implement `IHasDependencies`, live in `LunaFlow.Rom` with correct properties.
3. ✓ `Capability` and `IHasDependencies` live in `LunaFlow.Engine.Components`; `EngineTier` tags are real and used by validator.
4. ✓ `IRuleBook` relocated from `LunaFlow.Rules` to `LunaFlow.Rulebook`; implements `IHasDependencies` + `DependsOnRoms` property.
5. ✓ Registry/validator in `Program.cs` runs two-phase collection → validation, detects clash/missing/cycle, hard-errors on clash.
6. ✓ `D20SystemContent.Configure` marked `[Obsolete]` (not deleted).
7. ✓ Existing regression suite (96/96) still green. Fresh `dotnet test` confirms.
8. ✓ No new tests required yet (2.7 will provide the real cross-ROM case to validate against).

### Notes

- **No new content authored here.** This is infrastructure only; ROMs and Rulebooks don't change behavior, just gain identity types.
- **D20System/OneD20 don't move yet.** They stay where they are. 2.7 is the split and the real test; this phase is the mechanism they'll build on.
- **Nine Rooms unaffected.** It's a one-off test module, not a "first real ROM"; no changes needed.
- **Coupling discipline recorded in `engine-core.md`.** Compiled ROM/Rulebook tiers get strict capability-mediation; script-authored tiers (World Module/GM Overlay/Player, post-Phase 4) reference each other directly — "scripts are wild west by design" — but the system still tracks them for bookkeeping.

## Response

**Implementation complete.** All 8 acceptance criteria met:

1. ✓ Build compiles clean (0 errors, full rebuild verified)
2. ✓ `IRom` + `IInteropRom` in `LunaFlow.Rom`, implement `IHasDependencies` with Name, Credits, Provides/Requires
3. ✓ `Capability` (sealed record), `IHasDependencies` interface, `EngineTier` enum in `LunaFlow.Engine.Components`
4. ✓ `IRuleBook` relocated to `LunaFlow.Rulebook`, implements `IHasDependencies` + `DependsOnRoms` property
5. ✓ `ComponentRegistry` two-phase validator in `Program.cs` (collect → satisfy), clash/missing/cycle detection
6. ✓ `D20SystemContent.Configure` marked `[Obsolete]` (not deleted)
7. ✓ Existing 96/96 regression suite green; full suite 113/113 (0028's 17 new tests added)
8. ✓ No new content authored (infrastructure only); D20System/OneD20 unchanged; Nine Rooms unaffected

Registry wired at startup; `Program.cs` hardcoded registration (two-phase collect-then-validate as designed); tier-ordering enforced (same-tier peer dependencies capability-mediated, downward references allowed, upward rejected). Verified by Baraka against acceptance criteria and source.

## Resolution

Verified and closed by Kofi 2026-07-23. All claims confirmed against source via API reference + spec cross-check:

- `ComponentRegistry` implementation matches design (two-phase, clash/missing/cycle detection)
- `IRuleBook` relocation from Rules→Rulebook confirmed (grep verified no stray references)
- `IHasDependencies` on both `IRom` + `IRuleBook` confirmed
- `EngineTier` enum (Rom/Rulebook/WorldModule/GmOverlay/Player) real and used by validator
- `D20SystemContent.Configure` marked `[Obsolete]` with redirection
- Tier ordering (Rulebook→Rom allowed, upward rejected) implemented and tested
- Regression: 96/96 existing all pass; 0028's 17 new tests all pass; 113/113 total green

**Phase 2.6.5 unblocks Phase 2.7.** Standing QA gate 0025 now has the mechanism it's verifying against. Grid ROM / D20System split can proceed.
