# Rulebook Content Extraction — Implementation Spec

**Status:** in review — draft complete, acceptance criteria stated.
**Companion design doc:** [`../design/architecture.md`](../design/architecture.md#target-project-layout) — read for rationale; also [`cartridge.md`](../design/cartridge.md) for why content doesn't live inside a Cartridge's implementation.
**Related specs:** none — this is a zero-behavior-change structural move, not a mechanism change.

## Purpose

`src/LunaFlow.Rulebook` currently holds both the Rulebook tier's infrastructure (`IRuleBook.cs`) and
the entire OneD20 Rulebook's concrete content (twelve files under `OneD20/`) in the same project.
That's the one piece of tier discipline the ROM tier already went through and Rulebook never did:
`src/LunaFlow.Rom` sheds real content out to dedicated `example/` projects (D20System, GridMap, Loot,
Reputation), keeping only tier infrastructure plus Nine Rooms as a disclosed, disposable exception
(`nine-rooms.md`). This spec extracts OneD20's content into its own project the same way, leaving
`src/LunaFlow.Rulebook` holding only `IRuleBook`.

**This spec does not implement** any behavior change, any change to `IRuleBook`'s shape, or any move
of Rulebook content *into* the D20 Cartridge (`OneD20SharedRoms`/`D20SystemExampleA`/`D20SystemExampleC`)
— that was the first framing considered and rejected: Rulebook content sitting inside a Cartridge's
implementation assembly would violate the Cartridge structure rule (`cartridge.md`, "nothing outside a
Cartridge may name an implementation type" — a Rulebook is a consumer of the Cartridge, not part of
it) and the roadmap's standing constraint that a Rulebook must not depend on a specific ROM assembly.
OneD20 stays exactly what it is today — a Rulebook that depends on the D20 Cartridge's contract
surface plus the `Loot`/`Reputation` peer ROMs — just relocated to its own project, outside both
`src/LunaFlow.Rulebook` (infra) and the Cartridge (implementation).

## Scope

**In:**
- A new project, `example/OneD20/OneD20.csproj`, holding everything currently under
  `src/LunaFlow.Rulebook/OneD20/`.
- `src/LunaFlow.Rulebook` reduced to `IRuleBook.cs` and its `.csproj`.
- Project reference updates on the new project, `MyLunaFlowServer`, and `LunaFlow.ContentTests`.
- `architecture.md`'s target project layout section, updated to list the new project.

**Out:**
- Any change to `IRuleBook`, `OneD20RuleBook`'s declared `DependsOnRoms`/`Provides`/`Requires`, or any
  other Rulebook-tier contract.
- Any change to the D20 Cartridge (`OneD20SharedRoms`, `D20SystemExampleA`, `D20SystemExampleC`) or its
  contract surface.
- Namespace changes. Every type keeps its current `LunaFlow.Rulebook.OneD20` namespace regardless of
  which project physically holds it — see [Namespace discipline](#namespace-discipline) below.
- Registering the new project with `ComponentRegistry` differently than `OneD20RuleBook` already is —
  this is a file/project move, not a re-registration.
- Rulebook eventually becoming a Cartridge-held tier itself, generalizing this pattern the way `IRom`
  generalized ROM identity. Named as a future direction in the design conversation that produced this
  spec, not scoped here — no second Rulebook exists yet to generalize from, same discipline
  `non-goals.md` applies everywhere else.

## Current layout

```
src/LunaFlow.Rulebook/
├── IRuleBook.cs                                    — tier infrastructure, stays
└── OneD20/                                          — content, moves
    ├── DefaultOneD20WorldModule.cs
    ├── DefaultOneD20WorldModule.FlavorText.cs
    ├── HollowKeepWorldModule.cs
    ├── HollowKeepWorldModule.FlavorText.cs
    ├── IOneD20WorldModule.cs
    ├── LuaOneD20WorldModule.cs
    ├── OneD20RuleBook.cs
    ├── OneD20RulebookReference.cs
    ├── OneD20ScenarioData.cs
    ├── OneD20SessionAssembler.cs
    ├── OneD20WorldModuleValidator.cs
    └── Content/
        └── DefaultOneD20WorldModule.lua              — embedded resource
```

## Target layout

```
src/LunaFlow.Rulebook/
├── IRuleBook.cs
└── LunaFlow.Rulebook.csproj                          — references: Engine, Rules, Scripting (unchanged)

example/OneD20/
├── OneD20.csproj                                     — references: Engine, Rules, Scripting, LunaFlow.Rulebook
├── DefaultOneD20WorldModule.cs
├── DefaultOneD20WorldModule.FlavorText.cs
├── HollowKeepWorldModule.cs
├── HollowKeepWorldModule.FlavorText.cs
├── IOneD20WorldModule.cs
├── LuaOneD20WorldModule.cs
├── OneD20RuleBook.cs
├── OneD20RulebookReference.cs
├── OneD20ScenarioData.cs
├── OneD20SessionAssembler.cs
├── OneD20WorldModuleValidator.cs
└── Content/
    └── DefaultOneD20WorldModule.lua
```

`LunaFlow.Rulebook.csproj`'s own reference set (`Engine`, `Rules`, `Scripting`) is unchanged — those
are what `IRuleBook` itself needs, nothing here adds to them. The `<EmbeddedResource
Include="OneD20\Content\*.lua" />` item currently in `LunaFlow.Rulebook.csproj` moves to
`OneD20.csproj` along with the file it targets.

`example/OneD20/OneD20.csproj`'s reference set is the current `LunaFlow.Rulebook.csproj` set
(`Engine`, `Rules`, `Scripting`) plus one new reference, to the now-slimmed `LunaFlow.Rulebook`, for
`IRuleBook` — the same shape `OneD20SharedRoms.csproj` already has referencing `LunaFlow.Rom` for
`IRom`.

## Namespace discipline

Every moved file keeps its `namespace LunaFlow.Rulebook.OneD20` declaration unchanged. Project name
and .NET namespace are already independent elsewhere in this solution — `OneD20SharedRoms.csproj`
holds code in `LunaFlow.Rom.GridMap`/`.Loot`/`.Reputation`, not `LunaFlow.Rom.OneD20SharedRoms` — so
this isn't a new pattern. The payoff: every `using LunaFlow.Rulebook.OneD20;` in `Program.cs` and
`LunaFlow.ContentTests` stays valid untouched; only the two consuming `.csproj` files'
`<ProjectReference>` entries change.

## Consumer updates

- **`MyLunaFlowServer.csproj`** — add `<ProjectReference Include="..\..\example\OneD20\OneD20.csproj" />`.
  The existing reference to `LunaFlow.Rulebook.csproj` stays (transitively needed for `IRuleBook`
  if `Program.cs` names the interface directly; keep it explicit rather than relying on the transitive
  edge through `OneD20.csproj`, matching this solution's existing style of listing every direct
  dependency).
- **`LunaFlow.ContentTests.csproj`** — same addition. `tests/LunaFlow.ContentTests/Rulebook/OneD20/`
  already imports `LunaFlow.Rulebook.OneD20` directly (`LuaOneD20WorldModuleTests.cs`,
  `OneD20WorldModuleValidatorTests.cs`, `OneD20SessionAssemblerTests.cs`) — no test code changes, only
  the project reference.
- **`LunaFlow_A.sln`** — register the new `example\OneD20\OneD20.csproj` project, same section as its
  `example/` siblings.

## Open question — API reference generation parity

`LunaFlow.Rulebook.csproj` carries a `GenerateApiReference` post-build target
(`DefaultDocumentation.Console`, per `api-doc-generation.md`); `OneD20SharedRoms.csproj` does not.
Whether `example/OneD20/OneD20.csproj` should get the target (documenting OneD20's public surface the
way `LunaFlow.Rulebook`'s did) or go without (matching its nearer sibling, `OneD20SharedRoms`) isn't
resolved by this spec — flagged for Baraka to raise back if the two existing precedents don't already
answer it by the time this is implemented, rather than silently picking one.

## Acceptance criteria

- [ ] `example/OneD20/OneD20.csproj` exists, builds, and contains exactly the twelve files (plus the
      embedded `.lua` resource) listed under [Current layout](#current-layout) above — moved, not
      copied; nothing left behind in `src/LunaFlow.Rulebook/OneD20/`.
- [ ] `src/LunaFlow.Rulebook` contains only `IRuleBook.cs` and its `.csproj` — no `OneD20/` folder.
- [ ] No file's `namespace` declaration changed.
- [ ] `MyLunaFlowServer` and `LunaFlow.ContentTests` both build against the new project reference with
      zero source changes (no `using` updates).
- [ ] `LunaFlow_A.sln` lists the new project under the `example` folder, alongside its siblings.
- [ ] `dotnet build LunaFlow_A.sln` — 0 errors, 0 warnings introduced.
- [ ] `dotnet test tests/LunaFlow.EngineTests` and `dotnet test tests/LunaFlow.ContentTests` — full
      suite green, identical pass count to pre-move, read from an Arryn-run `latest.trx`.
- [ ] `architecture.md`'s target project layout updated to list `example/OneD20`.

## Related documents

- [`../design/architecture.md`](../design/architecture.md#target-project-layout) — the target layout
  this spec brings `LunaFlow.Rulebook` into line with
- [`../design/cartridge.md`](../design/cartridge.md) — why this content does not move into
  `OneD20SharedRoms` instead
- [`../Roadmap/phase-2-11-1.md`](../Roadmap/phase-2-11-1.md) — the phase this spec is built against
- [`nine-rooms.md`](../NineRooms/nine-rooms.md) — the one disclosed exception to "tier infra in
  `src/`, content in `example/`," named here for contrast, not touched by this spec
