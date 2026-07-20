# 0012 — Automated markdown API reference for Juno

**Status:** open
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-19
**For:** Dev — Implementation (Baraka)
**Coordinate with:** [Architect (Amara)](../../Roles/architect.md) on the API-surface scope (see below).

## Context

The new [Game Designer (Juno)](../../Roles/game-designer.md) needs an API reference to design ROM/
Rulebook/scripting content against — the public *authoring surface* — without digging through
`src/`. The codebase is already dense with `/// <summary>` XML doc comments, so the source material
exists; generating from it beats a hand-maintained markdown doc that drifts the moment a signature
changes.

Decisions already made (don't re-open):
- **Tool:** a **markdown-native generator** (e.g. `XmlDocMarkdown`/`xmldocmd` or
  `DefaultDocumentation`) — one clean `.md` per type from the built assembly + its XML doc file. Not
  DocFX (HTML-site-first; wrong shape for an LLM consumer). *Verify the current recommended tool +
  version at implementation — tooling moves.*
- **Trigger:** a **post-build target** — regenerates on every build so it can't go stale.

## Ask

1. **Enable XML doc output** (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`) on the
   authoring-surface projects: `LunaFlow.Rules`, `LunaFlow.Rom`, and the public host surface of
   `LunaFlow.Engine`. (The `internal` engine plumbing — `GameEngineCore` etc. — is auto-excluded,
   which is the point: the doc scopes itself to the public authoring API for free.)
2. **Add the doc generator to the tool manifest** (`.config/dotnet-tools.json`, alongside `nbgv`),
   pinned to a specific version.
3. **Wire a post-build MSBuild target** that regenerates the markdown into a dedicated generated
   folder (suggest `Documents/ApiReference/`, your call) with a header banner on the index page:
   **"GENERATED — do not hand-edit; regenerated on every build from XML doc comments."**
4. **Confirm the API-surface scope with Amara** if `public`/`internal` alone doesn't curate it
   cleanly — e.g. a public type that's really plumbing Juno shouldn't see. "What is the authoring
   API" is a framework-boundary question (hers); the wiring is yours. Don't over-ticket it — a quick
   seam-check, not a full design round, unless it turns out non-obvious.

## Notes / sub-decisions for you to resolve

- **Git tracking of the generated output.** Post-build regen + committed generated md = constant git
  churn (and Arryn runs git, dislikes churn). Leaning **gitignore the generated folder** and rely on
  local post-build regen (agents read the local mounted folder; the user builds in VS2022 locally so
  it's always present). If you'd rather commit it, that's fine — just flag the churn. Your call.
- **Single source of truth:** once this exists, it's *the* API reference. Design/spec docs should
  **link to it, not restate signatures** (the anti-drift rule). Juno's role page should point at it —
  I'll wire that link once the folder path is fixed.

## Done when

A `dotnet build` regenerates a browsable markdown API reference of the public authoring surface
(`ARule`, `ILunaFlowReference`, `GeneratorServices` attributes / `RulePrototype` / `RuleExport`, ROM
fact patterns, `IEngineHost`) into the generated folder, marked GENERATED, that Juno can read
page-by-page. No hand-maintained API markdown.

Repo convention (`CLAUDE.md`): no state-changing git commands.

## Response

_(Dev to fill in — tool + version chosen, projects touched, output path, git decision.)_

## Resolution

_(Opener to fill in on close.)_
