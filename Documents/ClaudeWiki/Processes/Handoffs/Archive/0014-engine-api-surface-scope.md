# 0014 — LunaFlow.Engine API-surface curation for the generated reference

**Status:** resolved
**Opened by:** Dev — Implementation (Baraka)
**Opened:** 2026-07-19
**For:** Architect (Amara)

## Context

Working [ticket 0012](0012-api-doc-generation.md) (automated markdown API reference for Juno).
The generator (`DefaultDocumentation`) can only curate by visibility level (`public`/`internal`/
etc.), not by arbitrary type — so "generate the public surface" pulls in everything `public` in
`LunaFlow.Engine`, and that assembly's public types split into two groups:

- Looks like real host surface: `IEngineHost`, `IGameEngine`, `IGameEngineTranslator`, `RuleEngine`
- Looks like host-wiring plumbing that happens to be `public`: `ConsoleEngine`, `DefaultCommand`,
  `DefaultCommandDispatcher`, `DefaultCommandParser`, `ICommand`, `ICommandHandler`,
  `ICommandDispatcher`, `ICommandParser`, `CommandOptions`

Ticket 0012 itself anticipated this ("confirm the API-surface scope with Amara if public/internal
alone doesn't curate it cleanly") and left it to me to raise if it turned out non-obvious. It did.

Constraint worth flagging: `ConsoleEngine` isn't just publicly visible by oversight — it's actually
consumed cross-assembly, `MyLunaFlowServer/Program.cs:11` does `new ConsoleEngine()`. So "just mark
the plumbing internal" isn't a free mechanical fix; it's a real design call (would need e.g.
`InternalsVisibleTo` toward `MyLunaFlowServer`, or restructuring how the console host is composed).

## Ask

Rule on how the generated authoring-surface reference should treat the plumbing group. Options as
I see them, not exhaustive:

1. **Accept as-is** — ship the full public surface; a bit of host-wiring visible in Juno's reference
   isn't actively harmful, just noise.
2. **Reclassify the plumbing types `internal`** — requires resolving the `MyLunaFlowServer`
   cross-assembly access question above; a real API-shape change, not a doc-tooling tweak.
3. **Something else** — e.g., a second scoped generation pass that only targets the four host-surface
   types, if that's worth the extra wiring.

I implemented ticket 0012 shipping option 1 for now (per Kofi's call, so the folder isn't half-built
while this is open) — `Documents/ApiReference/LunaFlow.Engine/` currently includes both groups, with
a note in `Documents/ApiReference/README.md` pointing at this ticket. No urgency to block on this;
route back to me when there's a ruling and I'll adjust the generator's scope accordingly if needed.

## Response

Ruled against both options as posed. Neither 1 nor 2 survives contact with what `IEngineHost`,
`IGameEngine`, and `IGameEngineTranslator` actually are — checked source directly rather than
ruling off the ticket's framing alone.

**The four-type "real host surface" bucket is wrong; there are three audiences, not two.**

- `IEngineHost` — its own doc comment says it: "the engine surface exposed to content
  assemblies (ROM today, Rulebook once that tier exists)." This is Juno's. Genuinely
  authoring-surface.
- `RuleEngine` — `IEngineHost.Rules` returns it, and its public members (`Insert`/`Update`/
  `Retract`/`Fire`/`Query<T>`) are exactly what content code calls through that property.
  Transitively authoring-surface — belongs with `IEngineHost`, not filed separately.
- `IGameEngine` / `IGameEngineTranslator` — a different audience entirely: whoever implements a
  *new host* (a WebSocket translator, a test harness translator). `IGameEngineTranslator` is
  currently `// add stuff here`, `IGameEngine` is one method. Neither is content-facing; Juno has
  no reason to read them. Ticket 0012's own "Done when" list only names `IEngineHost` from this
  assembly — it already didn't ask for these two, the surface-level grouping just obscured that.
- `ConsoleEngine` / `DefaultCommand*` / `ICommand*` / `CommandOptions` — the console translator's
  own internals. Correctly excluded from Juno's reference; not in dispute.

**Ruling: option 3, scoped correctly — not option 1, not option 2.**

- **Don't touch access modifiers.** `public` is doing real work here independent of the doc
  generator: `ConsoleEngine` is genuinely cross-assembly consumed
  (`MyLunaFlowServer/Program.cs:11`), and `IGameEngine`/`IGameEngineTranslator` being `public`
  isn't premature abstraction to revisit under `non-goals.md` — it's an existing seam
  (`ConsoleEngine : IGameEngineTranslator`) that needs to keep compiling, not a hypothetical
  second implementation being built ahead of need. `InternalsVisibleTo` toward
  `MyLunaFlowServer` to satisfy a *documentation tool's* filtering is solving a docs problem at
  the compiler layer — wrong layer for the job, and not a call to make on the strength of one
  ticket.
- **Also don't accept-as-is indefinitely.** Once ticket 0012 actually lands in Juno's hands,
  `ConsoleEngine`/`DefaultCommandDispatcher` sitting in her authoring reference is real noise
  against a reference whose stated purpose is "design ROM/Rulebook/scripting content against."
  That's not "no urgency," that's the ticket's own done-bar not fully met yet.
- **Concretely:** Baraka, point the `LunaFlow.Engine` generator pass at an explicit type
  allowlist — `IEngineHost` + `RuleEngine` — rather than the whole assembly's public surface.
  `DefaultDocumentation` should support scoping by namespace or explicit type list (verify at
  implementation; if it genuinely can't, a manually-curated single-page stub for these two types
  is an acceptable fallback, but check the allowlist option first — it's the smaller change).
  `IGameEngine`/`IGameEngineTranslator`/`ConsoleEngine`/command-plumbing stay `public` in code
  (nothing to fix there) but drop out of the *generated* folder — no host-implementer audience
  exists yet to read a reference for them (no second translator; `non-goals.md`'s WebSocket-
  interface line is Phase 5), so building that second reference now would be exactly the
  "general thing on one example" call this role exists to say no to. Revisit when a second
  translator is real.

**Follow-up, not blocking this ticket:** the audience split above (content-authoring vs.
host-implementing vs. this-host's-wiring) isn't written down anywhere in `Documents/design/` —
`architecture.md`'s authoring-surface table covers *content* roles (Developer/ROM/Rulebook/
Player), not this engine-internal host/translator layering. Worth a short addition once there's
a second translator to generalize from; not doing it speculatively now, same reasoning as above.

## Resolution

Implemented as ruled, with one addition flagged for visibility rather than silently folded in:
**`LunaFlowEngine` added to the allowlist alongside `IEngineHost`/`RuleEngine`.** It's the static
gateway every ROM handler actually calls through (`LunaFlowEngine.Host.Rules...`,
`LunaFlowEngine.Host.Announce(...)`) — without it the reference would document `IEngineHost` with
no documented way to reach an instance of one. Its own doc comment ("the only way to reach
IEngineHost from outside LunaFlow.Engine") passes the same content-facing-audience test the ruling
applied to `IEngineHost` itself, so treating it as in scope. Flagging here rather than treating it
as obviously covered by the ruling — reopen if this read is wrong.

**Mechanism:** `DefaultDocumentation`'s only filtering lever below "whole assembly" is
`Markdown.Exclude` — a regex list, config-file-only (no CLI/csproj-property equivalent), matched
against each DocItem's XML-doc-id. Added `src/LunaFlow.Engine/DefaultDocumentation.json` with one
`^\w:LunaFlow\.Engine\.<Type>(\.|$)` pattern per excluded type (`IGameEngine`,
`IGameEngineTranslator`, `ConsoleEngine`, `CommandOptions`, `ICommand`/`ICommandHandler`/
`ICommandDispatcher`, `DefaultCommand`/`DefaultCommandDispatcher`/`DefaultCommandParser`,
`ICommandParser`, the three built-in `*CommandHandler`s, `GameEngineMessagePostedEventArgs`,
`GameEngineOptions`, `RuleEngineOptions`) plus two `^N:...$` patterns to drop the now-empty
`LunaFlow.Engine.Commands`/`LunaFlow.Engine.Game` namespace stubs so the assembly index doesn't
link to dead pages. Wired via `--ConfigurationFilePath` on the `LunaFlow.Engine.csproj` generator
target — the documented default auto-discovery (a `DefaultDocumentation.json` beside the project)
did **not** actually trigger in testing (verified: settings dump showed no exclude config loaded
without the explicit flag), and passing a relative `--ConfigurationFilePath` crashes the tool
(`Environment.CurrentDirectory` set to an empty string) — needed the absolute
`$(MSBuildThisFileDirectory)DefaultDocumentation.json` form.

**Verified via full `dotnet build`:** `Documents/ApiReference/LunaFlow.Engine/` now contains exactly
`IEngineHost`, `RuleEngine`, `LunaFlowEngine`, and their members — no plumbing, no empty namespace
dead ends. `LunaFlow.Rules`/`LunaFlow.Rom` untouched. Build green, 0 errors. `README.md` and ticket
0012 updated to match.
