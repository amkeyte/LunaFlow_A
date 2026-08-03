# 0060 — Implement world-module-extraction-spec.md

**Status:** open
**Opened by:** Kofi (PM)
**Opened:** 2026-08-02
**For:** Baraka (Impl Dev)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

Phase 2.8 ([`phase-2-8.md`](../../../Roadmap/phase-2-8.md)) is design-complete and ready to build —
Amara's design (`design/world-module.md`) and buildable spec
([`world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md)) are both in
place, reviewed, and unblocked now that Phase 2.7.1 has closed (see
[0057](0057-phase-2-8-design-package-ready-for-kickoff.md)). No new framework capability anticipated
— this is Rulebook-tier restructuring only, same "toolbox-first" discipline the last two content
phases held.

### Ask

Build [`world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md) in full
against its own acceptance criteria:

1. **`IOneD20WorldModule`** contract in `LunaFlow.Rulebook.OneD20`; **`DefaultOneD20WorldModule`**
   reproduces today's exact shipped content (9 rooms, 9 challenges, 5 monsters, loot table, PC
   stats, flavor text) — verified by diff against `OneD20RulebookContent`/`OneD20FlavorText`'s
   current values, not just a shape match.
2. **Retire the static surface** — `OneD20RulebookContent`/`OneD20FlavorText` gone; zero references
   left in `LunaFlow.Rulebook`, `example/`, or `src/MyLunaFlowServer`.
3. **Decouple `OneD20SessionAssembler`** — takes an `IOneD20WorldModule`, no references to the old
   statics by name.
4. **A load-time validation pass** at the World Module boundary (spec's Mechanics section lists the
   checks) — demonstrate it rejects a deliberately broken module with a clear, non-zero-exit error.
5. **A second, independently-authored `IOneD20WorldModule`** proving the swap — content is Juno's
   call, tracked separately as [0059](0059-second-world-module-reskin-content.md); don't block
   starting 1–4 on it landing, but the phase's actual Done-When bar (full session, zero Rulebook/ROM
   changes) needs it before this ticket can close.

If content/implementation surfaces a real framework gap the spec's toolbox can't express, that's a
ticket to Amara, not a workaround — same discipline `loot-rom-content-expansion-spec.md` held.

Once both `IOneD20WorldModule` implementations run clean and the validation pass is demonstrated:
full suite green, read from an Arryn-run `latest.trx`, per the spec's own last acceptance item.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
