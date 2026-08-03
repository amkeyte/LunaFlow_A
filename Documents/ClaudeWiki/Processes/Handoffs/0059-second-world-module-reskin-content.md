# 0059 — Second World Module reskin content

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-02
**For:** Juno (Game Designer)

## Decision

Reskin delivered: **Hollow Keep**, a wilderness-ruin dungeon-delve, replacing the riverside-warehouse
heist. Room-for-room structural match (same 9 grid positions, same Fight/SkillCheck slots at 1/5/8 vs.
the rest, same 9 challenges' skill/ability/DC, same 5 monsters' stat blocks) — only names, descriptions,
and narration change, per Juno's call on theme and the "match room-for-room" structural approach.
Content is spec'd, not yet implemented; hand to Baraka whenever convenient, doesn't gate ticket 0060.

## Details

### Context

Phase 2.8's kickoff ([0057](0057-phase-2-8-design-package-ready-for-kickoff.md)) named one open
sequencing call: `world-module-extraction-spec.md`'s acceptance criteria need a second,
independently-authored `IOneD20WorldModule` implementation to prove the swap is real, and content
for it is your call — a reskin is enough to prove the type-level claim, new balance numbers aren't
required (spec's Scope section). Ruling: run this alongside Baraka's build
([0060](0060-implement-world-module-extraction-spec-md.md)), not gating its start — the interface,
`DefaultOneD20WorldModule`, decoupling, and validation pass don't need your content to begin; only
the final "second module runs a full session" acceptance criterion does. Same shape 0034 → CodePlan
0037 had for Phase 2.7.

### Ask

Design a second `IOneD20WorldModule` implementation — a reskin of the existing 9-room/9-challenge/
5-monster/loot/PC content, not a rebalance. Concretely: same room count, challenge-type positions,
and structural shapes the spec's validation pass checks for (`Rooms` has keys `1..9`, `Challenges.
Count >= 6`, `Monsters.Count >= 3`, every flavor key resolves), but different names/flavor/theming
throughout — different monster names and descriptions, different room descriptions, different
skill-check flavor text. Hand it to Baraka whenever it's ready; it doesn't need to land before he
starts the extraction/validation work, only before the phase's own Done-When bar (full session run
against your module) can close.

### Response

Spec + flavor companion filed:
[`1-d20-worldmodule-hollow-keep-spec.md`](../../../Specs/1-d20/1-d20-worldmodule-hollow-keep-spec.md) /
[`1-d20-worldmodule-hollow-keep-flavor.md`](../../../Specs/1-d20/1-d20-worldmodule-hollow-keep-flavor.md).

Premise: a wandering blade-for-hire investigates firelight and screams reported from Hollow Keep, a
wilderness ruin abandoned a generation ago — deliberately distinct genre/tone from Department-7's
modern warehouse heist. Structural choice: match room-for-room (same grid positions, same
Fight/SkillCheck slot assignment, same 9 challenges' skill/ability/DC, same 5 monsters' stat blocks —
`Defense`/`AttackBonus`/`DamageDie`/`DamageBonus`/`MaxHp` all byte-for-byte identical to
`loot-rom-content-expansion-spec.md`'s roster) with only names/descriptions/narration reskinned. PC
sheet (`design/1-d20.md`'s Strong-hero stats and ability scores) is also unchanged — not a rebalance,
per the ticket's ask. Full room, challenge, monster, and item tables plus complete flavor-key coverage
are in the two linked files; nothing here needs Baraka to re-derive intent.

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
