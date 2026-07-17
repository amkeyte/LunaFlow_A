# Non-Goals

Things LunaFlow is deliberately not doing right now, and why. The planning transcripts this project grew out of ranged across AI, a web server, scripting, and multiplayer all at once — useful for exploring the space, bad as a build order. This doc exists so a design conversation that starts drifting toward one of these has somewhere to point.

Each entry says what would make it worth revisiting.

**Not a general-purpose game engine.** No rendering, no physics, no asset pipeline. LunaFlow is a rules/state engine for text- and GM-mediated tabletop-style play; visuals are entirely the client's problem, if a graphical client ever exists at all. *Revisit: never, by design — this is a scope boundary, not a sequencing question.*

**Not building a GUI client.** The interface layer (`interfaces.md`) is CLI and WebSocket/text-protocol only. A graphical client is out of scope for this repo; it would be a separate consumer of `LunaFlow.Interface`'s DTOs. *Revisit: once the WebSocket interface (Roadmap Phase 5) is stable enough for a client to build against.*

**Not targeting multiplayer scale.** No auth, no matchmaking, no persistence backend, no concurrent-session isolation beyond what the singleton `GameEngineCore` gives for free. The near-term target is one GM running one live session with one or a few connected clients. *Revisit: after Phase 5, once a real interface layer exists to even define "multiple sessions" against.*

**Not committing to a hosted/cloud LLM.** The AI GM assistant (`ai-gm-assistant.md`) targets a local Ollama model specifically so the tool works offline at a table. Cloud-API backends (OpenAI, Anthropic, etc.) aren't planned, though the design leaves a model-adapter seam that could support one later. *Revisit: only if local inference quality/speed turns out to be a real blocker, and only as an optional adapter, not a replacement.*

**Not abstracting over multiple rule engines.** NRules is a direct, committed dependency (`ARule` derives from `NRules.Fluent.Dsl.Rule` directly), not hidden behind an engine-agnostic interface. `GeneratorServices` abstracts *rule authoring* (C# vs. script), not *rule execution*. *Revisit: only if NRules itself becomes a blocker (licensing, performance, maintenance) — not preemptively.*

**Not building the player-contributed rule sandbox.** `PlayerRule` is named and roughly scoped in `architecture.md`'s role table, but no sandboxing mechanism, review flow, or authoring UI has been designed. This is explicitly the last rung of the role ladder and depends on every other role's tooling existing first. *Revisit: after Roadmap Phase 3, once there's a real game loop worth letting players decorate.*

**Not designing rule schema versioning yet.** `rule-pipeline.md` flags this as an open question rather than a decision; there's no `version:` tag or migration story for scripted rule content today. *Revisit: once `scripting-content.md`'s script format is actually chosen (Phase 4) — versioning a format that doesn't exist yet is premature.*

**Not building the general ROM/Rulebook component model yet.** `engine-core.md` describes a target shape — `IEngineEventSubscriber`, `AEngineComponent`, tiered dependency validation, an event bus for startup/turn/result-feedback — but it's explicitly future design, not something to build against Nine Rooms or a single ROM. Today's loading is a hardcoded `RuleEngineOptions.FromAssembly(...)` call in `Program.cs`, and that's sufficient as long as there's only ever one real content assembly to load. *Revisit: once a second real ROM (or the first real Rulebook) needs dependency ordering or lifecycle coordination the hardcoded call can't express — not preemptively.*

**Not enforcing the ROM conduit at the access-modifier level yet.** `GameEngineCore.Rules` and `Post` are public, so nothing currently stops Rulebook-tier code from bypassing a ROM and touching NRules directly — "ROM as conduit" (`engine-core.md`) is a documented convention, not a compiler-enforced one. Acceptable for now since nothing is violating it on purpose. *Revisit: once there's an actual instance of code bypassing the convention, or a second ROM/Rulebook pairing makes the boundary worth enforcing for real — not preemptively.*

**Not building config-driven ROM/Rulebook packaging yet.** Which ROM modules and which Rulebook assembly to load is a compile-time decision (a literal call in `Program.cs`), not something read from a manifest at startup. A config-defined package format only earns its cost once a Rulebook is meant to be swapped without recompiling the host — "play D&D or Monopoly with the same engine" as an actual runtime choice, not just an architectural claim. *Revisit: when Rulebook work starts in earnest (Roadmap Phase 2+) and there's a second Rulebook/ROM combination to ship.*
