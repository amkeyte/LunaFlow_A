# 0043 — Generated rules have no seam for Rulebook-supplied flavor text

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-29
**For:** Amara (Architect)

## Decision

<Open — no ruling yet. Fill in on close per `handoff-tickets.md`'s convention.>

## Details

### Context

Building ticket [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)
(the GridMap/D20System Cartridge split) meant bringing `move`/`look`/`check`/`fight`/`flee` up to
`1-d20-spec.md`'s full flavor-text narration. That was straightforward for these — they're
hand-written `ICommandHandler`s, and handlers have a constructor/DI seam: `Program.cs` (the
composition root) passes `OneD20FlavorText.Text` in directly, same pattern as any other static
Scenario data.

The declarative rules — the 9 challenge-prompt rules and `GameEndRule`, both authored as
`RulePrototype`s through `GeneratorServices`/`RuleExport` per Phase 2.5's condition — have no such
seam. `engine-core.md` says this plainly: "Generated rule code has no constructor or DI seam." The
pre-split, shipped-and-QA-verified version of this content solved it by reaching a static holder —
`D20SystemContent.FlavorText[...]` — from inside the generated `Then()` lambda. That's exactly the
static-mutable-content-holder pattern `D20SystemContent`'s own deprecation (Phase 2.7) just retired;
using it again for the new split content would be building the split on top of the thing being
torn down.

The split spec itself names this as open and hands the resolution to whoever builds it: "Whether
`RoomChallenge`/`GridRoomData` need to be seeded as NRules facts... or can stay plain dictionary
lookups in generated `Then()` code. Not decided here; `RulePrototype` generation shape is Baraka's
translation of this spec's stated behavior" (`grid-rom-d20-split-spec.md`, Open questions). I raised
two options with the user rather than deciding unilaterally, since it reopens a hard-won "no static
globals" principle:

- **A.** A narrow, write-once-at-startup static flavor-text holder per rule group (e.g.
  `D20SystemRuleGroup.FlavorText`/`GridMapRuleGroup.FlavorText`), set exactly once by `Program.cs`
  immediately before `RuleExport.Compile(...)`, read-only from then on. Much narrower than
  `D20SystemContent` — no mutable game state, no `Configure()` re-entrancy — but still a static
  global.
- **C.** Leave the declarative rules announcing generic placeholder text for now (what they already
  do today), and raise this as a design question rather than deciding it inline.

**User chose C.** `GridMapRuleGroup`'s win-line rules/`GameEndRule` and `D20SystemRuleGroup`'s
challenge-prompt rules still announce their existing generic placeholder strings (e.g.
`"[Room {roomId}] Challenge prompt."`, `"You win!\n"`), not real flavor keys. This is a real,
disclosed gap against `1-d20-spec.md`'s stated behavior for those two rule families specifically —
not silently decided, flagging it here per house convention.

### Ask

Rule on how generated `RulePrototype` rules should reach Rulebook-supplied runtime content (flavor
text today; likely not the last case) without reintroducing a static, mutable, cross-cutting holder
like the one just deprecated. This will recur — any future generated rule that needs to announce
Rulebook-authored prose hits the identical wall. Candidate shapes worth weighing (not exhaustive,
Amara's call):

- A narrow, explicitly-immutable-after-seed static table scoped to just this concern (option A
  above), accepted as a deliberately smaller exception to "no static globals" than `D20SystemContent`
  was.
- Seeding the needed data (`RoomChallenge`, `GridRoomData`) as NRules facts themselves, so a rule's
  `Match<T>` can capture the fact and read its own key fields directly — sidesteps a lookup table
  entirely for the *key*, but still needs something to turn a key into displayable text.
- Something at the `GeneratorServices`/`RuleExport` pipeline level — a compile-time constant baked
  into generated code, if the Rulebook's content is fixed at rule-compile time (it is, for 1.d20, but
  is that a safe general assumption?).

Whatever's ruled, `GridMapRuleGroup`'s win-line/`GameEndRule` and `D20SystemRuleGroup`'s
challenge-prompt rules are the concrete first consumer — ready to update once there's a shape to
build against.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
