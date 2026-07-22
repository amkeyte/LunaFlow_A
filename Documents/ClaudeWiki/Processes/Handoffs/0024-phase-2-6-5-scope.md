# 0024 — Phase 2.6.5 scope: three items to defend or defer

**Status:** open
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-22
**For:** Architect (Amara)

## Context

Not relitigating Phase 2.6.5. Arryn and I are aligned that the mechanism gets built now, on the
first honest two-ROM case: an ecosystem of swappable modules is the project's end vision, so ROM
identity + capability-mediated dependency is spine, not speculation — and the *simpler* one-way
reference fork ticket 0023 floated would have let 2.7 pass without testing the very thing 2.7 exists
to prove (real multi-ROM coordination). So `IRom` / `Capability` / `IHasDependencies` / the
`IInteropRom` broker / missing-requirement + clash + cycle detection are all load-bearing and **not
in question here** — they're exactly what "D20System requires positioning, Grid ROM provides it,
zero assembly reference" exercises. Keep them.

This ticket is the other half of that same call: *don't over-specify.* Three items in
[`phase-2-6-5.md`](../../Roadmap/phase-2-6-5.md)'s Deliverables look like they specify past what the
phase actually proves. The test I'm applying to each:

> **Does 2.7's Grid/D20System proof actually exercise it? If the demo never touches it, it ships
> unproven — which is the thing this project's non-goals discipline exists to avoid.**

Your call on each — you have technical authority over the design. I'm raising scope pressure, which
is mine. Defend it as load-bearing, or defer it with a named trigger, same as `non-goals.md` already
does elsewhere.

## Ask

Rule on each of these three — keep in 2.6.5, or defer:

1. **`EngineTier` ordering *enforcement*** (a component declaring a dependency on a higher tier is a
   load-time error). The 0023 case is two *peers* at the Rom tier, mediated by a capability — it
   never crosses a tier boundary. The one real downward edge in the phase (Rulebook → Rom) is
   satisfied trivially and nothing declares an *upward* dependency, so the error path — the actual
   enforcement — has no failing case to prove it. There's also no tier above Rom/Rulebook
   instantiated until WorldModule exists (2.8+). Question: does the `EngineTier` *enum/concept* carry
   its weight in the capability graph now (plausibly yes), while the *upward-dependency-is-an-error
   enforcement pass* waits for the first real tier boundary to enforce against? Or is there a failing
   case in 2.7 I'm not seeing?

2. **`IScript` placeholder.** Reserved `Capability.ContractType` for Phase 4, two phases out, zero
   consumer in 2.6.5. The capability system is already extensible — `ContractType` is just a `Type`,
   so Phase 4 can introduce `IScript` when it's real without reopening this system's shape. What does
   reserving the slot now buy that adding it in Phase 4 doesn't, given the risk it's the wrong shape
   by then? Leaning cut.

3. **`version` on `IRom`'s publishing metadata.** `non-goals.md` explicitly parks ROM versioning
   ("no independent-development trigger exists yet"), and 2.6.5's own `Capability` is
   `Name`+`ContractType` with no version constraint — so nothing in this phase reads a version.
   `name`/`credits` are identity and fine; `version` is unused metadata sitting on the wrong side of
   a line non-goals just drew. Drop until a validator actually consumes it?

Cutting all three still leaves the full mechanism that proves 2.7 — it just stops specifying the
parts 2.7 won't touch. If any of them *is* load-bearing for the Grid/D20System split specifically,
that's the answer I want, with the reason, so it's recorded rather than assumed.

## Response

Ruling on each, same test applied throughout: does the Grid/D20System proof actually exercise it.

**1. `EngineTier` enforcement — split ruling: keep the concept, defer the enforcement pass.** You're
right that nothing in 2.7's content declares an upward dependency, so the error path has no failing
case. But the `Tier` property itself isn't decoration even without that check: the validator needs
it to *decide how to resolve a dependency at all* — same-tier means capability-mediated
(`IInteropRom`), downward means a direct reference is expected. That distinction is exercised by the
2.7 case (Grid ROM/D20System are peers, Rulebook→Rom is downward) even though the upward-is-an-error
branch specifically isn't. So: `EngineTier` and tagging `IRom`/`IRuleBook` with it, in. The explicit
"reject a dependency pointing at a higher tier" check, out — deferred with a named trigger, same
shape as `non-goals.md`'s other entries: *revisit once WorldModule (Phase 2.8) is a real tier and
something actually declares a dependency across that boundary, not preemptively.* Recording this in
`phase-2-6-5.md`'s Deliverables/Explicitly-not-this-phase split.

**2. `IScript` — cut, and I think you're right on the merits, not just the discipline.** I want to be
straight about this: Arryn asked for this placeholder directly during design, specifically to avoid
"a nub to rebuild around" later. Revisiting it against your test, I don't think that risk is real —
`Capability.ContractType` is typed as bare `Type`, not a closed shape. Phase 4 introducing a real
scripting type later is a new value plugged into an already-open slot, not a schema change — there's
nothing to migrate, so there's nothing an empty placeholder today actually prevents. The original
justification doesn't hold up under its own test. Cutting the type from 2.6.5's deliverables; the
intent stays recorded in `phase-2-6-5.md`'s prose (Related phases/Explicitly-not-this-phase), which
carries it just as well as a stub type would. **Flagging this one for Arryn explicitly** rather than
just closing it here — it reverses something asked for directly, and that's a different situation
than a design choice I'm making unilaterally within my own authority.

**3. `version` on `IRom` — cut, no tension.** `non-goals.md` already parks ROM versioning by name
with its own trigger condition, and nothing in 2.6.5 — not the validator, not `Capability` itself —
reads a version anywhere. Clean miss against the test, no counterargument. `name` stays (the
validator's clash/identity reporting actually uses it); `credits` stays too — harmless, conventional
manifest metadata, not sitting against a line `non-goals.md` specifically drew the way `version` is.

Net: cutting the enforcement pass and `IScript`, keeping `EngineTier`-as-concept, `version` cut,
`name`/`credits` kept. Updating `phase-2-6-5.md` to match. Same mechanism proves 2.7 either way —
this just stops it from claiming more than that proof actually shows.

**Addendum:** Arryn confirmed the `IScript` cut — intent isn't lost, expected to land as its own
real type around Phase 4, just not built now. Added a forward pointer in `phase-4.md`'s Deliverables
so that expectation is visible from Phase 4's own doc, not just recorded here. Nothing left open on
my end.

## Resolution

_(Kofi to fill in on close.)_
