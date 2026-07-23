# CLAUDE.md

Entry point for agent sessions working in this repository. **Keep this file lean** — working
conventions, commands, a thin orientation, and a map to the docs that own the detail. Architecture,
process, and status live in those docs; don't restate them here, because two copies drift and the
duplicate rots. If you're tempted to add a paragraph of internals, add it to the owning doc and link
it instead.

## Working conventions

- **Do not run state-changing `git`** (commit, add, checkout, etc.) from an agent session — the user
  develops in VS2022 and runs git themselves to avoid file-lock conflicts. Read-only
  `git status`/`diff`/`log` is fine.
- **Status updates flow through the roadmap only.** Phase status is updated in `Documents/Roadmap/roadmap.md`
  and the specific phase doc (`phase-X.md`). All other pages — design docs, specs, role pages, process pages —
  link to the roadmap rather than restating status. This prevents drift and keeps the roadmap as the single
  source of truth. See `Documents/ClaudeWiki/Processes/wiki-style.md#where-status-lives`.

## Commands

.NET 9 solution (`LunaFlow_A.sln`): five projects under `src/`, a test project under `tests/`, and a
`Documents` project (docs + API-reference generation).

```
dotnet build LunaFlow_A.sln                                   # build everything
dotnet test tests/LunaFlow.EngineTests                        # run all tests
dotnet test --filter "FullyQualifiedName~RuleTests.Test2"     # run a single test
dotnet run --project src/MyLunaFlowServer                     # run the console host
```

Tests use MSTest. Conventions — parallelism, the `[DoNotParallelize]`-for-singleton rule, capturing
rule/handler output, anti-patterns — live in `Documents/ClaudeWiki/Processes/testing.md` and the
runbook/patterns/agent-workflow pages beside it. Read those before writing tests.

Versioning is Nerdbank.GitVersioning (`nbgv`, driven by `version.json`); run `dotnet tool restore`
once if `nbgv` isn't available locally.

## Architecture at a glance

Four content/framework tiers plus a host, layered so each depends only downward:

- `src/LunaFlow.Engine` — runtime: command handling, the game loop/host (`GameEngineCore` +
  `ConsoleEngine`), and `RuleEngine`, a thin wrapper over NRules.
- `src/LunaFlow.Rules` — the tier-agnostic rule-authoring surface: `ARule`, the reference/identity
  model, `IMechanic`, and `GeneratorServices` (the author-declaration → NRules pipeline).
- `src/LunaFlow.Rom` — the ROM tier: reusable game-system toolboxes, with direct engine access
  (today: `D20System`, `NineRooms`).
- `src/LunaFlow.Rulebook` — the Rulebook tier: a specific game's data built on a ROM (today:
  `OneD20`).
- `src/MyLunaFlowServer` — console entry point / composition root (`Program.cs`); wires everything
  together.
- `tests/LunaFlow.EngineTests` — MSTest suites.

**Load-bearing invariant:** Rulebook-tier content module never touches a `LunaFlow.Engine` primitive directly —
ROM is the conduit. Composition-root code (`Program.cs`) is exempt.

Don't infer detail from this list — read the owning doc:

- Tiers, layout, roles → `Documents/design/architecture.md`
- Host/runtime internals → `Documents/design/engine-core.md`
- Rule pipeline (`GeneratorServices`) → `Documents/design/rule-pipeline.md`
- What's actually built vs. planned, and the current phase → `Documents/Roadmap/roadmap.md`

## Where to look

- **`Documents/README-QUICK-START.md`** — agent FAQ, navigation map, and key conventions. **Start here if you're new.**
- **`Documents/ClaudeWiki/`** — how the team of sessions works: roles (`Roles/`, each with a
  paste-in bootstrap prompt), processes (`Processes/` — handoff tickets, testing), and the ticket
  ledger (`Processes/Handoffs/`). Start at `Documents/ClaudeWiki/index.md`.
- **`Documents/Roadmap/`** — the phase sequence and status; the authoritative "what's real / what's
  next."
- **`Documents/design/`** — technical design docs (intended shape).
- **`Documents/Specs/`** — buildable specs the devs implement against.
- **`Documents/ApiReference/`** — generated API docs (type shapes/signatures) for the
  authoring-surface projects.
- **`planning/`** — raw, aspirational design transcripts. Useful for *why* something is shaped as it
  is; not a spec for what exists, and not current. Don't build against it.
