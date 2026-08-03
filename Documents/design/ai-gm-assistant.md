# GM Assistant (AI)

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

## Purpose

The assistant helps a GM (see the role table in [`architecture.md`](architecture.md)) turn a natural-language request into a rule-consistent outcome, without the GM needing to know the exact fact or rule that governs it. The design goal stated in planning is that the assistant be traceable (each step logged and inspectable), GM-friendly (the GM can preview or override every step), and modular (each stage independently testable and swappable) — in particular, swappable enough that the backend LLM isn't load-bearing for the rest of the design.

## Why multi-pass

A single LLM call given "Billie jumps into the lava" has no way to know Billie's fire resistance, current HP, or whatever house rule the table uses for environmental hazards — it would have to either hallucinate those facts or the GM would have to paste them into the prompt by hand every time. Splitting the interaction into two passes avoids both: the model first asks for exactly the facts it needs, the engine resolves them from real game state, and only then does the model produce a narrated outcome or script grounded in real numbers.

## Pipeline

```
GM Input Console
  "Billie jumps into the lava"
        │
        ▼
Scene Context Builder
  summarizes current location, actors, recent events from GM scene notes
        │
        ▼
Prompt Composer
  formats an LLM prompt from GM input + context, asking "what info do you need?"
        │
        ▼
AI Pass 1: Query Analyzer
  LLM replies with a structured list of needed facts (character, rules, etc.)
        │
        ▼
Fact Resolver
  pulls the requested data from real game state: character sheet, rulebook, environment
        │
        ▼
AI Pass 2: Response Generator
  LLM generates narration, Lua, or rule logic using GM input + resolved facts
        │
        ▼
Suggestion Display Engine
  shows the result to the GM: accept / edit / run
```

Optional caching layers sit alongside this: a scene-notes memory feeding the Scene Context Builder, a log of which facts were actually used (for tracing/debugging), and an LLM model adapter as the one seam that talks to a specific backend (Ollama, or another provider later).

## Backend

- **Model**: a local LLM served by Ollama, using quantized GGUF models — a local-first choice, not a hosted-API one, which matters for a GM tool that may need to run at a table with no reliable internet.
- **Structured context injection**: scene summaries, active character data, and current location/recent events are assembled by the Scene Context Builder and injected into both passes, rather than relying on conversation history alone.

## Where this plugs into the rest of the system

- The Fact Resolver's job — pulling character sheet, rulebook, and environment data — depends on `LunaFlow.Rom` and the engine's fact store existing in a queryable form; this can't be built meaningfully before [`engine-core.md`](engine-core.md)'s `GameState`/`GameWorld` types have real shape.
- Pass 2's output ("Lua, narration, or logic") overlaps directly with [`scripting-content.md`](scripting-content.md) — a generated script needs to go through the same validation/sandbox path as a human-authored one, not bypass it because the AI produced it.
- The Suggestion Display Engine implies a GM-facing interface capable of showing a diff/preview before committing — this is an `LunaFlow.Interface`-layer concern (see [`interfaces.md`](interfaces.md)), not something the AI pipeline should own directly.

## Open design questions

- Nothing has been decided about the `LLM Model Adapter` interface shape — what a swap from Ollama to another backend would actually require changing.
- How much of "accept/edit/run" is a UI concern vs. something the engine needs to model explicitly (e.g. a pending, uncommitted rule/fact that can be discarded).
- Whether Pass 1's "requested facts" have a fixed vocabulary tied to the ROM's shape, or are free-form and resolved best-effort.
