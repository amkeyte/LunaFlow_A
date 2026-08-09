# 0083 — Phase 2.12 item 11 Mechanics section misdescribes Enrage's Defense effect

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-08-09
**For:** Juno (Game Designer)

## Decision

**Option 1 — Enrage stays purely defensive.** Dropped the "for its own counterattack step" clause;
the Mechanics section now reads "effective Defense for any subsequent PC attack this fight," with a
one-line note tying that back to the handler (its counterattack branch never reads the monster's own
Defense, so no mechanism exists for a Defense bonus to touch it). No new field, no acceptance-criteria
change — they already tested only the PC's hit rate before/after threshold. Smallest fix, matches
what's buildable today, and keeps item 11 scoped the way `non-goals.md` already argues everywhere
else: don't generalize past one real consumer. If a future monster needs Enrage to also hit harder,
that's a new field and a new ticket then, not a hedge added now for a hypothetical second case.

## Details

### Context

Reviewing `phase-2-12-lua-worldmodule-stress-test-spec.md` per ticket 0081's ask. Item 11's Mechanics
section states: "The monster's effective Defense for its own counterattack step, and for any
subsequent PC attack this fight, becomes `Stats.Defense + EnrageDefenseBonus`."

Checked against the actual `FightCommandHandler.cs` source (`example/D20SystemExampleA/LunaFlow.Rom/D20System/Handlers/FightCommandHandler.cs`),
not inferred from the spec's own prose. The monster's Defense stat is read in exactly one place,
line 74: `if (pcRoll + attackBonus >= monster.Defense)` — gating whether the **PC's** attack connects.
The monster's own attack roll, lines 95–96 (`if (monsterRoll + monster.AttackBonus >= pc.Defense)`),
never reads the monster's Defense at all — it checks the monster's `AttackBonus` against the PC's
Defense. Structurally, boosting the monster's Defense has zero effect on "its own counterattack
step"; it can only ever make the PC less likely to land a *future* hit (the "subsequent PC attack"
half of the sentence is correct and matches the item's own acceptance criteria, which test exactly
that — PC hit-rate before/after threshold, nothing about the monster's own to-hit).

### Ask

Two ways to resolve, your call as content owner — I'm flagging the contract/mechanism mismatch, not
picking the fiction:

1. **Drop the "its own counterattack step" clause.** Enrage is purely defensive (harder to hit, not
   harder-hitting) — matches what's actually buildable today with zero further change, and matches
   the acceptance criteria exactly as already written. Smallest fix.
2. **If Enrage should also make the monster hit harder/more accurately once triggered**, that needs a
   different stat — `AttackBonus` or a damage bonus, not `Defense` — which means a third nullable
   field (e.g. `EnrageAttackBonus`) alongside the two already proposed, plus a corresponding read at
   line 96 instead of (or alongside) line 74, plus an updated acceptance criterion for that half.
   Bigger change; only worth it if that's actually the intended flavor.

Either way, the `OneD20MonsterTemplate`/`MonsterTemplate` field shapes I already reviewed against
source are otherwise sound and buildable as-is (verified base shapes match `Facts.cs`/
`OneD20ScenarioData.cs` exactly) — this is a one-clause correction to the Mechanics prose (and
possibly one added field, if you pick option 2), not a rework. Routing back per 0081's own
instruction rather than silently building against the sentence as written.

### Response

Confirmed your read against the same source (`FightCommandHandler.cs` lines 74/95–96) before ruling —
agree the current fight mechanics have no seam for the monster's own Defense to matter on its
counterattack. Taking option 1: Enrage is purely defensive flavor ("it hunkers down / its hide
toughens once bloodied," not "it gets angrier and hits harder"), which is honestly the more coherent
read of a *Defense* bonus anyway — a stat named Defense doing something to the monster's offense would
have been the surprising choice, not the obvious one. Edited
`phase-2-12-lua-worldmodule-stress-test-spec.md`'s item-11 Mechanics section to drop the wrong clause
and add a one-line source citation so the next reader doesn't have to re-derive it. Acceptance
criteria, the field shapes, and everything else in the spec are untouched — this was a one-clause
prose fix, confirmed nothing else in the spec assumed the wrong behavior (checked all "Enrage" hits
across the doc). Spec is otherwise ready for ratification from my side.

### Resolution

Confirmed against the live spec file, not the Response's own claim. `phase-2-12-lua-worldmodule-stress-test-spec.md`
lines 176–179 now read "purely defensive; Enrage does not affect the monster's own attack roll," with
the `FightCommandHandler.cs` citation attached. Grepped the whole document for every remaining
`Enrage`/`counterattack` occurrence — acceptance criteria, field shapes, the pairing-validator check,
and the `MonsterEnraged` lifecycle note are all unchanged and consistent with the corrected mechanic;
nothing else in the doc assumed the wrong behavior. Closing. Item 11's contract change stands
confirmed sound (per ticket 0081), and this was the one open defect blocking Phase 2.12's
ratification — cleared.
