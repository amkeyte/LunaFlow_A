# 0043 — Generated rules have no seam for Rulebook-supplied flavor text

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-29
**For:** Amara (Architect)

## Decision

**Ruling: seed the resolved text as a fact, same as pre-split.** `Room.ChallengePrompt` (NineRooms,
pre-split) was never a key requiring lookup — it was already-resolved text, sitting on a fact the
rule matched and read directly. That's the pattern to restore, not invent: a small, purpose-built
fact per generated-rule consumer (`RoomChallengePrompt(int RoomId, string Text)` for the
challenge-prompt rules, a one-instance `GameEndAnnouncement(string Text)` for `GameEndRule`),
seeded once at startup by `Program.cs` — which already resolves `OneD20FlavorText.Text` keys for
every hand-written handler — via the existing `.Seed(...)` mechanism already proven for
`GridRoomData`/`PlayerPosition`/`PlayerCharacter`/`Monster`. No static holder, no dictionary lookup
inside generated code, no change to `RoomChallenge`'s existing key fields (handlers keep resolving
those themselves, unaffected). General answer for recurrence: any future generated rule needing
Rulebook prose gets one small fact carrying the *already-resolved* string, never a key and never the
raw flavor dictionary — resolution happens once, at the composition root, where both the Rulebook's
flavor map and the ROM's structural data already meet.

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

None of your three candidates is quite it, but the second is closest and the fix is smaller than it
looked from inside the build — I went and checked what the pre-split code actually did rather than
reasoning from `engine-core.md`'s "no DI seam" line in the abstract. `NineRooms/ChallengeRules.cs`'s
`Room1ChallengeRule` (still in the tree) does exactly this:

```csharp
Room room = null!;
When().Match<PlayerPosition>(p => p.RoomId == 1).Match<Room>(() => room, r => r.Id == 1)...
Then().Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
```

`room.ChallengePrompt` was never a key needing a lookup table — it was the **already-resolved
string**, sitting on a fact the rule matched and captured. There was no static holder for this case
pre-split; `D20SystemContent.FlavorText[...]` (the thing we're rightly avoiding rebuilding) was
something else, a different content type's seam. So this isn't "pick between a narrower static
table and fact-seeding" — fact-seeding with pre-resolved text is what already shipped and passed QA,
just for a fact shape (`Room`) that split apart and needs its replacement named.

**Concretely, two new facts, both content, both D20-Cartridge-side (no cross-ROM question here —
each is matched by the rule in the same ROM that needs it):**

1. **`RoomChallengePrompt(int RoomId, string Text)`** in `LunaFlow.Rom.D20System`. `Program.cs`
   builds one per room — `flavorText[room.PromptKey]` for skill-check rooms, `flavorText["monster-desc"]`
   for fight rooms, mirroring `1-d20-spec.md`'s existing branching exactly — and adds them to the
   same `.Seed(() => ...)` block that already seeds `gridMapRoomData`/`PlayerPosition`/
   `PlayerCharacter`/`Monster`. `ChallengePromptRule`'s `WhenStatements` gains
   `Match<RoomChallengePrompt>(() => prompt, p => p.RoomId == {roomId})`; `ThenStatements` becomes
   `Do(ctx => LunaFlowEngine.Host.Announce(prompt.Text))` — no key, no dictionary, in generated code.
2. **`GameEndAnnouncement(string Text)`** in `LunaFlow.Rom.GridMap` — one instance, seeded once from
   `flavorText["win-announce"]` (confirmed the real key in `1-d20-flavor.md`: *"Three for three —
   that's enough to call it. Mission complete."*). `GameEndRule` gains
   `Match<GameEndAnnouncement>(() => end)` alongside its existing bare `Match<WinMarker>()`, and
   announces `end.Text` instead of the hardcoded `"You win!\n"` literal.

**`RoomChallenge`'s own `PromptKey`/`SuccessKey`/`FailureKey` fields are untouched** — the
hand-written handlers (`CheckCommandHandler`/`FightCommandHandler`) already resolve those themselves
via their own constructor-injected `flavorText`, and that's the right mechanism for them; nothing
about this ruling should make handler code start reading facts for content it already gets by DI.
The two new fact types exist specifically for the generated-rule seam, not as a general replacement
for `RoomChallenge`.

**Why not seed the flavor dictionary itself as a fact (candidate 2 as literally written)?** Because
then generated code would still need to *do a lookup* (`flavor.Text[challenge.PromptKey]`) — legal,
but it re-introduces "generated code reasons about keys" for no benefit over resolving once at the
one place (`Program.cs`) that already holds both the Rulebook's flavor map and the ROM's structural
data. Push the resolution to composition-root time; hand the generated rule only the finished string.
That's also the general answer for recurrence: **the fact a generated rule matches always carries
already-resolved text, never a key, and the flavor dictionary itself never needs a seam into
generated code at all.**

**Flag for whoever implements this:** these are two more facts in working memory at startup —
worth a check that `D20SystemRuleFixtureTests.cs`'s `ClassSetup` (already touched once by ticket
0044) doesn't assert an exact fact count anywhere that this would quietly break.

Once built: the three `ChallengePrompt_*`/`GameEnd_*`/win-line assertions ticket 0044 rewrote to
characterize the placeholder text (per this ticket's disclosed gap) get rewritten again to assert
the real flavor content — that's expected, not a regression, since those assertions were explicitly
marked as tracking this open ticket.

**Addendum, scaling — why this doesn't generalize further today.** Content volume (more rooms, more
keys) is a non-concern at any size this framework will plausibly see — NRules' alpha network filters
per type, not against total working memory, and this is a turn-based single-player loop. The real
cost axis is *how many distinct cases* of "a generated rule needs Rulebook prose" exist: each one, as
ruled, costs a new small fact type plus seed-wiring. Two cases today is cheap. The generalization
that removes that per-case cost is visible now: one fact type, `AnnouncementText(string Key, string
Text)`, seeded once per flavor key, any generated rule matching on `Key` instead of a bespoke type.
Not ruling it in — there are exactly two consumers, and building the keyed-general version off two
examples is the mistake this project has refused to make everywhere else (`non-goals.md`'s
discipline, the Cartridge itself waiting for a second real ROM pair). It also trades away real
value: `RoomChallengePrompt.RoomId : int` gets compile-time correlation safety a stringly-typed key
wouldn't. Revisit if a third distinct case shows up, or if per-case wiring actually starts to hurt.

One scaling property is already free, worth recording since it's easy to miss: this is per-Rulebook
content, seeded by that Rulebook's own composition root. A second Rulebook built on the same
GridMap/D20System Cartridge would seed its own text with its own strings — no coupling to OneD20's
flavor keys, no ROM-side change at all.

### Resolution

Closed 2026-07-30. Built exactly per the ruling above:

- `RoomChallengePrompt(int RoomId, string Text)` added to `LunaFlow.Rom.D20System`'s `Facts.cs`;
  `GameEndAnnouncement(string Text)` added to `LunaFlow.Rom.GridMap`'s `Facts.cs`. `RoomChallenge`'s
  own `PromptKey`/`SuccessKey`/`FailureKey` fields untouched, as specified.
- `D20SystemRuleGroup.ChallengePromptRule`: gained a `RoomChallengePrompt prompt = null!;` preamble
  and `Match<RoomChallengePrompt>(() => prompt, p => p.RoomId == roomId)`; `ThenStatements` now
  `Do(ctx => LunaFlowEngine.Host.Announce(prompt.Text))` — no key, no dictionary, in generated code.
- `GridMapRuleGroup.GameEndRule`: gained a `GameEndAnnouncement end = null!;` preamble and
  `Match<GameEndAnnouncement>(() => end)` alongside the existing `Match<WinMarker>()`; announces
  `end.Text` instead of the hardcoded `"You win!\n"` literal.
- `Program.cs`: new `.Seed(() => ...)` block resolves one `RoomChallengePrompt` per room
  (`flavorText["monster-desc"]` for Fight rooms, `flavorText[r.PromptKey!]` for SkillCheck rooms —
  the exact branching `1-d20-spec.md` specifies) plus one `GameEndAnnouncement` from
  `flavorText["win-announce"]`, right alongside the existing `PlayerPosition`/`PlayerCharacter`/
  `Monster` seeding.

**Verified via manual playthrough** (`dotnet run --project src/MyLunaFlowServer`), not just build
success: room 5's fight prompt now announces the real `monster-desc` text ("A seedy-looking man
steps out of the shadows..."), room 6's skill-check prompt announces its real `PromptKey` text ("No
way around it — you'll have to wade through (Swim)."), and completing the combat-free `[3,6,9]`
column announces the real `win-announce` text ("Three for three — that's enough to call it. Mission
complete.") — all three replacing the generic placeholders, end to end.

**Full suite: 111/119, 8 failures — all expected, none a regression**, confirmed by inspecting each:
they're exactly the placeholder-text assertions ticket 0044 wrote while explicitly tracking this
ticket (`ChallengePrompt_*_AnnouncesGenericPlaceholder`, `GameEnd_AnnouncesOnAnyWinMarker`), plus
`D20SystemHandlerTests.cs` cases where the rule now goes fully silent rather than showing different
text — its `StartSession()` test harness wires ROMs independently of `Program.cs` and doesn't seed
the two new facts, so `Match<RoomChallengePrompt>`/`Match<GameEndAnnouncement>` never matches there.
Both are test-harness updates (seed the two new facts in `StartSession()`/`ClassSetup`, rewrite the
placeholder assertions to check real content) — Nadia's territory, not touched here, consistent with
this ticket's own "expected, not a regression" note on exactly this outcome.
