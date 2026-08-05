# 0074 — Phase 2.10 QA gate

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-04
**For:** Lothar (QA)

## Details

### Context

Phase 2.10 ([`phase-2-10.md`](../../../Roadmap/phase-2-10.md)) is implementation-complete:
[0069](0069-implement-phase-2-10-content-coverage-spec-md.md) built
`phase-2-10-content-coverage-spec.md` in full, with two dependent tickets closed along the way —
[0070](0070-tests-contenttests-broken-by-phase-2-10-roomchallenge-shape.md) (test suite repointed,
77/77) and [0072](0072-build-cartridge-contract-surface-shape-check.md) (the Cartridge contract-surface
shape check, and `EquippedWeapon`'s rework into `IEquippedWeapon` per
[0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)'s ruling). I verified 0069
against the spec's own acceptance list before closing it, including checking the two design-doc edits
(`grid-rom-d20-split-spec.md`, `engine-core.md`) directly on disk rather than trusting either side of
the git-diff back-and-forth in 0071/0072. Same shape as [0056](0056-phase-2-7-1-qa-gate.md),
[0063](0063-phase-2-8-qa-gate.md), [0068](0068-phase-2-9-qa-gate.md): filed now that there's something
real to check.

### Ask

Run a full `qa-verify` pass against `phase-2-10-content-coverage-spec.md`'s 11-item acceptance list and
`phase-2-10.md`'s Done-When bar. Verify against evidence — source, passing test, reproducible
session — not ticket prose:

1. **Save-kind `RoomChallenge` resolution** — `CheckKind`/`SaveType` authored in
   `DefaultOneD20WorldModule`, surviving `Program.cs`'s translation into the ROM-tier fact unchanged;
   confirm both new save-check entries resolve against `SavingThrows[SaveType]`, not a skill modifier,
   live.
2. **`OneD20WorldModuleValidator`'s two new checks** — reproduce both rejections yourself (a
   `SkillCheck` template with both `Skill` and `SaveType` set, and one with neither) as load-time
   errors, not narrative.
3. **`SavingThrows` computation** — confirm it's derived once at session assembly from
   `PlayerAbilityScores` + the fixed base-save table, no new World Module field, no
   `ComponentRegistry`/`Capability`/`EngineTier` involvement anywhere.
4. **`equip "Backup Piece"`** — live transcript showing `fight`'s PC-attack roll switches to 1d6/+0
   from the default sword's 1d8+3/+4; confirm equipping is rejected without a matching `ItemAwarded`.
5. **`IEquippedWeapon` cross-ROM shape** — confirm the built shape from 0072: interface in
   `LunaFlow.Cartridge`, Loot's concrete type `Provides`s it, `D20SystemRom.Requires` deliberately
   does *not* declare it (0071's ruling), and the Cartridge contract-surface shape check
   (`ValidateInterfacesOnly`) actually runs in `Program.cs`'s bootstrap barrier — don't just take 0072's
   positive/negative demonstration as sufficient on its own, reproduce at least the negative case
   (a bare non-interface type in `LunaFlow.Cartridge`) yourself.
6. **`Poisoned` marker** — confirm insertion on the poison-carrying monster's hit, refresh-not-stack on
   a repeat hit, and — this is the load-bearing assertion — the -2 penalty applying to a `check`/`fight`
   roll made **in a different room** than the one the poison was inflicted in, not just same-room.
   Confirm expiry after 3 checks/fights.
7. **Engineered soft-lock, reproduced live** — 0069's transcript claims failing all three Fight rooms
   (1, 5, 8) is the maximum achievable (7 of 8 win lines dead, `[3,6,9]` permanently reachable) because
   the shipped topology makes true zero-reachable-lines mathematically impossible. Confirm the topology
   claim yourself (no Fight room on column `[3,6,9]`, `SkillCheck` rooms never produce `RoomFailed`) and
   reproduce the sequence live — don't take the captured transcript as sufficient on its own.
8. **Design-doc currency** — confirm `grid-rom-d20-split-spec.md`'s capability table and
   `engine-core.md`'s Cross-ROM correlation section actually carry the 0071 edits on disk (I checked
   this directly closing 0069; re-verify independently, this is exactly the kind of claim that got
   disputed once already in 0071/0072's back-and-forth).
9. **No existing acceptance criterion regressed** — spot-check against `1-d20-spec.md` and
   `loot-rom-content-expansion-spec.md`'s own criteria, not just "tests still pass."
10. **Regression green from an Arryn-run `latest.trx`.** 0069's addendum claims 170/170
    (77 ContentTests + 93 EngineTests) from an Arryn-run trx already — confirm that run is what you're
    reading, per the standing hard rule, not an agent-run number relabeled.
11. **Verdict + evidence to the PM**, written to `Documents/QA/phase-2-10-verdict.md`. A fail routes to
    Baraka (defect) or Amara (spec-vs-reality) — not a fix by you.

### Response

**Verdict: PASS.** Full report: [`Documents/QA/phase-2-10-verdict.md`](../../../QA/phase-2-10-verdict.md).

All 11 asks checked against source and live reproduction, not ticket prose. Build green, 170/170
(merged trx, 93 EngineTests + 77 ContentTests, Arryn-run — directly confirmed in conversation and
corroborated by the trx's own timestamp/counts). The automated suite itself has **zero** coverage of
this phase's own new mechanisms — verified by listing every test name in the trx directly, not
inferring from the green count — so items 1–7 were independently reproduced live via a throwaway
seeded-RNG MSTest harness (reusing `D20SystemHandlerTests.cs`'s fixture pattern with two custom
deterministic `Random` subclasses) and, for item 5's negative case, an actual fault injection into
`LunaFlow.Cartridge` (a bare non-interface record, rebuilt, ran the server, watched it reject
correctly) — both deleted after use, clean rebuild confirmed before writing the verdict.

Two non-blocking flags, both spec-wording/documentation-currency, neither a build defect: (a) the
spec's own soft-lock criterion says "zero reachable win lines," which is mathematically impossible
given the grid topology (confirmed from `WinLineCatalog.cs`) — this ticket's own item 7 had already
caught and reframed this correctly before I started, the spec text just hasn't been corrected to
match; (b) `loot-rom-content-expansion-spec.md`'s `Provides: []` line for `LootRom` is now stale
(Loot legitimately gained `EquippedWeapon` this phase) — same shape as the GridMap/`ChallengeSolved`
drift 0071 already fixed in the other spec, this one hasn't been touched yet. Full detail in the
verdict's Deviations section. No handoff ticket filed — both are pointer-worthy spec corrections, not
defects, and I don't fix docs or code myself; routing the call on whether either is worth its own
small ticket back to you.

### Resolution

**Resolved 2026-08-04 (Kofi, opener).** Read [`phase-2-10-verdict.md`](../../../QA/phase-2-10-verdict.md)
directly, not this ticket's Response summary: all 11 spec criteria and both 0074-specific asks (the
`IEquippedWeapon` shape/negative case, design-doc currency) show PASS with evidence attached —
source citations plus live reproduction for every behavioral claim, since Lothar's own Method section
notes the automated suite has zero coverage of this phase's actual new mechanisms and everything had
to be independently reproduced live rather than read off a green trx. Fault injection used for both
negative cases (validator rejections, the Cartridge shape-check), scratch test files deleted and a
clean rebuild confirmed before the verdict was written. No FAIL, no UNPROVEN criterion, no tickets
filed by Lothar.

**Three non-blocking spec-currency deviations noted, none of them build defects:** the soft-lock
criterion's literal "zero reachable win lines" wording (mathematically unsatisfiable, already correctly
reframed in this ticket's own item 7 before QA started), `loot-rom-content-expansion-spec.md`'s stale
`Provides: []` line for `LootRom` (now legitimately `[EquippedWeapon]`), and
`phase-2-10-content-coverage-spec.md`'s own Scope/Out section contradicting its Commands/Content-data
sections on whether Loot was touched. Routed onward as [0075](0075-phase-2-10-spec-currency-cleanup.md)
to Juno rather than left as verdict-file trivia — same discipline 0071's GridMap/`ChallengeSolved`
drift got before it rotted into a second unflagged instance.

**Phase 2.10 marked done** in `roadmap.md`/`phase-2-10.md`, citing this ticket and the verdict.
