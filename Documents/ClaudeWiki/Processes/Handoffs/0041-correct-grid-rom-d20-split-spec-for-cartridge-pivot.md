# 0041 — Correct `grid-rom-d20-split-spec.md` for the Cartridge pivot

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-27
**For:** Juno (Game Designer)

## Decision

All 12 itemized corrections applied to `grid-rom-d20-split-spec.md`, plus two more Juno caught that
weren't itemized (stale broker language in the `look`/`flee` prose and the Commands & handlers
table). `DependsOnRoms` confirmed to name two ROMs, not three. Checked against the live spec file
and against `cross-rom-capability-spec.md` §1/§4.1–§4.4 (all cited sections exist and say what she
says they say) before closing — not just against her Response text. Spec is internally consistent
and ready for Baraka.

## Details

### Context

Next in the Juno → Baraka → Nadia sequence from ticket [0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md).
The Cartridge design ([0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md),
`design/cartridge.md`) cuts `IInteropRom.Resolve` and rules that `D20SystemInterop` is a Cartridge
contract surface — its own project (`LunaFlow.Cartridge`, confirmed by Arryn), not a registered
`IRom`/`IInteropRom`. This reverses ticket [0036](0036-grid-rom-d20system-project-layout-decision.md)
(your own filed question, ruled directly by Arryn at the time), which is now marked superseded on
its own page. Baraka's build ticket ([0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md))
is already amended to match — he was mid-build on `Resolve` when this landed, so his ticket carries
a stop-work banner.

I read [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) against the new
design before opening this. Per 0039's own routing note: **this is a mechanism correction, not a
content change — your fact shapes and vocabulary are unaffected.** Every `GridRoomData`,
`RoomChallenge`, `PlayerPosition`, `RoomSolved`, `CombatRound`, and the command/handler behavior
they drive stays exactly as designed. What's wrong is only the spec's description of what
`D20SystemInterop` *is* and how the one service-shaped capability (`IPlayerMovement`) gets wired.

### Ask

Correct the spec's framing in place (same doc, not a new one — nothing here rises to a redesign).
Sections that need it, in order of appearance:

1. **Overview (¶2):** "plus a third, real ROM that is the interop broker itself (`D20SystemInterop`,
   `IInteropRom`), per ticket 0036's resolved ruling" — 0036 is superseded; `D20SystemInterop` is a
   **Cartridge contract surface** (project `LunaFlow.Cartridge`), not a ROM, not a broker, holds no
   `IInteropRom` implementation.
2. **Scope → In:** "carried by the third ROM (`D20SystemInterop`)" — same correction, it's the
   contract-surface project, not a ROM.
3. **Fact vocabulary → `D20SystemInterop` section header and opening paragraph:** currently titled
   "own project — a real `IRom`/`IInteropRom`, **not** a passive library" and opens "Per ticket
   0036's ruling: this is a genuine third ROM performing the negotiation between the other two."
   Invert this: it **is** exactly the passive contract surface that paragraph says it isn't —
   project renamed `LunaFlow.Cartridge`, holds the three fact-shaped interfaces plus
   `IPlayerMovement`, performs no negotiation itself.
4. **`IPlayerMovement`'s described wiring:** "resolved once via `D20SystemInterop`'s own `Resolve<T>`
   at composition-root time" is gone. Corrected shape: `Program.cs` constructs
   `GridMovementService` directly and passes it to `FleeCommandHandler`'s constructor as
   `IPlayerMovement` — plain DI, no resolve call anywhere.
5. **"Plus the concrete `IInteropRom` implementation... `Resolve`/`Resolve<T>` machinery" paragraph:**
   delete. No such class exists under this design — `LunaFlow.Cartridge` holds interfaces only.
6. **Capability declarations comment:** "`D20SystemInterop` — provides no game-facing capability of
   its own; it's the broker the two content ROMs' `Provides`/`Requires` resolve through, registered
   as its own `IRom` for identity" — wrong on the mechanism, right on the effect. Correct version:
   `LunaFlow.Cartridge` isn't registered at all (not an `IRom`); GridMap and D20System declare
   `Provides`/`Requires` **directly against each other**, and `ComponentRegistry` validates the pair
   with no third component in the loop. The `Capability` lists themselves (both ROMs' `Provides`/
   `Requires` blocks) are unchanged — only the "who mediates" framing is wrong.
7. **Acceptance criteria, first bullet:** "`IRom` instances exist for GridMap, D20System, and
   `D20SystemInterop` (the latter also implementing `IInteropRom`)" → drop the third instance;
   two `IRom`s, not three.
8. **`OneD20RuleBook.DependsOnRoms` naming "all three":** worth your explicit confirmation rather
   than my inference, but since `LunaFlow.Cartridge` isn't an `IRom`, `DependsOnRoms` has nothing to
   validate a `"D20SystemInterop"` entry against — reads like it should name only `"GridMap"` and
   `"D20System"` now. Flag back if that's wrong rather than silently keeping three.
9. **The "zero project reference... confirming the broker resolves by `Capability`/`object` alone"
   acceptance bullet:** still true and now trivially so (a contract-only assembly can't help but have
   zero reference) — just drop "broker," there isn't one.
10. **Framework dependencies, first bullet:** "`D20SystemInterop`'s aggregate/resolve implementation
    — phase-2-7.md's Architect-owned deliverable" — moot, strike it. There's no aggregate/resolve
    behavior left to size or build.
11. **Open questions, first bullet:** the `Query<T>` vs. broker-`Resolve` question for
    `IPlayerMovement` is **settled, not open** — there is no broker `Resolve` under any reading now.
    Strike or mark resolved.
12. **Open questions, `D20SystemInteropRom`'s class name:** moot — no `IInteropRom` implementation
    exists to name.

Not part of this ask: anything in Fact/marker vocabulary's actual data shapes, the Mechanics/rules
section's command behavior, or the Commands & handlers table — all unaffected, per 0039's own read
that this is mechanism-only.

### Response

All 12 corrections applied in place to
[`grid-rom-d20-split-spec.md`](../../Specs/grid-rom-d20-split-spec.md) — same doc, no redesign, per
the ask. Notes on a few:

- Items 1–6 (Overview, Scope, the `D20SystemInterop` section itself, `IPlayerMovement`'s wiring, the
  deleted `Resolve`/`Resolve<T>` paragraph, capability-declarations comment): done as specced. The
  section is retitled "The Cartridge contract surface (`LunaFlow.Cartridge`)" and inverted to state
  plainly what it corrects from.
- Item 8, **`DependsOnRoms` — confirmed, not left to inference:** names only `"GridMap"` and
  `"D20System"`. `LunaFlow.Cartridge` isn't an `IRom`, so there's nothing for a third entry to
  validate against — this follows directly from the design, not a judgment call I had to weigh.
- Items 9–12: applied. Also caught two mentions ticket 0041's list didn't itemize but that would have
  left stale broker language sitting next to the correction — the `look`/`flee` prose in Mechanics/
  rules and the Commands & handlers table both still said "resolved via `D20SystemInterop`"; corrected
  to plain constructor injection by `Program.cs`, consistent with the rest of the doc. Also added a
  location note confirming GridMap/D20System stay in `example/` (tickets 0036/0038) while
  `LunaFlow.Cartridge` is a new project under `src/` (ticket 0040) — not itemized in the ask, but
  needed once the project got renamed and the aggregate/resolve Framework-dependency bullet came out.
- Fact/marker vocabulary's data shapes, Mechanics/rules' command behavior, and the Commands & handlers
  table are otherwise untouched, per the ask's explicit boundary.

### Resolution

Closed 2026-07-27. Read the live spec against all 12 items plus Juno's two self-caught extras before
closing — all correct at the time.

**Addendum, same day:** the project-location detail this ticket closed against (`LunaFlow.Cartridge`
as "a new project under `src/`," Response item 4) was itself wrong, caught right after close — see
[0042](0042-correct-cartridge-project-location-in-spec.md). Short version: the four interfaces stay
in `example/D20SystemExampleC`'s existing project (content, not engine-agnostic infrastructure, same
rule as `GridMap`/`D20System`), and only the C# *namespace* inside it becomes `LunaFlow.Cartridge` —
not a project rename, not a move to `src/`. Everything else this ticket resolved (items 1–3, 5–12,
and both of Juno's extras) stands untouched. Left this ticket's own Ask/Response text as written,
historical trail of what was known when — 0042 carries the correction forward.
