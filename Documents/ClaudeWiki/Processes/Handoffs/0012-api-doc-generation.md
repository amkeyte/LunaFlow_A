# 0012 — Automated markdown API reference for Juno

**Status:** resolved
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

**Tool:** switched mid-implementation. Started with `xmldocmd` (`XmlDocMarkdown`) 2.9.0 per the
ticket's suggestion, wired it into `.config/dotnet-tools.json` + a post-build `Exec` target — but
it failed at first real build with `System.IO.FileNotFoundException` / `ReflectionTypeLoadException`
trying to load `NRules.Fluent` and `System.Runtime, Version=9.0.0.0`. Root cause verified directly:
`xmldocmd` 2.9.0's NuGet package only ships `net6.0`/`net7.0` tool binaries, so `dotnet xmldocmd`
runs hosted on net7.0 and can't reflect over a net9.0 assembly's dependency closure. Not a wiring
bug — a genuine TFM incompatibility with this solution. Swapped to **`DefaultDocumentation.Console`
1.2.3**, whose dotnet-tool flavor explicitly targets net8.0/net9.0/net10.0 and reads assemblies via
`ICSharpCode.Decompiler` rather than `Assembly.LoadFrom`, sidestepping the dependency-load problem
entirely. Verified: full solution rebuild is green (0 errors) and regenerates all three folders.

**Projects touched:** `GenerateDocumentationFile=true` + a `GenerateApiReference` `Target`
(`AfterTargets="Build"`, `Exec` calling `dotnet defaultdocumentation --GeneratedAccessModifiers
Public --IncludeUndocumentedItems`) added to `LunaFlow.Rules.csproj`, `LunaFlow.Rom.csproj`,
`LunaFlow.Engine.csproj`.

**Output path:** `Documents/ApiReference/<ProjectName>/`, one folder per project, plus a static
hand-authored `Documents/ApiReference/README.md` (tracked in git) carrying the required
"GENERATED — do not hand-edit" banner and a structure guide — chose a static top-level banner over
prepending text into the tool-generated `index.md` on every run, since that would mean fragile
text-surgery on a file the tool owns and overwrites each build.

**Git decision:** gitignored (`Documents/ApiReference/*/` added to `.gitignore`) per the ticket's
lean and Kofi's call — only `README.md` is tracked.

**Finding — doc-comment coverage isn't as dense as the ticket assumed.** Build surfaced ~140
`CS1591` warnings (missing XML doc comments on public members) across all three projects, heaviest
in `LunaFlow.Rom` — `NineRoomsContent` (the actual content Juno would want) had *zero* documented
members, so with the generator's default `IncludeUndocumentedItems=false` its whole reference page
generated empty. Fixed mechanically by adding `--IncludeUndocumentedItems` to all three targets, so
every public signature appears (with a blank summary where no comment exists) rather than being
silently dropped — but the underlying comment-writing gap is still open. Didn't backfill comments
myself (content authoring beyond this ticket's wiring scope); flagging for your call on whether
that's a follow-up ticket.

**Scope seam raised separately, now resolved:** filed [ticket 0014](0014-engine-api-surface-scope.md)
to Amara on `LunaFlow.Engine`'s public-surface curation; she ruled a 3-type allowlist
(`IEngineHost`/`RuleEngine`, plus `LunaFlowEngine` which I added and flagged — the actual
content-facing entry point). Implemented via `src/LunaFlow.Engine/DefaultDocumentation.json`
(`Markdown.Exclude` regexes — the tool's only sub-assembly filtering lever, config-file-only).
Verified via full rebuild: `Documents/ApiReference/LunaFlow.Engine/` now contains exactly those
3 types and their members, no plumbing, no empty-namespace dead ends. 0014 closed.

**Done-bar check:** `dotnet build` regenerates all three folders from XML doc comments, no manual
step. `ARule`, `ILunaFlowReference`, `GeneratorServices` attributes/`RulePrototype`/`RuleExport`,
ROM fact patterns (`Room`, `NineRoomsContent`, `WinMarker`, etc.), and `IEngineHost` all present and
browsable. No hand-maintained API markdown was written.

**Update 2026-07-19 — doc-comment sweep, authorized directly by Arryn.** After the coverage gap
above was flagged, Arryn authorized closing it rather than leaving it as a follow-up: swept all 28
files carrying a `CS1591` (28 files, 136 distinct members) across the three projects and added
`/// <summary>` comments. Used `<inheritdoc/>` for mechanical interface-implementing members
(`ARule.Reference` overrides, `GeneratedRuleReference`'s `ILunaFlowReference` members) — verified
these resolve real inherited text in the generated output, not blank stubs, including NRules'
own `Rule.Define()` doc pulling through. Wrote real per-rule summaries for the 9 `Room*ChallengeRule`
and 8 `WinLine*Rule` classes (scripted, since they're structurally identical) rather than leaving
them generic. Also fixed a pre-existing typo in `ILunaFlowReference.ClassUID`'s comment ("complie"
→ "compile") since it was now visibly propagating into the generated reference.

Full clean rebuild: **0 `CS1591`/`CS1573` warnings, 0 errors.** Reverted `--IncludeUndocumentedItems`
consideration is moot now — every public member has a real comment, not a blank-summary fallback,
though the flag stays on as a safety net for anything added later without one. Spot-checked
`NineRoomsContent.md`, `WinLineTopRowRule.md`, and `GeneratedRuleReference.md` in the regenerated
output — all show genuine prose, not placeholders.

**Update 2026-07-19 — VS2022 MAX_PATH break, caught by Arryn.** Default filename scheme
(`FullName`) embedded every parameter's *fully-qualified* type name into member filenames —
`RulePipeline`'s 3-interface constructor produced a 242-character filename, which combined with
the repo path broke Windows' 260-char limit and errored `Documents.csproj` in Visual Studio (that
project globs everything under `Documents/` by default, for Solution Explorer display, and picked
up the new `ApiReference/` folder). Fixed two ways:
- Switched `--FileNameFactory` to `DirectoryName` on all three generator targets — namespace/type
  hierarchy becomes actual folders instead of a dotted filename, and member filenames use short
  (non-fully-qualified) parameter type names, e.g. `RulePipeline/RulePipeline(IRuleParser,
  IRuleIRGenerator,IRuleCompiler).md`. Longest resulting full path is 171 characters, ~90 under the
  limit. Verified this still disambiguates real overloads (short param names are retained, not
  dropped) — a documented alternative (`NameAndMd5Mix`) was in play but rejected since it would've
  turned every parameterized filename into an opaque hash.
- Excluded `ApiReference\**` from `Documents.csproj` via `<None Remove>` regardless — a purely
  generated, gitignored folder has no business being tracked as VS project items at all, independent
  of filename length.

Rebuilt clean after the fix: 0 errors, filenames verified short.

## Resolution

Verified and closed by Kofi, 2026-07-19. Two-part check:

1. **Fresh build evidence.** Arryn ran a full VS2022 Rebuild All (not incremental — first attempt
   was, correctly flagged, and redone). Result: 6/6 projects succeeded, 0 failed. All three
   target projects (`LunaFlow.Rules`, `LunaFlow.Engine`, `LunaFlow.Rom`) recompiled and the
   `DefaultDocumentation` post-build target fired on each, logging its full config (confirms the
   `Markdown.Exclude` allowlist from ticket 0014 is wired on `LunaFlow.Engine`). Zero `CS1591`/
   `CS1573` in the log — only pre-existing, unrelated nullable and unused-variable warnings.
   Matches the response's "0 CS1591/CS1573, 0 errors" claim.
2. **Disk inspection.** Read `Documents/ApiReference/README.md` directly — GENERATED banner
   present, layout matches. Confirmed all three project folders exist
   (`LunaFlow.Rules/`, `LunaFlow.Engine/`, `LunaFlow.Rom/`) with real namespace/folder hierarchy
   (not dotted filenames — the MAX_PATH fix held). Spot-checked
   `LunaFlow.Rom/.../NineRoomsContent/index.md` — real prose summaries on the class and every
   member, not blank placeholders, confirming the doc-comment sweep actually landed. `.gitignore`
   has `Documents/ApiReference/*/`, only `README.md` tracked, per the ticket's lean.

Done-bar met: `dotnet build`/Rebuild regenerates a browsable, real-content markdown API reference
of the scoped public authoring surface with no manual step. No further action.

**Addendum 2026-07-20 (Baraka, direct chat with Arryn) — `<None Remove>` reverted on purpose.**
Arryn reported `ApiReference` had disappeared from Solution Explorer entirely after this ticket's
`<None Remove="ApiReference\**" />` fix. Root cause: that line pulled the folder out of the
`None`-item glob completely, not just out of MAX_PATH danger — it was defense-in-depth, not load-
bearing, since the real fix (`FileNameFactory=DirectoryName`) already keeps every filename short.
Baraka removed the `<None Remove>` line; `Documents.csproj` no longer has it, by design. Rebuilt
clean; longest path unchanged at 171 characters absolute (Kofi independently re-measured
2026-07-20: 122-char relative path + 51-char repo prefix = 171, matches). Flagged as worth
watching, not acting on, if the API surface grows enough to push that margin — no threshold set,
just noted.

**Kofi correction, same day:** misread the missing `<None Remove>` as drift/regression during a
routine catch-up pass and re-added it without checking whether it was intentional — reverted by
Arryn's correction, this addendum written in its place. Lesson for future passes: a shared build
file changing between sessions isn't automatically a regression; check for a same-day direct-chat
resolution (accepted per `handoff-tickets.md`'s "supplements, not replaces" clause) before treating
absence as drift.
