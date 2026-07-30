# 0049 — Delete orphan `src/LunaFlow.Cartridge` project

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-29
**For:** Baraka (Impl Dev)

## Decision

Delete it. Not a design question — ticket [0042](0042-correct-cartridge-project-location-in-spec.md)
already ruled "no `src/LunaFlow.Cartridge` project exists or is planned." This is leftover from the
initial (wrong) location call, never cleaned up after the correction to `example/D20SystemExampleC`.

## Details

### Context

Flagged in `phase-2-7-verdict.md`'s "Vestigial concepts" section: `src/LunaFlow.Cartridge` is
registered in `LunaFlow_A.sln`, has no `.cs` files, and nothing anywhere references it as a project
— an empty DLL nobody consumes. The real, working contract surface is
`example/D20SystemExampleC/LunaFlow.Cartridge/`, correctly placed per 0037/0042.

### Ask

Remove `src/LunaFlow.Cartridge` — the project and its entry in `LunaFlow_A.sln`. Confirm
`dotnet build LunaFlow_A.sln` still clean afterward and that nothing referenced it (expected — the
verdict already confirmed zero project references).

### Response

Removed, per Arryn directly.

### Resolution

Closed 2026-07-29. `src/LunaFlow.Cartridge` and its `.sln` entry are gone — confirmed by Arryn, not
independently re-checked against a build on my end (no toolchain in this session). Worth a quick
`dotnet build LunaFlow_A.sln` before or during Lothar's next pass just to have it in the evidence
trail, but not holding this ticket open for that. Vestigial-concept flag from `phase-2-7-verdict.md`
is now moot.

**Correction, 2026-07-30 (Baraka):** the promised `dotnet build` check surfaced a gap in the above —
the project folder/`.csproj` were indeed gone, but the `.sln`'s `Project(...)`/`EndProject` block and
its four `GlobalSection(ProjectConfigurationPlatforms)` lines (GUID
`6E6EEC6A-42F2-493D-964A-EB288B0256C4`) were still there, and `dotnet build` failed outright
(`MSB3202: The project file "...LunaFlow.Cartridge.csproj" was not found`) while working ticket 0043.
Removed both from `LunaFlow_A.sln`; confirmed `dotnet build LunaFlow_A.sln` clean afterward. Not
reopening this ticket — the underlying ask (remove the orphan) is done and was always the point;
recording here since this ticket's own Resolution is what claimed the `.sln` was already clean.
