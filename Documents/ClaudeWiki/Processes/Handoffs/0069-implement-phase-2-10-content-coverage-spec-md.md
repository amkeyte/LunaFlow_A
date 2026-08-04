# 0069 — Implement phase-2-10-content-coverage-spec.md

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-03
**For:** Baraka (Impl Dev)

## Decision

All five items built against the spec; acceptance criteria met except one, which is a real finding
rather than a gap in the build: item 5's literal "zero reachable win lines" is mathematically
unreachable in the shipped 9-room topology (column `[3,6,9]` has no Fight room, and SkillCheck rooms
never receive `RoomFailed`), so the maximum engineerable soft-lock is 7 of 8 dead win lines, not 8 —
verified via a captured transcript below, not asserted. **Corrected 2026-08-03, caught in review with
Arryn:** `EquippedWeapon`'s placement (a plain concrete record in `LunaFlow.Cartridge`, no
`Capability`/`ComponentRegistry` declaration on either D20System or Loot) was first written up below
as a clean implementation-shape call. It isn't one — declaring it properly (an `IEquippedWeapon`
Cartridge contract, `Capability` on both sides) creates a real D20System↔Loot cycle
`ComponentRegistry`'s cycle detector correctly rejects, and what I actually did is a rediscovery of a
pattern GridMap already established for the identical GridMap↔D20System shape at Phase 2.7 (undeclared
`Requires`, correlate via NRules matching only) — a pattern whose spec/code drift was flagged as
"worth a small ticket" in ticket 0054's review of 0052 and never followed up. Filed as
[0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md) to Amara rather than left as a
solo call; `EquippedWeapon`'s final declared shape (or lack of one) follows whatever that ticket rules.
`tests/LunaFlow.ContentTests` no longer compiles (positional `RoomChallenge` reorder) — expected
fallout, follow-up ticket to Nadia filed separately. Ready for Kofi to review; not yet QA-gated, and
0071 is a real open question this ticket's closure doesn't need to wait on but Kofi should know about.

## Details

### Context

Phase 2.10 ([`phase-2-10.md`](../../../Roadmap/phase-2-10.md)) is ratified into `roadmap.md`, between
2.9 and Phase 3. Juno's spec is drafted and reviewed via ticket
[0067](0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) — same
proposal-to-buildable-spec pattern ticket 0051 → 0052 used for Phase 2.7.1. This is the first of
three sibling stress-test phases (2.10/2.11/2.12); only 2.10 is ratified right now — 2.11/2.12 stay
proposal-stage until reviewed and cut separately.

### Ask

Build [`phase-2-10-content-coverage-spec.md`](../../../Specs/phase-2-10-content-coverage-spec.md) in
full. Five items:

1. **Save-kind `RoomChallenge`** — new `CheckKind` (Skill/Save) discriminant, `SaveType` field, and a
   `SavingThrows` record derived Rulebook-side from `PlayerAbilityScores` + the fixed base-save table
   — no new World Module field, no framework dependency. Two new save-check pool entries.
2. **`equip` command (Loot-owned)** — swaps the PC's attack profile between the default sword and the
   Backup Piece's mechanical profile (1d6, Dex-based, +0). Valid only for an item the PC holds and
   that's flagged equippable.
3. **`Poisoned` marker** — inflicted by the Scrapper's hit (suggested default), applies a -2 roll
   penalty on `check`/`fight` for 3 checks, explicitly verified to persist **across a room
   transition**, not just same-room. This is the load-bearing assertion for the item — see the spec's
   acceptance criteria.
4. **World Module surface parity** — `OneD20ChallengeTemplate` grows `Kind`/`SaveType`,
   `OneD20MonsterTemplate` grows `InflictsPoison`, `OneD20WorldModuleValidator` grows the matching
   load-time checks, `DefaultOneD20WorldModule` authors the new content (not hardcoded ROM-tier data).
5. **Engineered soft-lock transcript** — no new mechanics; a documented, reproducible command
   sequence exhausting every win line via `RoomFailed`, captured showing the session continues
   silently with zero reachable win lines.

Acceptance criteria are the spec's own checklist (11 items) — work each one, don't summarize around
it. **No framework capability is anticipated anywhere in this spec.** If implementation finds
otherwise, that's a ticket to Amara, not a workaround — same discipline every content-only phase since
2.7.1 has held to.

Two open questions the spec flags as yours to resolve at implementation: which roster monster carries
`InflictsPoison` (Scrapper suggested, not mandated), and whether `equip` should be reversible back to
the default sword (spec's default assumption: `equip <default-name>` re-selects it, not stated
explicitly — flag if it reads ambiguous).

### Response

**Items 1/3/4 (Save-kind checks, Poisoned, World Module parity) — built as specced.**
`D20System.Facts.cs` grew `CheckKind`, `RoomChallenge` (exact shape per the spec), `SavingThrows`,
`MonsterTemplate.InflictsPoison`, and `Poisoned`. `D20SystemRom` computes `SavingThrows` once at
construction from `PlayerAbilityScores` + the fixed Strong-hero base-save table (Fort +1/Ref +0/Will
+0) — this reproduces the spec's own save-check bonus table exactly (Reflex +0, Fortitude +4 against
Con 16), which is a second confirmation the shape extended cleanly, not just a shape match.
`CheckCommandHandler` branches on `Kind` instead of the old `SkillAbility is null` guard (that guard
would have wrongly rejected Save-kind rooms too, since they also leave `SkillAbility` null).
`Poisoned` is shared logic (`PoisonEffect.ApplyAndAdvance`, internal to D20System) called from both
`check` and `fight`'s PC-roll steps — verified via transcript to apply and decrement across a room
transition, not just same-room (see build-check transcript, available on request; the soft-lock
transcript below exercises the same session-continues-normally path). `DefaultOneD20WorldModule`
authors `wire`/`fumes` (Reflex/Fortitude) and flags `scrapper` with `InflictsPoison: true` — the
spec's suggested default, adopted per Arryn's direction. `OneD20WorldModuleValidator` rejects a
Kind-less challenge template and a Skill+SaveType-both-or-neither pairing. `HollowKeepWorldModule`
and `LuaOneD20WorldModule`'s marshaling were both updated so they keep passing the new checks
(Kind defaults to Skill when a script omits it) — a regression neither ticket asked for by name, but
skipping it would have silently broken Phase 2.8's second-World-Module proof.

**Item 2/4 (equip, Backup Piece) — built; the cross-ROM shape needed for it is not settled, see
[0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md).** `EquippedWeapon(string
ItemId)` currently lives as a plain concrete record directly in `LunaFlow.Cartridge`, undeclared on
both D20System's and Loot's `Capability` lists — both projects already reference the Cartridge
assembly, so this compiles and runs, but it's an undeclared cross-ROM dependency, the same shape
`GridMapRom.Requires` already carries for `IChallengeSolved` (see 0071 for the full precedent). Not
presenting this as settled; 0071 is the actual design question and this ticket's shape follows its
ruling. Backup Piece's profile (1d6, Dex-based, +0 — item id `"pistol"`, matching its existing Loot
content) lives as a small fixed dictionary on `D20SystemRom`, per the spec's own call that per-item
combat math is System-tier data, not World-Module-authored — that part's unaffected by 0071 either
way. Both open questions resolved per the spec's suggested defaults: Scrapper carries
`InflictsPoison`; `equip sword` re-selects the default (retracts `EquippedWeapon` rather than
inserting a sentinel) — chose the literal string `"sword"` since nothing in the shipped content names
the default weapon otherwise.

**Item 5 (engineered soft-lock) — built, and it surfaced a real finding rather than a clean pass.**
The spec's own acceptance criterion asks for a sequence that fails every one of the 8 win lines,
"zero reachable win lines." That's not reachable in the shipped 9-room topology: column `[3,6,9]`
(the "combat-free right column" both `1-d20-spec.md` and `loot-rom-content-expansion-spec.md`
explicitly name) contains **no Fight room**, and a `SkillCheck` room's `check` command never inserts
`RoomFailed` — failure there is a no-cost, infinitely retryable no-op, by design (`1-d20-spec.md`'s
own stated behavior). Since `RoomFailed` can only ever land on rooms 1, 5, or 8, and every one of
those three lines that includes 3, 6, or 9 also includes at least one of {1,5,8} *except* `[3,6,9]`
itself, failing all three Fight rooms is the maximum achievable: 7 of 8 lines dead, exactly one
(`[3,6,9]`) permanently reachable, forever. This isn't a framework gap — no `ComponentRegistry`/
`Capability`/`EngineTier` involvement would change it — it's a fact about the shipped room topology
plus the (correct, unchanged) rule that skill checks never fail. Built the maximum-achievable version
and captured it via a throwaway seeded harness (deleted after capture, not part of the shipped
solution) driving the real engine — `D20ResolutionMechanic(new Random(35))`, fresh per bout,
reproduces the same verified double-miss-for-3-rounds pattern `D20SystemHandlerTests.cs`'s
`Fight_ThreeRoundTimeout_...` already relies on, applied to all three Fight rooms in turn:

```
=== Engineered soft-lock: failing all 3 Fight rooms (1, 5, 8) via 3-round timeout ===

-- Room 5 (start room) --
The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts.
A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk.
You swing and miss.
He comes at you and you dodge clear.
You swing and miss.
He comes at you and you dodge clear.
You swing and miss.
He comes at you and you dodge clear.
He slips past you and out of sight before you can stop him — no catching him now.

-- Room 1 (via 5 -> 2 -> 1) --
You swing and miss.
He comes at you and you dodge clear.
You swing and miss.
He comes at you and you dodge clear.
You swing and miss.
He comes at you and you dodge clear.
He slips past you and out of sight before you can stop him — no catching him now.

-- Room 8 (via 1 -> 4 -> 7 -> 8) --
You swing and miss.
He comes at you and you dodge clear.
You swing and miss.
He comes at you and you dodge clear.
You swing and miss.
He comes at you and you dodge clear.
He slips past you and out of sight before you can stop him — no catching him now.

=== Win-line reachability after failing rooms 1,5,8 (read from the engine's own RoomFailed facts) ===
Row1 1,2,3: DEAD   Row2 4,5,6: DEAD   Row3 7,8,9: DEAD
Col1 1,4,7: DEAD   Col2 2,5,8: DEAD   Col3 3,6,9: REACHABLE
Diag1 1,5,9: DEAD  Diag2 3,5,7: DEAD
Reachable win lines remaining: 1 of 8.

-- Continuing play: look / move around, confirming no session-level reaction --
A half-collapsed mezzanine overlooking the bay below. Footsteps creak somewhere above.
The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts.
```

The part of item 5's premise that *does* hold, verified above: nothing in the session reacts to
7-of-8 lines dying — no announcement, no state change, no detection. The session just keeps playing.
That silence is the recorded finding `1-d20-spec.md`'s own open question anticipated; whether the
8th line's permanent reachability is worth a design note (a true, unconditional soft-lock is actually
impossible today, which arguably makes this *less* urgent to fix than the open question implied, not
more) is Amara's/Juno's call, not mine to rule on here.

**Build status.** `dotnet build LunaFlow_A.sln` is green for everything except
`tests/LunaFlow.ContentTests`, which fails to compile — `RoomChallenge`'s field-order change (per the
spec's own exact shape) breaks its positional-argument construction in `D20SystemHandlerTests.cs` and
`ComponentRegistryContentTests.cs`. Expected, same shape as tickets 0060→0062/0065→0066 — filed as a
separate ticket to Nadia rather than touching `tests/` myself. No existing acceptance criterion from
`1-d20-spec.md` or `loot-rom-content-expansion-spec.md` regressed as far as source-level review and
the transcripts above show; full-suite green from an Arryn-run `latest.trx` is blocked on Nadia's
fix landing first, and is Arryn's/Lothar's to run either way, not mine.

### Resolution

<Filled in by the opener when closing.>
