# 0034 — Handoff: Juno builds against cross-rom-capability-spec.md

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-26
**For:** Juno (Game Designer)

## Decision

**Confirmed against the spec itself, not the response text.** Juno built the split against §3 as
asked, found no framework gap, and surfaced (not decided) the one real open question — project
layout — via [0036](0036-grid-rom-d20system-project-layout-decision.md), which Arryn ruled directly:
three real projects (GridMap, D20System, and a genuine third ROM `D20SystemInterop` performing the
negotiation, not a passive contracts library).
[`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) is real, detailed, and
consistent with both tickets — capability declarations, fact/contract split, all five commands
mapped, ten acceptance criteria, open questions honestly flagged rather than papered over. Ready for
Baraka.

## Details

### Context

Phase 2.7's cross-ROM mechanism is specced and ungated: `IInteropRom`'s aggregate/resolve behavior,
the shared-contract-type pattern for fact correlation, and the NRules interface-matching question it
depended on are all settled —
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md), gated question closed by
[0033](0033-spike-does-nrules-match-facts-through-interfaces.md). The 2.6.5 mechanism
(`IRom`/`IInteropRom`/`Capability`) plus the addendum work in
[0031](0031-code-currency-vs-2-6-5-design.md) (cycle detection, component-name uniqueness) are also
landed and verified — 119/119 on the 2026-07-26 trx.

What's left for 2.7 to actually start is content: which ROMs split apart, and what proves the split
works. That's explicitly not architecture's call —
[`phase-2-7.md`](../../../Roadmap/phase-2-7.md): "Which specific ROMs split apart is deliberately not
named here. That's content-design territory — Juno's call once she's designing against this phase's
real content." Amara held that line even while specifying the mechanism — see her note on
`cross-rom-capability-spec.md` §7: "depends on whether the split produces a natural broker or needs a
neutral one, which is content-shaped and not yet known."

### Ask

**Build the Grid/D20System split (or whatever split you land on) against
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §3, "The authoring
contract."** That section is stable and ready to build against regardless of anything else in the
spec — the gating question (§2) is resolved, and §3 doesn't change with it.

A few things worth being explicit about, since this is the first time content design meets this
mechanism:

1. **The model is toolbox-first.** You build content against the surface the spec's §3 defines
   (contract types in an interop assembly, `Capability` declarations, `Match<TContract>` in rules,
   `IInteropRom.Resolve` for handler-side lookups). The framework doesn't pre-guess what your split
   needs.
2. **If your content needs something the surface doesn't offer** — a capability shape §3 doesn't
   cover, a case the spec's §5 (Deliberately excluded) rules out that you have a real case for — **you
   file a ticket** rather than working around it or extending the surface yourself. That's the
   established path: [0023](0023-cross-rom-fact-mediation.md) is the precedent for "content design
   surfaces a real gap in the mechanism," and [0015](0015-imechanic-dice-resolution.md) is the
   precedent for "content design needs a framework decision made explicit." Both went to the
   Architect, not around her.
3. **Which ROMs split, and what content proves it, is entirely your call.** Nobody upstream has
   named it and nobody's going to — that's deliberate, per `phase-2-7.md`'s own framing, so the split
   is shaped by real content rather than an architecture-invented example.
4. **Two things the spec leaves open on purpose, not by oversight:** §7 doesn't say where the interop
   assembly lives (a new project vs. an existing ROM designated broker) — that depends on what your
   split produces, so it's yours to surface once you have a shape. And `phase-2-7.md`'s Done-When still
   has one clause unwritten (the broker's own acceptance criteria) — same reason, waits on the real
   surface.

### Response

Built the split against §3, spec at
[`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md). Answers to the numbered
points:

1. Toolbox-first, confirmed — the spec's capability declarations use §3's exact shapes
   (`Capability`, `Match<TContract>`, `IInteropRom.Resolve<T>` for the one action-shaped case).
2. No surface gap found. §3/§4 covered everything the split needed once I separated *observing* a
   peer's fact (three contracts, matched/queried directly — no broker call, per §1) from *invoking*
   a peer's action (one contract, `IPlayerMovement`, resolved via `IInteropRom` and injected into a
   handler's constructor — the `flee` case, which needs GridRom to move the PC on its own facts, not
   D20System reaching in and mutating them itself).
3. **The split: GridRom (spatial — position, movement, win-line topology) / D20System (trimmed —
   skill checks, combat).** Real content, not invented: 1.d20's existing `Room` record already
   conflated grid fields (`Row`/`Col`) with challenge fields (`ChallengeType`/`DC`/etc.) — the split
   just separates what was already two concerns wearing one record. Proven by four capabilities
   crossing the boundary, two each direction, both patterns represented (fact-observation and
   action-invocation) — not a toy example, all four map onto behavior `1-d20-spec.md` already ships
   (`check`/`fight` needing current position, win-lines needing solved-state, `look` needing combat
   status, `flee` needing to move the PC).
4. **Interop assembly location: surfaced, not decided** — filed as
   [ticket 0036](0036-grid-rom-d20system-project-layout-decision.md), since it turns out to hinge on
   a prior question §7 didn't anticipate: whether "zero project reference, verified against the
   assembly graph" requires GridRom/D20System to actually become separate projects (they're both
   namespaces in one `LunaFlow.Rom` project today, same as NineRooms). That's a `src/` structural
   call outside this role's authority, so it goes to Amara rather than me picking a project layout.
   phase-2-7.md's still-unwritten broker acceptance clause likely wants to wait on 0036 too, for the
   same reason.

No other framework gaps found — §3/§4 as specced was sufficient for all of 1.d20's real cross-ROM
needs once I'd separated observation from invocation.

**Follow-up, [ticket 0036](0036-grid-rom-d20system-project-layout-decision.md) now resolved
(Arryn, direct):** two corrections to the above, both folded into
[`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md). First, "GridRom" above is
renamed **GridMap** throughout. Second, and more substantive: the interop assembly isn't a passive,
logic-free contracts library — it's a genuine **third ROM**, `D20SystemInterop`, itself registered
as `IRom`/`IInteropRom` and doing the actual negotiation between the two content peers. So this
split proves three independently-loadable ROMs, not two — GridMap and D20System as the content
peers, `D20SystemInterop` as the broker, with zero project reference between the two content peers
and a one-directional reference from each of them down to the broker only.

### Resolution

**Resolved 2026-07-26 (Kofi, opener).** Checked against source, not the response summary:
`Documents/Specs/grid-rom-d20-split-spec.md` exists, is a real buildable spec (not a stub), and its
content matches every claim above — the three-project layout from 0036, four capability declarations
(three fact-shaped, one service-shaped), all five 1.d20 commands mapped with explicit cross-ROM read/
write columns, ten checkable acceptance criteria, `D20SystemContent.Configure`'s retirement folded in
per §4.3, and honestly-flagged open questions rather than hidden gaps. `example/D20SystemExampleA`/
`B`/`C` (0036's scaffold) also confirmed present on disk. Nothing outstanding on this ticket — spec is
ready for Baraka; framework-side work (`D20SystemInterop`'s implementation, `DependsOnRoms`
validation) stays tracked on `phase-2-7.md`, not here.

**Also fixed while reviewing:** this ticket and 0035 both had the same `../..`→`../../..` relative-
path miss just corrected in 0035 — right after fixing it elsewhere, ironic but caught by the same
`doc-lint` pass. Fixed in place; 0036 had one instance of the same bug plus a duplicated `## Details`/
`### Context` heading pair, both fixed.
