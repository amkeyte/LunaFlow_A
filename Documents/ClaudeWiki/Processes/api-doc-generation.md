# API Doc Generation — `DefaultDocumentation` wiring & the MAX_PATH failure mode

Operational reference for the generated markdown API reference under `Documents/ApiReference/`
(origin: [ticket 0012](Handoffs/0012-api-doc-generation.md)). Read this before touching any
authoring-surface project's post-build doc target, or if VS2022 throws a "Documents" project error
mentioning "exceeds the OS max path limit."

## What's wired, and where

`LunaFlow.Rules`, `LunaFlow.Engine`, and `LunaFlow.Rom` each carry a `GenerateApiReference` MSBuild
target (`AfterTargets="Build"`) that shells out to `dotnet defaultdocumentation`, writing into
`Documents/ApiReference/<ProjectName>/` (gitignored except a static banner `README.md`). Two of the
three (`LunaFlow.Engine`, `LunaFlow.Rom`) also carry a `DefaultDocumentation.json` next to the
`.csproj`, used to curate the generated surface via `Markdown.Exclude` (a list of regexes matched
against each member's XML-doc-comment ID, e.g. `M:LunaFlow.Rom.D20System.D20SystemContent.Configure`
for a method, `T:...` for a type) — the same lever [ticket 0014](Handoffs/0014-engine-api-surface-scope.md)
used to keep `LunaFlow.Engine`'s doc scoped to its real content-facing surface instead of every
`public` member.

**If a project has a `DefaultDocumentation.json`, its `.csproj` target's `Exec` command MUST pass
`--ConfigurationFilePath &quot;$(MSBuildThisFileDirectory)DefaultDocumentation.json&quot;` explicitly.**
`dotnet defaultdocumentation` does **not** reliably auto-discover the config file from its working
directory — confirmed empirically 2026-07-20: a project with no config of its own printed a
*different* project's `Markdown.Exclude` list when the flag was omitted, and a project's own config
sitting right next to its `.csproj` was silently ignored (no `Markdown.Exclude` section at all in the
tool's logged settings) until the flag was added. This isn't documented tool behavior to rely on —
always wire the flag explicitly when a config file exists. (Root cause of the 2026-07-20 recurrence
below: `LunaFlow.Rom.csproj`'s target never had this flag — its `DefaultDocumentation.json` had no
effect until it was added.)

## The MAX_PATH failure mode

**Symptom:** Visual Studio throws, on the "Documents" project (which globs everything under
`Documents/` for Solution Explorer): *"Cannot expand metadata in expression... item metadata
'%(FullPath)' cannot be applied to the path '...' — exceeds the OS max path limit. The fully
qualified file name must be less than 260 characters."*

**Root cause:** the `--FileNameFactory DirectoryName` setting (chosen deliberately — see ticket
0012 — over embedding a fully-qualified type name in the filename) still names each *member's* file
after its short (non-namespace-qualified) signature, including every parameter's type. A method with
several parameters — especially generic ones like `IReadOnlyDictionary<TKey, TValue>`, which renders
as `IReadOnlyDictionary_TKey,TValue_` — produces a long filename fast. Combine a long member
signature with the repo's own absolute path prefix, and the total can exceed Windows' legacy 260-char
`MAX_PATH`. VS2022's own project-glob evaluation hits this even though `dotnet build` from the CLI
often won't (different code paths for path validation) — **CLI success does not mean VS is safe**;
if you're diagnosing this, check the file's actual full absolute path length, not just whether
`dotnet build` exits 0.

**History:**
- **2026-07-19** (ticket 0012): first occurrence, `RulePipeline`'s 3-interface constructor produced
  a 242-character filename. Fixed by switching to `--FileNameFactory DirectoryName` (from the
  default, which embeds *fully-qualified* type names — much worse) — brought the longest path to
  171 characters, ~89 characters of margin under 260. Flagged then as "worth watching, not acting
  on, if the API surface grows enough to push that margin — no threshold set, just noted."
- **2026-07-20** (ticket 0022, Phase 2.6 implementation): that margin got pushed. The new
  `D20SystemContent.Configure(...)` registration method (7 parameters, several nested generic
  dictionaries) produced a filename alone over 180 characters, well past the limit combined with the
  repo path. Root two causes, both fixed:
  1. `LunaFlow.Rom.csproj`'s target was missing `--ConfigurationFilePath` (see above) — its
     `DefaultDocumentation.json` (added same session) was silently inert.
  2. Once wired, `Configure(...)` was excluded via `Markdown.Exclude` rather than redesigned — it's
     composition-root wiring (called once by `Program.cs`/test setup), not something a Rulebook
     author calls, so it was never real "authoring surface" content in the first place. Excluding it
     lost no genuine documentation value, same reasoning ticket 0014 applied to `ConsoleEngine` et
     al.
  - **Current measured margin: 214 characters at the longest (`FightCommandHandler`'s 5-parameter
    constructor and `CheckCommandHandler`'s 4-parameter constructor, both ~186-char filenames), 46
    characters of headroom under 260.** Tighter than 2026-07-19's 89-char margin — flagging this
    explicitly rather than repeating the "no threshold set" non-committal note that didn't stop the
    problem recurring. Treat 46 characters as the number to watch.

## Why not just hash the filenames?

Tempting, and worth writing down why it's rejected — this will get proposed again. `dotnet
defaultdocumentation` offers two hash-based `--FileNameFactory` options; both were tested directly
against `LunaFlow.Rom`'s real assembly (2026-07-20), not just reasoned about:

- **`NameAndMd5Mix`** (short name + hash suffix, e.g. `Configure.TTUL0VB9QNG0GIPGRSPWONI8E.md`) —
  bounds the length problem (longest filename dropped from 186 to 63 characters in the test), but it
  is **flat, no per-namespace folders**. `LunaFlow.Rom` already has two unrelated classes both named
  `MoveCommandHandler` (`NineRooms` and `D20System`) and two both named `LookCommandHandler`, same
  split. Under a flat scheme both collide onto the identical filename and **silently overwrite each
  other** — confirmed: only one `MoveCommandHandler.md` survived generation, with the total file
  count coming up short by exactly the collided pairs. No build error, no warning — the losing
  class's documentation just silently disappears. **Worse than the problem being solved**: a loud VS
  error is at least visible and forces a fix; this fails invisibly.
- **`Md5`** (pure hash, no readable component at all) — no collisions (confirmed, correct file
  count), fully bounded length, but filenames are completely opaque — nothing greppable, nothing
  scannable in a directory listing. Defeats the actual reason `DefaultDocumentation` was chosen over
  DocFX in ticket 0012 in the first place ("browsable... not DocFX's HTML-site-first, wrong shape for
  an LLM consumer" — an opaque hash dump is arguably worse for that audience than DocFX would have
  been).

**Conclusion: don't switch to a hash-based `FileNameFactory`.** The namespace-per-folder structure
`DirectoryName` already provides is load-bearing — it's what prevents same-named types in different
ROM modules from colliding — not just a readability nicety. There is no built-in factory that gives
bounded length *and* collision-safety *and* readability simultaneously; the reactive
exclude-when-it-breaks approach below is the correct tradeoff, not a stopgap waiting to be replaced.

**One real cost of the exclude approach, found while testing this:** excluding a member via
`Markdown.Exclude` removes its own page, but doesn't fix any `<see cref="TheExcludedMember"/>`
elsewhere in the doc comments — those silently resolve to a broken external `learn.microsoft.com`
link instead of a local one (DefaultDocumentation's cross-reference fallback assumes an unresolvable
local `cref` must be a BCL member). If you exclude a member, grep the project for `<see
cref="TheMemberName"` and change any hits to `<c>TheMemberName</c>` (plain code formatting, no
cross-reference) — this bit `D20SystemContent`'s own class summary in the 2026-07-20 recurrence.

## What to do if this happens again

1. **Confirm it's this failure, not something else** — read the actual VS error text for "max path
   limit" / "260 characters." If so, find the named file under `Documents/ApiReference/` and measure
   its full absolute path length.
2. **Decide: is the offending member real authoring-surface content, or composition-root/plumbing?**
   - If it's plumbing (a wiring/registration method a content author never calls directly — the
     `Configure()` pattern) → **exclude it** via that project's `DefaultDocumentation.json`
     (`Markdown.Exclude`, an XML-doc-ID regex prefix — see the two existing files for the pattern).
     Create the json file if the project doesn't have one yet, and **make sure the `.csproj`
     target's `Exec` command passes `--ConfigurationFilePath` explicitly** — don't assume
     auto-discovery works. After excluding, grep for `<see cref="TheExcludedMember"` and fix any
     dangling cross-references (see "Why not just hash the filenames?" above).
   - **Do not** switch `--FileNameFactory` to a hash-based option to sidestep this — see "Why not
     just hash the filenames?" above; it trades a loud, visible failure for a silent one.
   - If it's genuine authoring-surface content that belongs in the doc → reduce its own parameter
     count/complexity (e.g. an options/record parameter instead of many). Note this does **not**
     automatically help on its own: a record's own constructor gets documented too, with the same
     parameter list, so wrapping params in a record only helps if the record type itself is either
     simple enough to stay short or is also excluded as plumbing.
3. **Re-measure after the fix** — don't just confirm the build succeeds; check the actual longest
   path in `Documents/ApiReference/` the way this page's History section does, and update the
   measured-margin line above so the next recurrence has a real number to compare against instead of
   re-deriving it from scratch.
4. **Verify in VS2022 specifically**, not just `dotnet build` from the CLI (see Root cause above —
   they hit this differently). If you can't drive VS2022 directly, ask the user to confirm a
   Rebuild All is clean.

## Related pages

- [ClaudeWiki index](../index.md)
- [Handoff ticket 0012 — API doc generation](Handoffs/0012-api-doc-generation.md) — origin of the
  tool choice, the `DirectoryName` `FileNameFactory` decision, and the first MAX_PATH incident
- [Handoff ticket 0014 — Engine API surface scope](Handoffs/0014-engine-api-surface-scope.md) —
  origin of the `Markdown.Exclude` curation pattern this page reuses for the plumbing-exclusion case
- [Handoff ticket 0022 — Implement Phase 2.6](Handoffs/0022-implement-phase-2-6.md) — the
  2026-07-20 recurrence this page was written from
