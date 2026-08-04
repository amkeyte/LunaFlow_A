# 0068 — Phase 2.9 QA gate

**Status:** open
**Opened by:** Kofi (PM)
**Opened:** 2026-08-03
**For:** Lothar (QA)

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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
