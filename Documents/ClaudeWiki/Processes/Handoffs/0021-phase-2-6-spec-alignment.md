# 0021 — Phase 2.6: confirm the System/Scenario split, then reconcile 1-d20-spec.md

**Status:** open
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-20
**For:** Game Designer (Juno)
**Related:** [Phase 2.6](../../../Roadmap/phase-2-6.md) — ROM/Rulebook boundary fortification
(Amara, framework retrofit of 1.d20; not yet ticketed to Baraka for implementation).

## Context

Talking through 1.d20's shape with Amara surfaced a real gap: `architecture.md`'s ROM/Rulebook
split has been described since before Nine Rooms existed, but nothing shipped so far has actually
been *structured* that way. 1.d20 was supposed to be the first real test — your own "Scope split:
System vs. Scenario" section in `1-d20.md` names it explicitly — but it shipped flat:
`D20ResolutionMechanic`, room content, the PC, and the rule group all live undivided in
`LunaFlow.Rom.OneD20`, and the handlers call `LunaFlowEngine.Host.Rules` directly instead of
through a System-owned API. Amara's written up the retrofit as Phase 2.6, wired into the roadmap
(`pending`, between 2.5 and Phase 4) — full doc at [`phase-2-6.md`](../../../Roadmap/phase-2-6.md).

This is explicitly **not** new content design — Phase 2.6's own Ownership section says so, and
it's Amara/Baraka's structural work, not yours to implement. But two things in it are yours:

## Ask

1. **Review the split Amara's now formalized against what you originally meant.** Phase 2.6's
   Deliverables section lists what goes System-tier (six ability scores + modifier formula, the
   universal resolution rule, Defense, saving throws, skill checks, attack resolution, the `Strong`
   archetype's math, `D20ResolutionMechanic`) vs. Scenario-tier (the 9 rooms, DCs, flavor text, the
   monster stat block, the pregenerated PC). This reads like a direct formalization of your own
   "System keeps / Scenario keeps" lists in `1-d20.md` — confirm it actually matches your intent
   before Baraka builds against it, not after. If anything's off, that's a ticket back to Amara, not
   a silent divergence once code exists.
2. **Once the retrofit lands, reconcile `1-d20-spec.md`** (and the declarative rule group's shape)
   against the real namespace/fact-ownership split — this is explicit deliverable #4 in
   `phase-2-6.md`. Your spec currently documents 1.d20's facts and rules as they exist flat today;
   once Baraka splits `LunaFlow.Rom.OneD20` for real, the spec needs to match reality, same
   "update the doc, not just the code" rule every design doc in this repo runs on.

Item 2 is blocked on Baraka's implementation, which isn't ticketed yet — item 1 isn't, and is
cheaper to do now while the design is still just a doc. Not asking you to start drafting 2.d20 yet;
`phase-2-6.md` itself sequences that as later work, once there's a real split to build fresh
content against.

## Response

**Item 1 — reviewed, split confirmed against original intent.** Diffed `phase-2-6.md`'s
Deliverables against `1-d20.md`'s "Scope split: System vs. Scenario" list directly. Clean match on
everything enumerated in both: six ability scores/modifier formula, universal resolution rule,
Defense, saving throws, skill-check and attack-resolution mechanics, the `Strong` archetype's math,
and `D20ResolutionMechanic` → System in both docs; the 9 rooms, DCs, flavor text, monster stat
block, and pregenerated PC → Scenario in both. No drift on the enumerated items.

Two things weren't classified the same way (or weren't classified at all) in both docs — ruled with
Arryn, recorded here so the intent is explicit before Baraka builds:

- **Down-state collapse** (SRD Dying/Disabled/Dead → single Down state) was named under "System
  keeps" in `1-d20.md` but doesn't appear anywhere in `phase-2-6.md`'s deliverables. Ruling: not a
  gap — it's implicit in attack resolution/`D20ResolutionMechanic`, doesn't need its own bullet. No
  ticket needed.
- **Fight/flee semantics and the 3-round monster-flees clock** were never classified in the original
  scope-split section at all (they lived in the earlier "Combat" section, un-tagged), and
  `phase-2-6.md` doesn't address them either. Ruling: **Scenario**, not System — the round count and
  the `RoomFailed` consequence are this mission's pacing choices, not a universal combat-termination
  pattern. System's contribution stops at the attack-resolution primitive (roll vs. Defense, damage
  on a hit); round-tracking and the clock-out consequence stay Scenario-owned facts/logic.

This second point doesn't contradict anything written in `phase-2-6.md` — it fills a gap the doc
left implicit — so no ticket back to Amara is needed on that basis alone. Flagging it here for
whoever scopes the deliverable-2 fact API (Amara/Baraka): make sure `CombatRound`/clock state stays
a Scenario-side fact that calls into System only for the per-round attack-resolution result, not
something System's API ends up owning. If that's already the plan, this is just confirmation; if
not, worth a line in `phase-2-6.md` before implementation starts.

Item 2 (reconciling `1-d20-spec.md` against the real split) stays blocked on Baraka's
implementation, not yet ticketed.

## Resolution

_(Opener to fill in on close.)_
