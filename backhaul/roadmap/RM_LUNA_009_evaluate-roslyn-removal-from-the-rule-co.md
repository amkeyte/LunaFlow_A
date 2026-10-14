---
id: RM_LUNA_009
uid: RM_LUNA
number: 9
kind: work
status: open
title: Evaluate Roslyn removal from the rule-compilation pipeline
owner: Amara
depends_on: []
created: '2026-08-05'
---

<!-- bh-header:start -->
**LunaFlow_A** — [Dashboard](../../BACKHAUL.md) · [Roadmap Index](../ROADMAP_INDEX.md) · RM_LUNA
<!-- bh-header:end -->

Roslyn compiles 1.d20's rule groups at ~14x the cost of the NRules Rete-build it feeds. Findings and charted options in `rule-pipeline-compilation-cost.md`. Independent branch — nothing currently in flight depends on this being resolved either way, but its ruling is a real input to World Builder operational (RM_LUNA_011) once GM Overlay's live-reload cost model is in scope. See ticket 0073.
