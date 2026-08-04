# Lua World Module Authoring — Implementation Plan (Ticket 0065 / Phase 2.9)

**Author:** Baraka (Impl Dev)
**Status:** revised per Amara review — one correction applied, ready to build
**Ticket:** [0065](../Documents/ClaudeWiki/Processes/Handoffs/0065-implement-lua-world-module-spec-md.md)
**Spec:** [`lua-world-module-spec.md`](../Documents/Specs/lua-world-module-spec.md)

## Amara review (2026-08-03)

One finding, everything else confirmed as-drafted. The spec's Mechanics section requires execution
failures to propagate "naming the script source and the underlying MoonSharp error"; the original draft's
constructor took a bare `string luaSource` with no name/path, so no thrown message could actually identify
which script failed once more than one is ever in play — invisible today with one call site, but the same
hard-fail-naming discipline the validator and the Cartridge probe hold elsewhere, and the spec's own open
questions flag a loader (multiple scripts) as coming later. **Fix applied below:** an optional `sourceName`
threaded through the constructor and every thrown message, defaulted sensibly by `FromEmbeddedResource`/
`FromFile` to the resource/file name. Everything else — the verified `Preset_HardSandbox` composition, the
marshaling targets matching `world-module-extraction-spec.md`'s record shapes field for field, the
`Rulebook.OneD20` → `Scripting` reference direction, the zero-touch contract/validator/assembler/ROM
guarantee, and routing maintained test coverage to Nadia per the `dev.md` territory split — Amara confirmed
independently and raised no objection.

## Context

Phase 2.8 proved the World Module tier's shape contract (`IOneD20WorldModule`) with two independently-
authored C# implementations (`DefaultOneD20WorldModule`, `HollowKeepWorldModule`). Phase 2.9 adds a third,
differently-authored implementation — this time via Lua/MoonSharp — to prove the contract is genuinely
technology-agnostic, not just author-agnostic. Ticket 0065 (Kofi → Baraka) asks for the full buildable spec
[`lua-world-module-spec.md`](../Documents/Specs/lua-world-module-spec.md) to be built: a new
`LunaFlow.Scripting` project with a sandboxed MoonSharp execution primitive, `LuaOneD20WorldModule`
marshaling a Lua table into the existing contract, a verbatim transcription of `DefaultOneD20WorldModule`'s
shipped content as the demo, two-layer fail-fast, and a demonstrated sandbox escape rejection — with **zero
changes** to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, or any ROM.

Per the PM (Arryn): scope is the whole ticket in one pass, Baraka makes the "Baraka's call" decisions the
spec leaves open, MoonSharp gets pinned to latest **stable**, and this plan is the artifact that goes to
Amara (and possibly Juno) for review before any code is written — so every judgment call below is called
out explicitly rather than buried in the implementation.

## Research already done (verified, not assumed)

- Read `IOneD20WorldModule`, `DefaultOneD20WorldModule` (+ `.FlavorText.cs`), `HollowKeepWorldModule`,
  `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, `OneD20ScenarioData` (the four record shapes) —
  full picture of what `LuaOneD20WorldModule` must produce and how the validator/assembler consume it.
- Read `Program.cs`'s `Bootstrap()` — the exact composition-root wiring point (line ~85, a commented
  swap-line convention already exists for `HollowKeepWorldModule`) and its `InvalidOperationException`
  catch block, which is already generalized across `ComponentRegistry.Validate()` and
  `OneD20WorldModuleValidator.Validate()`.
- Confirmed via NuGet: package id `MoonSharp`, latest **stable** is `2.0.0` (a `3.0.0-beta.1` prerelease
  exists but isn't stable). Targets `netstandard1.6`, which resolves fine under `net9.0`.
- Confirmed via GitHub source **at the exact `v2.0.0.0` tag** (not `master`, which has since moved to the
  3.0 line) — per `CLAUDE.md`'s "upstream source before inference" rule:
  `CoreModules.Preset_HardSandbox = GlobalConsts | TableIterators | String | Table | Basic | Math | Bit32`.
  This **excludes** `IO`, `OS_System`, `OS_Time`, `LoadMethods`, and `Debug` — strictly stronger than the
  spec's stated minimum bar (excludes Io/Os/OS_Time/OS_System/LoadMethods). `Preset_HardSandbox` is the
  right preset to use as-is, no hand-rolled flag combination needed.

## Decisions flagged for Amara (and Juno, if scheduling makes it convenient)

The spec explicitly leaves these open ("Baraka's call") — recommended answer for each, stated so it can be
vetoed before code exists:

1. **Lua source delivery: raw string is the primary API; the demo ships as an embedded resource, not a
   copied loose file.** `LuaOneD20WorldModule`'s constructor takes a Lua source `string` directly (easiest
   to unit-test with inline malformed/hostile snippets, no filesystem dependency). A static
   `LuaOneD20WorldModule.FromEmbeddedResource()` convenience factory reads the demo script out of
   `LunaFlow.Rulebook`'s own assembly via `Assembly.GetManifestResourceStream`. Rejected: a loose `.lua`
   file with `CopyToOutputDirectory` — .NET SDK projects don't reliably propagate content-item copying
   *transitively* across a `ProjectReference` chain (`Rulebook` → `MyLunaFlowServer`), which is exactly the
   kind of "worked on my machine" fragility worth avoiding for a load-bearing demo. Embedding keeps the
   script a real, diffable `.lua` file in source control while sidestepping that path-resolution risk
   entirely, and matches the spec's own "no hot-reload, same lifecycle as the C# case" framing (compiled in,
   executed once).
2. **Fail-fast on the first marshal error, not batched.** Matches `OneD20WorldModuleValidator`'s own
   documented convention ("fail-fast on the first violated check") — consistency with the pattern already
   in this codebase, and the spec only requires failures be loud and specific, not exhaustive.
3. **All marshal/execution failures throw `InvalidOperationException`**, naming the script source and the
   specific field/room/challenge/monster key at fault — same exception type `OneD20WorldModuleValidator`
   already uses, so `Program.cs`'s existing catch block (already generalized across the registry and
   validator) needs **no change** to also catch marshal failures. *(Amara review: "naming the script
   source" wasn't concretely wired to anything in the original draft — fixed via the `sourceName`
   parameter above.)*
4. **MoonSharp `2.0.0`, `CoreModules.Preset_HardSandbox` as-is** (verified above) rather than a hand-rolled
   flag combination.
5. **The malformed-script and hostile-script acceptance-criteria demonstrations are throwaway proofs run
   ad hoc during implementation** (reported in the ticket response with the actual exception text), not
   fixtures checked into `src/`. Maintained regression coverage for both — plus the full-session-against-
   Lua-module integration test — becomes a follow-up ticket to Nadia, the same Baraka→Nadia handoff shape
   as 0052→0054 and 0060→0062. This keeps `tests/` untouched by Baraka per the territory split in `dev.md`.
6. **No Juno involvement needed** — the demo script is a verbatim transcription of already-shipped content,
   not new game content, same as the spec's own framing. Flagging only so the PM can decide whether to loop
   her in anyway; nothing here is blocked on it.

## Implementation

### 1. New project: `src/LunaFlow.Scripting/LunaFlow.Scripting.csproj`

Same shape as the other tier `.csproj`s (`net9.0`, `ImplicitUsings`, `Nullable`, `GenerateDocumentationFile`,
the `GenerateApiReference` post-build target). `PackageReference Include="MoonSharp" Version="2.0.0"`. No
`ProjectReference`s — this project is deliberately host-agnostic, not tied to `IOneD20WorldModule` or any
Rulebook (per spec's Scope). Registered in `LunaFlow_A.sln`.

- `ISandboxedScriptHost.cs` — `DynValue Execute(string luaSource)`, per the spec's own sketch.
- `SandboxedScriptHost.cs` — constructs `new Script(CoreModules.Preset_HardSandbox)`, calls
  `.DoString(luaSource)`. Lets MoonSharp's own `SyntaxErrorException`/`ScriptRuntimeException` propagate
  untouched (they already carry a decent message) — the source-naming context gets added one layer up, in
  `LuaOneD20WorldModule`, which is the layer that actually knows which script/demo this was.

### 2. `LuaOneD20WorldModule` — `src/LunaFlow.Rulebook/OneD20/LuaOneD20WorldModule.cs`

References `LunaFlow.Scripting` (new `ProjectReference` on `LunaFlow.Rulebook.csproj`). Implements
`IOneD20WorldModule` with all eight properties as `{ get; }` set once in the constructor (materialized
dictionaries, no re-execution on repeated access, per spec item 4).

Constructor: `LuaOneD20WorldModule(string luaSource, string sourceName = "<inline script>",
ISandboxedScriptHost? host = null)` — the optional host seam mirrors `OneD20SessionAssembler`'s existing
`Random? random = null` injectable-seam pattern (ticket 0019 precedent), defaults to
`new SandboxedScriptHost()`. **`sourceName` is the Amara-review addition**: threaded into every thrown
message below so a failure names *which* script broke, not just what broke — invisible with today's single
call site, but required by the spec's Mechanics section and cheap now vs. an awkward retrofit once a loader
exists. `FromEmbeddedResource`/`FromFile` (below) default it sensibly from the resource/file name; only a
raw, nameless ad hoc/test construction falls back to the placeholder.

Construction sequence, exactly per spec's Mechanics section:
1. `host.Execute(luaSource)`, wrapping any MoonSharp exception as
   `InvalidOperationException($"Lua World Module script '{sourceName}' failed to execute: {ex.Message}")`.
2. Confirm the return value is a `Table` with all eight required top-level keys (`startRoomId`, `rooms`,
   `challenges`, `monsters`, `itemIdByMonsterId`, `playerCharacter`, `playerAbilityScores`, `flavorText`),
   each of the right coarse shape — throw naming `sourceName` plus the missing/wrong-shaped key before
   touching per-field marshaling.
3. Marshal each section into the exact record shapes from `OneD20ScenarioData.cs`:
   - `rooms[1..9]` → `OneD20RoomData(Id, Row, Col, ChallengeType, DescriptionKey)` — `Id` comes from the
     Lua table's own integer key (matching how the C# dictionary key and `.Id` field are always the same
     value today); `challengeType` is a string, parsed case-insensitively via `Enum.TryParse` into
     `OneD20ChallengeType`, throwing by name on an unrecognized value.
   - `challenges[key]` → `OneD20ChallengeTemplate(Key, Skill, SkillAbility, DC, SuccessKey, FailureKey,
     PromptKey)` — `Key` comes from the Lua table's own string key, same redundancy-matches-C#-convention
     as rooms.
   - `monsters[id]` → `OneD20MonsterTemplate(Id, Name, DescriptionKey, Stats)`, with the nested `stats`
     sub-table marshaled into `OneD20CombatantStats(Defense, AttackBonus, DamageDie, DamageBonus, MaxHp)`.
   - `itemIdByMonsterId`, `playerAbilityScores`, `flavorText` — straightforward table → `Dictionary`
     marshaling (string/int/string values respectively).
   - `playerCharacter` → one `OneD20CombatantStats`.
   - `startRoomId` → `int`.
   A small set of private `RequireNumber`/`RequireString`/`RequireTable` helpers throw
   `InvalidOperationException` naming `sourceName` plus the specific field and its containing
   room/challenge/monster key — satisfies the acceptance criterion that failures name the specific problem,
   not just "marshal failed."

Static convenience factories, both defaulting `sourceName` from their own input rather than leaving it to
the caller:
- `FromEmbeddedResource(Assembly? assembly = null, string resourceName = "...")` — the demo/Program.cs
  path; passes `resourceName` itself through as `sourceName`.
- `FromFile(string path, ISandboxedScriptHost? host = null)` — kept as a cheap secondary option since the
  spec leaves file-path as a valid choice; passes `path` through as `sourceName`. Not the primary path
  given the propagation risk noted above.

### 3. Demonstration script — embedded resource in `LunaFlow.Rulebook.csproj`

`src/LunaFlow.Rulebook/OneD20/Content/DefaultOneD20WorldModule.lua` — a verbatim transcription of
`DefaultOneD20WorldModule.cs` + `DefaultOneD20WorldModule.FlavorText.cs`'s content (9 rooms, 9 challenges, 5
monsters, the loot table, PC stats/ability scores, and the full ~50-entry flavor-text table), in the table
shape the spec's Type Vocabulary section lays out. `<EmbeddedResource Include="OneD20\Content\*.lua" />` in
the csproj. Verified by a direct field-for-field diff against `DefaultOneD20WorldModule`'s C# values (same
bar `world-module-extraction-spec.md` held) — done by hand during implementation and reported in the ticket
response, not just asserted.

### 4. `Program.cs` — one new commented option, matching the existing swap-line convention

```csharp
IOneD20WorldModule worldModule = new DefaultOneD20WorldModule();
// IOneD20WorldModule worldModule = new HollowKeepWorldModule();
// IOneD20WorldModule worldModule = LuaOneD20WorldModule.FromEmbeddedResource();
```

No other `Program.cs` change — the existing `OneD20WorldModuleValidator.Validate(worldModule)` call and
`InvalidOperationException` catch block already work unmodified against a `LuaOneD20WorldModule` instance.

### 5. Sandbox-escape and malformed-script proofs (ad hoc, not checked in — see Decision 5)

During implementation, run two throwaway Lua snippets directly against `SandboxedScriptHost`/
`LuaOneD20WorldModule`:
- Malformed: syntax error, a missing `rooms` key, a `dc` field written as a string, an unrecognized
  `challengeType` — confirm each fails loudly, naming the problem, at construction.
- Hostile: a script referencing `io.open(...)` or `os.execute(...)` — under `Preset_HardSandbox` these
  globals don't exist in the sandboxed `Script` instance at all, so referencing them raises a Lua-level
  "attempt to index a nil value" runtime error, which is itself the proof the sandbox blocks the escape
  attempt (not a permissions error caught after the fact — the capability simply isn't there to call).

Capture the actual exception text for both in ticket 0065's Response section as evidence.

## Files touched

**New:**
- `src/LunaFlow.Scripting/LunaFlow.Scripting.csproj`
- `src/LunaFlow.Scripting/ISandboxedScriptHost.cs`
- `src/LunaFlow.Scripting/SandboxedScriptHost.cs`
- `src/LunaFlow.Rulebook/OneD20/LuaOneD20WorldModule.cs`
- `src/LunaFlow.Rulebook/OneD20/Content/DefaultOneD20WorldModule.lua`

**Modified:**
- `src/LunaFlow.Rulebook/LunaFlow.Rulebook.csproj` — add `ProjectReference` to `LunaFlow.Scripting`, add
  `EmbeddedResource` item for the `.lua` file.
- `src/MyLunaFlowServer/Program.cs` — one commented swap-line addition.
- `LunaFlow_A.sln` — register `LunaFlow.Scripting`.

**Untouched, confirmed:** `IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, all
Rulebook record shapes, every ROM.

## Verification

- `dotnet build LunaFlow_A.sln` — new project compiles, MoonSharp resolves, no changes ripple into ROM/Rom
  projects.
- Ad hoc console run (`dotnet run --project src/MyLunaFlowServer` with the Lua swap-line active) — a full
  session (move/look/check/fight/flee/inventory) plays correctly against `LuaOneD20WorldModule`, same as
  Phase 2.8's bar for `HollowKeepWorldModule`.
- The two ad-hoc malformed/hostile proofs above, output captured.
- Field-for-field diff of the demo `.lua` against `DefaultOneD20WorldModule.cs`'s values.
- `dotnet test tests/LunaFlow.EngineTests` and the content-tests project — confirm nothing existing broke
  (not adding to either suite; that's the Nadia follow-up ticket).
- Report done-bar met to Kofi via ticket 0065's Response section, naming the follow-up ticket to Nadia for
  maintained test coverage.
