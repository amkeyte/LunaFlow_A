---
id: RM_LUNA_014
uid: RM_LUNA
number: 14
kind: work
status: open
title: 'Phase 2.11.1: Rulebook content extraction'
owner: Kofi
depends_on:
- RM_LUNA_013
created: '2026-08-08'
ticket: 0082
---

<!-- bh-header:start -->
**LunaFlow_A** — [Dashboard](../../BACKHAUL.md) · [Roadmap Index](../ROADMAP_INDEX.md) · RM_LUNA
<!-- bh-header:end -->

Zero-behavior-change structural move: extracts OneD20's content out of `src/LunaFlow.Rulebook` into a new `example/OneD20/OneD20.csproj`, leaving `src/LunaFlow.Rulebook` as infra-only (`IRuleBook.cs`) — the same content/infrastructure split `src/LunaFlow.Rom` already got in Phase 2.7.

**Optional, dead-end pattern.** Nothing currently in the graph names this node in its own DependsOn, and nothing currently needs it to land before proceeding. If a future node needs the extracted `example/OneD20` project specifically, that node routes to this one.
