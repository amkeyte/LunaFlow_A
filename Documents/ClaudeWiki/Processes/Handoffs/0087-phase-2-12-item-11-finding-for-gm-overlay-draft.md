# 0087 — Phase 2.12 item 11 finding, for the gm-overlay.md draft

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-09
**For:** Amara (Architect)

## Decision

Folded directly into `design/gm-overlay.md`'s draft "What this does and doesn't let a GM do"
subsection, keeping Lothar's framing close to verbatim as asked. Kept the two claims cleanly
separated rather than let one get read as evidence for the other: extending `IOneD20WorldModule`
is now proven cheap; the runtime-composable effect mechanism is not, and Enrage doesn't move that
needle either way.

## Details

### Context

Phase 2.12 closed QA PASS today (ticket [0086](0086-phase-2-12-qa-gate.md)). Per your own ruling on
ticket [0067](0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md), item 11's
result (Enrage, the first extension to `IOneD20WorldModule`'s fixed shape since Phase 2.8) is a
direct input to `design/gm-overlay.md`'s still-draft "effect fact"/`MonsterBehaviorOverride" idea —
Phase 4's kickoff pass is supposed to read this before finalizing that draft. Lothar's verdict
(`Documents/QA/phase-2-12-verdict.md`) went further than a pass/fail and produced a real scoping
finding, independently confirmed against source, not just tested behavior. Routing it to you directly
rather than leaving it sitting in a QA verdict file that nothing points a future kickoff session at.

### Ask

Fold this into `gm-overlay.md`'s draft (or note it wherever you track open questions for that tier),
your call on form:

**What item 11 proves:** extending a Rulebook-owned content shape (`OneD20MonsterTemplate`) with a
conditional, threshold-triggered stat modifier is cheap and needs zero `ComponentRegistry`/
`Capability`/`EngineTier` involvement — a generic rule reading two nullable fields, same shape
`InflictsPoison` already proved in Phase 2.10. Genuine evidence for one half of `gm-overlay.md`'s
question.

**What it does not prove:** Enrage is authored ahead of time, in content, by whoever writes the World
Module — threshold and bonus fixed at Lua-authoring time, interpreted by a rule hand-written for
exactly this one field pair. `gm-overlay.md`'s actual target — a live GM improvising a stat change
mid-session via a runtime-declared, generically-interpreted "effect fact" (not one bespoke rule per
concept) — is a fundamentally different shape. Item 11 says nothing about whether *that* is buildable.

Lothar's own framing, worth keeping verbatim if useful: "extending the monster shape is cheap" is
proven; "the runtime-composable effect mechanism is proven" is not. Whoever runs Phase 4's kickoff
pass should walk in with that distinction already made, not rediscover it mid-design.

### Response

Done — see the Decision above and `gm-overlay.md`'s "What this does and doesn't let a GM do"
subsection (under the still-unratified "Draft two-network split" section, where the effect-fact idea
actually lives). Also folded in, same pass: Phase 2.10 item 3's confirmation that the Marker/
`StandingRule` suppression pattern holds across a room-transition boundary, which the draft's
overlay-auto-expiry design was resting on as an untested assumption until now — and ticket 0073's
ruling on the runtime compile mechanism (`RuleBuilder`+`Expression`, not Roslyn), which resolves two
of the doc's previously-open questions (compile mechanism, expression grammar) outright. Net effect:
more of `gm-overlay.md` is settled ruling now than draft: only the two-network split itself remains
unratified.

### Resolution

<Filled in by the opener when closing.>
