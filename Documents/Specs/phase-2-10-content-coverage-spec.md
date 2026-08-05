# Phase 2.10 — Content Coverage Stress Test — Implementation Spec

**Status:** active — implemented & QA-verified ([Phase 2.10 verdict](../QA/phase-2-10-verdict.md),
PASS). **Amended 2026-08-04** per [ticket 0075](../ClaudeWiki/Processes/Handoffs/0075-phase-2-10-spec-currency-cleanup.md)
to correct three text-drift items the verdict flagged (none are behavior/build changes) — the
soft-lock criterion's unsatisfiable wording, this document's own self-contradicting Scope/Out
bullet, and `EquippedWeapon`'s shape description, brought in line with
[0071](../ClaudeWiki/Processes/Handoffs/0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)/[0072](../ClaudeWiki/Processes/Handoffs/0072-build-cartridge-contract-surface-shape-check.md)'s
ruling.
**Companion design doc:** [`../design/1-d20.md`](../design/1-d20.md) — read for rationale.
**Companion to:** [`1-d20/1-d20-spec.md`](1-d20/1-d20-spec.md) (the base this extends) and
[`Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md)
(the reviewed proposal this turns into buildable form, per [ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md)).
**Supersedes:** nothing. Additive to [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md) —
its facts are unchanged, but **not** its `Provides` list: Loot legitimately gains `EquippedWeapon`
as a provided capability here (see Fact vocabulary and ticket 0075's item 2). That older spec's own
acceptance-list line is corrected separately, in the same pass.

## Purpose

Turns the proposal's Phase 2.10 candidate items (1–5) into buildable content. Each item is chosen
to press on a specific piece of 1.d20's fact vocabulary that's shipped but never actually exercised
— a third challenge kind, attack resolution past melee, a marker crossing a room boundary, an item
with a mechanical effect, and a deliberately-engineered soft-lock. **This spec does not implement
soft-lock detection** — item 5 demonstrates the known gap (`1-d20-spec.md`'s own open question), it
doesn't close it. No new framework capability is anticipated anywhere in this spec; if implementation
finds one is needed, that's a ticket to the Architect, not something to route around quietly.

## Scope

**In:**
- A third `RoomChallenge` check kind (**Save**, alongside the existing **Skill**), and the PC's
  saving-throw bonuses needed to resolve it — derived from data already supplied
  (`PlayerAbilityScores` + the fixed Strong-hero base-save table `1-d20.md` already specifies), no
  new World Module field.
- A mechanical effect for the already-named **Backup Piece** item (2.7.1's Marksman drop, currently
  display-only): equipping it swaps the PC's attack profile from melee to ranged.
- A **Poisoned** status effect, inflicted by a specific monster's hit, that persists across a room
  transition and imposes a roll penalty until it expires.
- One deliberately-engineered, documented input sequence that exhausts every win line via
  `RoomFailed` that can structurally be exhausted, with the currently-silent behavior recorded as a
  verified finding. **Corrected per ticket 0075:** the grid's fixed topology makes exhausting *every*
  line mathematically impossible — column `[3,6,9]` contains none of the three Fight rooms and can
  never be killed by `RoomFailed`. The achievable, verified-shipped target is 7 of 8 lines dead, with
  `[3,6,9]` permanently reachable.

**Out:**
- A new room or a change to the grid's 9-position topology — the save-check challenge joins the
  existing randomized skill-check pool, it doesn't add a 10th slot.
- Soft-lock *detection* — this spec proves the gap is real and reachable, it doesn't build the
  reachable-line check `1-d20-spec.md` names as a candidate follow-up.
- A general persistent-PC-HP system outside `CombatRound` — item 3's design deliberately avoids
  needing one (see Mechanics, below); if that gap matters later, it's a separate proposal, not
  smuggled in here.
- Any change to `GridMap` or the win-line rule family — both untouched by this entire spec.
  **Corrected per ticket 0075:** an earlier draft of this bullet also listed `Loot` as untouched,
  which contradicts this same document's Commands table and Content-data section — Loot **is**
  touched (the `equip` command, `EquippedWeapon`; see items 2/4 below and Fact vocabulary), and that
  was always the intent, just misstated here. The bullet is corrected to match the rest of the
  document and what shipped, not the other way around.

## Fact / marker vocabulary

### D20System (extended)

`RoomChallenge` grows a discriminated check kind. Skill fields are unchanged; Save fields are new
and mutually exclusive with them:

```csharp
public enum CheckKind { Skill, Save }   // NEW — only meaningful when ChallengeType == SkillCheck

public sealed record RoomChallenge(
    int Id,
    RoomChallengeType ChallengeType,
    CheckKind? Kind,            // NEW — null for Fight rooms, required for SkillCheck rooms
    string? Skill, string? SkillAbility, int? DC,      // Skill kind only
    string? SaveType,           // NEW — "Fortitude" | "Reflex" | "Will"; Save kind only
    string? SuccessKey, string? FailureKey, string? PromptKey,
    string? MonsterId);
```

`PlayerCharacter` grows saving-throw bonuses, computed Rulebook-side from data already supplied —
no new World Module field, no framework dependency:

```csharp
public sealed record SavingThrows(int Fortitude, int Reflex, int Will);
    // Derived once at session assembly: base save (fixed, Strong hero lvl 1 — Fort +1, Ref +0,
    // Will +0, per 1-d20.md) + the matching ability modifier already in PlayerAbilityScores
    // (Con for Fortitude, Dex for Reflex, Wis for Will). Attached alongside PlayerCharacter,
    // not inside CombatantStats — monsters don't make saves in this content, no reason to carry
    // an unused field on their record too.
```

`MonsterTemplate` grows one field, for item 3:

```csharp
public sealed record MonsterTemplate(
    string Id, string Name, string DescriptionKey, CombatantStats Stats,
    bool InflictsPoison = false);   // NEW — default false; true for exactly one roster monster
```

New persistent fact (item 3) — accumulates across rooms, not scoped to a `CombatRound`:

```csharp
public sealed record Poisoned(int ChecksRemaining);
    // Inserted (or refreshed — retract-then-insert, same convention as PlayerPosition) at 3 when
    // a poison-inflicting monster's attack connects. Decremented on every subsequent `check` or
    // `fight` roll, regardless of which room it happens in. Retracted at 0. Never itself ends a
    // session or fails a room — a roll penalty, not damage (see Mechanics for why).
```

New equippable-item state (item 2/4), Loot-owned. **Corrected per ticket 0075/0071/0072** — this
spec's original draft described `EquippedWeapon` as a bare Loot-owned record with no cross-ROM
declaration, silently assuming D20System's `fight` step could just read it directly. That's wrong:
GridMap/D20System/Loot are peers with no assembly references between them, so D20System reading a
Loot-owned fact is exactly the kind of cross-ROM correlation `cross-rom-capability-spec.md` governs.
The shipped shape, ruled in ticket 0071 and built in 0072:

```csharp
// LunaFlow.Cartridge — the contract, not a bare record (cartridge.md's Structure section: the
// contract surface is interfaces only).
public interface IEquippedWeapon { string ItemId { get; } }
```

```csharp
// Loot's own concrete type, alongside its other three facts.
public sealed record EquippedWeapon(string ItemId) : IEquippedWeapon;
    // At most one at a time — equipping retracts the old, inserts the new, same convention as
    // PlayerPosition. Absent by default (PC fights with the starting sword).
```

`Loot.Provides` gains `Capability.Provided<IEquippedWeapon, EquippedWeapon>("EquippedWeapon")`.
**`D20System.Requires` deliberately does *not* gain a matching entry** — Loot already `Requires:
[ChallengeSolved]` from D20System, so declaring the reverse direction would create a genuine
two-node cycle in `ComponentRegistry`'s dependency graph (0071's finding). D20System's `fight`
handler correlates via `rules.Query<IEquippedWeapon>()` — ordinary NRules fact matching on the
shared contract type, not a declared, registry-validated dependency — with a doc comment on
`D20SystemRom.Requires` stating why, mirroring the existing precedent `GridMapRom.Requires` already
set for the same shape of peer correlation.

### World Module surface — the actual authoring layer, per Phase 2.8/`world-module-extraction-spec.md`

**Caught in self-review, worth stating plainly rather than fixing silently:** the fact shapes above
(`RoomChallenge`, `MonsterTemplate`) are ROM-tier — what `Program.cs` translates *into*, unchanged in
that role. Since Phase 2.8, they are **not** where content is authored; `IOneD20WorldModule` is.
Extending only the ROM-tier shapes and stopping there would leave this content with nowhere for a
World Module (or a Lua script, once Phase 2.12 exists) to actually supply it from — the same gap
Phase 2.8 closed for the original room/challenge/monster data has to stay closed for this spec's
additions too. Concretely:

- `OneD20ChallengeTemplate` (Rulebook-owned, `world-module-extraction-spec.md`) grows the same two
  fields as `RoomChallenge`: `CheckKind? Kind`, `string? SaveType`.
- `OneD20MonsterTemplate` grows `bool InflictsPoison = false`, mirroring `MonsterTemplate`.
- `OneD20WorldModuleValidator` grows two checks: every `SkillCheck`-type `OneD20ChallengeTemplate`
  has `Kind` set, and exactly one of (`Skill`+`SkillAbility`) or `SaveType` populated, never both,
  never neither — a malformed pairing fails at load, not mid-session.
- `Program.cs`'s existing `ConstructD20System` translation (`world-module-extraction-spec.md`'s
  "same translation path, new data source") carries `Kind`/`SaveType`/`InflictsPoison` through into
  the ROM-tier facts unchanged in logic — the World Module's value flows straight through, same as
  every other field already does.
- The two new save-check pool entries and the poison-carrying monster flag (Content data, below) are
  authored **in `DefaultOneD20WorldModule`**, not hardcoded as ROM-tier static content — that's the
  live convention as of Phase 2.8, and content authored any other way wouldn't be reachable from a
  second World Module (Hollow Keep, or Phase 2.12's Lua path) the way it needs to be.

**Backup Piece's mechanical profile is the one deliberate exception, not an oversight:** it stays
fixed Loot/D20System-tier data, the same way the PC's own starting sword stats are fixed rather than
World-Module-supplied. Per-item combat math reads as a System-level capability (universal to any
Scenario built on D20System), not Scenario-specific instance content the way a room's DC or a
monster's flavor is — `IOneD20WorldModule`'s contract has no item-mechanical-profile property today
and this spec doesn't add one. If a future item needs Scenario-varying mechanics, that's the trigger
to revisit this call, not a default to build toward now.

## Mechanics / rules

### `check` on a Save-kind room (new branch, D20System)

Same handler, same `D20ResolutionMechanic.ResolveCheck`, different inputs: rolls `1d20 +
SavingThrows[room.SaveType]` against `room.DC`, instead of `1d20 + ability modifier`. Success/
failure announcement and state changes (`RoomSolved`, no penalty on failure) are identical to a
Skill-kind room — the only thing that changes is which bonus feeds the roll. This is the concrete
proof (or disproof) of item 1's question: if this branch requires anything beyond a field-level
`RoomChallenge` change and a `SavingThrows` lookup, the shape didn't actually extend cleanly, and
that's the finding to report back, not paper over.

### Equipping `EquippedWeapon` and its effect on `fight` (items 2 + 4, deliberately combined)

**Why combined:** item 2 asks whether attack resolution generalizes past melee; item 4 asks for an
item with a mechanical effect. 2.7.1 already gave the PC's opponents a ranged option (the Marksman,
`2d6` damage, same `CombatantStats` shape, no mechanic change needed) — proving attack resolution
was already data-driven on the *monster* side. What's never been tested is the **PC's** side, and
giving the already-named, already-drop-table-linked Backup Piece a real effect is the natural way to
test it without inventing new content that doesn't cohere with what's already shipped.

A new `equip <item>` command (Loot-owned, reads `ItemAwarded`/`EquippedWeapon`, writes
`EquippedWeapon`): valid only for an item the PC actually holds (`ItemAwarded` exists for it) and
that's flagged equippable in Loot's content data (see below); silently rejects otherwise. `fight`'s
PC-attack step (D20System) queries `IEquippedWeapon` (cross-ROM, via the contract described in Fact
vocabulary above — not a direct read of Loot's concrete type) and resolves attack bonus/damage
die/bonus from it if present, else falls back to the pregenerated sheet's default (sword, STR-based)
— same lethality-first ordering, same lookup pattern the Save-kind branch above uses, no change to
`fight`'s control flow otherwise.

Backup Piece's profile: `1d6, Dex-based attack bonus (+0, since PC's Dex mod is +0)` — a real
trade-off against the default sword (`1d8+3, Str-based`), not a strict upgrade. This is the point:
it proves the mechanic supports a second weapon with a *different* keyed ability, not just a
reskinned melee stat block.

### `Poisoned` (item 3)

**Inflicted:** on `monster-hit` (PC takes non-lethal damage), if `MonsterTemplate.InflictsPoison`,
refresh `Poisoned(3)` in the same `Then()` — no separate rule, no separate roll.

**Applied:** both `check` and `fight`'s PC-roll steps read `Poisoned` before rolling: if present,
subtract 2 from the roll total (handler-level arithmetic, same place `SavingThrows`/skill modifiers
already get added), then decrement `ChecksRemaining` by 1 (replace fact), retracting at 0. This
applies whether the roll happens in the same room the poison was inflicted in or a different one —
that boundary-crossing is exactly what item 3 is testing.

**Why a roll penalty, not damage-over-time:** 1.d20 has no persistent PC HP fact outside an active
`CombatRound` — `PlayerCharacter.Stats.MaxHp` exists, but current HP only exists as `CombatRound
.PlayerHp`, scoped to one fight. A damage-over-time poison would need a new persistent-HP fact to
apply against between fights, which is real scope this phase didn't ask for and shouldn't invent as
a side effect of testing marker-persistence. A roll penalty tests the same thing (does a marker
survive a room-transition boundary) without touching PC HP at all. **This gap is recorded, not
solved — see Open questions.**

Assign `InflictsPoison = true` to exactly one existing roster monster (Baraka's/Juno's pick at
implementation — the Scrapper, since it's already the roster's "easy" fight and a lingering penalty
gives it a reason to matter past its low raw stats, is the suggested default).

### Engineered soft-lock (item 5) — no new mechanics, a documented sequence

No new facts or rules. The deliverable is a specific, reproducible command sequence — e.g.,
deliberately losing or timing out `fight` in the three Fight rooms (1, 5, 8) — captured as a
transcript (Nadia's format, same as any acceptance-criteria transcript) showing the session
continues silently with 7 of the 8 win lines dead. **Corrected per ticket 0075:** the eighth,
column `[3,6,9]`, contains no Fight room and can never be killed by `RoomFailed` — the transcript
should also show `[3,6,9]` remaining solvable and firing normally, confirming the achievable ceiling
is 7 of 8, not a bug in the sequence. This is evidence-gathering against `1-d20-spec.md`'s existing
open question, not a fix.

## Commands & handlers

| Command | Owner ROM | Args | Reads | Writes | Notes |
| --- | --- | --- | --- | --- | --- |
| `check` (Save branch) | D20System | — | `Room`, `SavingThrows` | `RoomSolved` | Existing command, new branch |
| `fight` (equipped-weapon branch) | D20System | — | `IEquippedWeapon` (cross-ROM, `Query<T>()`), `Poisoned` | `CombatRound`, `RoomSolved`/`RoomFailed`, `Poisoned` | Existing command, new lookups — the `IEquippedWeapon` read is cross-ROM (Loot `Provides`, D20System correlates without a declared `Requires`; see Fact vocabulary) |
| `equip` | Loot | item name | `ItemAwarded`, `EquippedWeapon` | `EquippedWeapon` | New — Loot-owned; this command's own read/write is same-ROM, unlike `fight`'s read of it |

## Content data

### Save-check pool addition (joins the existing 9-item skill pool; still 6-of-N drawn per session)

| Check | Save (ability) | DC | Bonus | Success |
| --- | --- | --- | --- | --- |
| Steady grip on a live wire (dodge/brace) | Reflex (Dex) | 10 | +0 | 55% |
| Shake off a lungful of dust and chemical fumes | Fortitude (Con) | 12 | +4 | 60% |

Two entries, matching the existing pool's density; DCs pulled from the same recalibrated
45–80%-success band `1-d20.md` establishes for the rest of the content.

### Backup Piece — mechanical profile (Loot content, extends the existing item entry)

| Field | Value |
| --- | --- |
| Attack bonus | +0 (Dex-keyed; PC's Dex mod is +0) |
| Damage | 1d6 |
| Equippable | yes |

### Poison carrier

`scrapper` (existing roster entry, `loot-rom-content-expansion-spec.md`) gains `InflictsPoison =
true`. No stat changes.

## Acceptance criteria

- [ ] The two new save-check entries and the poison-carrier flag are authored in
      `DefaultOneD20WorldModule` (`OneD20ChallengeTemplate.Kind`/`SaveType`,
      `OneD20MonsterTemplate.InflictsPoison`) and survive `Program.cs`'s translation into the
      ROM-tier `RoomChallenge`/`MonsterTemplate` facts unchanged — verified end to end, not just at
      the ROM-tier fact shape.
- [ ] `OneD20WorldModuleValidator` rejects a `SkillCheck`-type challenge template with both `Skill`
      and `SaveType` set, and one with neither set — both are load-time errors, not silently
      resolved one way.
- [ ] A Save-kind `RoomChallenge` resolves against `SavingThrows[SaveType]`, not a skill modifier —
      verified for both new save-check entries.
- [ ] `SavingThrows` is computed once at session assembly from `PlayerAbilityScores` and the fixed
      base-save table, with no new World Module field and no engine-tier change.
- [ ] `equip "Backup Piece"` changes `fight`'s PC-attack roll to 1d6/+0 instead of the default
      sword's 1d8+3/+4 — verified via a transcript comparing rolls before and after equipping.
- [ ] Equipping is rejected if the PC doesn't hold the item (no `ItemAwarded` for it yet).
- [ ] A hit from the poison-carrying monster inserts `Poisoned(3)`; a subsequent hit while already
      poisoned refreshes it to 3 rather than stacking.
- [ ] `Poisoned` applies a -2 penalty to a `check` or `fight` roll made **in a different room** than
      the one the poison was inflicted in — this is the load-bearing assertion for item 3, verify it
      explicitly, not just same-room application.
- [ ] `Poisoned` expires after 3 checks/fights and stops applying the penalty.
- [ ] The engineered soft-lock sequence is captured in a transcript showing 7 of 8 win lines dead
      and no session-level announcement or state change reacting to it — the silence itself is the
      recorded finding. **Corrected per ticket 0075** (this bullet previously said "zero reachable
      win lines," which the shipped grid topology makes mathematically unsatisfiable): the transcript
      must also confirm column `[3,6,9]` remains solvable and fires `WinMarker` normally when solved
      — the ceiling is 7 of 8, verified as a ceiling, not an unexplained shortfall.
- [ ] No existing acceptance criterion from `1-d20-spec.md` or `loot-rom-content-expansion-spec.md`
      regresses.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

None anticipated. Every item in this spec builds against fact/record shapes already established at
the ROM/Rulebook tier (`RoomChallenge`, `MonsterTemplate`, `ItemAwarded`) plus ordinary new records
at the same tier (`SavingThrows`, `Poisoned`, `EquippedWeapon`) — no `ComponentRegistry`,
`Capability`, or `EngineTier` involvement anywhere in this spec. If implementation finds otherwise,
that's a ticket to the Architect, not a quiet workaround.

## Open questions / edge cases

- **No persistent PC HP outside `CombatRound` — a real finding, deliberately not solved here.**
  Item 3's roll-penalty design sidesteps this rather than fixes it (see Mechanics). Worth a future
  proposal if a damage-over-time effect, or any other cross-room PC-HP concept, is ever wanted —
  not scoped into 2.10.
- **Save-kind DCs vs. skill-kind DCs on the same 45–80% band:** both new save checks land in-band
  against the PC's fixed save bonuses, but there's only one PC archetype to check against (same
  limitation `1-d20.md`'s original skill DCs accepted). Not a new problem, just inherited.
- **Whether `equip` should be reversible (an `unequip` back to the default sword):** not specified
  above — default assumption is `equip <default-name>` re-selects the sword the same way, but this
  wasn't stated explicitly and is Baraka's to flag if it reads ambiguous at implementation.
- **Whether other roster monsters should eventually get their own item-equip hooks** (Marksman's own
  weapon, e.g.) — explicitly out of scope; Backup Piece alone is enough to answer item 2/4's
  question.

## Related documents

- [`1-d20/1-d20-spec.md`](1-d20/1-d20-spec.md) — the base spec this extends
- [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md) — the roster/item
  content this spec builds on (`ItemAwarded`, `MonsterTemplate`, the Backup Piece/Scrapper entries)
- [`world-module-extraction-spec.md`](world-module-extraction-spec.md) — `IOneD20WorldModule`, the
  Rulebook-owned shapes this spec extends alongside their ROM-tier counterparts, and the
  `Program.cs` translation path both ends have to stay consistent through
- [`../design/1-d20.md`](../design/1-d20.md) — names Fortitude/Reflex/Will as System capabilities
  never exercised, and a ranged option as anticipated future content; both realized here
- [`../design/gm-overlay.md`](../design/gm-overlay.md) — the still-draft Phase 4 tier whose
  marker-plus-guard assumption this spec's item-3 result speaks to (see the proposal doc's flag)
- [`../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md) —
  the reviewed proposal this spec implements Phase 2.10 of
- [Ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) —
  the proposal's review/approval
- [Ticket 0075](../ClaudeWiki/Processes/Handoffs/0075-phase-2-10-spec-currency-cleanup.md) — the
  three text-drift corrections applied in this pass
- [Ticket 0071](../ClaudeWiki/Processes/Handoffs/0071-peer-rom-bidirectional-capability-cycle-detection-gap.md) /
  [Ticket 0072](../ClaudeWiki/Processes/Handoffs/0072-build-cartridge-contract-surface-shape-check.md) —
  the ruling and build behind `IEquippedWeapon`'s actual shipped shape
- [`../QA/phase-2-10-verdict.md`](../QA/phase-2-10-verdict.md) — the PASS verdict that flagged all
  three drift items this pass corrects
- [`content-spec-template.md`](../ClaudeWiki/Processes/content-spec-template.md) — the template this follows
