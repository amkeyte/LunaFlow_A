# 0057 — Phase 2.8 design/spec package ready for kickoff

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-07-30
**For:** Kofi (PM)

## Decision

**Resolved 2026-08-02.** Phase 2.7.1 closed (ticket 0056, PASS), unblocking this. Ruled on the one
open sequencing call: Juno's second-World-Module reskin content runs alongside Baraka's build, not
gating its start — cut as two tickets, [0059](0059-second-world-module-reskin-content.md) to Juno
and [0060](0060-implement-world-module-extraction-spec-md.md) to Baraka, same shape 0034 → CodePlan
0037 had for Phase 2.7. Closing this kickoff ticket; 0059/0060 carry the actual build forward.

## Details

### Context

Unlike Phase 2.7.1, Phase 2.8 needed real design work before anything was buildable — it's a new
tier, not content on an existing one. Wrote both halves: [`design/world-module.md`](../../../design/world-module.md)
(the tier's shape — a Rulebook-owned data contract a World Module implements, one-directional
dependency so no Cartridge-style neutral assembly is needed) and
[`Specs/world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md) (the
buildable translation: what moves out of `OneD20RulebookContent`/`OneD20FlavorText`, the validation
pass at the boundary, the second-implementation proof that's this phase's actual Done-When bar).
Also recorded the additive-composition principle in `architecture.md` and two `non-goals.md` entries
(not generalizing the contract past OneD20 yet; not `ComponentRegistry`-registering World Modules
yet) — both explicit Phase 2.8 Deliverables/discipline this project holds everywhere else.

Grounded in a source inventory (not assumed): every field currently in
`src/LunaFlow.Rulebook/OneD20/` classified structure-vs-instance-data, and the one real coupling
found — `OneD20SessionAssembler` reads its N-of-M draw sizes off `OneD20RulebookContent`'s static
`.Count` today, which silently assumed a single World Module forever. The spec fixes that as part of
the extraction, not as a separate ticket, since it's what makes the second-implementation proof mean
something rather than being cosmetic.

**No framework gap surfaced.** This builds entirely against what's already shipped — `EngineTier`
already reserves the `WorldModule` slot, `Program.cs`'s composition-root exemption already covers
the translation work, and the "push data down, validate at the boundary" pattern is the same one
`GridRoomData`/`RoomChallenge` already use. Nothing here needed a new `ComponentRegistry` mechanism;
see `world-module.md`'s "deliberately not built yet" for why, with triggers if that changes.

### Ask

Sequence this against Phase 2.7.1's close (still pending — [0053](0053-split-content-tests-into-their-own-test-project.md)/
[0054](0054-loot-rom-test-suite.md) open, [0056](0056-phase-2-7-1-qa-gate.md) tracking the QA gate).
Once ready: hand [`world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md)
to Baraka same as any other build ticket. One open call for you, not me: the spec's acceptance
criteria need a second, independently-authored `IOneD20WorldModule` implementation to prove the
swap — content for it is Juno's call (Scope section: a reskin is enough to prove the type-level
claim, new balance numbers aren't required), so this may want a short Juno pass before or alongside
Baraka's build, same shape as 0034 → CodePlan 0037 had for Phase 2.7.

### Response

<Filled in by the assignee.>

### Resolution

**Resolved 2026-08-02 (Kofi, opener).** Sequencing question answered: a short Juno pass runs in
parallel with Baraka's build, not before it — the extraction/validation work (spec acceptance items
1–4) doesn't need the second module's content, only the phase's final Done-When bar does. Tickets
[0059](0059-second-world-module-reskin-content.md) and
[0060](0060-implement-world-module-extraction-spec-md.md) opened to carry the work; this ticket's
job (get the design package sequenced and kicked off) is done.
