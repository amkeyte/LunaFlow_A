# RM-0009 — Evaluate Roslyn removal from the rule-compilation pipeline

**Kind:** work
**Status:** open
**Created:** 2026-08-05 (backfilled)
**Owner:** Amara
**DependsOn:** []

Roslyn compiles 1.d20's rule groups at ~14x the cost of the NRules Rete-build it feeds. Findings and
charted options in `rule-pipeline-compilation-cost.md`. Independent branch — nothing currently in
flight depends on this being resolved either way, but its ruling is a real input to `RM-0011`
(World Builder operational) once GM Overlay's live-reload cost model is in scope.

See [ticket 0073](../../ClaudeWiki/Processes/Handoffs/0073-rule-compilation-cost-and-runtime-mode.md).

## Related pages

- [RoadmapGraph proposal](../../ClaudeWiki/RoadmapGraph/proposal.md)
