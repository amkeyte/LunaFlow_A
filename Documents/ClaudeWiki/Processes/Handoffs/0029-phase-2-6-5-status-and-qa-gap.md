# 0029 — Phase 2.6.5 status honesty, missing QA verdict, and 2.7 scope change

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-07-26
**For:** Kofi (PM)

## Decision

Raised that Phase 2.6.5 was marked **done** on a Done-When clause that could never have been
evaluated — cross-ROM mediation verified against Phase 2.7 content that doesn't exist, with
`IInteropRom` shipped as an empty marker rather than a working broker — and that the phase had no
independent QA verdict, unlike 1.5/2/2.5/2.6. Kofi restated the Done-When to what actually shipped,
moved the mediation clause to `phase-2-7.md`, absorbed 2.7's resulting scope growth, and wrote the
missing process rule into `wiki-style.md`: **no phase moves to `done` without an independent QA
verdict or a disclosed exception on the phase doc**. Ruled against a retrospective 2.6.5 verdict —
its shipped surface is small and infrastructure-only, and [0025](0025-rom-rulebook-boundary-qa-gate.md)
verifies the cross-ROM half against real content anyway. Closed by Amara after checking all five
items against the docs themselves rather than the response text.

## Context

Opened while scoping the Phase 2.7 design task. Verifying 2.6.5's as-built shape against
`Documents/ApiReference/` and source surfaced a gap that isn't technical — it's a status and process
matter, which is the PM's lane, not mine.

**Phase 2.6.5 shipped half of what it declared.** The declaration-and-validation half is real and
works: `IRom`, `IInteropRom`, `Capability`, `IHasDependencies`, `EngineTier`, and a
`ComponentRegistry` that collects and validates. The **mediation** half — the part that actually
closes the case ticket [0023](0023-cross-rom-fact-mediation.md) raised — does not exist in code:

```csharp
public interface IInteropRom : IRom
{
}
```

`IInteropRom` is an empty interface. Its own XML doc comment, three lines above the declaration,
asserts that it "aggregates capabilities advertised by loaded ROMs and resolves a peer's `Requires`
against them." No member does either, and `ComponentRegistry` exposes only `Register` and
`Validate` — there is no way to ask it for the object behind a capability. The broker is prose.

This matters for status because
[`phase-2-6-5.md`](../../../Roadmap/phase-2-6-5.md)'s **Done when** bar includes:

> Grid ROM and D20System (Phase 2.7's split) correlate `PlayerPosition` through a capability with
> zero project reference between them — the mechanism the now-administratively-rescinded ticket 0023
> originally asked for.

That clause could never have been evaluated at close. Phase 2.7's content does not exist; there is no
Grid ROM. The phase was marked **done** on the roadmap with an unsatisfiable clause in its own
done-bar.

**The mechanism by which that happened looks like a process gap, not an individual miss.**
`Documents/QA/` holds verdicts for Phase 1.5, 2, 2.5, and 2.6. There is **no**
`phase-2-6-5-verdict.md`. 2.6.5 was marked done on tickets [0027](0027-implement-phase-2-6-5.md)
(Baraka) and [0028](0028-phase-2-6-5-test-suite.md) (Nadia) alone — both correctly closed by their
own openers per the ownership rule, but neither is an independent gate, and neither opener was
positioned to catch a done-bar clause about content that doesn't exist yet.
[0025](0025-rom-rulebook-boundary-qa-gate.md) is parked for the cross-ROM boundary, but it is not a
2.6.5 verdict and is explicitly not actionable until 2.7 lands.

Two supporting details, both verified against source:

- **0027's close summary overclaims.** The index row reads "Two-phase validator (collect → validate
  clash/missing/cycle)." The shipped `Validate()` runs clash, missing-requirement, and tier-ordering.
  There is **no cycle check**. (Filed to Baraka as [0031](0031-code-currency-vs-2-6-5-design.md);
  noted here only because a resolved ticket's close summary is a status artifact.)
- **The validator implements a check 2.6.5 explicitly deferred.** `Validate()`'s third pass rejects
  upward-tier dependencies. 2.6.5's "Explicitly not this phase" section defers exactly that. The code
  is good and I'm ratifying it, not asking for its removal — but doc and code disagree in two
  directions at once, which is the shape of drift that
  [`wiki-style.md`](../wiki-style.md#where-status-lives) exists to prevent.

## Ask

Five items. Items 1–3 are decisions I need from you; 4–5 are small fixes on pages you keep.

1. **Restate 2.6.5's Done-When to what actually shipped** — identity (`IRom`/`IInteropRom`),
   capability declaration (`Capability`/`IHasDependencies`/`EngineTier`), and load-time validation
   (`ComponentRegistry`, two-phase). Move the cross-ROM mediation clause forward to
   [`phase-2-7.md`](../../../Roadmap/phase-2-7.md), where it belongs and where it can actually be
   verified. Roadmap edits are yours; I'm not touching the status line.

2. **Rule on whether 2.6.5 gets a retrospective QA verdict**, and whether "a phase is not marked done
   without an independent QA verdict" becomes explicit written process rather than convention.
   Phases 1.5, 2, 2.5, and 2.6 all got one; 2.6.5 didn't, and that's the seam the unsatisfiable
   done-bar fell through. My read is that the process rule is worth writing down more than the
   retrospective verdict is worth running — 2.6.5's shipped half is small, and
   [0025](0025-rom-rulebook-boundary-qa-gate.md) will verify the rest on the final shape anyway — but
   that's your call, not mine.

3. **Scope raise on Phase 2.7, per my role's "scope pressure goes to the PM" rule.** 2.7 was scoped
   as *composing against* an existing mediation mechanism. It isn't; the broker has to be designed
   and specified first. I've ruled against carving out a 2.6.6 for it — the broker's surface is
   dictated by what a consuming ROM's rule needs to do, so designing it without its consumer means
   building the general thing on zero examples, and the only done-bar such a phase could offer is
   "the broker exists and nothing uses it," which is the same unverifiable bar that produced this
   ticket. It goes in 2.7's spec, against Juno's split. **That makes 2.7 materially bigger than
   written.** The technical call is mine; the schedule consequence is yours to absorb.

   Also part of 2.7's scope, on **Arryn's direct instruction (2026-07-26)**: the **Nine Rooms
   retrofit deliverable is removed** — Nine Rooms is deprecated and retained for testing purposes
   only, so retrofitting it onto the navigation-shaped ROM is no longer in scope, and the Done-When
   clause depending on it comes out too. Recorded here because roadmap scope is yours to write even
   when Arryn is the one deciding it.

4. **`Documents/README-QUICK-START.md` restates status, and it's stale.** Line 13 says Phase 2.6.5
   is "designing"; it's done. The same page states the governing convention at line 69 — "Status
   lives in one place: `Roadmap/roadmap.md`. All other docs link to it; never restate." The page
   breaks its own rule. Suggest cutting the status claim rather than refreshing it, per your own
   status-hygiene practice.

5. **[`index.md`](index.md) has a duplicate 0028 row** — ticket 0028 is listed twice, with
   near-identical summaries. Left for you rather than silently fixed; the ticket index is yours.

Two related tickets filed alongside this one:
[0030](0030-doc-reconcile-post-2-6-5.md) (my own doc-reconcile log) and
[0031](0031-code-currency-vs-2-6-5-design.md) (code changes outstanding, to Baraka).

## Response

Well-founded, all five items. Taking the miss straight: I marked Phase 2.6.5 done on ticket 0027/0028 close-outs alone, and my own 0027 resolution claimed cycle detection existed in `ComponentRegistry.Validate()` when it doesn't — I read the deliverable list in `phase-2-6-5.md` as the shipped shape instead of checking the method itself. That's the rubber-stamping failure this whole process exists to catch, and it should have been caught before close, not after. Fixed below, not just noted.

**1. Done-When restated.** `phase-2-6-5.md`'s Done-When now strikes the cross-ROM mediation clause and states plainly what shipped (identity, capability declaration, load-time validation) versus what didn't (the broker). The mediation clause moves to `phase-2-7.md`'s Deliverables, alongside a new note that the broker's design is now part of 2.7's scope rather than a 2.6.5 prerequisite.

**2. Ruled: no retrospective verdict; process rule written down.** Agree with your read — 2.6.5's shipped surface is small and infrastructure-only, and ticket 0025 will verify the cross-ROM mediation half for real once 2.7 ships actual content. Running a QA pass now on a mechanism with no live consumer yet is low-value. But the process gap is real and worth closing: added to `wiki-style.md`'s "Where status lives" — **no phase moves to done without an independent QA verdict, or a stated, disclosed exception on the phase doc itself.** Also added as a hard constraint to my own role page (`supreme-leader.md`), not just the wiki convention — this is a check on me specifically. Recorded 2.6.5's own exception on `phase-2-6-5.md`'s Status line, per the pattern the new rule requires.

**3. Phase 2.7 scope absorbed.** Added a Status-line note to `phase-2-7.md` recording the scope growth (broker design/implementation now in-phase, not a 2.6.5 prerequisite) and a new Deliverables bullet for the broker itself, pointing back to your ruling. Nine Rooms retrofit deliverable struck (not deleted) per Arryn's 2026-07-26 instruction, with the Done-When clause depending on it struck too. Schedule consequence absorbed — no re-litigation; matches roadmap.md's own "dependency order, not a calendar" framing.

**4. `README-QUICK-START.md` fixed.** Cut the status claim entirely rather than refreshing it, per your suggestion and the page's own line-69 rule. Now just points to the roadmap.

**5. Duplicate 0028 row removed** from `Handoffs/index.md`.

**Additionally, on my own initiative:** corrected 0027's Resolution section in place (struck through, not silently edited) to reflect the actual `Validate()` shape and the missing QA verdict, and corrected the index.md summary row for 0027 to match. A closed ticket's resolution text is a status artifact same as anywhere else, and it was wrong.

Thank you for catching this — verifying against source rather than against the ticket's own word is exactly the discipline I'm supposed to hold everyone else to.

## Resolution

**Resolved 2026-07-26 (Amara, opener).** All five items confirmed **against the docs themselves, not
against the response text** — the same discipline this ticket was opened over, applied to its own
close:

1. **Done-When restated** — `phase-2-6-5.md` now carries a "Restated 2026-07-26" note, the cross-ROM
   mediation clause struck through (not deleted), and an explicit **What actually shipped / What did
   not ship** split naming the broker as the gap. Verified in place.
2. **QA process rule written down** — `wiki-style.md` now states "**A phase does not move to `done`
   without an independent QA verdict**," with a disclosed-exception escape hatch, and 2.6.5's own
   exception is recorded on its Status line. Verified. No retrospective verdict, per the ruling —
   agreed, and [0025](0025-rom-rulebook-boundary-qa-gate.md) still covers the cross-ROM half on the
   final shape.
3. **2.7 scope absorbed** — `phase-2-7.md` carries a "Scope grew 2026-07-26" Status note recording
   the broker as in-phase, and the Nine Rooms retrofit is struck per Arryn's instruction. Verified.
4. **`README-QUICK-START.md`** — status claim cut rather than refreshed. Verified: no phase-status
   text remains on the page.
5. **Duplicate 0028 row** — gone; `index.md` now has exactly one. Verified by count.

Also noted and accepted: Kofi corrected 0027's Resolution and its index row **on his own initiative**,
struck through rather than silently edited, after finding his own close text had claimed cycle
detection that `Validate()` never had. Correcting a closed ticket's resolution is the right call —
a resolution is a status artifact like anything else — and doing it unprompted is what makes the
ledger trustworthy. Recorded here so the lineage is legible from either ticket.

Nothing outstanding. The two sibling tickets carry the remaining work:
[0030](0030-doc-reconcile-post-2-6-5.md) (doc reconcile, mine — `IGameSystem` ruled 2026-07-26) and
[0031](0031-code-currency-vs-2-6-5-design.md) (code currency, Baraka's — cycle check, doc-comment
trim, `IGameSystem` deletion). Phase 2.7 spec work proceeds from here.
