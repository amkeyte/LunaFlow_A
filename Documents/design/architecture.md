# Architecture

**Status:** target design. Very little of this exists in code yet — see [`CLAUDE.md`](../../CLAUDE.md) for what's actually implemented today. This document describes where the project is headed and why it's shaped that way, so that implementation decisions have something to check themselves against.

## Vision

LunaFlow is a developer-driven, rulebook-powered tabletop RPG framework. A game system (the rulebook — combat, classes, inventory, etc.) is defined in C# against a shared object model. World content (regions, monsters, encounters) is authored as data and light scripting on top of that rulebook, without touching engine internals. A GM runs sessions against the live engine, optionally assisted by an AI layer that turns natural-language requests into rule-consistent outcomes. The engine itself is a turn-based rule evaluation loop built on NRules.

The framework is meant to support several kinds of contributors at once, each with a different amount of trust and a different authoring surface. That separation of concerns is the main thing this document formalizes.

## Layered architecture

```
Core Library            Rule Engine, game loop, fact store, runtime, ROM base, I/O, AI
      ^
Rule Object Model (ROM)  Pluggable modules (Physics, Combat, ...) — provided/extended in C#
      ^
World Modules            Regions, monsters, factions, encounters — JSON/YAML + Lua
      ^          ^
      |     GM Overlays + Live Rules — runtime editor, plot events, cutscene triggers
      |
Player-Contributed Rules  Cosmetic/sandboxed behavior (pets, gestures, etc.)
```

Each layer only depends on the layer(s) below it. World Modules and GM Overlays both sit on top of the ROM but are independent of each other — a GM's live rule change shouldn't require a world module to be recompiled, and vice versa. Player-contributed rules are the most constrained layer and never affect anything outside their own sandbox.

### System, setting, and session

The fastest way to keep these layers straight, in tabletop terms: **ROM is the game system, a Rulebook is a setting built on that system, and World Modules / GM Overlays are a specific session of play.**

The ROM is the difference between playing D&D and playing Monopoly — an entirely different fact vocabulary and an entirely different set of mechanics (HP and spellcasting vs. property and rent). A Rulebook is a specific, compiled game built on top of one or more ROMs — "the Land of Oz version of D&D" or "the Land of Oz version of Monopoly" — the actual classes, spells, board spaces, or setting-flavored content a Rulebook Author writes in C#. World Modules and GM Overlays are what happens once you sit down to actually play a given Rulebook: "our visit to the Emerald City" is a World Module — pre-authored, reviewable content, most likely built by a World-Builder ahead of time — while "go directly to the Wizard, do not pass Go" is a GM Overlay — a live, unreviewed house-rule tweak made at the table, scoped to that one session.

This framing is also why the ROM is meant to be **pluggable rather than a single fixed model** — the diagram's "Player, WorldMap, CombatPhase" list is illustrative, not exhaustive or closed. A Physics ROM (trajectories, materials, spatial intersection tests) is a good example of a self-contained module a Rulebook can choose to depend on: it owns the trajectory math, the flammability lookup, and the intersection test as facts and calculations, while the Rulebook's rules read those ROM facts and decide what happens next (e.g. "flammable + intersects fire → ignites"). A more narrative-focused Rulebook can skip the Physics ROM entirely and use an abstracted damage roll instead, without changing the engine core or the rule pipeline at all — that's the actual payoff of keeping ROM modular: rule authors choose their simulation fidelity by choosing which ROM modules to build on, not by forking the engine.

## Roles and access scope

The layering above maps directly onto who is allowed to author what:

| Role | Responsibility | Access scope | Authoring surface |
| --- | --- | --- | --- |
| Developer | Engine core: loop, session management, performance, rule store, validation, debugging | Core Library (engine internals, versioning, system hooks) | C#, diagnostics, unit test scaffolds, serialization handlers |
| Rulebook Author | Core game system: classes, spells, combat, inventory, AI hooks. Extends the ROM. | Full write access to engine-level ROM types (`Player`, `Item`, `Monster`, `CombatPhase`, ...) | C# + `GeneratorServices` templates and factory APIs, statically compiled for stability |
| World-Builder | Zones, enemies, dungeons, maps, factions, custom items | Data via JSON/YAML plus partial scripted extensions; cannot change engine-level logic | `*.rules.json`, Lua-like scripts, or a DSL-backed module format |
| Storyteller (GM) | Live world-state edits, narrative exceptions, event triggers | `GMSceneScripts`; full read access to assets plus local override access | Pre-game YAML and live scripting (e.g. Lua or an in-engine editor); AI assistance (see [`ai-gm-assistant.md`](ai-gm-assistant.md)) |
| Player | Experiencing the game; optional cosmetic self-expression | `PlayerRule`, a sandboxed subset of `BalancedRule` | In-game GUI, constrained scripting (e.g. `"onPet()" => purr"`) |

A rough `BalancedRule` hierarchy expresses this in code terms:

```csharp
public abstract class RuleBase { /* core runtime logic */ }

public abstract class BalancedRule : RuleBase
{
    public abstract bool IsFair();
    public abstract string[] ApplicableScopes { get; }
}

public class PlayerRule : BalancedRule
{
    public string CreatorId { get; set; }
    public bool IsCosmeticOnly { get; set; }

    public override bool IsFair() => IsCosmeticOnly /* || runtime limits */;
    public override string[] ApplicableScopes => new[] { "self", "pet", "emote" };
}
```

`PlayerRule` can never see combat, world-structure, or balance-relevant facts — the type itself is the sandbox boundary, not a runtime check layered on top.

## Technology choices

| Concern | Technology | Why |
| --- | --- | --- |
| Game loop | C# / .NET | Manages state, turns, command flow |
| Rule engine | NRules | Declarative rule evaluation; already in use today via `LunaFlow.Engine.RuleEngine` |
| Scripting | MoonSharp (Lua) | Sandboxed, embeddable, well-suited to world-builder and GM scripting (see [`scripting-content.md`](scripting-content.md)) |
| World/content data | JSON | Familiar, tool- and AI-friendly, round-trips cleanly with C# |
| Narrative AI | Ollama (local LLM), multi-pass prompting | GM-in-the-loop assistant; see [`ai-gm-assistant.md`](ai-gm-assistant.md) |

Rule and mod authorship splits along the same line as the role table: **rule authors use C#** (compiled, statically checked, stable), **world-builders use JSON/Lua** (data-driven, hot-reloadable, no compile step).

## Target project layout

The current solution (`LunaFlow_A.sln`) has four projects — `LunaFlow.Engine`, `LunaFlow.Rules`, `MyLunaFlowServer`, `LunaFlow.EngineTests` — which correspond to the bottom of this layout. The target layout adds the pieces needed for scripting, multiple interfaces, and AI:

```
/LunaFlow_A
├── src/
│   ├── LunaFlow.ROM/             # Rule Object Model: game state, actors, actions, turn logic
│   ├── LunaFlow.Engine/          # Game loop, rule/apply logic (exists today)
│   ├── LunaFlow.Rules/           # NRules setup, rule authoring & generation (exists today)
│   ├── LunaFlow.Scripting/       # MoonSharp integration and script host
│   ├── LunaFlow.Interface/       # Shared messaging contracts (DTOs)
│   ├── LunaFlow.Interface.Web/   # ASP.NET Core WebSocket server
│   ├── LunaFlow.Interface.CLI/   # Console host, dev tools (MyLunaFlowServer today)
│   ├── LunaFlow.AI/              # AI prompt manager, context handlers
│   └── LunaFlow.Infrastructure/  # Logging, config, file I/O helpers
├── tests/
│   └── LunaFlow.EngineTests/     # Unit + integration tests (exists today)
├── scripts/
│   └── core/                     # Lua scripts for core events and behaviors
├── data/                         # JSON: characters, scenes, world state
├── doc_project/                  # DocFX project + generated API reference
└── docs/
    └── design/                   # This folder — hand-written design docs
```

`LunaFlow.ROM` doesn't exist yet as a project; today its concerns (`GameState`, `GameWorld`, `Session`) are empty placeholder classes living directly in `LunaFlow.Engine.Game`. Splitting them out is a natural step once there's real content to put in them, rather than something to do preemptively. When it is split out, expect it to hold multiple independent modules (a Physics ROM, a Combat ROM, and so on) that a Rulebook opts into individually — see [System, setting, and session](#system-setting-and-session) above — rather than one monolithic model.

Rulebooks (concrete games built on top of LunaFlow) are expected to live in their own solutions/`.csproj`s outside this repo, consuming `LunaFlow.Rules` and `LunaFlow.Engine` as libraries. An internal `LunaFlow.Examples` project may be added here for testing and documentation purposes, but it is not a rulebook itself.
