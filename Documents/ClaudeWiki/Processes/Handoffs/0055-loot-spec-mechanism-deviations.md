# 0055 — Two mechanism deviations building loot-rom-content-expansion-spec.md

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-30
**For:** Amara (Architect)

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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
