# 0015 — `IMechanic` needs real members: dice rolling + generic check resolution

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-20
**For:** Architect (Amara)

## Context

[`1-d20-spec.md`](../../../Specs/1-d20/1-d20-spec.md) (Juno, reviewed by Amara) is the buildable
spec for [Phase 2.5](../../../Roadmap/phase-2-5.md). Its **Framework dependencies** section names
a real, load-bearing gap rather than a someday-maybe: `IMechanic` is currently an empty marker
interface in `LunaFlow.Rules`, and both new command handlers (`check`, `fight`) need it as a real
operation, not hand-rolled per-handler dice arithmetic.

This was previously deferred in [`1-d20.md`](../../../design/1-d20.md)'s Open Seams as "ticket to
Amara once the spec makes the shape concrete" — the spec now exists and makes it concrete, so
filing this for real rather than leaving it flagged-in-prose only.

## Ask

Two things, both named explicitly in `1-d20-spec.md`'s Framework Dependencies section:

1. **Give `IMechanic` real members** backing two operations both `check` and `fight` need as the
   same underlying primitive with different inputs: (a) rolling a die (`1d20`, plus arbitrary
   damage dice like `1d8`), and (b) resolving a generic `d20 + modifier vs. target number` check.
   This is close to `glossary.md`'s own canonical Mechanic example ("a `Dice` object and how to
   roll it").
2. **Rule on `IMechanic`'s assembly/visibility home.** It's `internal` to `LunaFlow.Rules` today.
   The spec's own System/Scenario framing suggests a concrete resolution Mechanic fits
   `LunaFlow.Rom` instead (`architecture.md`'s "Dice object" example reads ROM-tier by that
   precedent) — which would need the same cross-assembly treatment ticket 0014 gave `IEngineHost`.
   Juno's spec deliberately doesn't presume an answer; it just needs one call signature to target.

Command handlers call this directly (same pattern `MoveCommandHandler` uses for
`LunaFlowEngine.Host.Rules` today) — no pipeline involvement. Declarative rules (challenge-prompt,
win-line, `GameEndRule`) are unaffected by this ticket; they're the separate, already-scoped
`GeneratorServices` half of the spec.

## Response

Ruling against item 1 as literally worded — "give `IMechanic` real members backing dice-rolling
and check-resolution" would bake one Mechanic's shape into the shared contract every Mechanic is
supposed to implement. Caught before it landed in code, not after — flagging so the reasoning's on
record for whoever builds the next Mechanic and wonders why `IMechanic` looks the way it does.

**Why this is wrong, not just stylistic:** `glossary.md`'s own definition is deliberately general —
"tracks or mutates multiple facts in a structured, hard-wired way, as opposed to a single
declarative rule." Nothing about dice. "A `Dice` object and how to roll it" is the *example*
`architecture.md` reaches for every time (repeated in `glossary.md`, `1-d20.md`, and this ticket),
and `architecture.md` names a second one in the same breath — "a Physics ROM's trajectory/
intersection math." If `RollDie`/`ResolveCheck` land on `IMechanic` itself, a future Physics
Mechanic can't implement it without carrying two dice methods that mean nothing to trajectory math.
That's the same "generalize from one example" failure `non-goals.md` exists to name, just showing
up as interface pollution instead of a visibility question this time.

**The precedent already exists in checked-in code, not just doc-prose.** `ARule : NRule,
IHasLunaFlowReference` — every rule gets `Reference` for traceability and *nothing* about what the
rule actually matches or does; that's left entirely to the concrete rule class.
`ILunaFlowReference.cs`'s own doc comment already says "Each Rule, Mechanic or Fact has a
reference" — Mechanic carrying one isn't a new idea, it's an existing, unfulfilled promise sitting
in the doc comments today.

**Ruling:**

1. `IMechanic` (`src/LunaFlow.Rules/IMechanic.cs`) grows exactly one thing: `IHasLunaFlowReference`.
   `public interface IMechanic : IHasLunaFlowReference { }` — mirrors `ARule` exactly. No dice
   members, no check-resolution members, nothing operation-specific. This is the *category* marker,
   not an API surface.
2. The actual operations 1.d20 needs — rolling a die (`1d20`, arbitrary damage dice like `1d8`) and
   resolving a generic `d20 + modifier vs. target number` check — belong on a **concrete class
   implementing `IMechanic`**, Baraka's to name and shape (illustratively, something like a
   `D20ResolutionMechanic` with a `Roll(string die)` and a `ResolveCheck(int modifier, int
   targetNumber)`-shaped pair of members, composed rather than duplicated per the "Mechanic calling
   Mechanic" composition already discussed with Arryn — but the exact signatures are implementation
   detail, not a spec call, same reasoning `dev.md` gives Baraka authority over local structure).
   Ticket 0015's item 1 is satisfied by this concrete class existing and `check`/`fight` calling it
   — not by `IMechanic` itself growing anything beyond `Reference`.
3. **Item 2, ruled as expected:** the concrete Mechanic is genuinely ROM-tier content per
   `architecture.md`'s own "Dice object" example, and belongs in `LunaFlow.Rom` alongside
   `NineRoomsContent`/`MoveCommandHandler` — not inside `LunaFlow.Rules`. For a `LunaFlow.Rom` class
   to implement it, `IMechanic` goes `public` — same cross-assembly reasoning ticket 0014 already
   gave `IEngineHost` (a real, single consumer, not speculative). Low-risk here: `IMechanic` carries
   nothing sensitive even after this ruling, just `Reference`.

Net: `IMechanic` stays small and permanent-shaped; the d20-specific work is content, not framework,
and lives where content lives. Over to Baraka to implement — this is a shape ruling, not code.

## Resolution

Verified and closed by Kofi, 2026-07-20. Every citation in the ruling checked against actual
source, not taken on the ticket's word:

- `src/LunaFlow.Rules/IMechanic.cs` is currently exactly `internal interface IMechanic { }`, doc
  comment matching `glossary.md`'s definition verbatim.
- `ARule : NRule, IHasLunaFlowReference` confirmed in `ARule.cs`; `IHasLunaFlowReference` in
  `ILunaFlowReference.cs` really is the single-member shape (`ILunaFlowReference Reference { get; }`)
  `IMechanic` is being ruled to mirror.
- `ILunaFlowReference.cs`'s "Each Rule, Mechanic or Fact has a reference" doc comment is real,
  pre-existing — the ruling's "unfulfilled promise" framing is accurate, not invented.
- `IEngineHost` is genuinely `public` today, supporting the cross-assembly precedent cited for
  item 2 — though that publicness predates ticket 0014, which only scoped doc generation, not
  access modifiers. The analogy holds; the ticket-0014 attribution is slightly loose. Not worth a
  re-bounce, noting for the record.

Ruling accepted as a sound fork, same treatment ticket 0008's forks got: `IMechanic` stays a pure
category marker (matches `ARule`'s shape, avoids baking dice-specific members into a contract
every future Mechanic — Physics included — would have to inherit); the actual roll/check-resolve
operations land on a concrete `LunaFlow.Rom` class Baraka shapes, and `IMechanic` goes `public` to
support that. Satisfies ticket 0015's ask in substance even though it declines item 1 literally.
Over to Baraka for implementation — no further PM action.
