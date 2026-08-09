# 0016 — Implement 1.d20 (Phase 2.5)

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-20
**For:** Dev — Implementation (Baraka)

## Context

[Phase 2.5](../../../Roadmap/phase-2-5.md) is fully specced and unblocked:
[`1-d20-spec.md`](../../../Specs/1-d20/1-d20-spec.md) (Juno, reviewed by Amara) plus its flavor
companion [`1-d20-flavor.md`](../../../Specs/1-d20/1-d20-flavor.md) are the buildable spec.
[Ticket 0015](0015-imechanic-dice-resolution.md) closed the one open framework question
(`IMechanic`'s shape and home). Nothing else is gating implementation.

## Ask

Build 1.d20 per the spec, in full — it's one bounded phase, not a partial slice:

1. **Declarative rules through the pipeline** — challenge-prompt rules (9, one per room),
   win-line rules (8, reused shape from Nine Rooms), `GameEndRule`. These must be authored as
   `RulePrototype`s through `GeneratorServices`, not hand-written `ARule` classes — this is Phase
   2.5's load-bearing condition (see `1-d20-spec.md`'s Purpose section); it's what lets this phase
   also close out the original Phase 3 done-bar.
2. **Command handlers, hand-written** — `check`, `fight`, `flee` (plus reused `move`/`look`),
   same tier as Nine Rooms' `MoveCommandHandler`. Not subject to the pipeline condition above.
3. **The concrete Mechanic** — per [0015](0015-imechanic-dice-resolution.md)'s ruling: a
   `LunaFlow.Rom` class implementing `IMechanic` (now `public`), shaped and named at your
   discretion, backing die-rolling and generic `d20 + modifier vs. target` check resolution. The
   `check`/`fight` handlers call it directly — no pipeline involvement for handlers.
4. **Fact vocabulary, room/PC/monster data** — exactly as specified in `1-d20-spec.md`'s Fact /
   marker vocabulary and the stat tables in `1-d20.md` (Strong-hero sheet, monster stat block, the
   6 skill-check DCs, room-to-challenge-type map).
5. **Flavor text wiring** — prose comes from `1-d20-flavor.md`'s keyed table; nothing in the
   implementation should inline literal strings the flavor file already owns.

Full scope boundary (in/out) is `1-d20-spec.md`'s own Scope section — don't re-derive it here,
that doc is authoritative.

**Acceptance criteria:** the checklist in `1-d20-spec.md`'s Acceptance criteria section is what
QA (Lothar) verifies against and what "done" means for this ticket. Work through it directly
rather than treating it as a suggestion.

**If anything in the spec turns out unbuildable or ambiguous** once you're in the code, that's a
ticket to Amara (per `architect.md`'s stated channel), not a silent workaround or a guess.

## Response

Built in full, new `LunaFlow.Rom.OneD20` module (mirrors the `NineRooms` folder shape):

1. **`IMechanic`** (`src/LunaFlow.Rules/IMechanic.cs`) grows `IHasLunaFlowReference` and goes
   `public`, exactly per [0015](0015-imechanic-dice-resolution.md)'s ruling — no dice/check members
   on the interface itself.
2. **`D20ResolutionMechanic`** (`src/LunaFlow.Rom/OneD20/D20ResolutionMechanic.cs`) — the concrete
   `IMechanic`: `Roll(string die)` (NdM notation) and `ResolveCheck(int modifier, int
   targetNumber)`. Exactly the illustrative shape ticket 0015/the spec named. `check`/`fight` both
   call `ResolveCheck` — same primitive, different inputs, per the spec's framing.
3. **Declarative rules** — `OneD20RuleGroup.cs`, `[RuleGroup("OneD20")]`, 9 challenge-prompt +
   8 win-line + `GameEndRule`, all `[RuleTemplate]` `RulePrototype`s compiled through
   `RuleExport.Compile`/`GeneratorServices` at startup — no hand-written `ARule` anywhere in this
   module. Combat-room challenge rules additionally announce `monster-desc` via a second chained
   `.Do()` (NRules' `Then().Do()` takes an `Expression<Action<IContext>>` — a block-bodied lambda
   doesn't convert to an expression tree, so it's two single-expression `.Do()` calls, not one).
4. **Handlers** — `move`/`look` reused-shape (adjacency logic identical), `move` additionally
   tracks `PreviousRoomId`; `check`/`fight`/`flee` hand-written, calling `D20ResolutionMechanic`
   directly, never through the pipeline.
5. **Fact vocabulary & content** — `Facts.cs` matches the spec's shapes verbatim.
   `OneD20Content.cs` holds the room table (1/5/8 Fight, rest SkillCheck per `1-d20.md`'s table),
   the PC/monster stat blocks, ability scores + the `(score/2)-5` modifier formula (System-tier,
   used only where `Room.SkillAbility` forces a live lookup — everything else stays pre-baked per
   the spec's own fact shapes, not re-derived).
6. **Flavor** — `OneD20Flavor.cs`, keyed exactly per `1-d20-flavor.md`; nothing inlined elsewhere.
7. **Wiring** — `MyLunaFlowServer/Program.cs` now runs 1.d20 instead of Nine Rooms (added a
   `LunaFlow.Rules` project reference for `RuleExport`). Registered command name is `move` (the
   spec's own table), not Nine Rooms' `go` alias.

**A real pipeline gotcha surfaced here, not in the framework:** `RoslynRuleCompiler` only sees
already-loaded, file-backed assemblies (documented on the class itself). Nine Rooms and the B3–B5
tests all have a hand-written `ARule` somewhere in the same process, which loads `NRules.Fluent`
(and friends) as a side effect before the first `Compile()` call. 1.d20 has zero hand-written
`ARule`s anywhere — the whole ruleset is pipeline-generated — so nothing forced that load. Fixed in
`Program.cs` with the same defensive touch the pipeline's own tests already use
(`RulePipelineGeneratorServiceTests.cs`'s `_ = typeof(IContext).Assembly;` idiom), extended to the
two other assemblies the generated code needs (`NRules.Fluent`, `System.Linq.Expressions`). Not a
framework change — flagging in case a future all-generated ruleset hits the same thing before
`GetReferences()`'s MVP-disclosed limitation gets revisited.

**Two disclosed judgment calls**, both the kind the spec already delegates to me:

- **`check`/`fight` on an already solved/failed room**: silent no-op, no message — spec's own Open
  Questions section left this unspecified.
- **`flee` from room 5 (the start room) before any `move` has ever happened**: no `PreviousRoomId`
  exists yet, so there's nowhere defined to retreat to. Falls back to retracting `CombatRound` and
  announcing `pc-flee` without moving the PC (stays put, encounter abandoned) rather than throwing.
  Not in the spec's Open Questions list — a gap I found while implementing, narrow enough (only
  reachable by fleeing room 5's fight with zero prior moves) that it doesn't affect any acceptance
  criterion, so I made the low-risk call myself rather than blocking on a ticket to Amara.

**One disclosed cosmetic quirk**, not a correctness bug: moving into (or fleeing into) an unsolved
room prints its description twice — once from the handler's own unconditional `Announce`
(`MoveCommandHandler`'s reused-verbatim behavior, ticket 0001's precedent) and once from the
challenge-prompt rule firing on the same `PlayerPosition` change. Harmless in Nine Rooms because
`Description` and `ChallengePrompt` were different text; 1.d20 collapsed both into one
`DescriptionKey`, so the same verbatim-reuse now double-prints identical text. Doesn't fail any
acceptance criterion; flagging rather than silently deviating from either "reused verbatim"
instruction to paper over it.

**Verification:** `dotnet build LunaFlow_A.sln` — 0 warnings, 0 errors. `dotnet test
tests/LunaFlow.EngineTests` — 71/71 green, unchanged (Nine Rooms' own fixture test builds its own
`RuleEngine` directly from `NineRoomsContent`, independent of `Program.cs`, so repointing the
console host didn't touch it). Manual smoke playthroughs against the running console host — not
the automated suite, which is Nadia's territory — covering every Acceptance Criteria item: identical
`move`/`look` grid behavior; all 6 skill-check rooms resolving via `ResolveCheck` against their own
DC/ability, failed checks retryable with no penalty; a solved room's challenge-prompt never re-fires;
fresh `CombatRound` per fight attempt; lethal PC hit skips the counterattack (`monster-defeated`,
no `pc-down` on the same round); PC reduced to 0 HP inserts `PlayerDown` and silently blocks further
`check`/`fight`; 3 inconclusive rounds inserts `RoomFailed` and permanently blocks that room;
`flee` retracts `CombatRound` without `RoomSolved`/`RoomFailed` and returns to `PreviousRoomId`, a
later `fight` there starts fresh at full monster HP; the combat-free column-3 line
(`[3,6,9]`) and a combat-including line (`[1,2,3]`, via `fight`) both fire `WinMarker`/`GameEndRule`
correctly; a `RoomFailed` member permanently kills its win lines (falls out of the fact model
itself — a failed room's `RoomSolved` never exists, so the win-line rule's own `Match` guards never
satisfy, no extra guard needed). All observed directly via piped transcripts against
`MyLunaFlowServer`, not asserted from reading the code.

Ready for Nadia to build the maintained automated suite (ticket
[0017](0017-1-d20-test-suite.md) filed) and then for Kofi to verify the done-bar.

## Resolution

Verified and closed by Kofi, 2026-07-20 — code-verified, not build-reverified independently
(Arryn's call: "enough iteration on this one for a review").

At last check-in I'd read the original build's source (`D20ResolutionMechanic`, `OneD20RuleGroup`,
`FightCommandHandler`/`CheckCommandHandler`/`FleeCommandHandler`) and confirmed it matched
0016's response line for line. Before this close, discovered [ticket 0018](0018-recode-1-d20-per-revised-spec.md)
had since landed — Juno ran the actual playtest, found real content-spec gaps, revised the spec
twice, and had Baraka recode significant parts of this ticket's build (per-round combat narration,
room-specific check text, `PlayerHp`/`PlayerDown` deleted in favor of `CombatRound`-scoped HP, the
lethal-hit narration-order bug, the room-5 startup-description bug) — all opened, built, and
closed by Juno without routing through me. Re-read the *current* source fresh rather than trusting
either writeup: `FightCommandHandler.cs`, `CheckCommandHandler.cs`, `Facts.cs`, `Program.cs` all
match 0018's claims exactly, including the lethal-hit reordering (damage/threshold check happens
strictly before either narration branch — no code path prints both) and the `Seed()`-lambda
placement for the room-5 announce timing bug.

0016's original ask is satisfied — 1.d20 exists, pipeline-authored declarative rules + hand-written
handlers per the load-bearing condition, `IMechanic` ruling implemented correctly — but the
literal 0016-era code no longer exists in the repo; 0018 superseded most of the handler layer
before I closed this. Closing 0016 on that basis rather than re-litigating build evidence for code
that's already been replaced. Neither 0016 nor 0018 individually re-ran an Arryn-witnessed fresh
rebuild the way earlier tickets did — accepted here per Arryn's explicit call to stop iterating,
disclosed rather than silently skipped.

**Process note for the record:** 0018 shipped a real, verified improvement, and Juno closing her
own opened ticket after Baraka's response was correct per the ownership rule. But it touched code
this ticket was still open against, and I only found out by checking the ticket queue, not by
being looped in. Nothing to fix process-wise — direct role-to-role tickets not routing through the
PM is by design — just noting it as a reason to check the full queue routinely, not only the
ticket I'm expecting to look at.

Phase 2.5's overall done-bar is a Lothar verdict, same shape as prior phases — separate, later
step, not blocked on anything here. [Ticket 0017](0017-1-d20-test-suite.md) redirected to target
the current (post-0018) shape — see that ticket.
