# 0064 — Phase 2.9 ready for the roadmap; Phase 4 needs a scope correction, not a 2.10

**Status:** in-review
**Opened by:** Amara (Architect)
**Opened:** 2026-08-03
**For:** Kofi (PM)

## Decision

Rulings on all five asks: (1) Phase 2.9 added to the roadmap, build ticket
cut to Baraka as [0065](0065-implement-lua-world-module-spec-md.md), no content-design ticket needed
per your own scoping; (2) Phase 4's Overview corrected to cite 2.9, not 2.8, for World Module
data-authoring, and flagged that its `LunaFlow.Scripting` Deliverable now inherits rather than
builds the project — worth confirming that's the intended read; (3) multi-module deferral
reaffirmed with a dated note on `non-goals.md`'s existing entry, not re-opened; (4) agreed — no
Phase 2.10, your kickoff design pass for Phase 4 is expected once 2.9 is underway, noted on
`phase-4.md`, not blocking 2.9's start; (5) **held, not actioned** — closing 0058 redefines Juno's
role scope, which is a structural call per this role's own authority boundary (act-then-report
covers doc fixes, not role-scope changes) — routing to Arryn directly before writing anything into
`game-designer.md`.

## Details

### Context

Working session with Arryn after 2.8's ticket closed, reviewing `architecture.md`'s World-Builder
line ("Regions, monsters, factions, encounters — World-Builder (JSON/YAML + Lua)") against what 2.8
actually proved. Two outputs: a buildable spec for a second `IOneD20WorldModule` authoring path
(Lua, via MoonSharp), and a scope correction to Phase 4's own framing, which had conflated two
different problems under one phase.

### Ask

**1. Add Phase 2.9 to the roadmap.** Spec is written and ready:
[`Specs/lua-world-module-spec.md`](../../../Specs/lua-world-module-spec.md), companion design in
[`design/world-module.md`](../../../design/world-module.md#two-authoring-paths-into-one-contract)
(new section) and [`design/scripting-content.md`](../../../design/scripting-content.md#refinement--the-world-module-tier-collapses-this-split-to-lua-only)
(refinement note). Suggested `phase-2-9.md` content, matching `phase-2-8.md`'s structure:

- **Status:** pending. Design and spec complete, ready for a build ticket.
- **Overview:** A second, independently-authored way to produce an `IOneD20WorldModule` — Lua via
  MoonSharp, hosted in a new `LunaFlow.Scripting` project — alongside the hand-written C# path 2.8
  proved. Lua table literals are a structural superset of JSON, so this collapses
  `scripting-content.md`'s JSON/Lua split into "Lua only" for World Module content specifically; see
  the design doc's refinement note for what that does and doesn't affect.
- **Ownership:** Framework/boundary work — Architect (Amara), already delivered as the spec above.
  Build — Dev (Baraka). No content-design role needed; the proof script is a verbatim transcription
  of already-shipped content, not new game content.
- **Deliverables:** `LunaFlow.Scripting` project with a sandboxed MoonSharp execution primitive;
  `LuaOneD20WorldModule` marshaling a Lua table into `IOneD20WorldModule`'s shape; a demonstration
  script reproducing `DefaultOneD20WorldModule`'s content, diff-verified; two-layer fail-fast
  (marshal errors, then the existing, unchanged validator); a demonstrated sandbox-escape rejection.
- **Done when:** A full session runs correctly against a Lua-authored World Module, with zero
  changes to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, or any
  ROM — same acceptance bar 2.8 held, now proven against a third, differently-authored
  implementation.
- **Related phases:** Previous — Phase 2.8. Next — Phase 4, which inherits `LunaFlow.Scripting`
  already stood up and MoonSharp-wired rather than creating it itself.
- **No new framework dependency** — no `ComponentRegistry`/`EngineTier` change. Relevant to the 0058
  watch-item below.

**2. Correct Phase 4's Overview, not its number.** `phase-4.md`'s current gating text says a
World-Builder script is meant to author the World Module tier "addable alongside other World
Modules rather than exclusive of them" — that sentence pointed at data-authoring, which 2.9 now
owns cleanly. Phase 4's actual Deliverables (`IRuleParser`/`IRuleCompiler` for a script DSL,
`IScript` capability type) are about scripted *behavior* compiled into the live rule engine, a
different mechanism from marshaling scripted *data* into a Rulebook-owned shape. Recommend
reworking that paragraph to cite Phase 2.9 instead of 2.8 as what proves World Module data-
authoring, and dropping the "addable alongside" framing from Phase 4's own gate — that's the
multi-module question, addressed below, and Phase 4 doesn't need it either.

**3. Multi-module/additive-composition loading — recommend explicitly reaffirming the deferral, not
scheduling anything.** Neither 2.9 nor Phase 4 as actually scoped needs two World Modules loaded at
once — Phase 4's literal Done-When bar ("a rule authored in the script format fires correctly in a
running session") doesn't require it. This stays exactly where `non-goals.md` already has it:
deferred until a real second consumer forces it. Worth a one-line note in the roadmap that this was
reconsidered at the 2.8/2.9 gate and reaffirmed, so it doesn't get re-litigated from scratch next
time someone reads "additive composition" and wonders if it's overdue.

**4. No Phase 2.10 — Phase 4 needs a kickoff design pass, same shape 0057 was for 2.8, not a phase
of its own.** What's still open before Baraka can get a buildable Phase 4 spec: committing to the
rule-DSL script format (`scripting-content.md`'s still-open question), and ruling which tier a
scripted *rule* actually belongs to. My working read: `scripting-content.md`'s own `StartCombat`
example (session-state mutation, live trigger) looks like GM-Overlay territory, not World-Module
content — and there's no GM-Overlay design doc at all yet, which would need writing as part of this
pass, the same way `world-module.md` didn't exist until 2.8's kickoff produced it. That's real
design work, and it's mine — I'd propose a Phase 4 kickoff ticket (same shape 0057) once 2.9 is
underway, not blocking 2.9's start, and not needing a phase number of its own to happen. **If** that
design pass surfaces a GM-Overlay-tier build substantial enough to separate from the rule-DSL
pipeline itself, that's the legitimate trigger for a "4.1" — split off once there's real, separate
scope, same pattern 2.6.5 and 2.7.1 both followed, not preemptively.

**5. Reminder: ticket 0058's trigger condition.** Two framework-quiet phases running was Juno's
named trigger for revisiting the content-design/mechanism-verification split. 2.7.1 and 2.8 both
qualified. 2.9 is scoped with "no new framework dependency anticipated" too (see above) — if it
lands clean, that's three. Worth closing out 0058's ruling at this same gate rather than carrying it
further.

### Response

**1.** [`phase-2-9.md`](../../../Roadmap/phase-2-9.md) created from your suggested content, added to
`roadmap.md`'s phase sequence as pending. Build ticket cut to Baraka:
[0065](0065-implement-lua-world-module-spec-md.md). No content-design ticket — matches your read
that the proof script is transcription, not new content.

**2.** `phase-4.md`'s Overview reworked: cites Phase 2.9 (not 2.8) for World Module data-authoring,
drops the "addable alongside" framing, and now states the kickoff-design-pass gate plainly (rule-DSL
format + GM-Overlay-tier question, both still open). **Beyond your literal ask:** `phase-4.md`'s own
Deliverables bullet said "Build the `LunaFlow.Scripting` project" — now that 2.9 stands that project
up first, I reworded it to "Extend," inheriting rather than recreating it. Flagging this since you
didn't ask for it explicitly; let me know if that's not the intended shape.

**3.** Reaffirmation landed as a dated addendum on `non-goals.md`'s existing "Not registering World
Modules with `ComponentRegistry` yet" entry (the one that already names the additive-composition
trigger) rather than a new entry — same fact, one owner, not duplicated across roadmap/design docs.

**4.** Agreed on all counts — no Phase 2.10, your kickoff design pass runs once 2.9 is underway, not
gating its start. Noted on `phase-4.md` directly, pointing back to this ticket.

**5.** Taken to Arryn directly. Ruling: leave the split alone for now — the trigger's fired, but he
doesn't think it's worth the overhead yet. Logged on [0058](0058-juno-role-scope-separate-content-design-from-mechanism-verification.md)
itself as a live watch item, not closed; `game-designer.md` unchanged.

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
