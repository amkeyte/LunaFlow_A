# 0042 — Correct Cartridge contract-surface project location in the spec

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-27
**For:** Juno (Game Designer)

## Decision

<In-review — Juno applied the location fix; awaiting Kofi's confirmation before this closes, per
`handoff-tickets.md`'s convention.>

## Details

### Context

Same-day follow-up to [0041](0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md), which you
just closed. Sorry for the churn — this is my error, not a new design question or a reopened one.

0041 had you rewrite the spec's interop section around a new `LunaFlow.Cartridge` project in `src/`,
alongside `LunaFlow.Rom`/`LunaFlow.Rulebook`. Arryn caught the actual mistake in that ask: `IPlayerLocation`/
`IChallengeSolved`/`ICombatStatus`/`IPlayerMovement` are **content** — they name player location,
challenge resolution, and combat status, exactly the game-specific vocabulary the engine tier has no
business knowing. The engine only knows how to validate and wire whatever a Cartridge declares; it
doesn't know what a `Capability` *means*. That's the identical reasoning that already sent `GridMap`
and `D20System` to `example/` rather than `src/`, in ticket 0038 — content lives in the example
projects; `src/` is for the genuinely engine-agnostic framework tiers (`Engine`/`Rules`/`Rom`/
`Rulebook`/`Server`). I conflated two different requirements in 0041: "needs its own assembly"
(true — that's what `cartridge.md`'s invariant actually requires) with "needs to live in `src/`"
(not required at all — `example/D20SystemExampleC` is already its own `.csproj`, which already
satisfies the invariant).

Fixed already on the framework/ticket side: [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)
(Baraka's build ticket), [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md),
and [0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md) (addendum). This ticket is just the spec
catching up to match.

### Ask

**Location only — nothing else from 0041 changes.** In `grid-rom-d20-split-spec.md`, everywhere the
just-applied correction said the contract surface is "its own project (`src/`, alongside
`Rom`/`Rulebook` — a real project, not a ROM)," change it to: **stays in `example/D20SystemExampleC`**
— already its own project (satisfies "own assembly, never inside a ROM" without needing to be under
`src/`), retyped from the original `D20SystemInteropRom : IInteropRom` to bare interfaces, same as
0041 already established. Sections likely affected, based on what 0041 touched:

1. Overview ¶2 — wherever it now says the contract surface is a project under `src/`.
2. The "Cartridge contract surface" section header/opening paragraph — same correction.
3. "Framework dependencies" → the project-layout bullet that currently says `LunaFlow.Cartridge` is
   "a real project under `src/`, alongside `Rom`/`Rulebook`."
4. Related documents, if the `LunaFlow.Cartridge`-as-`src/`-project framing is restated there.

**Unaffected by this correction** (already right from 0041, don't re-touch): the four interfaces
themselves, `GridMovementService`'s location in GridMap, the "no `Resolve`, no registration, no
`IRom`" facts, `DependsOnRoms` naming two ROMs, and everything in Mechanics/rules or the Commands &
handlers table.

**Naming, if you want to settle it while you're in there (not required):** the project could keep
being called `LunaFlow.Cartridge` even physically under `example/` — same pattern as `GridMap`
keeping its `LunaFlow.Rom.GridMap` namespace regardless of physical project location, per 0036. Or it
could revert to `D20SystemInterop` as originally named in `example/D20SystemExampleC`. Your call or
Amara's; flag it rather than me guessing if it matters to either of you.

### Response

Fixed both spots in [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) that said
`src/`:

1. The "Cartridge contract surface" section's opening paragraph — now states it **stays in
   `example/D20SystemExampleC`**, already its own project, and explains why (content vs. framework,
   same reasoning as ticket 0038), rather than "own project (`src/`, alongside Rom/Rulebook)."
2. The Framework dependencies project-layout bullet — now states all three (`GridMap`, `D20System`,
   `LunaFlow.Cartridge`) stay in `example/`, none of them under `src/`.

Overview ¶2 didn't actually assert a location (just named the project), so nothing there needed
fixing — checked per item 1, no change made. Related documents doesn't restate location either — no
change needed per item 4. Everything 0042 flagged as unaffected (interfaces, `GridMovementService`'s
location, no-`Resolve`/no-registration/no-`IRom`, `DependsOnRoms` naming two ROMs, Mechanics/rules,
Commands & handlers) — confirmed untouched.

**Naming call:** kept `LunaFlow.Cartridge` as the project/namespace name rather than reverting to
`D20SystemInterop`. It's already used ~11 times through the doc (lower churn to keep than rename),
and it matches `cartridge.md`'s own vocabulary and worked example ("Cartridge: D20"). Flagging per
your ask rather than deciding unilaterally: if Amara or Arryn would rather this project keep its
original `D20SystemInterop` name (physically renaming nothing, since that's what
`example/D20SystemExampleC` is already called), that's a one-string find/replace away — not a
design question, just a preference.

### Resolution

Checked both fixed sections against the live spec file — the "Cartridge contract surface" opening
paragraph and the Framework dependencies project-layout bullet both correctly state
`example/D20SystemExampleC`, cite tickets 0038/0041/0042 accurately, and give the right reasoning
(content vs. framework). Confirmed.

**Naming, settled by Arryn directly — namespace only, not a project rename (corrected same day
after an initial overreach on my part):** `D20SystemInterop` goes away as the **C# namespace** the
four interfaces live in — `LunaFlow.Cartridge` replaces it there. The **physical project** is
unaffected: `example/D20SystemExampleC` stays exactly what it is, still an example project, no move
to `src/`, no rename of the project/folder itself. Same pattern as `GridMap` (`LunaFlow.Rom.GridMap`
namespace, `example/D20SystemExampleB` project — different axes). Also confirmed: no `src/LunaFlow.Cartridge`
project exists or is being created — that name is reserved only for a genuinely engine-agnostic
Cartridge-runtime type, if one is ever built (it isn't, per `non-goals.md`'s standing deferral); this
namespace usage inside the example project is unrelated to that. Routed to Baraka's ticket
([0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)): rename the
*namespace* to `LunaFlow.Cartridge` when he builds it, leave the project/folder identity alone.
