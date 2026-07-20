# 1.d20 — Implementation Spec

**Status:** draft
**Companion to:** [`../../design/1-d20.md`](../../design/1-d20.md) — the concept doc; read that
first for *why* these calls were made. This doc is about *what to build*.
**Supersedes:** nothing
**Flavor companion:** [`1-d20-flavor.md`](1-d20-flavor.md) — every prose field below is a key into
that file, never literal text here.

**Revised 2026-07-20:** Amara's review caught the spec conflating declarative rules with command
handlers — `check`/`fight`/`flee` are handler-shaped (branching, multi-fact mutation, early
returns), not rule-shaped, and an earlier draft's Framework Dependencies section read as if the
`GeneratorServices` load-bearing condition covered them too. Fixed: [Mechanics / rules](#mechanics--rules)
now splits declarative rules from command handlers explicitly, and the removed `MissionFailedRule`
turned out to be redundant once framed correctly — its one real job (blocking further `check`/
`fight` once `PlayerDown` exists) is a handler-level guard, not a rule at all.

## Purpose

Turns `1-d20.md`'s concept into something buildable: exact fact shapes, exact rules, exact
commands. Reuses Nine Rooms' grid/win-line facts and handlers wholesale (see
[`nine-rooms-facts.md`](../../NineRooms/nine-rooms-facts.md)) and adds the new d20 System/Scenario
layer on top.

**Rule vs. handler, stated up front (per Amara's review — this was ambiguous in an earlier draft):**
Phase 2.5's load-bearing condition applies to *declarative NRules rules* — the challenge-prompt,
win-line, and `GameEndRule` family — which must be authored through `GeneratorServices`/
`RulePrototype`, not hand-written `ARule` classes. It does **not** apply to `check`/`fight`/`flee`.
Those are `ICommandHandler`s, the same tier as Nine Rooms' checked-in `MoveCommandHandler` — hand-
written C#, reaching state through `LunaFlowEngine.Host` (and `IMechanic`, once it exists) directly,
never through the pipeline. Command handlers have always been hand-written; that precedent doesn't
change here. [Mechanics / rules](#mechanics--rules) below is split into two subsections along
exactly this line.

This spec **defines fact shapes and rule behavior**. It does not implement `IMechanic` — that's
Amara's Phase 2.5 ticket (dice rolling and attack/skill-check resolution need a real Mechanic
behind them; see [Framework dependencies](#framework-dependencies)).

## Scope

**In:** the 9-room grid (reused geometry), 3 combat rooms, 6 skill-check rooms, the pregenerated
PC, one monster template, `move`/`look` (reused) plus new `check`/`fight`/`flee` commands, the
8 win-line rules and win/loss end states.

**Out:** everything `1-d20.md`'s "Cut entirely" list already covers (Wealth, Reputation, character
creation, leveling, Advanced Classes, Occupations, Feats-as-a-system, Action Points, FX/psionics,
vehicles). Also out: a second monster type, a second weapon, multi-actor scheduling (all explicitly
deferred to later content or 1.x.d20 per the concept doc).

## Fact / marker vocabulary

Reused as-is from Nine Rooms, no changes:

```csharp
public sealed record PlayerPosition(int RoomId);
public sealed record RoomSolved(int RoomId);
public sealed record WinMarker(IReadOnlyList<int> Line);
```

New — static content, seeded once at session start:

```csharp
public enum RoomChallengeType { SkillCheck, Fight }

public sealed record Room(
    int Id, int Row, int Col,
    RoomChallengeType ChallengeType,
    string? Skill,          // key ability's skill name, e.g. "Search" — null for Fight rooms
    string? SkillAbility,   // "Str" | "Dex" | "Con" | "Int" | "Wis" | "Cha" — null for Fight rooms
    int? DC,                // null for Fight rooms
    string DescriptionKey); // → 1-d20-flavor.md

public sealed record CombatantStats(
    int Defense,
    int AttackBonus,
    string DamageDie,   // e.g. "1d8"
    int DamageBonus,
    int MaxHp);

public sealed record PlayerCharacter(CombatantStats Stats);  // singleton, seeded once
public sealed record MonsterTemplate(CombatantStats Stats);  // singleton — same monster, all 3 rooms
```

New — mutable session state:

```csharp
public sealed record PlayerHp(int Current);                          // singleton, updated not replaced-per-point
public sealed record CombatRound(int RoomId, int MonsterHp, int RoundsElapsed); // one at a time; retracted when a fight ends
public sealed record RoomFailed(int RoomId);                          // monster fled after 3 rounds — permanent
public sealed record PreviousRoomId(int RoomId);                      // updated by Move, read by flee
public sealed record PlayerDown();                                    // mission-ending marker
```

Notes, mirroring `nine-rooms-facts.md`'s own notes where they still apply:

- `PlayerPosition`, `PlayerHp`, `PreviousRoomId` — exactly one of each at a time; a change retracts
  the old fact and inserts a new one, not `Update`, same reasoning Nine Rooms gives.
- `RoomSolved` and `RoomFailed` both accumulate and never retract — a room's outcome, once decided,
  is permanent for the session.
- `CombatRound` exists only while a fight is in progress in the PC's current room; retracted on
  win, on the 3-round timeout, or on `flee`.
- No `SessionId` — same single-session assumption Nine Rooms makes, same reasoning.

## Mechanics / rules

Split per [Purpose](#purpose): declarative rules go through the pipeline, command handlers don't.

### Declarative rules (via `GeneratorServices`/`RulePrototype` — Phase 2.5's condition applies)

**Challenge-prompt rules** (one per room, or a generated equivalent — 9 total, same shape as Nine
Rooms' `Room1ChallengeRule`...`Room9ChallengeRule`): announce the room's `DescriptionKey` text while
`PlayerPosition` is that room and neither `RoomSolved` nor `RoomFailed` exists for it yet — same
guard/activation pattern Nine Rooms already relies on, not a new one. For combat rooms, the same
rule also announces `monster-desc` under the same guard (no separate "first visit" tracking fact —
matching Nine Rooms' existing repeat-while-unsolved behavior rather than inventing a once-only
variant).

**Win-line rules** (8 total, reused shape from `WinLineTopRowRule` etc.): unchanged from Nine Rooms
— fire off the same three-`RoomSolved` pattern per line, insert `WinMarker(Line)`.

**`GameEndRule`** (reused): announces `win-announce` plus the win-line's rooms once any `WinMarker`
exists. Single condition, single action — the shape this rule family is proven at.

### Command handlers (hand-written `ICommandHandler`, same tier as `Move`/`Look` — not subject to the pipeline condition)

**Reused verbatim:** `MoveCommandHandler`/`LookCommandHandler` and their adjacency rules — identical
grid, no changes. `MoveCommandHandler` additionally updates `PreviousRoomId` (retract old, insert
new) on every successful move, which Nine Rooms didn't need and this does (for `flee`).

Every handler below starts with the same guard, checked in code, not via an NRules `.Not<T>()`
clause (there's no rule here to attach one to): if `PlayerDown` exists, do nothing.

**`check`** (skill-check rooms only): valid only when `PlayerPosition` is a `SkillCheck` room with
no `RoomSolved` yet. Rolls `1d20 + ability modifier` (per the room's `SkillAbility`) against the
room's `DC` — via `IMechanic`'s generic check-resolution operation once it exists (see
[Framework dependencies](#framework-dependencies)), not hand-rolled arithmetic in the handler.

- Success: insert `RoomSolved(RoomId)`, announce `check-success`.
- Failure: announce `check-failure`, no state change — freely retryable, no clock, no penalty.

**`fight`** (combat rooms only): valid only when `PlayerPosition` is a `Fight` room with no
`RoomSolved`/`RoomFailed` yet. All rolls below go through the same `IMechanic` operation as `check`.

1. If no `CombatRound` exists for this room, insert one (`MonsterHp` = monster's `MaxHp`,
   `RoundsElapsed` = 0) — a fresh fight, whether this is the first attempt or a retry after an
   earlier `flee`.
2. PC attack: `1d20 + PlayerCharacter.AttackBonus` vs. `MonsterTemplate.Defense`. On a hit, reduce
   `CombatRound.MonsterHp` by `DamageDie + DamageBonus`.
3. If `MonsterHp ≤ 0`: insert `RoomSolved(RoomId)`, retract `CombatRound`, announce
   `monster-defeated`. Stop — no counterattack on the round that ends the fight.
4. Otherwise, monster attack: `1d20 + MonsterTemplate.AttackBonus` vs. `PlayerCharacter.Defense`.
   On a hit, reduce `PlayerHp.Current` by `DamageDie + DamageBonus`.
5. If `PlayerHp.Current ≤ 0`: insert `PlayerDown()`, retract `CombatRound`, announce `pc-down`.
   Stop. (This is the only place `PlayerDown` gets announced — no separate rule duplicates it.)
6. Otherwise increment `CombatRound.RoundsElapsed`. If it now equals 3: insert `RoomFailed(RoomId)`,
   retract `CombatRound`, announce `monster-fled`.

**`flee`** (combat rooms only, `CombatRound` active): retract `CombatRound` (no
`RoomSolved`/`RoomFailed` — reversible), announce `pc-flee`, move `PlayerPosition` to
`PreviousRoomId`'s value. No roll, no `IMechanic` involvement.

## Commands & handlers

All five are hand-written `ICommandHandler`s, same tier as Nine Rooms' checked-in
`MoveCommandHandler`/`LookCommandHandler` — not pipeline content; see [Purpose](#purpose).

| Command | Args | Valid when | Reads | Writes |
| --- | --- | --- | --- | --- |
| `move` | direction | always (grid rules apply) | `PlayerPosition` | `PlayerPosition`, `PreviousRoomId` |
| `look` | — | always | `PlayerPosition`, `Room` | — |
| `check` | — | in a `SkillCheck` room, unsolved | `Room`, ability mod | `RoomSolved` |
| `fight` | — | in a `Fight` room, unsolved/unfailed | `PlayerCharacter`, `MonsterTemplate`, `CombatRound` | `CombatRound`, `RoomSolved`, `RoomFailed`, `PlayerHp`, `PlayerDown` |
| `flee` | — | `CombatRound` active | `PreviousRoomId` | `CombatRound`, `PlayerPosition` |

## Acceptance criteria

- [ ] `move`/`look` behave identically to Nine Rooms on the same grid (adjacency, rejection at
      edges, room 5 start).
- [ ] Each of the 6 skill-check rooms resolves `check` using its own skill/ability/DC from the
      table in `1-d20.md`; a failed `check` leaves the room unsolved and retryable with no penalty.
- [ ] A successful `check` inserts exactly one `RoomSolved` for that room and never re-triggers the
      challenge-prompt announcement afterward.
- [ ] `fight` in a fresh combat room seeds a `CombatRound` at the monster's full HP, not carried
      over from a prior visit.
- [ ] A `fight` round that drops the monster to 0 HP or below solves the room and skips the
      counterattack step (verify via a transcript where the last hit is lethal).
- [ ] A `fight` round where the PC drops to 0 HP or below inserts `PlayerDown` and blocks all
      further `check`/`fight` commands session-wide.
- [ ] Three rounds of `fight` with neither combatant reduced to 0 inserts `RoomFailed`, retracts
      `CombatRound`, and makes `RoomSolved` permanently unreachable for that room — verify a later
      `fight` in the same room is rejected (nothing to fight).
- [ ] `flee` during an active `CombatRound` retracts it without inserting `RoomSolved` or
      `RoomFailed`, and moves the PC to `PreviousRoomId`; a later `fight` in that same room starts a
      fresh `CombatRound` at full monster HP.
- [ ] Completing all three rooms in column `[3,6,9]` (skill checks only, no combat) fires
      `WinMarker`/`GameEndRule` — the combat-free win path is real, not just claimed.
- [ ] Completing a line that includes a combat room (e.g. `[1,2,3]`) also fires `WinMarker`
      correctly when that room was solved via `fight`.
- [ ] `RoomFailed` on any room in a given win line permanently removes that line as a win path for
      the session (verify no `WinMarker` fires for a line with a `RoomFailed` member, regardless of
      the other two rooms' state).

## Framework dependencies

- **`IMechanic`** needs real members to back two things the `check`/`fight` command handlers need
  as first-class operations, not hand-rolled per-handler arithmetic: (1) rolling a die (`1d20`,
  plus the PC's and monster's damage dice) and (2) resolving a `d20 + modifier vs. target number`
  check generically, since both commands are the same underlying operation with different inputs.
  This is close to the glossary's own canonical Mechanic example ("a `Dice` object and how to roll
  it") — ticketed to Amara per [Phase 2.5](../../Roadmap/phase-2-5.md), not something this spec
  builds. The handlers call it directly, the same way `MoveCommandHandler` calls
  `LunaFlowEngine.Host.Rules` directly today — no pipeline involvement for handlers.
- **`IMechanic`'s visibility/assembly home is an open seam, not assumed here.** `IMechanic` is
  `internal` to `LunaFlow.Rules` today; if its concrete resolution Mechanic ends up living in
  `LunaFlow.Rom` (which fits this spec's own System/Scenario framing — a resolution Mechanic is
  exactly `architecture.md`'s "Dice object" example, ROM-tier by that precedent), that's a
  cross-assembly reference needing the same treatment ticket 0014 gave `IEngineHost`. This spec
  doesn't presume an answer — the command handlers above just need one call signature to hit,
  wherever it resolves to. Amara's call, flagged here so it's not lost between chat and doc.
- **Rule authoring via `GeneratorServices`** — scoped narrowly to the
  [Declarative rules](#declarative-rules-via-generatorservicesruleprototype--phase-25s-condition-applies)
  subsection only (challenge-prompt, win-line, `GameEndRule`): those need to reach the engine as
  `RulePrototype`s through the pipeline. The command handlers in the section below that are
  explicitly out of scope for this condition — see [Purpose](#purpose). Nothing here presumes a
  specific `RulePrototype` shape; that's the pipeline/Baraka's translation of this spec's stated
  rule behavior.
- No new engine-core capability (turn loop, component model) is presumed — see `1-d20.md`'s Open
  seams section, unchanged by this spec.

## Open questions / edge cases

- **Soft-lock detection:** if enough `RoomFailed`s accumulate that no win line is reachable, the
  session doesn't currently detect or announce this — the player just keeps playing a game they
  can no longer win. Not solved here; flagging rather than silently deciding. Candidate for a small
  follow-up rule (check reachable lines the way `WinLineCatalog` does, announce if none remain) if
  playtesting shows this is a real dead end rather than a rare edge case.
- **`check`/`fight` on a solved/failed room:** should re-issuing the command say something
  (`"already handled"`) or just silently fail? Left to Baraka's judgment — no player-facing
  behavior was specified because it doesn't affect any acceptance criterion above.
- **Weapon/proficiency fiat:** already decided in `1-d20.md` (both combatants treated as proficient
  by content-design fiat) — restated here only as a pointer, not re-litigated.

## Related documents

- [`../../design/1-d20.md`](../../design/1-d20.md) — concept doc this spec builds
- [`1-d20-flavor.md`](1-d20-flavor.md) — prose companion
- [`../../NineRooms/nine-rooms-facts.md`](../../NineRooms/nine-rooms-facts.md) — reused grid/win-line facts
- [`../../NineRooms/nine-rooms-gameplay.md`](../../NineRooms/nine-rooms-gameplay.md) — reused handler shapes
- [`../../Roadmap/phase-2-5.md`](../../Roadmap/phase-2-5.md) — the phase this closes, and the
  `GeneratorServices` load-bearing condition
- [`../../ClaudeWiki/Processes/content-spec-template.md`](../../ClaudeWiki/Processes/content-spec-template.md) — the template this follows
