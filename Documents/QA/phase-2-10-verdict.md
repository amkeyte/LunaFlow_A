# Phase 2.10 (Content Coverage Stress Test) — QA Verdict

**Verdict:** PASS
**Date:** 2026-08-04
**Ticket:** [0074](../ClaudeWiki/Processes/Handoffs/0074-phase-2-10-qa-gate.md)
**Build:** `dotnet build LunaFlow_A.sln` — Build succeeded, 0 Warning(s), 0 Error(s) (final clean state, after this pass's own scratch artifacts were removed — see Method below).
**Suite:** 170/170 (93 EngineTests + 77 ContentTests), read from a single merged `tests/LunaFlow.EngineTests/TestResults/latest.trx`, timestamped 2026-08-04T14:05:36 (today), 0 failed/error/aborted. Arryn directly confirmed running this pass (77/77 ContentTests) in-conversation; the merged-trx shape matches the precedent already established and confirmed in the [Phase 2.9 verdict](phase-2-9-verdict.md).

## Method

The automated suite's 170/170 does **not** cover Phase 2.10's actual new mechanisms — verified by
listing every test name in the trx: the ContentTests/EngineTests present are pre-existing behavior
(move/look/check/fight/flee/win-lines, ComponentRegistry, GameEngineCore, Lua/HollowKeep parity) plus
ticket 0072's two Cartridge shape-check tests. Zero tests reference Save-kind resolution, the
`equip`/Backup Piece swap, `Poisoned`, or the soft-lock sequence. Per the qa-verify skill's step 4
(manual pass for what the suite doesn't cover) and ticket 0074's explicit "reproduce it yourself"
asks, this pass:

1. Read every touched source file directly (not the API Reference, since these are behavioral claims).
2. Wrote a throwaway MSTest file (`QA_ScratchPhase210VerificationTests.cs`, 7 tests) reusing the
   existing `D20SystemHandlerTests.cs` fixture pattern (real `GameEngineCore`/`DispatchGameCommand`
   pipeline, injectable-seeded `D20ResolutionMechanic` per ticket 0019's seam), with deterministic dice
   via two `Random` subclasses (`FixedRoll`: pops a pre-planned queue, throws on exhaustion so an
   under-specified scenario fails loud; `ConstantRoll`: always returns one value, for the soft-lock's
   many identical misses). Every scenario's numbers were chosen to be **discriminating** — e.g. the
   Save-kind test uses a roll where `SavingThrows.Fortitude`(+4) succeeds but a hypothetical
   raw-ability-modifier-only bug (+3) would fail — not just "does it run without throwing."
3. Wrote a second throwaway file for the validator's two negative cases (no engine needed).
4. Ran `dotnet run --project src/MyLunaFlowServer` twice: once as a plain smoke pass, once after
   temporarily adding a bare non-interface record directly to `LunaFlow.Cartridge` to reproduce the
   §5.4 shape-check's negative case myself, per ticket 0074's explicit ask not to take 0072's
   demonstration on its own.
5. All 7 scratch tests passed; the fault-injection file was deleted and the solution rebuilt clean
   (0 warnings, 0 errors) before writing this verdict. Nothing from this pass's method remains in the
   tree.

## Criteria (`phase-2-10-content-coverage-spec.md`'s Acceptance criteria list)

| Criterion | Evidence | Result |
| --- | --- | --- |
| Save-check entries + poison flag authored in `DefaultOneD20WorldModule`, survive `Program.cs` translation unchanged, verified end to end | Source: `DefaultOneD20WorldModule.cs` ("wire"/"fumes" entries with `Kind`/`SaveType`; scrapper `InflictsPoison: true`); `Program.cs`'s `ToRoomChallenge`/`ConstructD20System` carry `Kind`/`SaveType`/`InflictsPoison` through unchanged. Live: `SaveKind_...` and `Poisoned_...` scratch tests exercise the full World-Module→ROM-tier→session path, not just the fact shape. | PASS |
| `OneD20WorldModuleValidator` rejects both-set and neither-set | Source: `OneD20WorldModuleValidator.cs:84-96`. Live: both negative cases reproduced directly (`Validate_SkillCheckChallengeWithBothSkillAndSaveTypeSet_Throws`, `Validate_SkillCheckChallengeWithNeitherSkillNorSaveTypeSet_Throws`), both threw `InvalidOperationException` with the expected message. | PASS |
| Save-kind `RoomChallenge` resolves against `SavingThrows[SaveType]`, not a skill modifier | Source: `CheckCommandHandler.cs:37-46` reads `_d20System.SavingThrows` directly on the `Save` branch. Live: `SaveKind_ResolvesAgainstSavingThrows_NotRawAbilityModifier` — d20=8 against "fumes" (DC 12) succeeds only because `SavingThrows.Fortitude`=4 (base +1, not just the raw Con modifier +3); a raw-modifier bug would have failed this exact roll. | PASS |
| `SavingThrows` computed once at session assembly, no new World Module field, no engine-tier change | Source: `D20SystemRom.cs:52-55` — plain property computed in the constructor from `PlayerAbilityScores` + a fixed `BaseSaves` static. No `ComponentRegistry`/`Capability`/`EngineTier` reference anywhere in its definition or computation. | PASS |
| `equip "Backup Piece"` changes `fight`'s PC-attack roll to 1d6/+0 vs. default 1d8+3/+4 | Source: `FightCommandHandler.cs:58-70` (equipped-weapon substitution), `D20SystemRom.cs:23-27` (pistol profile: `AttackBonus 0, DamageDie 1d6, DamageBonus 0`). Live: `Equip_BackupPiece_SwapsAttackBonus_...` — the *same* d20=10 roll hits pre-equip (+4 sword, total 14 ≥ Defense 14) and misses post-equip (+0 pistol, total 10 < 14), proving the substitution actually executes, not just that a fact exists. | PASS |
| Equip rejected without a matching `ItemAwarded` | Source: `EquipCommandHandler.cs:57-63`. Live: `Equip_RejectedWithoutMatchingItemAwarded` — no `EquippedWeapon` fact created, "You can't equip that." announced. | PASS |
| Poison-carrier hit inserts `Poisoned(3)`; a repeat hit refreshes to 3, not stacks | Source: `FightCommandHandler.cs:108-113`. Live: `Poisoned_...` — exactly one `Poisoned` fact after round 1 (`ChecksRemaining=3`), still exactly one fact at 3 (not 6) after round 2's repeat hit. | PASS |
| `Poisoned` applies -2 to a `check`/`fight` roll in a **different room** than inflicted | Source: `Poisoned` (`Facts.cs`) carries no `RoomId` — structurally not room-scoped; `PoisonEffect.ApplyAndAdvance` queries it globally. Live (the load-bearing assertion): poison inflicted in room 1, then a "fumes" check in room 2 with d20=8 **fails** (10 < DC 12) because the -2 still applies; the same roll would succeed (12 ≥ 12) without the penalty — a room-scoping bug would have flipped this outcome and been caught. | PASS |
| `Poisoned` expires after 3 checks/fights | Live: same scenario's 4th application, with the fact already retracted, succeeds on the identical d20=8 roll (12 ≥ 12, no penalty). | PASS |
| Engineered soft-lock: transcript showing the silent, no-reaction session state | Source: win-line topology (`WinLineCatalog.cs` + `DefaultOneD20WorldModule.Rooms`) confirms only `[3,6,9]` contains none of the three Fight rooms {1,5,8}; `CheckCommandHandler` has no `RoomFailed` branch, so a SkillCheck room can never kill a line. Live: `SoftLock_...` — failing rooms 1, 5, 8 via forced 3-round timeouts kills all 7 other lines (`Query<WinMarker>()` empty, no anomalous message), and `[3,6,9]` remains solvable and fires normally once solved. **See Deviations below** — the spec's own wording ("zero reachable win lines") is not what's achievable or what was built; ticket 0074 already reframes this correctly as "7 of 8 dead, one line permanently reachable." | PASS (spec-wording deviation flagged) |
| No existing criterion from `1-d20-spec.md`/`loot-rom-content-expansion-spec.md` regressed | 170/170 includes `D20SystemHandlerTests.cs`/`LootRomTests.cs`/`LootRuleGroupFixtureTests.cs`/`InventoryCommandHandlerTests.cs` run unmodified against the exact pre-2.10 fixture assignment — these directly exercise the touched `Check`/`Fight` handlers' pre-existing paths. **One stale-criterion flag** (not a functional regression) — see Deviations. | PASS (one stale-criterion flag) |
| Full suite green, Arryn-run `latest.trx` | See Suite line above. | PASS |

## Criteria (ticket 0074's own numbered asks 1–10; #11 is this document)

All ten map 1:1 onto the spec-criteria table above except two 0074-specific asks not already covered:

| 0074 ask | Evidence | Result |
| --- | --- | --- |
| `IEquippedWeapon` cross-ROM shape (item 5): interface in `LunaFlow.Cartridge`, Loot `Provides`s it, D20System `Requires` deliberately excludes it, §5.4 shape check runs in `Program.cs`'s bootstrap barrier, negative case reproduced myself | Source: `IEquippedWeapon.cs` (Cartridge), `LootRom.cs:32-35` (`Provides`), `D20SystemRom.cs:81-86` (`Requires` excludes it, with a comment citing 0071), `Program.cs:131-132` (`ContractSurfaceValidator.ValidateInterfacesOnly` called on the Cartridge assembly inside `Bootstrap()`). Live negative case: temporarily added `public sealed record QA_ScratchBogusOffender(int X);` directly to `LunaFlow.Cartridge`, rebuilt, ran the server — bootstrap correctly printed `Validation failed: Contract surface 'D20SystemExampleC' exposes non-interface type(s): LunaFlow.Cartridge.QA_ScratchBogusOffender...` and exited via the unhandled exception path (non-zero, no degradation), exactly as designed. Reverted; rebuild confirmed clean. | PASS |
| Design-doc currency: `grid-rom-d20-split-spec.md`'s capability table and `engine-core.md`'s Cross-ROM correlation section carry the 0071 edits | `grid-rom-d20-split-spec.md:207,222-225` — `GridMap Requires: Array.Empty<Capability>()` plus the explicit drift-correction note citing 0071. `engine-core.md:209-219` — the "When to leave `Requires` undeclared" subsection, stating the general correlation-vs-construction-order rule. Independently confirmed via `git log -1`: `grid-rom-d20-split-spec.md`, `engine-core.md`, and `cartridge.md` all land on the same commit `5b62123` (2026-08-04 00:02:12), matching Amara's 0071 correction and 0072's resolution exactly. | PASS |

## Deviations from spec

- **Soft-lock criterion's literal wording is unsatisfiable as written.** `phase-2-10-content-coverage-spec.md`'s acceptance bullet says the transcript should show "zero reachable win lines." Verified from source (`WinLineCatalog.cs` + the fixed grid topology) and reproduced live: this is mathematically impossible — column `[3,6,9]` contains none of the three Fight rooms and can never be killed. The maximum achievable is 7 of 8 lines dead. Ticket 0074 (Kofi) already caught and reframed this correctly in its own item 7 wording before I started ("the shipped topology makes true zero-reachable-lines mathematically impossible"), so this isn't a new finding — but the spec document itself still says "zero," not "7 of 8, with one line structurally unkillable." Worth a small correction to the spec's acceptance-criterion wording so a future reader doesn't take it literally. Not blocking — the deliverable's actual intent (demonstrate the gap, confirm silence) is fully met.
- **`loot-rom-content-expansion-spec.md`'s "`Provides: []`" criterion is now stale, not a regression.** That spec's acceptance list (line 276) states `LootRom`'s `Provides: []`. Current shipped code (`LootRom.cs:32-35`) now declares `Provides: [EquippedWeapon]`, per this phase's own deliberate design (0071/0072's ruling). Nothing broke — Loot legitimately gained a capability this phase — but read literally today, that older spec's bullet is false. Same shape as the GridMap/`ChallengeSolved` drift 0071 already reconciled in `grid-rom-d20-split-spec.md`; this one hasn't been reconciled yet. Flagging for Amara/Kofi to correct the older spec's table, same pattern as before — not a build defect and not blocking this phase.
- **The spec's own Scope/Out section self-contradicts.** `phase-2-10-content-coverage-spec.md`'s "Out" list states "Any change to `GridMap`, `Loot`, or the win-line rule family — untouched by this entire spec," but the same document's Commands table and Content-data section explicitly add the `equip` command and `EquippedWeapon` fact to Loot, and this is exactly what shipped and what ticket 0074 expected verified. The "Out" bullet is simply wrong (likely predates item 2/4's Loot-owned design landing in the same draft) — not a build issue, a spec drafting inconsistency worth a cleanup pass by Juno/Amara.
- Ticket-0073-adjacent instrumentation (`GameEngineCore.cs`/`Program.cs` timing `Post()`/`Console.WriteLine` additions, currently uncommitted) is unrelated Roslyn-vs-NRules compile-cost work, out of this phase's scope — confirmed by reading the diffs directly; not evaluated further here.

## Superseded reports

None — first verdict for Phase 2.10.

## Tickets filed

None. No FAIL or UNPROVEN criterion. The two stale-spec-wording items above are informational
deviations, not blocking findings, and are recorded here per the qa-verify skill's "Deviations from
spec" section rather than as a fresh handoff ticket (they're pointer-worthy corrections to two spec
documents, not defects in the build) — routed to Kofi to decide whether they're worth a small doc-fix
ticket to Amara/Juno.
