# 0037 — Conversation history

Full evolution of [ticket 0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md),
preserved here so the ticket itself can stay lean and current for Baraka. Read this if you want the
*why*; read 0037 if you want the *what to build*. Chronological, not restructured.

## 1. Original ticket (2026-07-26)

Opened once Phase 2.7's content design was done: [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md)
(Juno, via [0034](0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md)) and the
project-layout question it surfaced ([0036](0036-grid-rom-d20system-project-layout-decision.md),
ruled directly by Arryn) were both resolved. Checked the spec against source before opening — real
and buildable, not a placeholder: capability declarations, all five 1.d20 commands mapped with
explicit cross-ROM read/write columns, ten checkable acceptance criteria, honestly-flagged open
questions.

The framework side was also specced, not just content — [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md)
§4.1 gave `IInteropRom`'s `Resolve`/`Resolve<T>` signatures in full; §2's gating question (does
NRules match through interfaces) was answered and closed ([0033](0033-spike-does-nrules-match-facts-through-interfaces.md)).
The original ask: build the spec in full, plus `Resolve`, per the three-project scaffold from 0036
(`example/D20SystemExampleA`/`B`/`C` — `GridMap`, `D20System`, `D20SystemInterop`, the last one a
real `IRom`/`IInteropRom`).

## 2. Cartridge pivot — stop-work (2026-07-27)

Tickets [0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md)/[0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md)
cut `IInteropRom.Resolve` entirely (trust-boundary grounds: a runtime pull API is
undeclared-by-construction, `ComponentRegistry` can't validate it) and reframed `D20SystemInterop` as
a **Cartridge contract surface**, not a ROM. Baraka was already mid-build on `Resolve` when this
landed — stop-work banner added, flagged as urgent since a ticket edit alone doesn't interrupt a live
session. No git rollback (per 0040 — code is cheap, the reasoning isn't); redirect forward instead.

## 3. First location call: a new `src/LunaFlow.Cartridge` project (2026-07-27, same day)

Initial correction put the four interfaces in a brand-new `src/LunaFlow.Cartridge` project, alongside
`LunaFlow.Rom`/`LunaFlow.Rulebook` — reasoning: `cartridge.md`'s invariant that the contract surface
is its own assembly, never inside a ROM. This was wrong, caught same day (see next).

## 4. Location corrected: content stays in `example/`, not `src/` (2026-07-27, same day)

Arryn caught it: `IPlayerLocation`/`IChallengeSolved`/`ICombatStatus`/`IPlayerMovement` are
**content** — player/challenge/combat-specific vocabulary — not engine-agnostic infrastructure. The
error was conflating "needs its own assembly" (true) with "needs to live in `src/`" (false —
`example/D20SystemExampleC` was already its own `.csproj`, which alone satisfies the invariant). Same
rule that already sent `GridMap`/`D20System` to `example/` in ticket 0038 applies to their contract
surface too. Fixed on 0037, 0038, 0040 (addendum), `phase-2-7.md`, and via a same-day follow-up
ticket to Juno ([0042](0042-correct-cartridge-project-location-in-spec.md)) since she'd just closed
[0041](0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md) against the wrong location.

## 5. Naming ruling: `D20SystemInterop` retired (2026-07-27, same day)

Arryn ruled directly (via 0042): the name `D20SystemInterop` goes away entirely, replaced by
`LunaFlow.Cartridge`.

## 6. Second location error: project rename vs. namespace rename (2026-07-27, same day)

First pass at recording the naming ruling said "rename the project and its namespace" — wrong again,
same species of error as step 3/4. Arryn clarified: only the **C# namespace** changes
(`LunaFlow.Rom.D20SystemInterop` → `LunaFlow.Cartridge`). The physical project — whatever Baraka
names the folder/`.csproj` under `example/` — is untouched: still an example project, no rename, no
move. Identical to how `GridMap` is the `LunaFlow.Rom.GridMap` namespace living inside
`example/D20SystemExampleB` without the project itself being renamed (0036's own point). Also
confirmed: no `src/LunaFlow.Cartridge` project exists or is planned — that name is reserved for a
genuinely engine-agnostic Cartridge-runtime type (e.g. a hypothetical `ICartridge`), which isn't
being built (`non-goals.md`'s standing deferral). Fixed on 0036, 0037, 0042, and the index.

## Net result

See [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md) for the current,
lean ask — this file exists so that history didn't have to live there too.

## Related pages

- [0037 — the current ticket](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)
- [Handoff Ticket Index](_index.md)
