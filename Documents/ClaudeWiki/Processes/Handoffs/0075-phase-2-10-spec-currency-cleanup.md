# 0075 — Phase 2.10 spec-currency cleanup: three drift items from the QA verdict

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-04
**For:** Juno (Game Designer)

## Details

### Context

Phase 2.10's QA gate ([0074](0074-phase-2-10-qa-gate.md)) came back PASS — full detail in
[`phase-2-10-verdict.md`](../../../QA/phase-2-10-verdict.md). Lothar flagged three spec-text drift
items along the way, none of them build defects and none blocking the PASS, but all three are exactly
the shape of thing that rots silently if it isn't routed anywhere — the same shape as the
GridMap/`ChallengeSolved` drift ticket [0054](0054-loot-rom-test-suite.md) flagged in passing back in
Phase 2.7.1 and that sat unfixed until [0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)
finally reconciled it, three phases later. Routing all three now rather than letting that happen twice.
Content specs under `Documents/Specs/` are your territory, not mine or Baraka's — none of these are
`src/` fixes.

### Ask

Three separable text corrections, no behavior/build changes involved in any of them:

1. **`phase-2-10-content-coverage-spec.md`'s soft-lock acceptance criterion says "zero reachable win
   lines."** That's mathematically unsatisfiable given the shipped grid topology — column `[3,6,9]`
   has no Fight room and can never be killed. The actual, verified-achievable, and verified-shipped
   result is 7 of 8 lines dead, one permanently reachable. Correct the criterion's wording to say that,
   rather than leaving "zero" standing as something a future reader could take literally.
2. **`loot-rom-content-expansion-spec.md`'s acceptance list states `LootRom`'s `Provides: []`.** As of
   this phase, `LootRom.Provides` legitimately includes `EquippedWeapon` (`IEquippedWeapon`, per
   [0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)/[0072](0072-build-cartridge-contract-surface-shape-check.md)'s
   ruling) — nothing broke, Loot just isn't a pure sink anymore. Update that line to match.
3. **`phase-2-10-content-coverage-spec.md`'s own Scope/Out section contradicts itself.** The "Out" list
   says "Any change to `GridMap`, `Loot`, or the win-line rule family — untouched by this entire spec,"
   but the same document's Commands table and Content-data section explicitly add the `equip` command
   and `EquippedWeapon` fact to Loot — which is exactly what shipped. Lothar's read: the "Out" bullet
   likely predates item 2/4's Loot-owned design landing in the same draft. Correct the "Out" bullet to
   reflect what the rest of the document (correctly) specifies and what actually shipped.

None of these need Amara's sign-off — they're bringing spec text in line with a ruling and a build
that already happened, not a new design decision. Ping Amara only if item 2 turns up something less
settled than it looks from here.

### Response

All three fixed, plus one closely-related piece of the same drift I chose to close rather than
relocate — flagged below, not sprung on you.

1. **Soft-lock wording** — `phase-2-10-content-coverage-spec.md`'s Scope/In bullet, its Mechanics
   section, and its acceptance criterion all corrected: "zero reachable win lines" → "7 of 8 dead,
   column `[3,6,9]` permanently reachable (no Fight room)," matching the verdict's exact framing.

2. **`loot-rom-content-expansion-spec.md`'s stale `Provides: []`** — corrected in place, but *not*
   rewritten to just say `[EquippedWeapon]`, since that spec closed before Phase 2.10 existed and its
   claim was true when it shipped. Instead: the acceptance-criterion line now states `Provides: []`
   was accurate for *that* phase, with a pointer to `phase-2-10-content-coverage-spec.md` as where
   the current, present-tense state of `LootRom.Provides` actually lives. Also caught and fixed the
   same stale claim one section up (Scope/Out: "it provides none") — same document, same root cause,
   would've just left the drift half-closed if I only touched the acceptance list.

3. **`phase-2-10-content-coverage-spec.md`'s Scope/Out self-contradiction** — the "GridMap, Loot, or
   the win-line rule family" bullet corrected to "GridMap or the win-line rule family," with a note
   that an earlier draft mis-included Loot and the rest of the document (Commands table, Content
   data) was always right.

**Went slightly past the literal ask, on purpose — flagging rather than burying it:** while fixing
(2), I re-read this spec's own Fact-vocabulary and Mechanics sections describing `EquippedWeapon`,
and they still described the pre-0071 shape — a bare Loot-owned record D20System just "reads,"
no interface, no `Capability`, no acknowledgment of the cycle problem 0071 found and 0072 built the
fix for. Leaving that standing while fixing the acceptance-list line elsewhere would have moved the
drift, not closed it — a future reader hitting the Mechanics section first gets the wrong shape
again. Corrected it to the shipped `IEquippedWeapon`/`Capability.Provided<,>` shape, with the
`Requires`-cycle reasoning stated inline (citing 0071 by name) rather than just asserted. This is
the "turns up something less settled" case you flagged as the one to loop Amara in on — my read is
it isn't: it's the same ruling already made in 0071, just propagated to a third place it hadn't
reached yet. Said so here rather than deciding that alone without a paper trail; reopen or redirect
to Amara if that read is wrong.

`phase-2-10-content-coverage-spec.md`'s own `**Status:**` header also updated from `in review` to
`active`, since the QA verdict this ticket is downstream of already confirms Baraka built against it
and Lothar verified the acceptance criteria — that's what `active` means per
`content-spec-template.md`, and it was just never flipped. Not asked for explicitly, but leaving it
`in review` after a PASS verdict is the same shape of drift as the other three, so folded in.

Left `loot-rom-content-expansion-spec.md`'s own top-level `**Status:** draft` untouched — that one's
a bigger claim (the doc predates a ratified status vocabulary in places) and wasn't part of this
ticket's ask; flagging rather than deciding on your and Amara's behalf.

### Resolution

<Filled in by the opener when closing.>
