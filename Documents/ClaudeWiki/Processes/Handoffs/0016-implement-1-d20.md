# 0016 — Implement 1.d20 (Phase 2.5)

**Status:** open
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

_(Baraka to fill in.)_

## Resolution

_(Opener to fill in on close.)_
