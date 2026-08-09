# Phase 2.12 — World Module Authoring Surface Stress Test (Lua Path) — Implementation Spec

**Status:** active. Built (ticket 0084), maintained test coverage landed (ticket 0085), QA-verified
PASS (ticket [0086](../ClaudeWiki/Processes/Handoffs/0086-phase-2-12-qa-gate.md), see
[`phase-2-12-verdict.md`](../QA/phase-2-12-verdict.md)). Per `content-spec-template.md`'s lifecycle,
`active` means implemented and acceptance-criteria-verified, not just designed.
**Companion design doc:** [`../design/world-module.md`](../design/world-module.md#two-authoring-paths-into-one-contract) — read for rationale.
**Companion to:** [`lua-world-module-spec.md`](lua-world-module-spec.md) (the mechanism this content
is authored against, unchanged by this spec) and
[`Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md)
(the reviewed proposal, per [ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md)).
**Supersedes:** nothing.

## Purpose

Turns the proposal's Phase 2.12 candidate items (9–12) into buildable content. Phase 2.9's Lua proof
is explicitly a verbatim transcription of `DefaultOneD20WorldModule` — it never tested whether the
authoring surface holds up for someone actually *designing* content in Lua, using real computation,
or pushing past what the current contract can express. This spec is that test. **Item 11 is the
load-bearing one** — per Amara's ruling on ticket 0067, its result is a direct input to the
still-draft `gm-overlay.md` "effect fact" idea for Phase 4, and whoever runs that kickoff pass should
read this item's outcome first.

**This spec does not implement** any change to `OneD20SessionAssembler`, `LunaFlow.Scripting`'s
sandboxed host, or the marshal/validate two-stage failure discipline `lua-world-module-spec.md`
already built — items 9/10/12 exercise that machinery as-is. **Item 11 is the one exception**: it
does propose a scoped `IOneD20WorldModule`/`OneD20MonsterTemplate` extension, stated precisely so it
isn't confused with the separate, still-untriggered `IWorldModule` generalization (`non-goals.md`).

## Scope

**In:**
- **Item 9:** a genuinely new World Module, authored directly in Lua, with no C# equivalent to diff
  against — a real scenario, not a transcription.
- **Item 10:** within that same Lua script (or a variant of it), real computation — loops/
  conditionals generating repeated structure — rather than a flat `return {...}` literal.
- **Item 11:** one monster whose behavior can't be expressed as static `CombatantStats` — an
  Enrage effect triggered by an HP threshold — and the scoped contract extension needed to let a
  World Module declare it.
- **Item 12:** two deliberately-broken Lua scripts, one failing at marshal time and one failing at
  `OneD20WorldModuleValidator`, proving the two-stage failure discipline actually surfaces
  through the Lua path a real author would hit, not just the unit-tested cases.

**Out:**
- Any change to `OneD20SessionAssembler`, `LuaOneD20WorldModule`'s marshaling mechanism, or
  `ISandboxedScriptHost` — all unchanged, exercised as shipped.
- Generalizing `MonsterTemplate`'s new field (item 11) into a general "monster ability" system —
  scoped narrowly, see Mechanics.
- The `IWorldModule` generalization, or anything touching a second Rulebook — `non-goals.md`'s
  deferral is explicitly not triggered by this spec (see Related documents).
- A GM-Overlay-tier mechanism of any kind — item 11's finding is an *input* to that still-draft work,
  not an implementation of it.

## Fact / marker vocabulary

### Item 11 — the scoped `IOneD20WorldModule`/`OneD20MonsterTemplate` extension

The finding this item is designed to produce: a monster whose Defense improves once it's bloodied
cannot be expressed as a flat `CombatantStats` block, because `CombatantStats` has no notion of
"changes partway through a fight." The narrowest fix that stays data (not a bespoke per-monster
rule) is one new nullable field on the existing Rulebook-owned monster shape:

```csharp
// OneD20MonsterTemplate (Rulebook-owned, world-module-extraction-spec.md) grows one field:
public sealed record OneD20MonsterTemplate(
    string Id, string Name, string DescriptionKey, OneD20CombatantStats Stats,
    double? EnrageThresholdFraction = null,   // NEW — e.g. 0.5 means "at or below half MaxHp"
    int? EnrageDefenseBonus = null);          // NEW — flat bonus applied once triggered
```

Both fields are nullable and default `null` — every existing monster (the five from Phase 2.7.1,
Hollow Keep's own roster) is unaffected, satisfies the shape with both fields absent, and needs no
migration. **This is the scoped contract change Amara pre-authorized on ticket 0067**: it changes
`IOneD20WorldModule`'s data shape (via the Rulebook-owned record it returns), it does **not**
generalize to a shared `IWorldModule`, and it does not introduce a scripted-behavior mechanism —
it's a data field a generic D20System rule interprets, same as `MonsterId`/`InflictsPoison`
(Phase 2.10) already are.

New session fact, D20System-owned (parallels `Poisoned`'s shape from Phase 2.10):

```csharp
public sealed record MonsterEnraged(int RoomId);   // inserted once per CombatRound when the
                                                       // threshold is crossed; retracted with the
                                                       // CombatRound, same lifecycle as any other
                                                       // combat-scoped state
```

### Items 9/10 — no new C# record shapes

Both are content authored entirely within `IOneD20WorldModule`'s existing surface (rooms,
challenges, monsters, PC, flavor text) plus, for item 9 specifically, the `Save`-kind challenge and
`InflictsPoison` fields Phase 2.10 already added to the Rulebook-owned shapes — reused, not
reinvented, so this World Module proves it can express everything the two prior phases added, not
just the Phase 2.8 baseline.

## Mechanics / rules

### Item 9 — "Signal Static": a new World Module, Lua-authored, no C# equivalent

A second riverside-adjacent Department-7 mission (distinct fiction from the warehouse, same grid
shape) — nine rooms, the same fixed win-line topology, populated with:

- The existing randomized pool mechanism (`OneD20SessionAssembler`, unchanged) drawing from a
  **new** pool of 9 skill/save challenges and 5 monsters authored directly in the Lua file — not the
  warehouse's pool re-skinned, a distinct set, so a diff against `DefaultOneD20WorldModule` would be
  meaningless (there's nothing to diff against, which is the point).
- At least one `Save`-kind challenge and one `InflictsPoison` monster, proving the Lua marshaling
  path round-trips the fields Phase 2.10 added to the Rulebook-owned shapes, not just the Phase 2.8
  baseline `lua-world-module-spec.md` proved.
- Full flavor text for all nine rooms and the challenge/monster pool, per the localization-style
  split — kept inside the `.lua` file's `flavorText` table, same convention
  `lua-world-module-spec.md` established.

Authored by Juno directly as a `.lua` source file (no `.cs` draft first) — that authoring process
*is* the test of item 9's question, not just its output.

### Item 10 — real computation inside the same script

Within "Signal Static"'s Lua source, at least one section is generated procedurally rather than
written as a literal table — the concrete choice: the 5-monster roster's stat progression is
computed from a small base-plus-step table instead of five hand-typed blocks:

```lua
local monsterBase = { defense = 11, attackBonus = 2, maxHp = 6 }
local monsters = {}
for i = 1, 5 do
  monsters["signal-monster-" .. i] = {
    name = "Static Hound " .. i,
    descriptionKey = "signal-monster-" .. i .. "-desc",
    stats = {
      defense = monsterBase.defense + i,
      attackBonus = monsterBase.attackBonus,
      damageDie = "1d6",
      damageBonus = 0,
      maxHp = monsterBase.maxHp + (i * 2),
    },
  }
end
```

This is deliberately simple arithmetic, not a stress case in itself — the point is confirming the
marshaler handles a table *built* by a loop identically to one written as a literal, since nothing
in `LuaOneD20WorldModule`'s construction (per `lua-world-module-spec.md` §"Mechanics") distinguishes
how the returned table was produced. If it doesn't, that's the finding.

### Item 11 — the ROM-tier mirror and translation (completing the World Module chain)

**Stated explicitly, per the same self-review finding recorded in
`phase-2-10-content-coverage-spec.md`:** `OneD20MonsterTemplate` (above) is the Rulebook-owned,
World-Module-facing shape — extending it alone doesn't reach the live game, because
`Program.cs`'s `ConstructD20System` translation and D20System's own ROM-tier `MonsterTemplate` fact
are what a `fight` rule actually reads. `MonsterTemplate` (ROM-tier, `loot-rom-content-expansion-spec.md`)
grows the mirrored fields:

```csharp
public sealed record MonsterTemplate(
    string Id, string Name, string DescriptionKey, CombatantStats Stats,
    bool InflictsPoison = false,             // Phase 2.10
    double? EnrageThresholdFraction = null,  // NEW
    int? EnrageDefenseBonus = null);         // NEW
```

`Program.cs`'s translation carries both new fields through from `OneD20MonsterTemplate` unchanged in
logic, same as every other monster field already does. `OneD20WorldModuleValidator` grows a pairing
check: if either `EnrageThresholdFraction` or `EnrageDefenseBonus` is set on a monster template, the
other must be too, and `EnrageThresholdFraction` must be in `(0, 1]` — a monster with a threshold and
no bonus (or vice versa) fails at load, not mid-fight.

### Item 11 — the Enrage rule (new, D20System, generic — not per-monster)

One new rule/handler-level check in `fight`'s existing flow, reading the two new ROM-tier
`MonsterTemplate` fields above (already carried through from the World Module by the translation
step, same lookup pattern `MonsterId`/`InflictsPoison` already use per Phase 2.10):

After the PC's attack step reduces `CombatRound.MonsterHp` (and the fight didn't end on that hit):
if `EnrageThresholdFraction` is set, `MonsterHp <= EnrageThresholdFraction * MaxHp`, and
`MonsterEnraged` doesn't already exist for this room, insert `MonsterEnraged(RoomId)` and announce
an enrage flavor key. The monster's *effective* Defense for any subsequent PC attack this fight
becomes `Stats.Defense + EnrageDefenseBonus` while `MonsterEnraged` is present — purely defensive;
Enrage does not affect the monster's own attack roll (`FightCommandHandler.cs`'s counterattack branch
never reads the monster's Defense at all, only its `AttackBonus` against the PC's Defense, so there is
no mechanism for a Defense bonus to touch it). Retracted with `CombatRound`, same lifecycle as
`Poisoned`'s combat-scoped cousin would
be if it had one (it doesn't — `Poisoned` is deliberately cross-room, `MonsterEnraged` is
deliberately combat-scoped; the contrast is worth keeping straight, not a copy-paste of the same
shape).

Assign this to exactly one "Signal Static" monster (Baraka's/Juno's implementation pick) —
`EnrageThresholdFraction: 0.5, EnrageDefenseBonus: 3`.

### Item 12 — two deliberately-broken Lua scripts

**Marshal-failure probe:** a `.lua` file whose `challenges` table includes an entry with
`dc = "ten"` (a string where the marshaler expects a number). Expected: `LuaOneD20WorldModule`'s
constructor throws during marshaling, naming the offending challenge key and field —
`OneD20WorldModuleValidator` never runs.

**Validator-failure probe:** a second `.lua` file, marshal-clean (every field the right type) but
supplying only 4 skill/save challenges against the grid's 6-slot requirement. Expected:
`LuaOneD20WorldModule` constructs successfully; `OneD20WorldModuleValidator` rejects it at the
existing pool-size check `world-module.md` already specifies ("supplied pools are large enough to
fill the grid's fixed slot counts").

Neither probe script is ever assembled into a playable session — same discipline
`phase-2-11-capability-stress-test-spec.md` uses for its clash/cycle probes: these exist to be
rejected, verified via a test fixture, not wired into `Program.cs`.

## Commands & handlers

No new player-facing commands. `fight`'s existing handler gains the Enrage check (item 11) as an
additional step in its already-specified sequence — no new command, no change to its signature.

## Acceptance criteria

- [ ] **Item 9:** "Signal Static" loads, validates, and plays through a full session (a win via a
      combat-free line, a win via a line touching combat, at least one `RoomFailed`) with zero
      changes to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, or `OneD20SessionAssembler` —
      same acceptance bar `phase-2-9.md` held for Hollow Keep, now against genuinely new content.
- [ ] **Item 9:** at least one Save-kind challenge and one `InflictsPoison` monster round-trip
      correctly through the Lua marshal path — verified against the fields' actual values in the
      constructed `IOneD20WorldModule`, not just that the session doesn't crash.
- [ ] **Item 10:** the procedurally-generated monster roster's stats match the loop's arithmetic
      exactly (verified against the five expected `Defense`/`MaxHp` values computed by hand from the
      same base-plus-step formula) — confirms the marshaler is indifferent to literal-vs-generated
      Lua tables, not just that *a* roster of five monsters exists.
- [ ] **Item 11:** a fight against the Enrage-flagged monster shows its Defense increase exactly once
      it crosses the HP threshold — verified via a transcript comparing the PC's hit rate before and
      after the threshold (the same monster, same PC attack bonus, a materially different hit
      outcome expected post-threshold).
- [ ] **Item 11:** `MonsterEnraged` is retracted when its `CombatRound` ends (win, loss, timeout, or
      flee) — a fresh fight against the same monster in a retry starts un-enraged.
- [ ] **Item 11:** every existing monster (Phase 2.7.1's five, Hollow Keep's own roster) continues to
      construct and fight correctly with both new fields absent — no migration needed, no regression.
- [ ] **Item 11:** the Enrage fields survive the full chain — authored on `OneD20MonsterTemplate` in
      "Signal Static"'s Lua source, through `LuaOneD20WorldModule`'s marshaling, through
      `Program.cs`'s translation, into the ROM-tier `MonsterTemplate` fact the `fight` rule actually
      reads — verified end to end, not just at the Rulebook-owned shape.
- [ ] **Item 11:** `OneD20WorldModuleValidator` rejects a monster template with only one of
      `EnrageThresholdFraction`/`EnrageDefenseBonus` set, and one with `EnrageThresholdFraction`
      outside `(0, 1]`.
- [ ] **Item 12:** the marshal-failure probe throws at `LuaOneD20WorldModule` construction, naming
      the offending field — `OneD20WorldModuleValidator` is never reached (verified by a call-count
      or equivalent, not just the end exception).
- [ ] **Item 12:** the validator-failure probe constructs successfully but is rejected by
      `OneD20WorldModuleValidator`'s existing pool-size check, with an error naming the shortfall.
- [ ] No existing acceptance criterion from `lua-world-module-spec.md`, `1-d20-spec.md`,
      `loot-rom-content-expansion-spec.md`, or `phase-2-10-content-coverage-spec.md` regresses.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

**Item 11 is the one item in this spec with a real dependency, already flagged, not hidden in
prose:** it extends `OneD20MonsterTemplate` (a Rulebook-owned type) with two nullable fields and adds
one generic rule to D20System interpreting them. Both are ROM/Rulebook-tier content changes — no
`ComponentRegistry`, `Capability`, or `EngineTier` involvement, so **no ticket to the Architect is
anticipated for the mechanism itself**. What *is* a real dependency, per Amara's ruling on ticket
0067: **the Phase 4 GM-Overlay kickoff pass should read this item's result before finalizing
`gm-overlay.md`'s "effect fact"/`MonsterBehaviorOverride` draft**, since a threshold-triggered stat
change is structurally the same shape that draft is trying to solve for live GM improvisation. This
spec doesn't implement anything for that tier — it produces the data point.

## Open questions / edge cases

- **Whether Enrage should be a named, reusable concept (`OneD20MonsterTemplate.EnrageThresholdFraction`)
  or the first instance of a more general "conditional stat modifier" shape** — deliberately left as
  the narrower, single-purpose version here, per the same "don't generalize past one real consumer"
  discipline `non-goals.md` applies everywhere else. A second monster needing a *different* kind of
  conditional behavior is the trigger for generalizing, not this spec.
- **Whether "Signal Static"'s full flavor text should live in a companion `-flavor.md` file** the way
  `1-d20-flavor.md`/`loot-rom-content-expansion-flavor.md` do, even though the actual text lives
  inside the `.lua` file's `flavorText` table for this authoring path — a documentation-consistency
  question, not a mechanism one; Juno's call at implementation, doesn't block acceptance.
- **Item 10's arithmetic is intentionally simple** — if it turns out MoonSharp handles this cleanly
  (expected), that's a small positive data point for `gm-overlay.md`'s open question about reusing
  MoonSharp for the harder, repeatedly-evaluated reactive case, not proof of it either way (see that
  document's own framing of why the two cases aren't the same question).

## Related documents

- [`lua-world-module-spec.md`](lua-world-module-spec.md) — the mechanism (host, marshaling,
  two-stage failure) this spec's content exercises, unchanged
- [`world-module-extraction-spec.md`](world-module-extraction-spec.md) — `IOneD20WorldModule`,
  `OneD20WorldModuleValidator`, and the Rulebook-owned record shapes item 11 extends
- [`phase-2-10-content-coverage-spec.md`](phase-2-10-content-coverage-spec.md) — the `Save`-kind
  challenge and `InflictsPoison` fields item 9 reuses, and `Poisoned`'s cross-room lifecycle
  contrasted against `MonsterEnraged`'s combat-scoped one in item 11
- [`../design/world-module.md`](../design/world-module.md) — the two-authoring-paths design and the
  additive-composition principle this spec's second Lua-authored module is a further proof point for
- [`../design/gm-overlay.md`](../design/gm-overlay.md) — the still-draft Phase 4 tier item 11's
  result is a direct input to (`MonsterBehaviorOverride`/"effect fact"), and item 10's secondary
  relevance to its MoonSharp-reuse question
- [`../design/non-goals.md`](../design/non-goals.md) — confirms item 11's scoped change does not
  trigger the separate `IWorldModule`-generalization deferral
- [`phase-2-11-capability-stress-test-spec.md`](phase-2-11-capability-stress-test-spec.md) — the
  sibling spec whose probe-content discipline (never wired into `Program.cs`) item 12 follows
- [`../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md`](../Roadmap/Proposals/2-10-through-2-12-content-framework-stress-test.md) —
  the reviewed proposal this spec implements Phase 2.12 of
- [Ticket 0067](../ClaudeWiki/Processes/Handoffs/0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) —
  the proposal's review/approval, including Amara's item-11 sequencing ruling
- [`content-spec-template.md`](../ClaudeWiki/Processes/content-spec-template.md) — the template this follows
