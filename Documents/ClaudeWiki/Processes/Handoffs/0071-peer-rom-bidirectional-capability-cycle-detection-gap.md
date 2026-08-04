# 0071 — Peer-ROM bidirectional Capability dependencies trip ComponentRegistry's cycle detector

**Status:** in-review
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-03
**For:** Amara (Architect)

## Decision

Documentation-only ruling, no `ComponentRegistry` change. `Requires` exists to answer a
construction-order question, not "these two ROMs are related" — two peers correlating purely through
NRules fact-matching (`Match<T>`/`Query<T>()`) never have an ordering problem, so that side of a
bidirectional pair stays undeclared, with a doc comment saying why (`GridMapRom.Requires`'s existing
comment is the model). `grid-rom-d20-split-spec.md`'s capability table was simply wrong — corrected to
match shipped code (`GridMap.Requires: Array.Empty<Capability>()`), not the other way around; nothing
was ever built or behaved per the stale table. General rule written into `engine-core.md`'s Cross-ROM
correlation section. Baraka applies this to `EquippedWeapon` himself, per his own framing above.

## Details

### Context

Building item 2/4 of [`phase-2-10-content-coverage-spec.md`](../../../Specs/phase-2-10-content-coverage-spec.md)
(ticket [0069](0069-implement-phase-2-10-content-coverage-spec-md.md)): the `equip` command is
Loot-owned and writes `EquippedWeapon`; D20System's `fight` needs to read it to swap the PC's attack
profile. That's a genuine, functional cross-ROM dependency — D20System needs live state from Loot —
in a Cartridge where Loot already `Requires: [ChallengeSolved]` from D20System. Declaring the
textbook version (a new `IEquippedWeapon` Cartridge contract, `Loot.Provides` += `EquippedWeapon`,
`D20System.Requires` += `EquippedWeapon`) creates D20System→Loot and Loot→D20System — a two-node
cycle in `ComponentRegistry`'s dependency graph. Traced `DetectCyclesDfs`
(`src/LunaFlow.Engine/Components/ComponentRegistry.cs`) by hand: it throws unconditionally on any
`Requires`-chain revisiting a component still on the DFS stack, with no distinction between a real
construction-order cycle and two peers independently correlating on unrelated NRules facts (which
has no actual ordering problem — both facts sit in the same session, `RuleEngine.Fire()` is
synchronous, nothing needs to construct before the other).

Went looking for precedent rather than assuming this was new, and found the same shape already
resolved once — just never generalized or written down. `GridMapRom.Requires` is
`Array.Empty<Capability>()`, with a comment: *"Win-line rules match IChallengeSolved facts through
NRules, not capability mediation."* But GridMap's win-line rules genuinely do match
`IChallengeSolved` (D20System-owned), and `grid-rom-d20-split-spec.md`'s own capability table lists
`GridMap — Requires: [ChallengeSolved]`. Declaring that entry for real would create the identical
GridMap↔D20System cycle (D20System already `Requires: [PlayerLocation, PlayerMovement, GameWon]`
from GridMap) — so it was quietly left undeclared instead. This drift was already caught once: ticket
[0054](0054-loot-rom-test-suite.md)'s review of [0052](0052-implement-loot-rom-content-expansion-spec-md.md)
(2026-07-30) flagged it by name —

> One pre-existing drift surfaced, not this ticket's to fix: `grid-rom-d20-split-spec.md`'s capability
> table still lists GridMap `Requires: [ChallengeSolved]`, but the shipped `GridMapRom.cs` never
> declares it... That's a Phase 2.7 spec/code gap, not something 0052 introduced. Worth a small
> ticket to reconcile the spec's table to what actually shipped; flagging rather than silently
> carrying it forward.

— and no follow-up ticket was ever opened for it. `ComponentRegistryContentTests.cs`'s
`ComponentRegistry_GridMapD20SystemLootAllRegistered_ValidatesSuccessfully` pins the current
(undeclared) shape as correct, and `ComponentRegistryTests.cs`'s
`ComponentRegistry_TwoPeerMutualCycle_ThrowsWithClearMessage` independently confirms the cycle
detector itself is working exactly as designed — this isn't a bug in `ComponentRegistry`, it's a real
modeling gap in what `Capability`/`Requires` can express for two peers that legitimately need each
other.

Per Arryn: this shape of finding — something that should have built against a real architectural
constraint but didn't make it from a design conversation (with Juno, in this case) into the buildable
spec — is exactly what the concrete content-expansion phases (2.10/2.11/2.12) exist to surface. Not
treating this as this ticket's problem to route around.

### Ask

Two things, not one, and they're separable:

1. **Reconcile the drift 0054 already flagged.** `grid-rom-d20-split-spec.md`'s capability table
   (GridMap `Requires: [ChallengeSolved]`) doesn't match shipped `GridMapRom.cs`
   (`Requires: Array.Empty<Capability>()`). Either the spec's table is wrong and should be corrected
   to match what shipped, or the code is wrong and the entry should actually be declared — which
   can't happen today without also resolving (2) below, since declaring it reproduces the cycle.

2. **Rule on the general case.** Is "leave the `Requires` side undeclared, correlate purely through
   NRules fact matching on the shared contract type, and say so in a comment" the sanctioned pattern
   whenever two peer ROMs need a real bidirectional dependency — worth writing down in
   `engine-core.md`'s "Cross-ROM correlation" section or `non-goals.md` so the next implementer finds
   it instead of re-deriving it (as I did) or missing it? Or should `ComponentRegistry`'s cycle
   detector itself learn to distinguish a genuine construction-order cycle from two components
   correlating on independent NRules facts with no real ordering dependency — and if so, roughly how
   (a `Capability` flag marking "matched via NRules, not resolved as an instance"? something else)?

For the immediate case (`EquippedWeapon`), I did neither cleanly — I put `EquippedWeapon` as a plain
concrete record directly in `LunaFlow.Cartridge` (no interface, no `Capability` declaration on either
side), reasoning post-hoc that it needed no new `ComponentRegistry`/`Capability` machinery per the
spec's own framing. That's closer to GridMap's precedent than I gave myself credit for when I shipped
it, but I didn't recognize the precedent at the time, framed it as a novel implementation-shape
choice instead of a repeat of a known, previously-flagged pattern, and didn't check for prior art
before deciding solo — which is the part actually worth a ticket, not just the code. Whatever you
rule for the general case, I'll bring `EquippedWeapon`'s declaration (or lack of one) in line with it.

### Response

Both asks, ruled:

1. **The drift is the spec's, not the code's.** `grid-rom-d20-split-spec.md`'s capability table has
   listed GridMap `Requires: [ChallengeSolved]` since this spec was written; `GridMapRom.cs` has
   always shipped `Requires: Array.Empty<Capability>()` with a comment explaining why. The spec's own
   [Mechanics / rules](../../Specs/grid-rom-d20-split-spec.md#mechanics--rules) section already
   describes the win-line rules matching `IChallengeSolved` through ordinary NRules matching, not
   capability resolution — the prose was always consistent with what shipped; only the capability
   code block wasn't. Corrected the table to `Array.Empty<Capability>()`, with a note pointing at this
   ticket and at `engine-core.md`'s general rule below.

2. **General case: leave the correlation-only side undeclared; don't teach the cycle detector to
   distinguish.** `Requires` exists so `ComponentRegistry` can answer a construction-order question —
   can this graph be built in some valid order. Two peers that only correlate on a shared NRules fact
   type (`Match<T>`, `rules.Query<T>()`) never have a construction-order dependency: both facts live
   in the same session, `RuleEngine.Fire()` is synchronous, matching happens against whatever's
   present at fire time regardless of which ROM's constructor ran first. Declaring `Requires` on both
   sides of a peer pair encodes a construction-order claim that isn't true, and the cycle detector is
   *correct* to reject it — from where it sits, that's genuinely indistinguishable from two components
   each waiting on the other to be built first. So: declare `Requires` only for the direction that's a
   real capability the registry needs to size and order (D20System really does need GridMap
   constructed first, to receive `IPlayerMovement`'s concrete instance); leave the fact-correlation
   direction undeclared, documented with a comment on the ROM, same as `GridMapRom.Requires` already
   does. Wrote this into `engine-core.md`'s [Cross-ROM
   correlation](../../design/engine-core.md#cross-rom-correlation--see-the-spec) section so the next
   implementer finds the reasoning instead of re-deriving it. `ComponentRegistry`'s cycle detector is
   untouched — it's doing exactly what it's for; the gap was in what `Requires` was being asked to
   mean, not in the detector.

Scope note: this ruling doesn't reach `EquippedWeapon` itself — per your own closing paragraph, that's
yours to bring in line with the general rule now that it's written down. Sounds like your current
shape (plain record, no interface, no `Capability` declaration) already matches it, since Loot→D20System
would be the construction-order-real direction if either is, not the reverse.

### Resolution

<Filled in by the opener when closing.>
