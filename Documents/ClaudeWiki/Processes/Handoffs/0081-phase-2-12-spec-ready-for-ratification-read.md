# 0081 — Phase 2.12 spec ready for ratification read

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-08
**For:** Amara (Architect)

## Decision

Item 11 does force a Rulebook-tier contract change (extends `OneD20MonsterTemplate`/`IOneD20WorldModule`'s
data shape, mirrored onto ROM-tier `MonsterTemplate`) — already pre-authorized on ticket 0067, and the
shape is sound: nullable-pair-plus-validator-check, verified against the real current record shapes in
`Facts.cs`/`OneD20ScenarioData.cs`, doesn't touch `ComponentRegistry`/`Capability`/`EngineTier`, and
doesn't trigger the `IWorldModule` generalization deferral. **Found one real defect, now fixed:** verified against `FightCommandHandler.cs` source, not spec
prose — item 11's Mechanics section claimed Enrage boosted the monster's Defense "for its own
counterattack step," but the monster's Defense is never read in its own attack branch (only gates
whether the PC's attack connects). Routed to Juno as ticket [0083](0083-phase-2-12-item-11-mechanics-section-misdescribes-enrage-s-defense-effect.md) —
**resolved**: she ruled Enrage stays purely defensive, dropped the wrong clause, added a source
citation. Verified against the live spec file before closing 0083; nothing else in the doc assumed
the old behavior. Items 9/10/12 are clean, framework-light, buildable as written. **No remaining
blocker — spec is ready for `phase-2-12.md`/roadmap ratification.**

## Details

### Context

Phase 2.11 closed QA PASS today (ticket 0080). The 2.10–2.12 proposal
([`Proposals/2-10-through-2-12-content-framework-stress-test.md`](../../../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md))
is now 2/3 done — 2.10 and 2.11 both shipped and ratified into real `phase-N.md` docs. 2.12 (Lua
World Module authoring-surface stress test) is the one piece left: Juno's already drafted the
buildable spec ([`phase-2-12-lua-worldmodule-stress-test-spec.md`](../../../Specs/phase-2-12-lua-worldmodule-stress-test-spec.md)),
but its own status line still reads "in review... awaiting Amara's read" — that read never happened
while 2.11 was live. Ratifying 2.12 into the roadmap sequence (a new `phase-2-12.md` doc, a row in
`roadmap.md`) is named in the proposal doc itself as "Kofi/Amara's call," not Juno's — she's already
delivered her half.

### Ask

Read `phase-2-12-lua-worldmodule-stress-test-spec.md` and rule on the one open question flagged
against your own name on ticket 0067: **item 11** ("content that outgrows `IOneD20WorldModule`'s
fixed shape") — does the spec's approach force a Rulebook-tier contract change, and if so, is the
shape it proposes sound? Ticket 0067 already ruled this is a real sequencing dependency for Phase 4's
GM-Overlay "effect fact" draft, not just adjacent work — whoever runs Phase 4's kickoff pass needs
item 11's result first, so this can't be left implicit going into that kickoff.

Beyond item 11 specifically: a general read for whether the spec is buildable as written (same bar
you gave 2.11's spec). If it's clean, say so and I'll cut `phase-2-12.md` and ratify it into
`roadmap.md`'s sequence, same as 2.10/2.11. If it needs a correction, route it back to Juno the way
0071/0072 did for 2.10.

### Response

**Item 11 — contract change, and it's sound.** Extends `OneD20MonsterTemplate` (Rulebook-owned) with
two nullable fields (`EnrageThresholdFraction`, `EnrageDefenseBonus`), mirrored onto ROM-tier
`MonsterTemplate`. Checked both current record shapes directly — `example/D20SystemExampleA/LunaFlow.Rom/D20System/Facts.cs:73`
and `src/LunaFlow.Rulebook/OneD20/OneD20ScenarioData.cs:52` — the spec's stated base shapes (`Id,
Name, DescriptionKey, Stats, InflictsPoison = false`) match exactly, and both new fields are nullable,
default `null`, trailing after the existing default param — syntactically clean, no migration for the
nine existing monsters. Same shape discipline `InflictsPoison` (Phase 2.10) already used. Framework
dependencies section is accurate: no `ComponentRegistry`/`Capability`/`EngineTier` involvement, no
ticket to me needed for the mechanism itself. Scoping is precise, per my own 0067 flag —
`IOneD20WorldModule`'s shape changes; the separate `IWorldModule` generalization deferral in
`non-goals.md` is correctly not triggered.

**One real defect, found by checking the handler, not the spec's own prose.** Item 11's Mechanics
section says Enrage boosts the monster's effective Defense "for its own counterattack step." Read
`FightCommandHandler.cs` directly: `monster.Defense` is checked exactly once, line 74
(`pcRoll + attackBonus >= monster.Defense`), gating the **PC's** attack. The monster's own attack
roll, lines 95–96, checks `monster.AttackBonus` against `pc.Defense` — never the monster's own
Defense. So "for its own counterattack step" describes an effect the current fight mechanics
structurally cannot produce; boosting the monster's Defense only ever suppresses future PC hits,
which is exactly — and only — what item 11's acceptance criteria already test (a hit-rate
transcript, nothing about the monster's own to-hit). The acceptance criteria are fine; the Mechanics
prose has one wrong clause. Filed as ticket [0083](0083-phase-2-12-item-11-mechanics-section-misdescribes-enrage-s-defense-effect.md)
to Juno rather than silently building past it — her call whether Enrage stays purely defensive (drop
the clause) or should also hit harder once triggered (needs a different field, targeting `AttackBonus`
instead of `Defense`).

**Items 9/10/12 — clean.** No new record shapes, no changes to `OneD20SessionAssembler`/
`LuaOneD20WorldModule`/`ISandboxedScriptHost`, both marshal/validator failure probes match
`lua-world-module-spec.md`'s existing two-stage discipline and `phase-2-11-capability-stress-test-spec.md`'s
never-wired-into-`Program.cs` probe convention. Framework-light, same bar as 2.11's spec.

**General read:** buildable as written, except item 11's one Mechanics clause. Recommend holding
`phase-2-12.md`/roadmap ratification until 0083 closes — it's a one-sentence (at most one-field)
correction, not a scope question, so shouldn't take long, but Baraka would build the wrong thing if he
started from the sentence as it stands today.

### Resolution

**Resolved 2026-08-09 (Kofi, opener).** Checked the live spec file directly, not just Amara's or
Juno's write-ups: `phase-2-12-lua-worldmodule-stress-test-spec.md` lines 176–179 read "purely
defensive; Enrage does not affect the monster's own attack roll," with the `FightCommandHandler.cs`
citation attached — matches ticket 0083's Resolution exactly. Grepped the whole doc for every
`Enrage`/`counterattack` occurrence myself: acceptance criteria, field shapes, the pairing-validator
check, and the `MonsterEnraged` lifecycle note are all consistent with the corrected mechanic, nothing
left assuming the old (wrong) behavior. Item 11's contract extension (`OneD20MonsterTemplate`/
`MonsterTemplate` growing two nullable fields) is confirmed sound against the real record shapes,
no `ComponentRegistry`/`Capability`/`EngineTier` involvement, doesn't trigger the `IWorldModule`
generalization deferral. Items 9/10/12 clean, framework-light.

**No remaining blocker. Phase 2.12 ratified** into `roadmap.md`'s sequence as
[`phase-2-12.md`](../../../Roadmap/phase-2-12.md), build ticket cut to Baraka as
[0084](0084-implement-phase-2-12-lua-worldmodule-stress-test-spec-md.md).
