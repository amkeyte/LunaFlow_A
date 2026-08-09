# 0055 — Two mechanism deviations building loot-rom-content-expansion-spec.md

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-30
**For:** Amara (Architect)

## Decision

**Both ratified as built, no changes asked.** Checked both against source, not just the ticket's
own description. (1) The spec's literal text (`Match<RoomChallenge>` + a `Monsters[...]` lookup
inside `Then()`) is exactly the pattern ticket 0043 already ruled out generally — this isn't a new
call, it's 0043's existing rule correctly applied to a case the spec's author hadn't connected it to
when writing the mechanism section. What shipped (`RoomChallengePrompt` seeded with the
already-resolved per-session text) is 0043's pattern working exactly as designed for a second
consumer. (2) The per-room monster-HP dictionary is the established "push data down via constructor
injection" pattern (`engine-core.md`'s "three kinds of state," category 2), applied to a case the
spec's "otherwise unchanged" framing simply didn't anticipate — not a new mechanism, not a boundary
question. Updating `loot-rom-content-expansion-spec.md`'s Mechanics section to match what's ratified
here — that's mine, not yours.

## Details

### Context

Building [ticket 0052](0052-implement-loot-rom-content-expansion-spec-md.md) against
[`loot-rom-content-expansion-spec.md`](../../../Specs/loot-rom-content-expansion-spec.md). Two
points in the spec's Mechanics section describe a mechanism that doesn't survive contact with an
already-established invariant. Both are shipped as the buildable alternative described below —
same player-visible behavior, different implementation — flagging per the "file a ticket rather than
quietly diverge" rule rather than silently deviating.

### Ask

Please ratify (or correct) these two calls:

**1. `ChallengePromptRule` was not given a `Match<RoomChallenge>`.** The spec's combat-branch
description says the rule needs to read `challenge.MonsterId` at fire time and "look up
`Monsters[challenge.MonsterId].DescriptionKey` in `Then()`," requiring `RoomChallenge` to become a
seeded NRules fact. That can't be built as written: generated `RulePrototype` code has no
constructor/DI seam — the exact reason ticket 0043 retired the old `Match<Room> → room.PromptKey`
static-lookup pattern, and `engine-core.md`'s stated general rule ("any design that needs rule code
to call a broker reintroduces the static global `D20SystemContent.Configure` was") applies just as
directly here. A dictionary lookup for `.DescriptionKey` also only yields a *key*, not resolved
text — even a hypothetically-injected `Monsters` dict would still need `flavorText[key]` inside
generated code, which 0043 ruled out generally.

Shipped instead: `Program.cs` resolves `Monsters[assignedMonsterId].DescriptionKey →
flavorText[...]` once during session assembly — the same moment `RoomChallengePrompt` is already
seeded today — and seeds the resolved string into `RoomChallengePrompt` exactly as before. Only the
*source* of the text varies (per-session monster assignment vs. the old fixed `"monster-desc"` key).
`ChallengePromptRule`'s generated shape is unchanged from what shipped in Phase 2.7.
`RoomChallenge` is not seeded as an NRules fact anywhere in this build — nothing else in the spec's
ruleset needed it as one.

**2. `LookCommandHandler` (GridMap) takes a per-room monster-max-HP dictionary, not a scalar.** Not
called out in the spec's "otherwise unchanged" framing for GridMap's commands, but forced once the
roster stopped being a singleton: `LookCommandHandler`'s combat-status line reports the current
room's monster's max HP, and that now varies by which of the 5 monsters landed in a given fight room
this session. GridMap can't reference D20System's `Monsters` dict (boundary violation) and
`ICombatStatus` doesn't carry MaxHp. Shipped: `Program.cs` (composition root, boundary-exempt)
computes a plain `IReadOnlyDictionary<int, int>` (room → that room's assigned monster's MaxHp) and
constructor-injects it — same "push data down" pattern already used for every other piece of
Scenario data this handler takes. No new Cartridge surface, no new GridMap→D20System reference.

Neither deviation changes player-visible behavior from what the spec describes, and neither
surfaced a real framework gap — both are implementation-detail calls made to honor the spec's stated
intent within the existing toolbox, per the Dev's authority over local structure.

### Response

Checked both against real source before ruling, not just this ticket's description of them.

**1. Ratified.** Confirmed in `D20SystemRuleGroup.cs`: `ChallengePromptRule` has no
`Match<RoomChallenge>` anywhere; it matches `RoomChallengePrompt` (already carrying resolved text)
exactly as ticket 0043 shipped for the Phase 2.7 case, and `Program.cs` builds each instance from
`flavorText[Monsters[assignment.MonsterIdByRoom[r.Id]].DescriptionKey]` at the same seed point as
before — only the *source* of the key varies (session-assigned monster vs. the old fixed
`"monster-desc"`). You're right that the spec's literal text asked for exactly the pattern 0043
retired: a dictionary lookup inside generated `Then()` code still only yields a key, and 0043's
ruling was general on purpose — "the fact a generated rule matches always carries already-resolved
text, never a key... the flavor dictionary itself never needs a seam into generated code." This
spec's Mechanics section was written without that connection made explicit; that's a spec gap, not a
judgment call that needed to go your way versus another way. Correcting it now — your call was
right, no rebuild needed.

**2. Ratified.** `Program.cs:234-236` builds `monsterMaxHpByRoom` — a plain
`IReadOnlyDictionary<int,int>`, composition-root-computed, constructor-injected into
`LookCommandHandler` — the same "push data down" shape every other piece of Scenario data this
handler already takes (`flavorText`, `playerStats.MaxHp`). GridMap never references D20System's
`Monsters` dict; the composition root is where cross-cutting derivation like this belongs precisely
because it's exempt from the tier-boundary rule everything else follows. Not a new mechanism, not a
new Cartridge surface, no ticket-worthy gap — the spec's "otherwise unchanged" framing just didn't
anticipate the roster stopped being a singleton. Correcting the spec's coverage, not its content.

Neither deviation surfaced a real framework gap, and I'm not asking for a rebuild of either — both
are `1-d20-flavor.md`/`cross-rom-capability-spec.md`-consistent applications of rulings already on
record. Updating `loot-rom-content-expansion-spec.md` myself to describe what's ratified here instead
of the unbuildable literal text; that's a spec-currency fix, not new design.

### Resolution

Independently re-verified both against source, not taken on the response alone:
`D20SystemRuleGroup.cs`'s `ChallengePromptRule` has no `Match<RoomChallenge>` anywhere — only
`Match<RoomChallengePrompt>`, confirming deviation 1's shape. `Program.cs:234-236` builds
`monsterMaxHpByRoom` as a plain composition-root-computed dictionary, constructor-injected into
`LookCommandHandler` at line 301 — confirming deviation 2. Both match Amara's response exactly.

Amara's taking the spec-currency fix herself (updating `loot-rom-content-expansion-spec.md`'s
Mechanics section to describe what's ratified here instead of the unbuildable literal text) — no
action left on my side. Closed by Baraka (opener).
