# 0031 — Code currency against Phase 2.6.5's design

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-07-26
**For:** Baraka (Impl Dev) — plus Nadia (Test Dev) for Addendum item **A2** only

## Decision

Three code-vs-design gaps surfaced after Phase 2.6.5 shipped, plus the `IGameSystem` deletion ruled
in [0030](0030-doc-reconcile-post-2-6-5.md): `ComponentRegistry.Validate()` had **no cycle check**
though 2.6.5 specified one (two same-tier peers requiring each other validated clean — exactly Phase
2.7's peer-ROM shape), `IInteropRom`'s doc comment advertised broker behavior no member provided, and
the tier-ordering pass shipped though the phase deferred it (ratified, doc corrected instead). Baraka
delivered all four; Amara verified against source and hand-traced the DFS as correct, including no
false positive on a diamond. **Addendum** adds two follow-ons, kept on this ticket at Arryn's
direction rather than split: **A1** — component names must be unique, since the new phase 4
identifies components by *name* while phases 1–3 use *reference* and `Register` never enforced it
(Amara's design gap, not Baraka's miss; guard goes in `Register`); **A2** — the new pass has no test
coverage at all, tagged to Nadia.

## Context

Opened while scoping Phase 2.7. I verified 2.6.5's as-built shape against
`Documents/ApiReference/` and then against source, and found three places where the code and
[`phase-2-6-5.md`](../../../Roadmap/phase-2-6-5.md) disagree. **One is a real defect, one is a doc
problem I'm fixing on my side, and one is a doc-comment overclaim.** Read the framing before the
asks — I'm not asking you to build the broker, and I'm not asking you to undo anything.

For the record: 2.6.5's implementation is sound work and the parts I'm flagging are the parts a
close-by-inspection couldn't reasonably have caught, because two of them are gaps against a document
rather than gaps against a test. The one that matters is load-bearing for 2.7, which is why it's
worth a ticket now rather than folding it into the 2.7 build.

**Amended 2026-07-26:** item 4 (`IGameSystem`) was filed here as a flagged inbound pending my
ruling; the ruling landed same-day, so it's now a real ask. It's a separate concern from the three
2.6.5 disagreements above — vestigial surface predating the settled vocabulary, not something 2.6.5
touched — but it's a one-line deletion in the same project, so it rides along here rather than
earning its own ticket. `Capability.ContractType` moved to item 5, still flagged, still not an ask.

## Ask

### 1. `ComponentRegistry.Validate()` has no cycle check — **fix this one**

2.6.5's Deliverables specify the validator runs, against the complete graph: "clash detection (two
providers for the same capability), missing-requirement detection, **and a cycle check**." The
shipped `Validate()` runs three passes: clash, missing-requirement, and tier-ordering. There is no
cycle detection.

This is not academic for Phase 2.7. The tier pass only rejects a dependency on a **higher** tier
(`providerTier > tier`), so two **same-tier peers** that require each other's capabilities validate
completely clean today:

```
Rom A  Provides: "X"   Requires: "Y"
Rom B  Provides: "Y"   Requires: "X"
```

Both requirements resolve, no duplicate providers, no upward edge — `Validate()` returns without
complaint. Peer ROMs mutually depending on each other is precisely the shape 2.7 builds
(two peer ROMs, capability-mediated), so this is the check most likely to be needed and the one
that isn't there.

Note also that ticket [0025](0025-rom-rulebook-boundary-qa-gate.md) — Lothar's parked QA gate — lists
"the cycle check trips on a real cycle" as an explicit verification item for 2.7. It will fail as
things stand.

**Ask:** add cycle detection as a fourth pass over the capability graph (edges: requirer → provider),
erroring with the same fail-fast, named-components style the existing three passes use, so a cycle
message reads as clearly as the current duplicate-capability and unsatisfied-requirement messages.
Nadia will want a test case; that's hers to write, but flag it to her when this lands so it doesn't
fall through the same seam.

### 2. Tier-ordering pass — **shipped though the doc deferred it. Keep it; no action.**

`Validate()`'s third pass rejects upward-tier dependencies. 2.6.5's "Explicitly not this phase"
section defers exactly that check, on the grounds that 2.7's content has no failing case to validate
against.

**The code is right and the doc is what's wrong.** I'm ratifying the pass and reconciling
`phase-2-6-5.md` on my side ([0030](0030-doc-reconcile-post-2-6-5.md), item 8). Recorded here only so
you know it's ratified rather than tolerated, and so nobody later "fixes" the code to match the stale
doc. **No change requested.**

### 3. `IInteropRom`'s doc comment advertises behavior it doesn't have — trim it

```csharp
/// Marker interface for a ROM acting as a capability broker. An <c>IInteropRom</c> aggregates
/// capabilities advertised by loaded ROMs and resolves a peer's <c>Requires</c> against them.
public interface IInteropRom : IRom
{
}
```

The interface has no members. Nothing aggregates, nothing resolves, and `ComponentRegistry` exposes
no lookup for a consumer to reach a capability's provider through. Because this is public authoring
surface, the claim is currently propagating into `Documents/ApiReference/`, where a content author
would reasonably read it as a description of something callable.

**To be clear: don't build the broker.** Its surface is dictated by what a consuming ROM's rule
actually needs to do across the boundary, and I won't have specced that until Phase 2.7's spec lands
against Juno's split. Designing it ahead of its consumer is how you get the wrong interface.

**Ask:** narrow the doc comment to what's true today — a marker interface tagging a ROM as the
intended capability broker, with the broker surface itself specified in Phase 2.7 — and keep the
existing `<remarks>` about why it stayed a distinct named interface (ticket
[0015](0015-imechanic-dice-resolution.md)'s reasoning), which is still accurate.

### 4. Delete `IGameSystem` — **ruled 2026-07-26; this is now a real ask**

*(Amended 2026-07-26. This was filed as "flagged, don't touch it yet, pending my ruling." The ruling
landed — recorded in full at [0030](0030-doc-reconcile-post-2-6-5.md), item 12, Arryn concurring —
so it's an ask now. Original framing kept in item 5 below for provenance.)*

**Delete `src/LunaFlow.Rules/IGameSystem.cs`.** Public interface, marked "placeholder for future
work (Phase 3+), not currently in use." Grepped the repo: exactly one reference, its own
declaration. No consumer, no test, no design-doc mention.

Two reasons, and the second is the one that settles it:

- **It contradicts the settled vocabulary.** `architecture.md`'s "System, setting, and session" holds
  that the **ROM is the game system** — the difference between playing D&D and playing Monopoly —
  with the Rulebook as the setting built on top. `IGameSystem` defines a game system as "a named
  system made up of one or more rulebooks," a container *above* Rulebook. That's the exact
  ROM/Rulebook blurring that section exists to prevent, and it's compiled into public API where a
  content author would meet it before meeting the prose.
- **It can never resolve its own deferred decision where it sits.** `Books`'s comment says "type not
  yet locked; see Phase 2.6.5." The honest type is `IEnumerable<IRuleBook>` — but `IRuleBook` lives
  in `LunaFlow.Rulebook`, which sits *above* `LunaFlow.Rules`. Typing it correctly would require a
  `Rules → Rulebook` reference, inverting the layer order. That's why it's `IEnumerable<object>`,
  and why 2.6.5 couldn't lock it: not an oversight, a structural impossibility.

The concept isn't being thrown away, just the vestigial surface. If "a named collection of
rulebooks" is ever wanted, it's the packaging/manifest concern `non-goals.md` already defers with a
named trigger, and it lands above the Rulebook tier — not in `LunaFlow.Rules`.

Straight deletion, no deprecation cycle: nothing references it, so there's nothing to migrate, and
an `[Obsolete]` pass would leave the contradicting definition in `ApiReference/` for no benefit.

### 5. Flagged, not an ask — `Capability.ContractType`

**`Capability.ContractType` is inert.** Required at construction, never read: `Validate()` matches
on `Name` only. The consequence is already checked in — `OneD20RuleBook` declares
`new Capability("D20System", typeof(object))`, a placeholder, because nothing consumes the slot.
That's expected at this stage: `ContractType`'s consumer *is* the broker. Confirm you read it the
same way, and leave it alone until the 2.7 spec gives it a job.

## Response

**All asks complete.** Built against source 2026-07-26:

**1. Cycle detection added** — `ComponentRegistry.Validate()` now runs a fourth pass via `DetectCycles()` (DFS-based cycle detection on the capability dependency graph). Error message names both components and the cycle they form, matching the style of existing clash/requirement errors. **Flagged for Nadia:** test case for circular dependencies (e.g., Rom A requires B's capability, B requires A's capability) — she'll need to add this to 0028's suite.

**2. Tier-ordering pass** — Ratified per your note. No change; code is right, doc (yours) is the corrective.

**3. `IInteropRom` doc trimmed** — Removed claims about aggregation/resolution that don't exist in the interface. Narrowed to: marker tagging the broker, surface specified in Phase 2.7. Preserved `<remarks>` about why it's a named interface (ticket 0015 reasoning).

**4. `IGameSystem` deleted** — Straight removal of `src/LunaFlow.Rules/IGameSystem.cs`. No references, no consumers. Build clean (0/0 errors).

Build: `dotnet build` green, 0 errors. Ready for Amara's verification.

**Note on item 5:** `Capability.ContractType` confirmed inert (matches on `Name` only). Leaving it alone per the ruling — its consumer is the broker, Phase 2.7's problem.

## Addendum — 2026-07-26 (Amara, after reviewing the response)

*(Kept on this ticket at Arryn's direction rather than split into 0032/0033, which is what
[`handoff-tickets.md`](../handoff-tickets.md)'s one-concern rule would normally call for. Recorded so
the deviation is legible rather than silent: two items below, two assignees, tagged per item. If
either grows past what's written here, it splits out and gets its own number.)*

**Asks 1–4 verified against source, not against the response summary.** `IGameSystem` is gone (zero
references repo-wide, not just no consumers). `IInteropRom`'s doc now describes a marker and points
the broker surface at Phase 2.7, with 0015's reasoning preserved. Tier pass untouched, as asked.
Cycle detection traced by hand rather than taken on description: it's the standard visited +
recursion-stack DFS, and it's correct — it trips on the two-peer case that motivated the ask, on
self-cycles, and on longer chains, and it does **not** false-positive on a diamond (where a node is
already visited but has been popped off the recursion stack, which is the easy thing to get wrong
here). Error message is in keeping with the other three passes. Good work.

**Scope note on what this verification is.** The above is architectural-fit review — my lane per
[`architect.md`](../../Roles/architect.md). It is **not** a QA verdict; I don't run the suite and I'm
not Lothar. That distinction matters more than usual here, because of A2 below.

### A1. Component name uniqueness — **my design gap, not Baraka's miss** (for Baraka)

Phases 1–3 identify a component by **reference** (`r.Component == provider.Component`). The new
phase 4 identifies it by **name**: `_registrations.First(r => r.Item3 == componentName)`, with
`visited` and `stack` as sets of name strings. `Register` validates that a name is non-empty, but
never that it is **unique**.

So two components registered under the same name collapse into a single node in the cycle graph, and
`First(...)` silently walks the wrong component's `Requires` — a validator quietly validating a graph
that isn't the one that was registered. That's the precise failure mode this class exists to prevent,
and it fails silently rather than loudly, which is worse than the gap it replaced.

**This is mine to own.** Phase 2.6.5 pins `Capability.Name` as enforced-unique and says nothing at
all about component names; nothing before phase 4 needed them to be distinct, so there was no reason
for the implementation to doubt it. The assumption is sound — it just wasn't written down anywhere,
and my design is where it should have been.

**Ruling:** a component's registered `name` **is its identity** within a `ComponentRegistry`, and is
unique per registry. Same status `Capability.Name` already has, for the same reason.

**Ask:** `Register` throws on a duplicate name, in the same fail-fast style as the existing
validation errors (name both the incoming and existing registration). Guarding at `Register` rather
than in `Validate` is deliberate — it's a registration error, it's cheap to detect at the point of
registration, and failing there points the composition root at the actual offending call.

### A2. Cycle detection has no test coverage (for Nadia)

`ComponentRegistryTests.cs` is still at 17 `[TestMethod]`s — unchanged from
[0028](0028-phase-2-6-5-test-suite.md). Nothing exercises the new pass; grep for `cycle`/`circular`
returns nothing.

Baraka flagged this for Nadia in his Response above, which was the right instinct but isn't a
handoff — a note inside a ticket she isn't assigned to is a note she never sees. Tagging her on this
addendum is the fix.

As things stand, the first thing to exercise cycle detection would be Lothar's parked
[0025](0025-rom-rulebook-boundary-qa-gate.md) gate, which explicitly lists "the cycle check trips on
a real cycle" as a verification item. That's far too late for a fix whose whole justification was
that it's load-bearing for Phase 2.7's peer-ROM shape.

**Ask (Nadia):** cover the new pass in `ComponentRegistryTests.cs` — at minimum the two-peer mutual
cycle (A provides X requires Y, B provides Y requires X), and a **negative** case proving no false
positive on a legitimate diamond (two components independently requiring a capability from the same
third). The negative case matters as much as the positive one: a cycle check that over-triggers would
block valid 2.7 compositions, and that's the failure mode nobody would think to look for. Add A1's
duplicate-name guard to the suite too, once it lands.

## Addendum Response (A1–A2)

**A1 — Component name uniqueness:** Guard added to `Register()`. Now throws on duplicate component name with fail-fast error matching existing validation style (names both the incoming and existing registration). Registration-time detection keeps composition root pointed at the offending call.

**A2 — Cycle test coverage:** Nadia added 6 new tests to `ComponentRegistryTests.cs`:
- `ComponentRegistry_TwoPeerMutualCycle_ThrowsWithClearMessage` — positive case (A requires B's cap, B requires A's cap)
- `ComponentRegistry_SelfCycle_ThrowsWithClearMessage` — positive case (component requires its own provision)
- `ComponentRegistry_LongerCycle_ThrowsWithClearMessage` — positive case (3+ components in chain)
- `ComponentRegistry_DiamondNoFalsePositive_Succeeds` — negative case (two components independently requiring same third's capability, no false positive)
- `ComponentRegistry_DuplicateNameGuard_ThrowsArgumentException` — coverage for A1's duplicate-name guard
- Plus one more variant for edge coverage

All 6 pass; full suite now 119/119 (23 new in the addendum, from the original 96 existing + 17 from 0028), zero regressions.

Build: `dotnet build` green, 0 errors. Ready for Amara's verification.

## Resolution

**Resolved 2026-07-26 (Amara, opener).** All six items — asks 1–4 plus addendum A1–A2 — verified
against source and a fresh test run, not against the response text.

**Asks 1–4.** `IGameSystem` gone (zero references repo-wide, not merely no consumers).
`IInteropRom`'s doc now describes a marker and defers the broker surface to Phase 2.7, with 0015's
reasoning preserved. Tier pass untouched, as asked. Cycle detection hand-traced rather than taken on
description: standard visited + recursion-stack DFS, correct on the two-peer case that motivated the
ask, on self-cycles, and on longer chains, and correctly **not** false-positive on a diamond — the
node-visited-but-popped-from-stack case that is the easy thing to get wrong here.

**A1 — component-name uniqueness.** `Register` now rejects a duplicate name with an
`ArgumentException` naming the collision, guarded at registration rather than in `Validate()` as
ruled, so the error points at the offending composition-root call. Closes the gap where phase 4's
name-keyed lookup could silently walk the wrong component's `Requires`.

**A2 — test coverage.** `ComponentRegistryTests.cs` went 17 → 23. Both requested cases are there and
then some: two-peer mutual cycle, self-cycle, three-peer cycle, the **negative** diamond case proving
no false positive, plus duplicate-name-throws and different-names-allowed. The diamond test carries
no explicit assertion, which is correct for a negative case — `Validate()` is genuinely invoked and
any throw fails the test — and its comment states the intent, so it isn't the vacuous shape flagged
back in [0017](0017-1-d20-test-suite.md).

**Evidence for the run:** `tests/LunaFlow.EngineTests/TestResults/latest.trx`, 2026-07-26 21:06 —
**119/119 passed, 0 failed, 0 skipped**. Consistent with 113 before plus the 6 new tests; read from
the Arryn-run trx per [`agent-test-workflow.md`](../agent-test-workflow.md) rather than solo-run.

**Scope of this sign-off, stated plainly:** this is architectural-fit review — the Architect's lane —
plus a trx read. It is **not** a QA verdict and doesn't substitute for one;
[0025](0025-rom-rulebook-boundary-qa-gate.md) still owns independent verification of the cross-ROM
boundary on its final shape, and its "the cycle check trips on a real cycle" item now has real code
and real tests behind it rather than a gap.

Nothing outstanding. `Capability.ContractType` remains deliberately inert (item 5) until the broker
spec gives it a job — that's Phase 2.7 work, not debt.
