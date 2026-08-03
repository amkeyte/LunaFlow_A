# Phase 6 — GM AI assistant

**Status:** pending

See [`design/ai-gm-assistant.md`](../design/ai-gm-assistant.md).

## Overview

Add an AI assistant to help GMs reason through rule outcomes and suggest actions.

Deliberately last: the Fact Resolver stage needs a real, queryable `GameState`/`GameWorld` (Phase 1+) and the Suggestion Display Engine needs a real interface to show a preview in (Phase 5).

## Deliverables

- `LunaFlow.AI` project
- Ollama adapter
- Two-pass prompt flow

## Done when

A GM can type a natural-language action, see the assistant's resolved facts and suggested outcome, and accept or discard it.

## Related phases

- Depends on: [Phase 1 — Engine core consolidation](phase-1.md), [Phase 5 — Interface expansion](phase-5.md)
