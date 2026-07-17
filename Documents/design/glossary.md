# Glossary

Terms as used across the design docs in this folder, alphabetical. Where a term collides with an NRules or .NET term that means something related but different, that's called out explicitly — those are the ones most likely to cause confusion in code review.

**AEngineComponent** — the future abstract base for governed, tiered game-logic components (`ARom`, and eventually a Rulebook-level equivalent). Adds `Tier` (`EngineTier`) and `DependsOn` on top of `IEngineEventSubscriber`. Not implemented yet — see "Component model" in `engine-core.md`, and note it's an explicit non-goal until a second real ROM exists.

**ARom** — the future abstract base every ROM module would derive from (`AEngineComponent` with `Tier => EngineTier.Rom`). Not implemented yet; today's one ROM-shaped thing (Nine Rooms) is plain `ARule` subclasses with no `ARom` involved at all. See `engine-core.md`.

**ARule** — `LunaFlow.Rules.ARule`, the base class every LunaFlow rule (hand-written or generated) derives from. Combines NRules' `Rule` (via the `NRule` global alias) with an `ILunaFlowReference`, so every rule is both loadable by NRules and traceable to a dotted path.

**BalancedRule** — the abstract rule tier used for anything that must respect game balance (as opposed to purely cosmetic player rules). Sits between `RuleBase` and `PlayerRule` in the sketch hierarchy in `architecture.md`.

**EngineTier** — the future enum (`Rom`, `Rulebook`, `WorldModule`, `GmOverlay`, `Player`) tagging an `AEngineComponent`, sized to match `architecture.md`'s full layered-architecture stack rather than just Rom/Rulebook. Used for two things once it exists: rejecting a component whose `DependsOn` points at a higher tier than itself, and ordering event delivery so lower tiers see `OnTurnAdvance` before higher ones. See "Component model" in `engine-core.md`.

**Fact** — NRules terminology, used as-is: an object inserted into a rule session (`session.Insert(fact)`) that rules can match against. Not a LunaFlow-specific type.

**GeneratorServices** — the `LunaFlow.Rules.GeneratorServices` namespace: the rule-authoring pipeline (parser → IR generator → compiler) described in `rule-pipeline.md`.

**GM / Storyteller** — the role that runs a live session: modifies world state, triggers events, uses the AI assistant. See the role table in `architecture.md`.

**GM Overlay** — a live, session-scoped rule or scripted change a GM makes during play (e.g. "Goblin 3 is unkillable," or "go directly to the Wizard, do not pass Go"), distinct from a World Module (which is authored ahead of time and reviewable).

**IEngineEventSubscriber** — the future small interface (`OnStartup`, `OnTurnAdvance`, `OnResultFeedback`) for anything that wants to observe engine events without being a governed, tiered component. Wider net than `AEngineComponent`: a `Translator` or the AI assistant's narration layer would implement just this, not the fuller contract. See "Component model" in `engine-core.md`.

**Marker** *(also: Standing Rule, Guard)* — a fact type whose mere presence should suppress other rules, e.g. an `ExitGameMarker` that stops further turn processing once a game has ended. Enforced today by hand (`.Not<T>()` in a rule's `When()`); the rule pipeline's `[StandingRule]` attribute is meant to inject this automatically. See "Settling markers" in `engine-core.md`.

**Mechanic** — `LunaFlow.Rules.IMechanic`: an object that tracks or mutates multiple facts in a structured, hard-wired way, as opposed to a single declarative rule. Currently an empty marker interface.

**NRules** — the third-party declarative rule engine (`NRules.Fluent.Dsl.Rule`, `RuleRepository`, `ISession`) LunaFlow builds directly on top of. Not a LunaFlow type; see the [NRules docs](https://github.com/NRules/NRules) for its own vocabulary (Agenda Group, `When()`/`Then()`, etc.).

**PlayerRule** — the most restricted rule tier: cosmetic, sandboxed, scoped to `self`/`pet`/`emote` only. See `architecture.md`.

**Reference** *(`ILunaFlowReference`)* — the hierarchical identity every rule, mechanic, and fact is meant to carry: a dotted path like `MyRuleBook.Chapter(A).Subsection(s)`, with `FullPath`/`Path`, a `Parent`, and `Subreferences`. Used for tracing and organization, not for rule matching.

**ROM (Rule Object Model)** — the layer above the Core Library in `architecture.md`'s diagram: the game-domain types and calculations (`Player`, `WorldMap`, `CombatPhase`, a Physics ROM's trajectory/intersection math, a `Dice` object, etc.) built by a **ROM Developer** in C# — see that entry for how this role differs from a Rulebook Author. Meant to be **pluggable** — a Rulebook depends on whichever ROM modules it needs, not one fixed model. In the "system / setting / session" shorthand (`architecture.md`), the ROM is the game system itself — the difference between playing D&D and playing Monopoly. Not yet a separate project — its concerns currently live as empty placeholders (`GameState`, `GameWorld`) directly in `LunaFlow.Engine.Game`.

**ROM Developer** — the role that builds reusable ROM modules: object types and the mechanics for using them (e.g. a `Dice` object and how to roll it), with no particular game in mind. Distinct from a **Rulebook Author**, who decides *when and why* those tools get used for a specific game (e.g. "use `Dice` to decide how many monsters are in the room"). See "System, setting, and session" in `architecture.md`.

**Rulebook** — the top-level organizational unit of a specific, compiled game built on top of one or more ROMs (`IRuleBook` in `LunaFlow.Rules`), containing rules and rule groups. In the "system / setting / session" shorthand (`architecture.md`), a Rulebook is the setting built on a ROM's system — "the Land of Oz version of D&D" as distinct from D&D's underlying mechanics. Currently `IRuleBook` is an empty marker interface.

**Rulebook Author** — the role that builds this specific game's rules in C# — classes, combat sequencing, win conditions — by *using* ROM tools a ROM Developer already built, not by extending the ROM itself. The `Dice`-object example in `architecture.md`'s "System, setting, and session" section is the clearest illustration of the boundary between the two roles.

**RuleEngineOptions** — `GameEngineOptions.Rules`, the concrete, hardcoded-by-design mechanism for loading rule assemblies and seeding initial facts today: `FromAssembly`/`FromTypes` plus `Seed`, applied to `RuleEngine` at `GameEngineCore.Start()`. Defined in [`nine-rooms-spec.md`](../Specs/nine-rooms-spec.md). This is what "hardcoded ROM registration" means in practice, and it's deliberately *not* the same thing as the future `AEngineComponent`/registry model in `engine-core.md`.

**RuleGroup** — `[RuleGroup("Name")]`, the attribute marking a static class as a set of related `[RuleTemplate]`-attributed rules. Also the informal name for that class itself.

**RuleIR / RuleIRGroup** — the parsed, engine-agnostic intermediate representation of one rule / one group of rules, produced by an `IRuleParser`. See `rule-pipeline.md`.

**RuleCodeUnit / RuleCodeClass** — the generated-code stage of the pipeline: a `RuleCodeUnit` is one output file (namespace + classes + registry method), a `RuleCodeClass` is one generated rule class within it.

**RulePrototype** — the author-facing DTO for describing one rule (`Name`, `WhenStatements`, `ThenStatements`, etc.) before it's parsed into `RuleIR`.

**Session** — ⚠️ **overloaded, watch for this in code and docs.** Two unrelated meanings:
  1. **NRules session** (`ISession`, `factory.CreateSession()`) — a compiled, running rule engine instance you `Insert`/`Update`/`Retract`/`Fire` facts against. This is what `LunaFlow.Engine.RuleEngine` wraps.
  2. **LunaFlow `Session`** (`LunaFlow.Engine.Game.Session`) — a single connected client's context (input history, local state), unrelated to NRules. Currently an empty placeholder.

  When a doc or comment says "session" without qualifying which one, check context — this repo will eventually have both kinds active at once.

**Translator** *(`IGameEngineTranslator`)* — the adapter between one transport (console, WebSocket, test harness) and the singleton engine (`GameEngineCore.Instance`). See `engine-core.md` and `interfaces.md`.

**World-Builder** — the role that authors zones, monsters, and encounters as data (JSON/YAML) plus limited scripting, without touching engine-level logic. See `architecture.md`.

**World Module** — a unit of world-builder-authored content (a region, faction, encounter set) — data plus optional embedded Lua, authored ahead of time and reviewable, as opposed to a GM Overlay (live, unreviewed, session-scoped). Example: "our visit to the Emerald City."
