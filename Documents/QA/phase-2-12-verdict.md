# Phase 2.12 (World Module Authoring Surface Stress Test — Lua Path) — QA Verdict

**Verdict:** PASS
**Date:** 2026-08-09
**Ticket:** [0086](../ClaudeWiki/Processes/Handoffs/0086-phase-2-12-qa-gate.md)
**Build:** `dotnet build LunaFlow_A.sln` — Build succeeded, 0 Warning(s), 0 Error(s).
**Suite:** 200/200 (93 EngineTests + 107 ContentTests, arithmetic independently confirmed), read from a single merged `tests/LunaFlow.EngineTests/TestResults/latest.trx`. `runUser` reads `ARRYNSTATION\amkey`; timestamp `2026-08-09T01:47:37` — today, matching the ticket's cited run. Same merged-trx shape as 2.9/2.10/2.11.

## Method

This phase shipped with a checked-in verification spike (`example/Phase2_12Spike`), built specifically
because ticket 0084's context notes it caught two real bugs before they'd have shipped as false
confidence (a missing `move` between rooms, and a validator probe passing for the wrong reason). Per
the qa-verify discipline, a checked-in artifact making a claim isn't evidence until re-run — so:

1. **Independently ran the spike myself** (`dotnet run --project example/Phase2_12Spike`) rather than
   trusting its checked-in existence or 0084's pasted output. All 47 of its `[PASS]`/`[FAIL]` checks
   came back `PASS`, exit code 0. The spike drives the real `GameEngineCore` singleton through
   scripted (not random) dice via a `Random` subclass identical in spirit to the harness I built by
   hand for the 2.10 pass — same technique, already checked in this time.
2. **Independently recomputed item 10's roster arithmetic by hand** from the spec's own Lua source
   (`defense = 11 + i`, `maxHp = 6 + i*2`, `attackBonus = 2` constant, `i = 1..5`) before looking at
   the spike's hard-coded expected values, then compared both against the live output — all three
   agree for all five monsters.
3. **Read `FightCommandHandler.cs`'s actual counterattack branch directly** (ticket 0086 item 5's
   explicit "don't just trust the test's inline comment" ask) — confirmed the monster's counterattack
   check (`monsterRoll + monster.AttackBonus >= pc.Defense`, line 118) references neither
   `effectiveDefense` nor `monster.Defense` anywhere; `effectiveDefense` (lines 78–80) is used exactly
   once, gating the **PC's** attack (line 84). This isn't inferable from the test; it's a structural
   fact about which variable each branch reads.
4. **Read `EnrageEffect.RetractIfPresent`'s call sites directly** — identical call at all three of
   `FightCommandHandler.cs`'s terminal branches (defeat/loss/timeout) and `FleeCommandHandler.cs`'s
   only exit. Live-demonstrated for win/flee/retry by the spike; loss and timeout are demonstrated
   structurally (identical shared-helper call) and the timeout path's safe-no-op case is live-tested,
   but neither loss-while-enraged nor timeout-while-enraged was exercised live with an actually-enraged
   monster — see Deviations.
5. **Read the full field-carry chain's source at every hop**: `LuaOneD20WorldModule.cs`'s marshaling
   (`OptionalNumber(t, "enrageThresholdFraction"/"enrageDefenseBonus")`, defaulting null),
   `Program.cs`'s translation (carries both fields into the ROM-tier `MonsterTemplate` unchanged), and
   `FightCommandHandler.cs`'s read — not just the spike's black-box marshal assertion.
6. **Confirmed `IOneD20WorldModule.cs` and `OneD20SessionAssembler.cs` have zero `Enrage`-related text**
   (grepped directly), and that `OneD20WorldModuleValidator.cs`'s only Enrage-related addition is the
   pairing/range check item 11 itself specifies — nothing item 9's "Signal Static" content needed to
   add on top.
7. **Confirmed via trx per-class test counts** that every pre-2.12 test class kept its exact test
   count from the 2.11 baseline; the only changes are pure additions (`SignalStaticEnrageHandlerTests`,
   new; `LuaOneD20WorldModuleTests` +5; `OneD20WorldModuleValidatorTests` +5) — 181 + 9 + 5 + 5 = 200.

## Criteria (`phase-2-12-lua-worldmodule-stress-test-spec.md`'s Acceptance criteria list)

| Criterion | Evidence | Result |
| --- | --- | --- |
| Item 9: "Signal Static" full session — combat-free win, combat-touching win, `RoomFailed` | Live (independently re-run): spike's Scenario A (column-3 combat-free `WinMarker`, no `RoomFailed`), Scenario B (row-1 combat-touching `WinMarker`), Scenario C (room 8 `RoomFailed` via 3-round timeout). | PASS |
| Item 9: zero changes to `IOneD20WorldModule`/`OneD20WorldModuleValidator`/`OneD20SessionAssembler` beyond item 11's own | `IOneD20WorldModule.cs`/`OneD20SessionAssembler.cs`: zero `Enrage` references. `OneD20WorldModuleValidator.cs`'s only addition is item 11's own pairing/range check — nothing further for Signal Static specifically. | PASS |
| Item 9: Save-kind challenge + `InflictsPoison` monster round-trip, actual field values | Live: `signal-arc` (Save/Reflex), `signal-haze` (Save/Fortitude), `signal-monster-5` (`InflictsPoison=true`, the only one) — all asserted against actual constructed values, not crash-absence. | PASS |
| Item 10: procedural roster stats match hand-computed values exactly | Independently recomputed by hand from the spec's stated formula before looking at the spike's own numbers: Defense 12/13/14/15/16, MaxHp 8/10/12/14/16, AttackBonus 2 (constant) for monsters 1–5. Live output matches exactly. | PASS |
| Item 11: Defense increases exactly once at threshold, not before | Live: `MonsterEnraged present after round 1` (HP crosses 4, the threshold), guarded `!wasEnraged` read from source (`FightCommandHandler.cs:104`) confirming it can never re-fire once set. | PASS |
| Item 11: purely defensive — monster's own attack roll unaffected, byte-identical before/after for identical rolls | Source: the counterattack branch (line 118) reads only `monster.AttackBonus`/`pc.Defense`, never `effectiveDefense`/`monster.Defense` — structurally incapable of being affected, not merely untested. Live: identical monster-attack rolls produce identical outcomes before/after Enrage across the spike's Scenario B. Directly re-confirms ticket 0083's ruling against source myself, not the ticket's prose. | PASS |
| Item 11: retraction on all four `CombatRound` exits, retry-after-flee | Source: identical `EnrageEffect.RetractIfPresent` call at all 4 exit points. Live: win, flee, and retry-after-flee (identical roll that missed while enraged hits again at baseline) all demonstrated. Loss and timeout demonstrated structurally, not with a live enraged instance — see Deviations. | PASS (partial live coverage noted) |
| Item 11: every existing monster keeps working with both fields absent | 200/200 includes all of Phase 2.7.1's/Hollow Keep's pre-existing fight-room tests, unchanged pass counts (`D20SystemHandlerTests`: 16, same as 2.11). | PASS |
| Item 11: full field-carry chain, Lua source → ROM-tier fact | Source at every hop: `LuaOneD20WorldModule.cs` (marshal) → `Program.cs` (translation) → `FightCommandHandler.cs` (read). Live: spike's marshal-value assertions plus the full Scenario B fight. | PASS |
| Item 11: validator rejects one-field-set and out-of-range, accepts inclusive upper bound | Source: `OneD20WorldModuleValidator.cs`'s `hasThreshold != hasBonus` and `<= 0 \|\| > 1` checks. Live: all 5 spike cases (threshold-only, bonus-only, >1, ==0, ==1-accepted) reproduced. | PASS |
| Item 12: marshal-failure probe throws before the validator is ever reached | Live: spike's `validatorReached` flag (settable only past a successful constructor return) stays `false`; message names the script, entry, and field. | PASS |
| Item 12: validator-failure probe constructs cleanly, fails at the pool-size check specifically | Live: spike constructs successfully (marshal-clean), then `OneD20WorldModuleValidator` rejects it naming "Challenges"/"4". | PASS |
| No existing criterion from `lua-world-module-spec.md`/`1-d20-spec.md`/`loot-rom-content-expansion-spec.md`/`phase-2-10-content-coverage-spec.md` regressed | Per-class trx counts: every pre-2.12 class's count is identical to the 2.11 baseline; only pure additions. | PASS |
| Full suite green, Arryn-run `latest.trx` | See Suite line — `runUser`, timestamp, 93+107 arithmetic all confirmed directly. | PASS |

## Item 11's Phase-4 finding (stated explicitly, per the ticket's own instruction)

**Enrage is a clean data point for one half of `gm-overlay.md`'s question, but doesn't touch the
other half — worth stating plainly before Phase 4's kickoff reads this ticket as a green light for
more than it proved.**

What item 11 actually demonstrates: extending a Rulebook-owned content shape (`OneD20MonsterTemplate`)
with a conditional, threshold-triggered stat modifier is cheap, clean, and needs zero
`ComponentRegistry`/`Capability`/`EngineTier` involvement — a generic rule reading two nullable fields,
same shape `InflictsPoison` already proved in Phase 2.10. That's genuinely useful evidence for
`gm-overlay.md`'s "effect fact" idea.

What it does **not** demonstrate: Enrage is *authored ahead of time*, in content, by whoever writes the
World Module — the threshold and bonus are fixed at Lua-authoring time and interpreted by a rule
hand-written for exactly this one field pair. `gm-overlay.md`'s `MonsterBehaviorOverride` idea, as
described, is reaching for a **live GM improvising a stat change mid-session** — a fundamentally
different shape (runtime-composed, not authored-ahead, and not backed by a bespoke rule per
concept). Item 11 proves the contract-extension side of that question is cheap; it says nothing about
whether an arbitrary, runtime-declared "effect fact" the engine interprets generically (rather than one
rule per concept) is buildable at all. The spec's own Open Questions section already flags this
scoping choice as deliberate, not an oversight — but "deliberate" and "answers Phase 4's actual
question" are different claims, and only the first one is true here. Whoever runs the Phase 4 kickoff
pass should read Enrage as "extending the monster shape is cheap," not as "the runtime-composable
effect mechanism is proven."

## Deviations from spec

- **Loss-while-enraged and timeout-while-enraged retraction were verified structurally, not with a
  live enraged instance.** The spike's Scenario C (timeout) uses a plain, non-Enrage monster to prove
  the retraction call is a safe no-op when nothing was enraged; no scenario drives a monster to
  *timeout* or *PC-loss* while genuinely enraged. The source-level proof (identical
  `EnrageEffect.RetractIfPresent(rules, roomId)` call at all four `CombatRound`-ending branches, same
  trivial safe-conditional-retract helper) is strong — but it's one level more inferential than the
  win/flee cases, which were both actually driven through an enraged state live. Not blocking: the
  helper's logic is trivial enough that "identical call, four call sites" is solid evidence on its own,
  but flagging the asymmetry rather than letting all four exits read as equally live-proven.

## Superseded reports

None — first verdict for Phase 2.12.

## Tickets filed

None. No FAIL or UNPROVEN criterion. Item 11's Phase-4 finding is stated above per the ticket's ask —
informational scoping note for whoever runs that kickoff pass, not a defect and not a ticket.
