# Architecture

LunaFlow's layered, role-based architecture: what can be built, by whom, using what tools. For the rationale behind these choices, see [Design Rationale](#design-rationale) below.

---

## Quick Reference

### Layered architecture

```
Core Library              Rule Engine, game loop, fact store, runtime, ROM base, I/O, AI
      ^
Rule Object Model (ROM)   Pluggable modules (Dice, Physics, Combat, ...) — ROM Developer (C#)
      ^
Rulebook                  This game's specific rules, built on the ROM — Rulebook Author (C#)
      ^
World Modules             Regions, monsters, factions, encounters — World-Builder (JSON/YAML + Lua)
      ^          ^
      |     GM Overlays + Live Rules — runtime editor, plot events, cutscene triggers — Storyteller
      |
Player-Contributed Rules  Cosmetic/sandboxed behavior (pets, gestures, etc.) — Player
```

Each layer depends only on the layer(s) below it. World Modules and GM Overlays are independent siblings on top of Rulebook — see [`world-module.md`](world-module.md) and [`gm-overlay.md`](gm-overlay.md) for each tier's real boundary. Player-contributed rules never affect anything outside their own sandbox.

**This stack is a trust boundary, not just a layering scheme** — each tier upward is authored by a less-trusted contributor with a broader authoring surface, ending in unreviewed script. Data is therefore **pushed down and vetted by the receiver, never pulled up**; a deep layer that reaches into shallower territory inherits its trust level. The unit that makes this enforceable — a published **Cartridge**, being a contract surface plus the implementation behind it — is defined in [`cartridge.md`](cartridge.md), which also covers why the rule can't be delegated to load-time validation. Read it before designing anything that crosses a tier.

### Roles and access scope

| Role | Responsibility | Access scope | Authoring surface |
| --- | --- | --- | --- |
| Developer | Engine core: loop, session management, performance, rule store, validation, debugging | Core Library (engine internals, versioning, system hooks) | C#, diagnostics, unit test scaffolds, serialization handlers |
| ROM Developer | Reusable domain building blocks and their mechanics — object types plus the logic for using them (a `Dice` object and how to roll it, a Physics ROM's trajectory math). Domain-agnostic; doesn't decide when or why a tool gets used. | Full write access to one or more ROM modules | C#, statically compiled, designed to be reused across many Rulebooks |
| Rulebook Author | This game's specific rules: classes, spells, combat sequencing, win conditions — decisions about how the available ROM tools get used for this Rulebook | Full write access to Rulebook-level rules; read/consume access to whichever ROM modules the Rulebook depends on | C# + `GeneratorServices` templates and factory APIs, statically compiled for stability |
| World-Builder | Zones, enemies, dungeons, maps, factions, custom items | Data via JSON/YAML plus partial scripted extensions; cannot change Rulebook- or ROM-level logic | `*.rules.json`, Lua-like scripts, or a DSL-backed module format |
| Storyteller (GM) | Live world-state edits, narrative exceptions, event triggers | `GMSceneScripts`; full read access to assets plus local override access | Pre-game YAML and live scripting (e.g. Lua or an in-engine editor); AI assistance (see [`ai-gm-assistant.md`](ai-gm-assistant.md)) |
| Player | Experiencing the game; optional cosmetic self-expression | `PlayerRule`, a sandboxed subset of `BalancedRule` | In-game GUI, constrained scripting (e.g. `"onPet()" => purr"`) |

---

## Design Rationale

### Vision

LunaFlow is a developer-driven, rulebook-powered tabletop RPG framework. A game system (the rulebook — combat, classes, inventory, etc.) is defined in C# against a shared object model. World content (regions, monsters, encounters) is authored as data and light scripting on top of that rulebook, without touching engine internals. A GM runs sessions against the live engine, optionally assisted by an AI layer that turns natural-language requests into rule-consistent outcomes. The engine itself is a turn-based rule evaluation loop built on NRules.

The framework is meant to support several kinds of contributors at once, each with a different amount of trust and a different authoring surface. That separation of concerns is the main thing this section formalizes.

### System, setting, and session

Four authoring tiers build on each other, using one running example — a room full of monsters:

- **ROM Developer** builds the reusable toolbox: "here is a `Dice` object, and here's how you roll it." Domain-agnostic — nothing at this layer knows or cares that it'll be used to decide monster counts.
- **Rulebook Author** decides how the toolbox gets used in this game: "use the ROM's `Dice` object to decide how many monsters are in the room." A rule, written in C#, specific to this Rulebook — new *content*, not new ROM.
- **World-Builder** fills in the specifics once the rule exists: "the leader of the monsters is Kong the Great, and he carries a +5 club."
- **Storyteller (GM)** overrides it live, for this one session only: "three monsters is too many for this group — I'll drop that down to just Kong and one more."

Each tier consumes the one below it without needing to understand its internals: the Rulebook Author doesn't need to know how `Dice.Roll()` is implemented, just that it exists and what it returns; the World-Builder doesn't need to know the monster-count rule's formula, just that monsters have a leader and gear slots to fill in; the GM doesn't need to touch either, just the number sitting in front of them.

This is also the cleanest way to keep **ROM** and **Rulebook** from blurring together, which is easy to do since both are C# and both get loosely called "the rules." The ROM is the game **system** — the difference between playing D&D and playing Monopoly, an entirely different fact vocabulary and set of mechanics (HP and spellcasting vs. property and rent) — built by a ROM Developer with no particular game in mind. A Rulebook is the specific, compiled **setting** built on top of one or more ROMs by a Rulebook Author — "the Land of Oz version of D&D" or "the Land of Oz version of Monopoly" — decisions about *this* game, made using tools the ROM Developer already built. World Modules and GM Overlays are what happens once you sit down to actually play that Rulebook — a **session**: "our visit to the Emerald City" is a World Module (pre-authored, reviewable), "go directly to the Wizard, do not pass Go" is a GM Overlay (live, unreviewed, scoped to that one session).

**Rulebooks — and, by the same reasoning, World Modules — are additive, not exclusive.** Loading one
doesn't preclude loading another alongside it: a night-elf character ruleset and a human character
ruleset can both be active at once, and two World Modules could both contribute content to the same
session the same way. This isn't a swap-one-for-another model, and it's worth stating explicitly
because the four-tier diagram only implies it. Conflicts between simultaneously-loaded Rulebooks or
World Modules are a rule-authoring concern first — however NRules' own priority/agenda-group
machinery resolves two rules disagreeing — and past that, whoever assembles a given combination for
shipping is responsible for it, not something the engine arbitrates on its own. See
[`world-module.md`](world-module.md) for the tier this principle is first built against, in
[Phase 2.8](../Roadmap/phase-2-8.md).

This four-tier split is also why the ROM is meant to be **pluggable rather than a single fixed model** — a ROM Developer's Physics ROM (trajectories, materials, intersection tests) is a self-contained module a Rulebook Author can choose to depend on, the same way `Dice` is: it owns the trajectory math, the flammability lookup, and the intersection test as facts and calculations, while the Rulebook's rules read those ROM facts and decide what happens next (e.g. "flammable + intersects fire → ignites"). A more narrative-focused Rulebook can skip the Physics ROM entirely and use an abstracted damage roll instead, without changing the engine core, the rule pipeline, or any other Rulebook that *does* depend on it — that's the actual payoff of keeping ROM development separate from Rulebook authorship: rule authors choose their simulation fidelity by choosing which ROM modules to build on, not by forking the engine or the ROM itself.

### Code-level role hierarchy

A rough `BalancedRule` hierarchy expresses the role/access model in code terms:

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

### Technology choices

| Concern | Technology | Why |
| --- | --- | --- |
| Game loop | C# / .NET | Manages state, turns, command flow |
| Rule engine | NRules | Declarative rule evaluation; already in use today via `LunaFlow.Engine.RuleEngine` |
| Scripting | MoonSharp (Lua) | Sandboxed, embeddable, well-suited to world-builder and GM scripting (see [`scripting-content.md`](scripting-content.md)) |
| World/content data | JSON | Familiar, tool- and AI-friendly, round-trips cleanly with C# |
| Narrative AI | Ollama (local LLM), multi-pass prompting | GM-in-the-loop assistant; see [`ai-gm-assistant.md`](ai-gm-assistant.md) |

Rule and mod authorship splits along the same line as the role table: **ROM Developers and Rulebook Authors both use C#** (compiled, statically checked, stable — even though one's building a toolbox and the other's building a specific game with it), **World-Builders use JSON/Lua** (data-driven, hot-reloadable, no compile step).

---

## Implementation

### Target project layout

The current solution (`LunaFlow_A.sln`) has `LunaFlow.Engine`, `LunaFlow.Rules`, `MyLunaFlowServer`, and `LunaFlow.EngineTests`, plus an early-stub `LunaFlow.Rom` at `src/LunaFlow.Rom/` and already registered in the `.sln`, which correspond to the bottom of this layout. The target layout adds the pieces needed for scripting, multiple interfaces, and AI:

```
/LunaFlow_A
├── src/
│   ├── LunaFlow.Rom/             # Rule Object Model: pluggable domain modules, per-module fact vocabulary
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
└── Documents/
    └── design/                   # This folder — hand-written design docs
```

**Corrected 2026-07-20:** the `doc_project/`/DocFX line and the `docs/` path (this folder is actually at `Documents/design/`, not `docs/design/`) were stale. Ticket 0012 settled the generated-API-reference question differently than this sketch assumed: no `doc_project/`, no DocFX (rejected — "HTML-site-first; wrong shape for an LLM consumer") — instead `DefaultDocumentation.Console`, wired as a post-build target on each authoring-surface `.csproj`, generating markdown directly into `Documents/ApiReference/` (gitignored except a tracked banner `README.md`). See `Documents/ApiReference/README.md` and [handoff ticket 0012](../ClaudeWiki/Processes/Handoffs/0012-api-doc-generation.md).

**Corrected 2026-07-20 (Amara doc pass):** `LunaFlow.Rom` is no longer an empty stub — it holds real, checked-in Nine Rooms content (`NineRoomsContent`, `MoveCommandHandler`/`LookCommandHandler`/`AnswerCommandHandler`, the challenge/win-line/game-end rule classes, `NineRoomsReference`), and, as of the 1.d20 work, is where its d20 System/Scenario content is expected to land too. Don't confuse it with `GameState`/`GameWorld`/`Session` in `LunaFlow.Engine.Game`: those are Engine-owned turn/session bookkeeping (turn number, connected sessions, per-connection context), not ROM content. Nine Rooms' actual game-domain facts — `Room`, `PlayerPosition`, `RoomSolved`, `WinMarker` — live in `LunaFlow.Rom.NineRooms` instead, per [`nine-rooms-spec.md`](../NineRooms/nine-rooms-spec.md). Once ROM work grows past Nine Rooms, expect `LunaFlow.Rom` to hold multiple independent modules (a Physics ROM, a Combat ROM, and so on) that a Rulebook opts into individually — see [System, setting, and session](#system-setting-and-session) above — rather than one monolithic model. (Nine Rooms itself is a deliberate exception to that: per `nine-rooms.md`, it's disposable test content that doesn't get a reusable module boundary.)

Rulebooks (concrete games built on top of LunaFlow) are expected to live in their own solutions/`.csproj`s outside this repo, consuming `LunaFlow.Rules`, `LunaFlow.Engine`, `LunaFlow.Rulebook` (for `IRuleBook` and the Rulebook-tier identity/dependency contracts, relocated there in Phase 2.6.5 — see `engine-core.md`), and whichever `LunaFlow.Rom` modules they depend on as libraries. An internal `LunaFlow.Examples` project may be added here for testing and documentation purposes, but it is not a rulebook itself.

**Corrected 2026-07-21 (Phase 2.6.5 design):** this line previously omitted `LunaFlow.Rulebook` entirely, back when the project held only example content (`OneD20`) and `IRuleBook` lived in `LunaFlow.Rules`. Since `IRuleBook` moved to `LunaFlow.Rulebook` — the project *is* the Rulebook tier, not an example holder that happens to also be usable for more — a real external Rulebook needs it too.

---

## Implementation status

See [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md) — the roadmap is the authoritative source for what's real vs. planned.
