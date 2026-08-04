# 0068 — Phase 2.9 QA gate

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-03
**For:** Lothar (QA)

## Decision

**Verdict: PASS**, all ten ask items and the spec's full acceptance list confirmed against
evidence — see [`phase-2-9-verdict.md`](../../../QA/phase-2-9-verdict.md). Arryn-run 170/170
`latest.trx` (which also settled agent-test-workflow.md's open question — a single Test Explorer
"Run All" does merge both test projects into one trx), independent content-fidelity re-diff (0
diffs), and live reproduction of both fail-fast layers and the sandbox-escape rejection against real
code, not the original throwaway probes. No deviations, no tickets filed. **Phase 2.9 marked done on
the roadmap.**

## Details

### Context

Phase 2.9 ([`phase-2-9.md`](../../../Roadmap/phase-2-9.md)) is implementation-complete:
[0065](0065-implement-lua-world-module-spec-md.md) built `lua-world-module-spec.md` in full, and
[0066](0066-lua-world-module-maintained-test-coverage.md) landed maintained regression coverage
(15 new tests, 77/77 + 93/93). I verified 0065 against the spec's acceptance list and shape-checked
`LunaFlow.Scripting`/`LuaOneD20WorldModule` against the generated API reference before closing it —
every item held except the spec's own last line ("full suite green, read from an Arryn-run
`latest.trx`"), which 0065 flags as self-run only. Same shape as [0056](0056-phase-2-7-1-qa-gate.md)
and [0063](0063-phase-2-8-qa-gate.md): filed now that there's something real to check.

### Ask

Run a full `qa-verify` pass against `lua-world-module-spec.md`'s acceptance criteria and
`phase-2-9.md`'s Done-When bar. Verify against evidence — source, passing test, reproducible
session — not ticket prose:

1. **`LunaFlow.Scripting` project + sandboxed host** — confirm `ISandboxedScriptHost`/
   `SandboxedScriptHost` exist and that the `CoreModules` restriction was verified against the
   MoonSharp version actually pinned in `LunaFlow.Scripting.csproj` (0065 claims v2.0.0.0's
   `Preset_HardSandbox`, checked against source at that tag, not `master`) — confirm the version
   pin and the exclusion list yourself, per `CLAUDE.md`'s "upstream source before inference."
2. **`LuaOneD20WorldModule` implements `IOneD20WorldModule` completely** — confirm against source,
   not the API reference's shape alone.
3. **Content-fidelity diff** — the demo `.lua` script's 9 rooms / 9 challenges / 5 monsters / loot
   table / PC stats / 62 flavor-text entries against `DefaultOneD20WorldModule`'s values. 0065
   claims this was diffed twice (once by the building agent, once independently) — re-run the
   comparison yourself rather than trusting either pass.
4. **Two-layer fail-fast, demonstrated not asserted** — reproduce at least the syntax-error and
   missing-`rooms`-key cases from 0065's Response, confirm the error names the source and field,
   and confirm `OneD20WorldModuleValidator` itself required zero changes.
5. **Sandbox-escape rejection, reproduced live** — 0065 ran this via a throwaway probe project, not
   checked into `src/`/`tests/`. Reproduce `io.open`/`os.execute` (or equivalent) failing yourself;
   don't take the transcript as sufficient on its own.
6. **Zero changes to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`,
   or any ROM** — confirm via diff/git, not narrative.
7. **No new Cartridge/`ComponentRegistry` capability** — confirm nothing quietly introduced one.
8. **Full session, live** — move/look/check/fight/flee/inventory against a
   `LuaOneD20WorldModule`-backed session, win-line topology unaffected, loot correct. Swap
   `Program.cs`'s commented line yourself, play it, revert.
9. **Regression green from an Arryn-run `latest.trx`.** All numbers on record (155/155, then
   77/77 + 93/93 post-0066) are agent-run. Same hard rule 0063 held — have Arryn run the full
   solution suite before writing the verdict; flag back if what you receive isn't disclosed as such.
10. **Verdict + evidence to the PM**, written to `Documents/QA/phase-2-9-verdict.md`. A fail routes
    to Baraka (defect) or Amara (spec-vs-reality) — not a fix by you.

### Response

**PASS.** All ten ask items verified against evidence, not ticket prose — full detail in
[phase-2-9-verdict.md](../../../QA/phase-2-9-verdict.md).

1. MoonSharp `CoreModules`/`Preset_HardSandbox` verified against the actual bound v2.0.0.0 assembly
   (reflection over the exact DLL resolved from the `.csproj` pin), not source prose or memory —
   1387 = `GlobalConsts|TableIterators|String|Table|Basic|Math|Bit32`, excludes
   `IO`/`OS_System`/`OS_Time`/`LoadMethods`/`Debug` exactly as claimed.
2. `LuaOneD20WorldModule` confirmed against `IOneD20WorldModule` source, member by member — complete.
3. Content-fidelity diff re-run independently (own throwaway comparison program, not 0065's): 0 diffs
   across rooms/challenges/monsters/items/PC/abilities/62 flavor-text entries.
4. Two-layer fail-fast reproduced live: syntax error, missing `rooms`, `dc`-as-string, bad
   `challengeType` all throw naming the specific problem. `OneD20WorldModuleValidator.cs` confirmed
   zero-changed via `git show` on the build commit.
5. Sandbox-escape reproduced live against a freshly built probe (not 0065's original, per your
   go-ahead) — `io.open`/`os.execute` both throw, and `io`/`os`/`dofile` confirmed literally absent
   from the sandbox, not just erroring.
6. Zero changes confirmed via `git show 938cafa --stat` against `IOneD20WorldModule.cs`,
   `OneD20WorldModuleValidator.cs`, `OneD20SessionAssembler.cs`, and all of `LunaFlow.Rom` — empty
   diff on every path.
7. No new Cartridge/`ComponentRegistry` capability — grepped new Scripting/Lua source, zero matches;
   `Program.cs` diff is one inactive commented line, `.sln` diff is the new project plus VS platform
   boilerplate.
8. Full live session run against `LuaOneD20WorldModule` (swap/build/play/revert): all 9 rooms, all 6
   skill checks, all 3 fights, correct loot per monster, `flee` and `inventory` both correct,
   `win-announce` fired verbatim. Swap reverted, confirmed clean via `git diff`.
9. 170/170 from the Arryn-run `latest.trx` you provided — confirmed by parsing the trx directly: it's
   a single file that merges both `EngineTests`/`ContentTests` projects (16 classes), which resolves
   agent-test-workflow.md's previously-unconfirmed question about whether a Test Explorer "Run All"
   merges them. All 6 Lua-specific tests and the fail-fast/sandbox tests present and green.
10. Verdict written to `Documents/QA/phase-2-9-verdict.md`, index updated.

### Resolution

**Resolved 2026-08-03 (Kofi, opener).** Read `phase-2-9-verdict.md` directly: all ten criteria
PASS, evidence attached to every row (assembly-level reflection for the `CoreModules` check rather
than source prose, `git show`/`git diff` for every zero-change claim, two independent live sessions
for the fail-fast/sandbox/full-session bars, an independent re-diff of all content). Nothing
deferred, nothing non-blocking left open. Phase 2.9 marked done in `roadmap.md`/`phase-2-9.md`,
citing this verdict.
