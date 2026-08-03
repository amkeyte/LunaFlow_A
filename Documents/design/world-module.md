# World Module — the content tier above Rulebook

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

A **World Module** supplies concrete content instances to a Rulebook: the specific room-to-challenge
assignments, the specific monster stat blocks, the specific loot table, the specific flavor prose.
The Rulebook supplies *structure* — which challenge types exist, what shape an opponent or a room
needs to have to satisfy one, how session content gets assembled and validated. [`architecture.md`'s
role table](architecture.md#system-setting-and-session) has drawn this line since before either tier
was real code: "World-Builder fills in the specifics once the rule exists." This document makes it a
real type boundary, per [Phase 2.8](../Roadmap/phase-2-8.md).

## Why this is a smaller problem than Cartridge

[`cartridge.md`](cartridge.md) needed a neutral third assembly because two peer ROMs sit at the
*same* tier and neither may reference the other. World Module and Rulebook don't have that problem —
they sit at *different*, already-ordered tiers (`EngineTier.Rulebook < EngineTier.WorldModule`,
per [`engine-core.md`](engine-core.md#two-interfaces-not-one)'s tier enum, unused for this purpose
until now), and [`architecture.md`](architecture.md#quick-reference)'s stack rule is that each tier
depends only on the tier(s) below it. A World Module may reference the Rulebook's assembly directly
to implement its shape contracts. The Rulebook must not reference any World Module — same asymmetry
as the Rulebook/ROM boundary, one tier up, and no contract-surface project is needed to hold it
open, because there's exactly one direction to hold.

**One rule, and everything else follows from it:**

> A World Module implements shape contracts the Rulebook defines and owns. The Rulebook never names
> a World Module type.

## Structure

Unlike a Cartridge, a World Module isn't a published pairing of two things — it's one thing: a class
implementing whatever data-shape interface its Rulebook declares. For OneD20, that's
`IOneD20WorldModule` (`LunaFlow.Rulebook.OneD20`, alongside the shape records it returns):

```csharp
public interface IOneD20WorldModule
{
    IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; }
    int StartRoomId { get; }
    IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; }
    IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters { get; }
    IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; }
    OneD20CombatantStats PlayerCharacter { get; }
    IReadOnlyDictionary<string, int> PlayerAbilityScores { get; }
    IReadOnlyDictionary<string, string> FlavorText { get; }
}
```

`OneD20RoomData`/`OneD20ChallengeTemplate`/`OneD20MonsterTemplate`/`OneD20CombatantStats` stay
Rulebook-owned record *shapes* — they're the "what fields a room needs" structure. A World Module
populates them; it doesn't get to redefine them. This is deliberately **not** a generic
`IWorldModule` usable by any Rulebook — per `non-goals.md`'s standing discipline, that abstraction
gets built once a second Rulebook exists needing one, not speculatively on the strength of 1.d20
being the only consumer.

## Validation stays with the Rulebook, not the World Module

The Rulebook is the trusted side of this boundary — same reasoning `cartridge.md` gives for why
ROMs validate what a composition root hands them. A World Module's supplied content is data, handed
down; the Rulebook's assembly code (`OneD20SessionAssembler`, extended with a validation pass) checks
it at the boundary before using it: room IDs cover exactly the fixed grid's 9 positions, every room's
`ChallengeType` is a known enum value, skill-check fields are present exactly when required and
absent otherwise, and — this is the concrete failure mode a swap could actually hit — the supplied
`Challenges`/`Monsters` pools are large enough to fill the grid's fixed slot counts (≥6 skill-check
challenges, ≥3 monsters, per `phase-2-7-1.md`'s randomization). A World Module that fails this check
fails at load, not mid-session.

## The session-assembly coupling this extraction has to fix

`OneD20SessionAssembler.Assemble()` currently reads `OneD20RulebookContent`'s static fields directly
— its N-of-M draw is sized off whatever `.Count` those dictionaries happen to have. That's the one
real piece of *structure* (the shuffle-and-slot algorithm) that was quietly coupled to a specific
*instance's* pool sizes. Fixing it is what makes the second-World-Module proof meaningful rather than
cosmetic: `Assemble()` takes an `IOneD20WorldModule` as an argument (or the assembler's constructor
does), reads pools from it, and the algorithm itself stops knowing or caring how many entries a given
World Module happens to supply.

## Composition, and what's deliberately not built yet

`Program.cs` constructs a concrete `IOneD20WorldModule` and hands it to Rulebook-tier assembly code,
which validates it and produces the session assignment Program.cs then translates into ROM-owned
types — exactly the existing translation path, with one new input at the top. No `ComponentRegistry`
registration, no `IHasDependencies` on `IOneD20WorldModule`, no `EngineTier.WorldModule` enforcement
beyond the enum value already existing. That machinery solves peer-correlation and load-order
problems this boundary doesn't have: a World Module doesn't correlate NRules facts with a peer, and
there's exactly one of it feeding one Rulebook in this phase's proof. **Revisit when:** a second
Rulebook needs the same World Module contract generalized past OneD20, or the additive-composition
principle below is actually exercised with two World Modules loaded at once and something needs to
arbitrate between them — not before.

## Additive composition (see also `architecture.md`)

Loading a World Module doesn't preclude loading another alongside it — the full principle and its
consequences are recorded in
[`architecture.md`'s "System, setting, and session"](architecture.md#system-setting-and-session),
per this phase's own Deliverables, not duplicated here. What's specific to this document: the shape
contract above is written so a second, independently-authored `IOneD20WorldModule` is a real swap,
not a relabeling — nothing in `OneD20SessionAssembler` or the Rulebook's validation pass may assume
facts about *content* (specific room counts beyond the grid's fixed 9 slots, specific monster names,
specific DCs) that only the default implementation happens to satisfy.

## Related documents

- [`architecture.md`](architecture.md) — the four-tier diagram and trust gradient this tier fills in
- [`cartridge.md`](cartridge.md) — the sibling boundary problem one tier down, and why its solution
  (a neutral contract-surface assembly) is more machinery than this boundary needs
- [`engine-core.md`](engine-core.md) — `EngineTier`, and the "three kinds of state" push-down pattern
  this tier is one more instance of
- [`non-goals.md`](non-goals.md) — why `IOneD20WorldModule` isn't generalized to `IWorldModule` yet
- [`../Roadmap/phase-2-8.md`](../Roadmap/phase-2-8.md) — the phase this document answers
- [`../Specs/world-module-extraction-spec.md`](../Specs/world-module-extraction-spec.md) — the
  buildable spec for extracting 1.d20's content against this design
