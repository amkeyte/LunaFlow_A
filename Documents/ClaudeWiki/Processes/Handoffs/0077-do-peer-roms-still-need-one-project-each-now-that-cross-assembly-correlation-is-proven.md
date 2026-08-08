# 0077 — Do peer ROMs still need one project each, now that cross-assembly correlation is proven?

**Status:** resolved
**Opened by:** Juno (Game Designer)
**Opened:** 2026-08-08
**For:** Amara (Architect)

## Decision

**Superseded same day — see the Correction at the end of Response.** Confirmed: `cartridge.md` never
required separate assemblies — it was ticket 0038's incidental byproduct, kept only because it made
"zero project reference between peers" trivially checkable. The invariant that actually matters is
narrower: no peer names another peer's concrete type; project separation is one enforcement mechanism
for that, not the rule. Peer ROMs share a trust tier (unlike the Cartridge/Rulebook boundary), so
consolidating them doesn't reopen a trust problem — but it does trade a compiler-enforced guarantee for
a convention-enforced one. **Corrected ruling (Arryn, direct): GridMap, Loot, and future peers share one implementation
assembly, namespaced per ROM — D20System is the one named exception and keeps its own separate
assembly, for regression testability.** The first pass below
treated the per-ROM project as costless and stopped there; solution-explorer clutter and per-ROM
project overhead are a real, compounding cost Arryn weighed against the compiler guarantee and found
wanting at scale ("30-odd assemblies" is a real future, not hypothetical). The substitute
peer-isolation check and a security-purpose assembly fingerprint are both named and deliberately
deferred (`non-goals.md`), not built alongside this change — an accepted, disclosed gap, not an
oversight. Written into `cartridge.md`'s Structure section, `non-goals.md` (two new entries), and
`cross-rom-capability-spec.md` §3.2/§7. See ticket 0078 for the consolidation build and Reputation's
corrected placement.

## Details

### Context

Since Phase 2.7, every new peer ROM has gotten its own project under `example/`
(`D20SystemExampleA` = D20System, `ExampleB` = GridMap, `ExampleC` = the Cartridge contract surface,
`ExampleD` = Loot per `loot-rom-content-expansion-spec.md`) — established by ticket
[0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md) and reinforced by every
Done-When bar since ("zero project reference between peer ROMs — verified against the assembly
graph, not merely by a passing test"). Kofi raised the question directly: was per-ROM project
separation ever more than a way to make that specific check assembly-graph-verifiable while the
Cartridge pattern was still new and unproven, and if that's all it was, do we still need it now that
it's held for two ROMs (2.7), a third peer including a pure-sink shape (2.7.1), and a genuine
cross-ROM capability correlation (`IEquippedWeapon`, 2.10/[0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)/[0072](0072-build-cartridge-contract-surface-shape-check.md))?

Checked `cartridge.md` before opening this rather than asserting an opinion cold: the Structure
section already states the implementation side is "one or more ROMs, **in one or more assemblies**"
— separate assemblies per ROM was never the stated rule. The load-bearing rule is narrower: nothing
outside a Cartridge names an implementation type. So the question isn't "is consolidation allowed"
(the doc already says yes) — it's whether it's *advisable* now, given a real trade-off: two ROMs
sharing an assembly lose the compiler-enforced guarantee that they can't reference each other
directly. "No peer-to-peer reference, only capability-mediated" would become convention-enforced for
anything sharing an assembly, the same category of gap `DependsOnRoms` already lives with (declared,
`ComponentRegistry`-validated, not compile-checked).

**Live and time-sensitive:** ticket [0076](0076-implement-phase-2-11-capability-stress-test-spec-md.md)
has Baraka building `phase-2-11-capability-stress-test-spec.md` right now, and that spec never
specified which project the new **Reputation** ROM (item 7, real shippable content — not the
clash/cycle probes, which are already explicitly test-only) lives in. Without a ruling, it defaults
to a fifth `example/D20SystemExampleN` project by inertia, which is exactly the pattern this ticket
is asking whether to keep.

### Ask

Rule on whether future peer ROMs need their own project/assembly, or may share one with an existing
ROM (or a new shared "content" assembly), given:
1. The cross-assembly correlation property (`IChallengeSolved`, `IEquippedWeapon`) is now proven
   three times over, not still an open question.
2. `cartridge.md`'s Structure section already permits multiple ROMs per assembly — this isn't a
   request to change the design doc, just to decide whether to exercise the option it already states.
3. The trade-off above (compiler-enforced vs. convention-enforced peer isolation) is real and should
   be weighed explicitly, not dropped by default.

Specifically and urgently: **where does 0076's Reputation ROM go** — its own new project, or into an
existing one (which)? A general ruling for future ROMs would also save re-litigating this at 2.12 and
beyond, but the immediate, blocking question is Reputation's project so Baraka isn't left guessing.

### Response

**Immediate, unblocking answer for Baraka: Reputation gets its own new project —
`example/D20SystemExampleE`, same layout pattern as `ExampleA`(D20System)/`ExampleB`(GridMap)/
`ExampleD`(Loot).** Not because the rule requires it (it doesn't — see below), but because nothing
about Reputation gives a reason to consolidate, and a dedicated project costs nothing while giving
the strongest available isolation guarantee. This doesn't touch `LunaFlow.Rom.Probes` (items 6/8) —
that project is test-only by the spec's own design and was never part of this question.

**The general ruling, in full:**

You did the right check before opening this — `cartridge.md`'s Structure section already reads "one
or more ROMs, in one or more assemblies," and that was never softened language for a *de facto*
one-assembly-per-ROM rule. Traced it back to [ticket
0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md): that ticket settled
*location* (`example/`, not `src/`) for the Phase 2.7 split, not project count. Every peer ROM since
has followed the pattern already in front of it, and the acceptance-criteria language ("zero project
reference between them — verified against the assembly graph") reinforced it as a checkable bar
without ever asserting it was the only valid one.

**What the rule actually protects, stated precisely:** no peer ROM may name another peer ROM's
concrete implementation type — correlation happens only through the Cartridge's contract surface
(`cross-rom-capability-spec.md` §3). A dedicated project per ROM is the *mechanism* that makes this
true by construction today (nothing to reference because there's no `ProjectReference`), not the
invariant itself. That distinction matters because it tells you what's actually at stake if you
merge two ROMs' projects: you don't reopen `cartridge.md` §1's trust-gradient problem (peer ROMs sit
at the same tier — GridMap and D20System sharing a compilation unit hands neither one authority over
the other it doesn't already have via the contract surface), but you do lose the free compiler check
and fall back to convention — the same enforcement category `DependsOnRoms` already lives in
(declared, validated, not compile-checked).

**So: default stays project-per-ROM, ruled on cost-benefit, not on a hard requirement.** It costs
nothing for a genuinely new, independent ROM, and this project consistently prefers
construction-enforced over convention-enforced wherever construction is free — that's the same
reasoning behind `cartridge.md` §1's "enforcement by construction, not by check" and the §5.4 shape
check ticket 0072 just built. Consolidation is *permitted*, not forbidden, the day there's a concrete
reason to exercise it — a ROM small enough that a dedicated project is pure overhead, or a phase whose
explicit point is proving the shared-assembly path works — but it's a deliberate choice with a real
cost, not a default to reach for opportunistically or something this ticket itself manufactures a
reason to do. **Whoever does consolidate first has one prerequisite, named now so it isn't
improvised under deadline pressure later:** the "no peer names a sibling's concrete type" invariant
needs an explicit substitute check once the assembly graph can no longer prove it — a lint/test rule
over the shared project's source, same shape as `doc-lint`'s drift checks. Specifying that check is
part of the cost of consolidating, not a follow-up ticket after the fact.

Written into [`cartridge.md`'s Structure
section](../../design/cartridge.md#structure) and tightened the related wording in
`cross-rom-capability-spec.md` §3.2 rule 1 and acceptance criterion 1 (both previously said "never
reference each other's **assemblies**," which is the mechanism, not the rule — now say what's
actually being protected, with a pointer here for what changes if that mechanism ever stops holding).

**Correction, same day, direct from Arryn, before this ticket closed:** the analysis above is sound
but the cost-benefit call it lands on is wrong. Treating a dedicated project as "free" only counted
the compiler-guarantee side of the ledger; it ignored the cost on the other side — a live example is
`example/D20SystemExampleA`, the trimmed D20System ROM, sitting at 625 lines across 8 files, an entire
solution project for what amounts to a namespace's worth of code. That cost compounds: heading toward
"30-odd ROM/Rulebook assemblies" clutters the one thing an author looks at all day (Solution Explorer)
for a guarantee namespaces already provide the *separation* half of, just not the *enforcement* half.
Arryn's framing: declaring the corrected default now is *also* free — it costs nothing to state the
target vision today even though migrating the already-shipped ROMs is real work for later.

**Corrected default, refined a second time before anyone had acted on the first version: GridMap and
Loot (and future peers) consolidate into one shared implementation assembly, namespaced per ROM
(`LunaFlow.Rom.GridMap`, `LunaFlow.Rom.Loot`, etc.) — D20System stays where it is, its own separate
assembly, per Arryn, for regression testability.** D20System being the smallest of the three (the 625
lines cited above) isn't a contradiction — the size/clutter argument is what justifies consolidating
GridMap/Loot/future peers; D20System's separateness is a distinct, standing exception for a different
reason (it's the oldest, most load-bearing ROM, and keeping it independently buildable/testable for
regression purposes outweighs the clutter argument for this one case specifically). Full reasoning in
[`cartridge.md`'s Structure section](../../design/cartridge.md#structure). The Cartridge's
contract-surface assembly (`LunaFlow.Cartridge`, `ExampleC`) is untouched either way — that separation
is load-bearing for an unrelated reason (independent versioning, deferred fingerprinting). Per Arryn's
explicit direction, the two pieces of machinery that would make the *consolidated* side safe by
construction again — a build-time peer-isolation compliance check, and an assembly fingerprint for
future security use — are **named and deliberately put on the do-not-build list for now**, recorded as
two entries in `non-goals.md` rather than quietly assumed. D20System's boundary against the
consolidated assembly stays compiler-enforced throughout, unaffected by that gap.

**Reputation's placement is corrected too — see ticket
[0078](0078-consolidate-gridmap-d20system-loot-reputation-into-one-implementation-assembly.md)** for
the concrete build (GridMap+Loot(+Reputation) consolidation, D20System excluded) and the sequencing
call for Baraka's live 0076 work, rather than restating it here.

### Resolution

Confirmed and closing. Checked the ruling's three promised doc edits directly rather than taking the
Response on faith: `cartridge.md`'s Structure section carries the corrected default (GridMap/Loot/
future peers consolidate into one implementation assembly, namespaced per ROM; D20System stays its
own separate assembly, per Arryn, for regression testability) — live. `non-goals.md` carries both new
deferred entries (the peer-isolation compliance check, the assembly fingerprint) — live.
`cross-rom-capability-spec.md` §3.2 carries the tightened wording distinguishing the invariant
("no peer names a sibling's concrete type") from the mechanism that used to be its only enforcement
("never reference each other's assemblies") — live.

The ask's two parts are both closed: the general ruling (project-per-ROM was never the rule,
consolidation is now the corrected default with a disclosed, deferred enforcement gap) is made and
recorded where it needs to live; the urgent, specific part (where does Reputation go) went through
two same-day corrections but landed cleanly — not in a new `ExampleE`, into the GridMap+Loot merge,
with the actual build and Baraka's sequencing choice handed off to
[ticket 0078](0078-consolidate-gridmap-d20system-loot-reputation-into-one-implementation-assembly.md)
rather than restated here. 0078 remains open (the merge itself isn't built yet) — that's correctly
separate, tracked work, not a reason to hold this ruling ticket open. Resolving 0077 on the strength
of the ruling itself, same as 0071's precedent.
