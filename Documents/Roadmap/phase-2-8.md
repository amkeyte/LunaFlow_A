# Phase 2.8 — World Module extraction

**Status:** done — QA-verified 2026-08-03 (ticket
[0063](../ClaudeWiki/Processes/Handoffs/0063-phase-2-8-qa-gate.md), verdict
[`phase-2-8-verdict.md`](../QA/phase-2-8-verdict.md), **PASS**). All Done-When criteria confirmed
against source, an Arryn-run `latest.trx` (155/155), live fault injection on the validator, and a
live Hollow Keep session (a win, a loss, a skill check) — not ticket prose. Baraka built all five
spec items ([0060](../ClaudeWiki/Processes/Handoffs/0060-implement-world-module-extraction-spec-md.md)),
Juno's Hollow Keep reskin proved the swap
([0059](../ClaudeWiki/Processes/Handoffs/0059-second-world-module-reskin-content.md)), and the test
suite is repointed and green
([0062](../ClaudeWiki/Processes/Handoffs/0062-tests-lunaflow-contenttests-broken-by-phase-2-8-world-module-extraction.md)).
One non-blocking finding carried forward: `OneD20WorldModuleValidatorTests.cs` under-specifies its
own fixed-vocabulary/ability-loop coverage (1-of-11, 1-of-6) — an informal note to Nadia, not a
ticket, since the mechanism itself is demonstrated working live.

## Overview

Pulls concrete content instances out of the Rulebook tier into a genuine World Module tier.
Currently that content is "scrunched into" Rulebook — correctly, for 2.6's purposes; retrofitting
1.d20 into System/Scenario was the right first cut, and this is the next one, not a correction of it.

`architecture.md`'s own role table already draws the line this phase enforces: Rulebook is the game's
rules *as mechanism* ("a Fight room requires an opponent shaped like `CombatantStats`"); World Module
is specific content *instances* — "zones, enemies, dungeons, maps," the role table's own words, which
is literally "the monster's stat block" and "this room's specific assignment." Rulebook keeps the
shape of the game; World Module supplies what's actually in it.

**Rulebooks — and by the same reasoning, World Modules — are additive, not exclusive.** Loading one
doesn't preclude loading another alongside it: a night-elf character ruleset and a human character
ruleset can both be active at once, same as two World Modules could both contribute content to the
same session. This isn't a swap-one-for-another model. Conflicts between simultaneously-loaded
Rulebooks or World Modules are a rule-authoring concern first — however NRules' own priority/agenda-
group machinery resolves two rules disagreeing — and past that, it's whoever assembles a given
combination for shipping who's responsible for it, not something the engine arbitrates on its own.

## Ownership

- Framework/boundary work: [Architect (Amara)](../ClaudeWiki/Roles/architect.md).
- The second World Module that proves the split, and any real content decisions: [Game Designer
  (Juno)](../ClaudeWiki/Roles/game-designer.md).

## Deliverables

- **Extract concrete content instances from Rulebook into a real World Module tier** — the monster's
  specific stat block, the specific room-to-challenge/DC assignments, flavor text. Rulebook retains
  only structure and validation: which challenge *types* exist, what shape an opponent or a room
  needs to have to satisfy one.
- **A second, independently-authored World Module**, proving the tier is genuinely separated from
  Rulebook rather than just organized that way by convention — it runs correctly against the same
  Rulebook and ROMs with zero Rulebook changes required.
- **Record the additive-composition principle** (Overview, above) in `architecture.md`, since it
  isn't currently stated anywhere explicit — only implied by the four-tier diagram.
- This phase stays hand-authored C# — it proves the tier exists and is genuinely separable, not that
  it's authorable without a compile step. That's Phase 4's job, once this tier is real to build a
  pipeline against.

## Done when

A second, independently-authored World Module runs correctly against the same Rulebook and ROMs as
the first, with zero Rulebook or ROM changes required — proving content and structure are actually
separated, not just conventionally organized. Whether the two World Modules are also exercised
loaded simultaneously (per the additive-composition principle) is Juno's call on how far to push the
proof here versus leaving simultaneous multi-module loading to a later, content-driven need.

## Related phases

- Previous: [Phase 2.7 — Multi-ROM composition](phase-2-7.md)
- Next: [Phase 4 — World content & scripting](phase-4.md) — inherits a World Module tier proven
  genuinely separable from Rulebook and ROM to build a JSON/Lua authoring pipeline against, instead
  of designing that pipeline against a tier that only exists in prose.

## Related documents

- [`design/architecture.md`](../design/architecture.md) — the four-tier diagram and role table this
  phase enforces for real, plus the additive-composition principle this phase's Deliverables asked
  to have recorded there
- [`design/world-module.md`](../design/world-module.md) — the tier's shape: a Rulebook-owned data
  contract a World Module implements, why it needs less machinery than Cartridge did, and what's
  deliberately not built yet
- [`Specs/world-module-extraction-spec.md`](../Specs/world-module-extraction-spec.md) — the buildable
  spec: what moves out of `OneD20RulebookContent`/`OneD20FlavorText`, the validation pass, the
  second-implementation proof
- [`design/1-d20.md`](../design/1-d20.md) / [`Specs/1-d20/1-d20-spec.md`](../Specs/1-d20/1-d20-spec.md)
  — the content this phase's first World Module extraction draws from
- [Phase 2.6 — ROM/Rulebook boundary fortification](phase-2-6.md) — where this content currently
  lives, and why that was the right call at the time
