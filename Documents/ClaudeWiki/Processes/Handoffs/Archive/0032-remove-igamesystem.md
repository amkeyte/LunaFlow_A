# 0032 — Remove `IGameSystem` (vestigial, contradicts `architecture.md`)

**Status:** rescinded — duplicate, superseded by [0031](0031-code-currency-vs-2-6-5-design.md) item 4
**Opened by:** Kofi (Supreme Leader)
**Opened:** 2026-07-26
**For:** Baraka (Dev — Implementation)

## Rescinded

Filed within minutes of Amara independently reaching the same conclusion and ruling on it herself —
see [ticket 0030](0030-doc-reconcile-post-2-6-5.md), item 12 (her ruling, Arryn concurring), routed
to Baraka as a real ask in [ticket 0031](0031-code-currency-vs-2-6-5-design.md), item 4. Her
technical case is sharper than the one below: `IGameSystem`'s `Books` property can never resolve its
own "type not yet locked" comment where it sits — the honest type is `IEnumerable<IRuleBook>`, but
`IRuleBook` lives in `LunaFlow.Rulebook`, above `LunaFlow.Rules` in the layer order, so typing it
correctly would require an upward reference. Not an oversight; a structural impossibility. That's
the decisive fact, not just "it's unused and contradicts the vocabulary" (true, but not what settles
it).

Rescinded rather than left open to avoid Baraka getting two tickets asking for the same deletion.
**Live ask is [0031](0031-code-currency-vs-2-6-5-design.md), item 4** — do not action this ticket.
Kept in place per `wiki-style.md`'s supersede-don't-delete rule; original content below for
provenance only.

## Decision

~~Arryn ruled directly (2026-07-26): `IGameSystem` (`LunaFlow.Rules`) is vestigial and gets removed.~~ Superseded by Amara's own ruling above — same conclusion, reached independently, better-argued, already ticketed.

## Details

### Context

Amara found `IGameSystem` while scoping Phase 2.7 design work (verifying 2.6.5's as-built shape against the API reference), not through any systematic check — flagged in [ticket 0030](0030-doc-reconcile-post-2-6-5.md), item 12, and referenced again in [ticket 0031](0031-code-currency-vs-2-6-5-design.md)'s "flagged, not yet an ask" section. Her writeup names four problems, in descending order of severity:

1. **Contradicts `architecture.md` on the meaning of "game system."** The design doc is explicit that the ROM is the game system — "the difference between playing D&D and playing Monopoly" — with Rulebook as the setting built on top. `IGameSystem` defines it as "a named system made up of one or more rulebooks," a container *above* Rulebook. Two contradictory definitions of one term, one in prose and one in compiled public API — exactly the ROM/Rulebook blurring `architecture.md`'s "System, setting, and session" section says it exists to prevent.
2. **It's in `LunaFlow.Rules`** — a content-organization type inside the boundary Phase 2.6.5 just closed against exactly that (Rules stays scoped to NRules authoring infrastructure, not a content home).
3. **`IEnumerable<object> Books`** defers its own type decision with a comment pointing at "Phase 2.6.5" for resolution. 2.6.5 is closed and never touched it.
4. **It's `public`** on the authoring surface without the deliberate allowlist review ticket [0014](0014-engine-api-surface-scope.md) established as the standard for what's exposed there.

It's also absent from every design doc and from `glossary.md` — no page describes it, defines it, or claims it as intended shape. It's dead weight that happens to compile.

### Ask

1. Delete `IGameSystem` from `LunaFlow.Rules`.
2. Confirm no consumers exist before deleting — Amara's characterization is "public, unused," but verify with a grep/reference search rather than taking that as given. If something does reference it, that's a new finding, not a reason to keep it silently.
3. Confirm the `Books` property's deferred-type comment goes with it (no orphaned TODO pointing at a phase that already closed).
4. Full rebuild + regression suite after removal — expected to stay green since nothing consumes it, but confirm rather than assume.

### Acceptance criteria

1. ✓ `IGameSystem` no longer exists in `LunaFlow.Rules` (or anywhere in source).
2. ✓ Grep/reference search confirms zero remaining references before and after removal.
3. ✓ Build compiles clean (0 errors).
4. ✓ Regression suite green (113/113 expected, same as current baseline — no criteria depend on this type).

### Notes

- This is a deletion, not a relocation or retype — Arryn's ruling was explicit that it shouldn't exist, not that it belongs somewhere else.
- Not blocking Phase 2.7 — this is independent cleanup, can land whenever Baraka has a natural slot for it.
- See [Convention 9 — "No vestigial concepts"](../wiki-style.md#convention-9-no-vestigial-concepts) for why this got a ticket instead of sitting as a flagged comment.

## Response

_Filled in by Baraka on pickup._

## Resolution

_Filled in by Kofi when closing — what was verified, how._
