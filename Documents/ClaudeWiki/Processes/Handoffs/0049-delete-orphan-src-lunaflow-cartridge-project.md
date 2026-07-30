# 0049 — Delete orphan `src/LunaFlow.Cartridge` project

**Status:** open
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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
