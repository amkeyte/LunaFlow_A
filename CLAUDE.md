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
- **API Reference before `/src`:** Consult `Documents/ApiReference/` (generated post-build, kept current by Baraka) 
  before reading source files directly. The public surface is usually what you need; read code only for implementation details.
  **Scope of what it proves:** API Reference confirms *shape* — a method exists, its signature, its doc comment.
  It does not confirm *behavior* — whether the method actually does what the shape or a design doc claims (e.g.
  "the validator runs a cycle check"). Behavioral/acceptance-criteria claims are QA's (Lothar's) job, verified
  against source or a passing test — not read off the API Reference and not inferred from a design doc's
  deliverable list. See [0029](ClaudeWiki/Processes/Handoffs/0029-phase-2-6-5-status-and-qa-gap.md) for the
  case that made this distinction necessary to write down.
- **Upstream source before inference.** When a third-party dependency's behaviour is load-bearing for
  a design or implementation decision, read that dependency's **source at the version pinned in the
  `.csproj`** — before inferring from its prose docs, and before commissioning a spike to find out
  experimentally. Most of what we depend on is MIT and public; NRules' API docs link every member
  straight to GitHub. Two corollaries, both learned the hard way in
  [0033](Documents/ClaudeWiki/Processes/Handoffs/0033-spike-does-nrules-match-facts-through-interfaces.md):
  docs describe *intent* while source is *what shipped*, and a spike answers "did my test pass,"
  which is a different question from "how does this work" — a **failing** test proves only that the
  test failed, so a negative result needs a positive control before it's treated as a finding.
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
  model, and `GeneratorServices` (the author-declaration → NRules pipeline). (`IMechanic` retired
  2026-08-09 — see `design/glossary.md`.)
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
- **`Documents/ApiReference/`** — **read this first for code understanding.** Generated post-build, kept current, covers the public surface you usually need. Open after `dotnet build`. Only read `/src` if you need implementation details.
- **`Documents/ClaudeWiki/`** — how the team of sessions works: roles (`Roles/`, each with a
  paste-in bootstrap prompt), processes (`Processes/` — handoff tickets, testing), and the ticket
  ledger (`Processes/Handoffs/`). Start at `Documents/ClaudeWiki/index.md`.
- **`Documents/Roadmap/`** — the phase sequence and status; the authoritative "what's real / what's
  next."
- **`Documents/design/`** — technical design docs (intended shape).
- **`Documents/Specs/`** — buildable specs the devs implement against.
- **`planning/`** — raw, aspirational design transcripts. Useful for *why* something is shaped as it
  is; not a spec for what exists, and not current. Don't build against it.
