# 1.d20 — Implementation Spec

**Status:** in review — revised after a playtest of ticket 0016's build (see revision notes below).
Recode handed to Baraka via [ticket 0018](../../ClaudeWiki/Processes/Handoffs/0018-recode-1-d20-per-revised-spec.md);
moves to `active` once implemented and QA-verified against the Acceptance criteria below.
**Companion to:** [`../../design/1-d20.md`](../../design/1-d20.md) — the concept doc; read that
first for *why* these calls were made. This doc is about *what to build*.
**Supersedes:** nothing
**Flavor companion:** [`1-d20-flavor.md`](1-d20-flavor.md) — every prose field below is a key into
that file, never literal text here.

**Revised 2026-07-20 (Amara's review):** conflated declarative rules with command handlers —
`check`/`fight`/`flee` are handler-shaped (branching, multi-fact mutation, early returns), not
rule-shaped. Fixed: [Mechanics / rules](#mechanics--rules) splits declarative rules from command
handlers explicitly; the removed `MissionFailedRule` turned out to be redundant once framed
correctly.

**Revised 2026-07-20 (playtest pass, after ticket 0016's build):** Arryn ran the actual console
build and surfaced four real content-spec gaps, confirmed against `OneD20RuleGroup`/
`FightCommandHandler`/`CheckCommandHandler` source, not guessed:

1. **Double-announced room text** — `MoveCommandHandler` and the challenge-prompt rule both
   announced the same `DescriptionKey`. Nine Rooms never hit this because it has two distinct
   fields (`Description` always, `ChallengePrompt` only while unsolved); this spec had collapsed
   them into one. Fixed: `Room` gains a `PromptKey`, separate from `DescriptionKey` — see
   [Fact / marker vocabulary](#fact--marker-vocabulary).
2. **Silent `fight` rounds** — only 3 of the (many) possible outcomes ever called `Announce()`;
   an inconclusive round produced no output at all, reading as broken. Fixed: every roll narrates.
3. **No HP visibility, anywhere** — neither combatant's HP was ever surfaced. Fixed: combat
   narration states remaining HP, and `look` reports it while a fight is active.
4. **Generic `check` messages** — one shared success/failure pair across all 6 different skill
   checks. Fixed: each room gets its own pair, plus a mechanical roll/DC readout.

One more, found by design review rather than reported directly: **PC HP never recovered between
fights**, including after a `flee` — meaning a session's difficulty silently ratcheted with no
way to see it coming, and losing a fight was a full session-ending `PlayerDown` state with no way
to continue toward a different win line. Redesigned: `CombatRound` now seeds the PC's HP fresh
(same moment it seeds the monster's), matching `flee`'s existing "fresh fight" language, and
losing a fight now inserts `RoomFailed` — same as the 3-round timeout — instead of ending the
whole session. `PlayerDown` is removed. This also resolves an asymmetry that was never a deliberate
choice: two of combat's three outcomes already only failed the room; the third shouldn't have been
different by accident.

Ticket 0016's build matched this spec's *previous* revision faithfully — Baraka disclosed the
double-announce as a cosmetic quirk rather than silently patching around it, which is exactly
right and is how this got caught. [Ticket 0018](../../ClaudeWiki/Processes/Handoffs/0018-recode-1-d20-per-revised-spec.md)
asked for a recode against this revision; 0018 is what the two fixes below apply on top of.

**Revised 2026-07-20 (second playtest, after 0018's build):** two more, both confirmed against
`FightCommandHandler.cs`/`Program.cs` before concluding they were spec bugs, not execution bugs:

1. **A lethal hit narrated its own contradiction** — "he staggers but he's still on his feet" (the
   `pc-hit` line) immediately followed by "he drops the rapier and doesn't get up"
   (`monster-defeated`), because this spec's steps 2/3 said "announce the hit, then separately
   check if it was lethal" — a literal read always announces `pc-hit` before the death check runs.
   Fixed: lethality is now checked *before* choosing which line to announce, both directions
   (`pc-hit`/`monster-defeated` and `monster-hit`/`pc-lost`).
2. **Room 5 never announced its `DescriptionKey` at startup** — a side effect of the previous
   revision's own fix. The old (buggy) challenge-prompt rule used to announce `DescriptionKey` too,
   so it accidentally showed at startup via the initial-settle `Fire()`; correctly narrowing that
   rule to `PromptKey`-only removed the only thing that had been showing it, since
   `MoveCommandHandler` — the sole remaining `DescriptionKey` announcer — never runs for a seeded
   starting position. Fixed: startup now explicitly announces it once.

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
    string DescriptionKey,  // → 1-d20-flavor.md; announced by MoveCommandHandler, always
    string? SuccessKey,     // → 1-d20-flavor.md; SkillCheck rooms only, null for Fight rooms
    string? FailureKey,     // → 1-d20-flavor.md; SkillCheck rooms only, null for Fight rooms
    string? PromptKey);     // → 1-d20-flavor.md; announced by the challenge-prompt rule while
                             // unsolved/unfailed — SkillCheck rooms only. Fight rooms use the
                             // fixed `monster-desc` key instead (see Declarative rules below),
                             // so PromptKey is null for them.

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
public sealed record CombatRound(int RoomId, int MonsterHp, int PlayerHp, int RoundsElapsed);
    // one at a time; both HP values seeded fresh from PlayerCharacter/MonsterTemplate MaxHp
    // whenever a new CombatRound is created — no HP persists across separate fights, including
    // a retry after `flee`. Retracted when a fight ends (win, loss, timeout, or flee).
public sealed record RoomFailed(int RoomId);      // 3-round timeout OR PC HP reaches 0 — permanent
public sealed record PreviousRoomId(int RoomId);  // updated by Move, read by flee
```

Notes, mirroring `nine-rooms-facts.md`'s own notes where they still apply:

- `PlayerPosition`, `PreviousRoomId` — exactly one of each at a time; a change retracts the old
  fact and inserts a new one, not `Update`, same reasoning Nine Rooms gives. `CombatRound` follows
  the same replace-not-update pattern for its `MonsterHp`/`PlayerHp`/`RoundsElapsed` fields as a
  fight progresses.
- `RoomSolved` and `RoomFailed` both accumulate and never retract — a room's outcome, once decided,
  is permanent for the session. **No PC-HP-based session-ending state exists anymore** — losing a
  fight fails that room, same as the timeout; it never ends the session. See the revision note
  above for why.
- `CombatRound` exists only while a fight is in progress in the PC's current room; retracted on
  win, on loss, on the 3-round timeout, or on `flee`.
- No `SessionId` — same single-session assumption Nine Rooms makes, same reasoning.

## Mechanics / rules

Split per [Purpose](#purpose): declarative rules go through the pipeline, command handlers don't.

### Declarative rules (via `GeneratorServices`/`RulePrototype` — Phase 2.5's condition applies)

**Challenge-prompt rules** (one per room, or a generated equivalent — 9 total, same shape as Nine
Rooms' `Room1ChallengeRule`...`Room9ChallengeRule`): while `PlayerPosition` is that room and
neither `RoomSolved` nor `RoomFailed` exists for it yet — same guard/activation pattern Nine Rooms
already relies on, not a new one — announce:

- **Skill-check rooms:** the room's `PromptKey` (not `DescriptionKey` — that's the handler's job,
  see below). This is the fix for the double-announce: `DescriptionKey` and `PromptKey` are now
  different text, announced by two different call sites, the same separation Nine Rooms' own
  `Description`/`ChallengePrompt` fields already had.
- **Combat rooms:** the fixed `monster-desc` key (not room-specific — same monster, same line,
  every combat room). `PromptKey` is null for these rooms; unused.

**Win-line rules** (8 total, reused shape from `WinLineTopRowRule` etc.): unchanged from Nine Rooms
— fire off the same three-`RoomSolved` pattern per line, insert `WinMarker(Line)`.

**`GameEndRule`** (reused): announces `win-announce` plus the win-line's rooms once any `WinMarker`
exists. Single condition, single action — the shape this rule family is proven at.

### Command handlers (hand-written `ICommandHandler`, same tier as `Move`/`Look` — not subject to the pipeline condition)

**Reused verbatim:** `MoveCommandHandler`'s adjacency rules — identical grid, no changes. It
additionally updates `PreviousRoomId` (retract old, insert new) on every successful move, which
Nine Rooms didn't need and this does (for `flee`), and announces the destination room's
`DescriptionKey` unconditionally — same as Nine Rooms' `Description` behavior, and now the *only*
call site that announces it (see the challenge-prompt rule above for the fix).

**Startup must also announce the start room's `DescriptionKey` once, explicitly — not left to fall
out of the two call sites above.** `PlayerPosition(5)` is seeded directly, not reached via `move`,
so `MoveCommandHandler` never runs for it; the challenge-prompt rule's initial-settle `Fire()` only
covers `PromptKey`/`monster-desc` now that the double-announce fix took `DescriptionKey` out of its
hands. Without an explicit startup announcement, room 5's description never shows at all — a real
gap surfaced by playtest, not a hypothetical. Fix: content seeding (`Program.cs`, wherever the
initial `Announce()` calls already happen for this ROM) makes one synthetic "arrived in room 5"
announcement of `DescriptionKey` before the loop starts, in addition to whatever `Fire()`'s initial
settle already handles. This is a content-seeding-level fix, not a new engine capability — no
ticket to Amara needed. (Nine Rooms has the same latent gap, apparently never noticed; not this
spec's problem to fix, noted only for context.)

**`look`**: reused shape (announces the current room's `DescriptionKey` on demand, no fact
changes), plus one addition — if a `CombatRound` is active for the current room, also announce
both combatants' current HP (e.g. "You're at 8/11 HP. He's at 3/8 HP."). This is the fix for HP
being invisible: `look` is the on-demand way to check status mid-fight, no new command needed.

**`check`** (skill-check rooms only): valid only when `PlayerPosition` is a `SkillCheck` room with
no `RoomSolved` yet. Rolls `1d20 + ability modifier` (per the room's `SkillAbility`) against the
room's `DC` — via `D20ResolutionMechanic.ResolveCheck(modifier, targetNumber)`, the concrete
`IMechanic` [ticket 0015](../../ClaudeWiki/Processes/Handoffs/0015-imechanic-dice-resolution.md)
ruled on and [ticket 0016](../../ClaudeWiki/Processes/Handoffs/0016-implement-1-d20.md) already
built — see [Framework dependencies](#framework-dependencies).

- Success: insert `RoomSolved(RoomId)`, announce the room's `SuccessKey` (room-specific — see
  `1-d20-flavor.md`), plus a handler-constructed mechanical readout: skill name, roll, DC (e.g.
  "Search: rolled 14 vs. DC 10.").
- Failure: announce the room's `FailureKey` plus the same mechanical readout. No state change —
  freely retryable, no clock, no penalty.
- Re-issuing `check` on an already-`RoomSolved` room: silent no-op. (Formalizes Baraka's disclosed
  judgment call from ticket 0016 — sensible, adopted as specified behavior rather than left open.)

**`fight`** (combat rooms only): valid only when `PlayerPosition` is a `Fight` room with no
`RoomSolved`/`RoomFailed` yet. All rolls go through `D20ResolutionMechanic`, same as `check`.

1. If no `CombatRound` exists for this room, insert one — `MonsterHp` = monster's `MaxHp`,
   `PlayerHp` = the PC's `MaxHp`, `RoundsElapsed` = 0. **Both combatants start every fight at full
   HP**, whether this is the first attempt or a retry after an earlier `flee` or a prior loss in a
   *different* room — no HP carries over between separate `CombatRound`s, ever.
2. PC attack: `1d20 + PlayerCharacter.AttackBonus` vs. `MonsterTemplate.Defense`.
   - **Hit:** reduce `CombatRound.MonsterHp` by `DamageDie + DamageBonus`, *then check lethality
     before announcing anything* — the two are not independent steps:
     - If the result is `≤ 0`: insert `RoomSolved(RoomId)`, retract `CombatRound`, announce
       `monster-defeated` **only**. Stop — no `pc-hit` narration on the killing blow (it reads as
       "he's still on his feet" immediately followed by "he drops dead," a real contradiction a
       playtest caught — this is the fix), and no counterattack.
     - Otherwise: announce `pc-hit` plus the damage dealt and the monster's remaining HP. Continue.
   - **Miss:** announce `pc-miss`. No damage. Continue.
3. Monster attack (only reached if the PC's attack didn't end the fight):
   `1d20 + MonsterTemplate.AttackBonus` vs. `PlayerCharacter.Defense`.
   - **Hit:** reduce `CombatRound.PlayerHp` by `DamageDie + DamageBonus`, same lethality-first
     order as the PC's attack:
     - If the result is `≤ 0`: insert `RoomFailed(RoomId)` (not a session-ending state — see the
       revision note and [Fact / marker vocabulary](#fact--marker-vocabulary)), retract
       `CombatRound`, announce `pc-lost` **only** — no `monster-hit` narration first, same
       contradiction, same fix. Stop.
     - Otherwise: announce `monster-hit` plus the damage taken and the PC's remaining HP. Continue.
   - **Miss:** announce `monster-miss`. No damage. Continue.
4. Neither combatant reached 0: increment `CombatRound.RoundsElapsed`. If it now equals 3: insert
   `RoomFailed(RoomId)`, retract `CombatRound`, announce `monster-fled`. Otherwise persist the
   updated `CombatRound` (new `MonsterHp`/`PlayerHp`/`RoundsElapsed`) for the next `fight` call.
5. Re-issuing `fight` on an already-`RoomSolved`/`RoomFailed` room: silent no-op (same formalized
   call as `check`, above).

Every step's `Announce()` combines a flavor key (the words) with handler-constructed dynamic
values (the numbers — damage, HP, roll, DC). Flavor keys never contain numbers; numbers are never
hardcoded into flavor text. Keeps the localization-style split intact even for dynamic content.

**`flee`** (combat rooms only, `CombatRound` active): retract `CombatRound` (no
`RoomSolved`/`RoomFailed` — reversible), announce `pc-flee`, move `PlayerPosition` to
`PreviousRoomId`'s value. No roll, no `IMechanic` involvement. If `PreviousRoomId` doesn't exist
yet (only reachable by fleeing room 5's fight before any `move` has happened this session):
retract `CombatRound` and announce `pc-flee` without moving — stay put, encounter abandoned.
(Formalizes Baraka's disclosed judgment call from ticket 0016.)

## Commands & handlers

All five are hand-written `ICommandHandler`s, same tier as Nine Rooms' checked-in
`MoveCommandHandler`/`LookCommandHandler` — not pipeline content; see [Purpose](#purpose).

| Command | Args | Valid when | Reads | Writes |
| --- | --- | --- | --- | --- |
| `move` | direction | always (grid rules apply) | `PlayerPosition` | `PlayerPosition`, `PreviousRoomId` |
| `look` | — | always | `PlayerPosition`, `Room`, `CombatRound` (if active) | — |
| `check` | — | in a `SkillCheck` room, unsolved | `Room`, ability mod | `RoomSolved` |
| `fight` | — | in a `Fight` room, unsolved/unfailed | `PlayerCharacter`, `MonsterTemplate`, `CombatRound` | `CombatRound`, `RoomSolved`, `RoomFailed` |
| `flee` | — | `CombatRound` active | `PreviousRoomId` | `CombatRound`, `PlayerPosition` |

## Acceptance criteria

- [ ] `move`/`look` behave identically to Nine Rooms on the same grid (adjacency, rejection at
      edges, room 5 start).
- [ ] Entering any unsolved/unfailed room announces `DescriptionKey` exactly once (from the
      handler) and its `PromptKey`/`monster-desc` exactly once (from the rule) — never the same
      text twice, and never both a description and a prompt with identical content.
- [ ] Session startup announces room 5's `DescriptionKey` once, explicitly — verify a fresh
      transcript shows the description *before* `monster-desc`, not just `monster-desc` alone.
- [ ] A lethal PC hit announces `monster-defeated` only — never `pc-hit` first. Same for a lethal
      monster hit: `pc-lost` only, never `monster-hit` first. (Verify via a transcript where the
      killing blow's damage number is checked against the pre-hit HP to confirm it really was
      lethal, not just asserting the right string appeared.)
- [ ] Each of the 6 skill-check rooms resolves `check` using its own skill/ability/DC from the
      table in `1-d20.md`, announces that room's specific `SuccessKey`/`FailureKey` (not a shared
      generic message), and includes the mechanical roll/DC readout. A failed `check` leaves the
      room unsolved and retryable with no penalty.
- [ ] A successful `check` inserts exactly one `RoomSolved` for that room and never re-triggers the
      challenge-prompt announcement afterward.
- [ ] `fight` in a fresh `CombatRound` seeds *both* the monster's and the PC's HP at full `MaxHp`
      — never carried over from a prior visit to this room or from a different room's fight.
- [ ] Every `fight` round announces something — a hit/miss line plus damage and remaining HP on a
      connect — even when the round doesn't end the encounter (no more silent rounds).
- [ ] `look` while a `CombatRound` is active reports both combatants' current HP.
- [ ] A `fight` round that drops the monster to 0 HP or below solves the room and skips the
      counterattack step (verify via a transcript where the last hit is lethal).
- [ ] A `fight` round where the PC's `CombatRound.PlayerHp` drops to 0 or below inserts
      `RoomFailed` for that room (not a session-ending state) and retracts `CombatRound` — verify
      the session continues normally afterward and other rooms remain playable.
- [ ] Three rounds of `fight` with neither combatant reduced to 0 inserts `RoomFailed`, retracts
      `CombatRound`, and makes `RoomSolved` permanently unreachable for that room — verify a later
      `fight` in the same room is rejected (nothing to fight).
- [ ] `flee` during an active `CombatRound` retracts it without inserting `RoomSolved` or
      `RoomFailed`, and moves the PC to `PreviousRoomId`; a later `fight` in that same room starts a
      fresh `CombatRound` at full HP for both combatants.
- [ ] Fleeing room 5's fight with no prior `move` this session doesn't move the PC (no
      `PreviousRoomId` exists yet) and doesn't throw.
- [ ] Completing all three rooms in column `[3,6,9]` (skill checks only, no combat) fires
      `WinMarker`/`GameEndRule` — the combat-free win path is real, not just claimed.
- [ ] Completing a line that includes a combat room (e.g. `[1,2,3]`) also fires `WinMarker`
      correctly when that room was solved via `fight`.
- [ ] `RoomFailed` on any room in a given win line permanently removes that line as a win path for
      the session (verify no `WinMarker` fires for a line with a `RoomFailed` member, regardless of
      the other two rooms' state) — check this for both causes of `RoomFailed` (timeout and a lost
      fight).

## Framework dependencies

- **`IMechanic` — resolved, no longer an open dependency.**
  [Ticket 0015](../../ClaudeWiki/Processes/Handoffs/0015-imechanic-dice-resolution.md) ruled:
  dice/check-resolution members don't go on `IMechanic` itself (that would bake one Mechanic's
  shape into a shared contract); `IMechanic` grows only `IHasLunaFlowReference`, and goes `public`
  — confirmed ROM-tier, same reasoning as ticket 0014's `IEngineHost` ruling. The concrete
  implementation, `D20ResolutionMechanic` (`src/LunaFlow.Rom/OneD20/`), exists as of
  [ticket 0016](../../ClaudeWiki/Processes/Handoffs/0016-implement-1-d20.md): `Roll(string die)`
  and `ResolveCheck(int modifier, int targetNumber)`. `check`/`fight` call it directly — no
  pipeline involvement for handlers, same as `MoveCommandHandler` calling `LunaFlowEngine.Host`
  directly. This spec now names the concrete type instead of pointing at an open question.
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

- **Soft-lock detection — more likely to matter now than in the previous revision.**
  `RoomFailed` now has two causes (timeout *and* a lost fight) instead of one, so it's reachable
  more often. If enough accumulate that no win line is reachable, the session still doesn't detect
  or announce this — the player just keeps playing a game they can no longer win. Still not solved
  here; still flagging rather than silently deciding. Candidate for a small follow-up rule (check
  reachable lines the way `WinLineCatalog` does, announce if none remain) — worth prioritizing this
  time given the redesign makes it easier to hit.
- **Weapon/proficiency fiat:** already decided in `1-d20.md` (both combatants treated as proficient
  by content-design fiat) — restated here only as a pointer, not re-litigated.

Resolved since the last revision (formalized into the spec proper rather than left open):
re-issuing `check`/`fight` on a solved/failed room is a silent no-op (Baraka's disclosed call from
ticket 0016, adopted); fleeing room 5 before any `move` leaves the PC in place (same source).

## Related documents

- [`../../design/1-d20.md`](../../design/1-d20.md) — concept doc this spec builds
- [`1-d20-flavor.md`](1-d20-flavor.md) — prose companion
- [`../../NineRooms/nine-rooms-facts.md`](../../NineRooms/nine-rooms-facts.md) — reused grid/win-line facts
- [`../../NineRooms/nine-rooms-gameplay.md`](../../NineRooms/nine-rooms-gameplay.md) — reused handler shapes
- [`../../Roadmap/phase-2-5.md`](../../Roadmap/phase-2-5.md) — the phase this closes, and the
  `GeneratorServices` load-bearing condition
- [`../../ClaudeWiki/Processes/content-spec-template.md`](../../ClaudeWiki/Processes/content-spec-template.md) — the template this follows
- [Ticket 0015](../../ClaudeWiki/Processes/Handoffs/0015-imechanic-dice-resolution.md) — `IMechanic`
  shape/visibility ruling
- [Ticket 0016](../../ClaudeWiki/Processes/Handoffs/0016-implement-1-d20.md) — first build, against
  the pre-playtest revision of this spec; disclosed the double-announce quirk this revision fixes
- [Ticket 0017](../../ClaudeWiki/Processes/Handoffs/0017-1-d20-test-suite.md) — Nadia's automated
  suite, filed against the same pre-playtest revision — will need to move with this one
- [Ticket 0018](../../ClaudeWiki/Processes/Handoffs/0018-recode-1-d20-per-revised-spec.md) — recode
  against this revision
