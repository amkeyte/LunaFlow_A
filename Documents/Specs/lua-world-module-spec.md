# Lua-Hosted World Module — Implementation Spec

**Status:** draft
**Companion design doc:** [`../design/world-module.md`](../design/world-module.md#two-authoring-paths-into-one-contract)
— the tier's shape and why a second authoring technology needs no change to the contract, the
validator, or `OneD20SessionAssembler`; read that first, this spec is its buildable translation.
**Related specs:** [`world-module-extraction-spec.md`](world-module-extraction-spec.md) — defines
`IOneD20WorldModule`, `OneD20WorldModuleValidator`, and `DefaultOneD20WorldModule`, all consumed
unchanged here.
**Supersedes:** nothing. Adds a second, independently-authored way to produce an
`IOneD20WorldModule` instance, alongside hand-written C#. No change to the contract itself, to
`OneD20SessionAssembler`, or to `OneD20WorldModuleValidator`.

## Purpose

Turns `world-module.md`'s Lua-authoring design into buildable work: a `LunaFlow.Scripting` project
carrying a sandboxed MoonSharp execution primitive, and `LuaOneD20WorldModule` — a
`LunaFlow.Rulebook.OneD20` type that executes a Lua chunk once and marshals its returned table into
`IOneD20WorldModule`'s shape, proven by reproducing `DefaultOneD20WorldModule`'s exact shipped
content from a `.lua` source instead of a `.cs` one.

**This spec does not implement:** any change to `IOneD20WorldModule`, `OneD20WorldModuleValidator`,
`OneD20SessionAssembler`, or `Program.cs`'s translation logic beyond choosing which concrete
`IOneD20WorldModule` to construct. It does not implement the rule-scripting pipeline
(`IRuleParser`/`IRuleCompiler` for `rule-pipeline.md`'s `when`/`then` DSL) — that's a separate
mechanism for a separate tier concern (scripted behavior, not scripted data) and is out of scope
here even though it will likely also live in `LunaFlow.Scripting` eventually. It does not implement
simultaneous multi-World-Module loading — untouched by this spec, same as
`world-module-extraction-spec.md`'s own Scope/Out.

## Scope

**In:**
- New project `LunaFlow.Scripting`, referencing MoonSharp: a small sandboxed-execution primitive
  (`ISandboxedScriptHost` or equivalent — Baraka's naming call) that runs a Lua source string/file
  and returns the chunk's top-level return value (`DynValue`/`Table`), configured with a restricted
  `CoreModules` set. Nothing else lives here yet — no rule-DSL parser, no `IRuleParser`
  implementation; this spec only occupies the slot `scripting-content.md` reserved.
- `LuaOneD20WorldModule` (`LunaFlow.Rulebook.OneD20`, referencing `LunaFlow.Scripting`): implements
  `IOneD20WorldModule` by executing a Lua chunk via the host above and marshaling its returned table
  into `Rooms`/`StartRoomId`/`Challenges`/`Monsters`/`ItemIdByMonsterId`/`PlayerCharacter`/
  `PlayerAbilityScores`/`FlavorText`.
- A demonstration `.lua` script transcribing `DefaultOneD20WorldModule`'s exact shipped content
  (same 9 rooms / 9 challenges / 5 monsters / loot table / PC stats / flavor text) — proves the
  marshaling round-trips real content correctly, verified by diff against
  `DefaultOneD20WorldModule`'s values, same bar `world-module-extraction-spec.md` held for the C#
  case. This is a mechanism proof, not new game content — no Juno involvement needed.
- Marshal-time failure handling: malformed Lua (syntax error, wrong top-level return type, missing
  required field, wrong field type, unparseable `ChallengeType` string) fails loudly at
  `LuaOneD20WorldModule` construction, before `OneD20WorldModuleValidator` runs.
- A demonstration of the sandbox actually sandboxing: a deliberately hostile test script attempting
  file or OS access, confirmed to fail rather than succeed.

**Out:**
- Any change to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, or the
  Rulebook-owned record shapes (`OneD20RoomData`/`OneD20ChallengeTemplate`/`OneD20MonsterTemplate`/
  `OneD20CombatantStats`) — all `world-module-extraction-spec.md`'s, unchanged.
- The rule-scripting pipeline (`IRuleParser`/`IRuleCompiler`/`IScript` capability type) — separate
  mechanism, separate future spec, per `world-module.md`'s explicit "what this doesn't touch."
- A generic, reflection-based `LuaWorldModuleHost<T>` usable by any Rulebook — `non-goals.md`'s
  discipline; `LuaOneD20WorldModule` stays concrete to OneD20's shape.
- Simultaneous multi-World-Module loading — untouched, same as `world-module-extraction-spec.md`.
- Hot-reload — the Lua chunk executes once, at construction/bootstrap, same lifecycle
  `DefaultOneD20WorldModule`/`HollowKeepWorldModule` already have. No live-reload of an active
  session's World Module.
- A world-builder-facing authoring/validation tool (syntax highlighting, a friendlier error surface
  than an exception message) — `scripting-content.md`'s own open question about where script errors
  surface to a non-technical author is unaffected by this spec and stays open.

## Type vocabulary

No new Rulebook-owned shapes. `LuaOneD20WorldModule` populates the exact same properties
`IOneD20WorldModule` already declares (`world-module-extraction-spec.md`'s Type vocabulary section)
— this spec supplies a second populator, not a new shape.

### Expected Lua chunk shape (convention, not a C# type)

The chunk's single top-level `return` is a table with these keys, mirroring `IOneD20WorldModule`'s
properties one-for-one:

```lua
return {
  startRoomId = 5,
  rooms = {
    [1] = { row = 0, col = 0, challengeType = "Fight",     descriptionKey = "room-1-desc" },
    -- ...
  },
  challenges = {
    search = { skill = "Search", skillAbility = "Int", dc = 10,
               successKey = "search-success", failureKey = "search-failure",
               promptKey = "search-prompt" },
    -- ...
  },
  monsters = {
    ["monster-duelist"] = { name = "...", descriptionKey = "...",
      stats = { defense = 13, attackBonus = 3, damageDie = "1d6", damageBonus = 1, maxHp = 8 } },
    -- ...
  },
  itemIdByMonsterId = { ["monster-duelist"] = "item-rapier" },
  playerCharacter = { defense = 11, attackBonus = 4, damageDie = "1d8", damageBonus = 3, maxHp = 11 },
  playerAbilityScores = { Str = 16, Con = 16, Dex = 10, Int = 10, Wis = 10, Cha = 10 },
  flavorText = { ["room-1-desc"] = "...", ["pc-hit"] = "...", ["win-announce"] = "..." },
}
```

Key naming inside each sub-table (`row`/`col`/`challengeType` etc.) is this spec's own convention,
not pinned by `IOneD20WorldModule` — Baraka's translation call, same as
`world-module-extraction-spec.md` left `DefaultOneD20WorldModule`'s file layout to implementation.
`ChallengeType` is authored as a string (`"Fight"`/`"SkillCheck"`) since Lua has no enum type; the
marshaler parses it, case-insensitively, into `OneD20ChallengeType`.

## Mechanics / rules

### `LuaOneD20WorldModule` construction (eager, fail-fast)

Constructor takes a Lua source (file path or literal string — Baraka's call) and, synchronously,
before returning:

1. Runs the chunk through the sandboxed host (below). A Lua syntax error or runtime error during
   execution propagates as a clear, load-time exception naming the script source and the underlying
   MoonSharp error — not swallowed or deferred.
2. Confirms the chunk's return value is a table with all eight required top-level keys present and
   of the expected coarse shape (table vs. scalar) — a missing key or wrong top-level type fails
   here, by name, before any per-field marshaling is attempted.
3. Marshals each section into the matching `IOneD20WorldModule` property, per the Type vocabulary
   convention above. A field of the wrong Lua type (e.g. a string where `dc` expects a number), an
   unparseable `challengeType`/ability-score key, or a room id outside the expected range fails
   here, naming the specific field and room/challenge/monster key it came from.
4. Stores the marshaled results as immutable properties — `IOneD20WorldModule`'s getters return
   already-materialized dictionaries, no re-execution of the Lua chunk on repeated property access.

This mirrors `world-module-extraction-spec.md`'s own two-layer failure discipline one level
earlier: marshal-shape failures (this spec, new) happen before content-validity failures
(`OneD20WorldModuleValidator`, unchanged, still runs exactly as it does against
`DefaultOneD20WorldModule`).

### Sandboxed execution host (`LunaFlow.Scripting`)

A small, MoonSharp-backed primitive — not tied to `IOneD20WorldModule` or any Rulebook — with
roughly this shape:

```csharp
public interface ISandboxedScriptHost
{
    DynValue Execute(string luaSource);
}
```

Configuration requirement, not left to the default: the underlying `MoonSharp.Interpreter.Script`
must be constructed with a restricted `CoreModules` set that excludes file and OS access
(`Io`, `Os`, `OS_Time`, `OS_System`, `LoadMethods`) at minimum. **Confirm the exact module list
against the MoonSharp version pinned in `LunaFlow.Scripting.csproj`'s package reference** — per
`CLAUDE.md`'s "upstream source before inference" convention — rather than assuming a named preset's
contents from memory or prose docs; if MoonSharp ships a `Preset_HardSandbox`/`Preset_SoftSandbox`
that already excludes the right set, prefer it over a hand-rolled flag combination, but verify what
it actually excludes before relying on the name.

### `Program.cs` (changed: one more concrete `IOneD20WorldModule` option)

`Program.cs` gains the option to construct `IOneD20WorldModule worldModule = new LuaOneD20WorldModule(path)`
in place of `new DefaultOneD20WorldModule()` — same validate-then-assemble sequence
`world-module-extraction-spec.md`'s Phase F already established, unchanged. Which concrete type gets
constructed stays a compile-time choice at the composition root, same as today — this spec doesn't
add a loader.

## Acceptance criteria

- [ ] `LunaFlow.Scripting` project exists, references MoonSharp, and exposes a sandboxed
      script-execution primitive whose `CoreModules` configuration is verified (not assumed) against
      the pinned MoonSharp version.
- [ ] `LuaOneD20WorldModule` implements `IOneD20WorldModule` completely via the marshaling scheme
      above.
- [ ] A demonstration `.lua` script reproduces `DefaultOneD20WorldModule`'s exact shipped content —
      diffed against its values, not just a shape match, same bar `world-module-extraction-spec.md`
      held for the C# case.
- [ ] `OneD20WorldModuleValidator` accepts a `LuaOneD20WorldModule` instance built from the
      demonstration script without any change to the validator itself.
- [ ] A deliberately malformed Lua script (syntax error; missing `rooms` key; a `dc` field written as
      a string; an unrecognized `challengeType` value) is demonstrated to fail loudly at
      `LuaOneD20WorldModule` construction, naming the script and the specific problem — not a
      downstream crash, not silently accepted.
- [ ] A deliberately hostile test script (attempts `io.open`, `os.execute`, or equivalent) is
      demonstrated to fail — proving the sandbox configuration is actually restrictive, not just
      chosen by name.
- [ ] A full session run against a `LuaOneD20WorldModule`-backed session —
      move/look/check/fight/flee/inventory all work, win-line topology unaffected, loot awards
      correctly — with zero changes to `LunaFlow.Rulebook`'s structural code, GridMap, D20System,
      Loot, `IOneD20WorldModule`, or `OneD20WorldModuleValidator`.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

New: a MoonSharp package reference on `LunaFlow.Scripting` (new project). None on
`LunaFlow.Rulebook.OneD20` beyond referencing `LunaFlow.Scripting` itself — the marshaling logic is
plain C# against `DynValue`/`Table`, no additional package needed there. No `ComponentRegistry`/
`EngineTier` change — `LuaOneD20WorldModule` is Rulebook-tier content, same as
`DefaultOneD20WorldModule`, not new engine infrastructure.

## Open questions / edge cases

- **File path vs. embedded resource vs. literal string** for how `Program.cs` supplies the Lua
  source to `LuaOneD20WorldModule` — implementation-shape choice, not specced here, same
  "state behavior, not file layout" instinct `content-spec-template.md` asks for.
- **Whether marshaling errors should collect and report every problem found, or fail on the first
  one** — this spec only requires that failures are loud and name what's wrong; batching vs.
  fail-fast is Baraka's call unless a concrete authoring-experience need argues otherwise later.
- **The demonstration script's authorship** — since it's a verbatim transcription of already-shipped
  content (not new game content), it doesn't need Juno's involvement, but if this spec's acceptance
  work overlaps with her calendar it's a fine small task to hand her instead — a scheduling
  convenience, not a scope requirement.
- **`LunaFlow.Scripting`'s eventual second occupant** — the rule-scripting pipeline
  (`IRuleParser`/`IRuleCompiler`) will likely also live here per `scripting-content.md`'s original
  sketch. Not this spec's concern; flagged so whoever builds that later doesn't have to rediscover
  that the project already exists and already owns the MoonSharp reference.

## Related documents

- [`../design/world-module.md`](../design/world-module.md#two-authoring-paths-into-one-contract) —
  the design this spec builds against, including why this stays concrete rather than generic
- [`../design/scripting-content.md`](../design/scripting-content.md#refinement--the-world-module-tier-collapses-this-split-to-lua-only) —
  why Lua covers both the "data" and "behavior" cases for this tier specifically, and what stays
  separate (the rule-scripting pipeline)
- [`../design/non-goals.md`](../design/non-goals.md) — why `LuaOneD20WorldModule` isn't generalized
  into a reflection-based host yet
- [`world-module-extraction-spec.md`](world-module-extraction-spec.md) — `IOneD20WorldModule`,
  `OneD20WorldModuleValidator`, `DefaultOneD20WorldModule`, all consumed unchanged here
- [`../../CLAUDE.md`](../../CLAUDE.md) — "upstream source before inference," the convention this
  spec's sandboxing requirement leans on directly
- [`../Roadmap/phase-2-8.md`](../Roadmap/phase-2-8.md) — the phase this spec extends
