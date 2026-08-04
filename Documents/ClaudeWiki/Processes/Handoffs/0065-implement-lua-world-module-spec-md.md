# 0065 — Implement lua-world-module-spec.md

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-03
**For:** Baraka (Impl Dev)

## Decision

Built in full against `lua-world-module-spec.md`'s acceptance criteria — plan reviewed by Amara first
(one correction: a `sourceName` parameter threaded through the marshaler's error paths, since the spec
requires failures to name which script broke), see
[`CodePlans/0065-lua-world-module-implementation-plan.md`](../../../Roadmap/CodePlans/0065-lua-world-module-implementation-plan.md).
`LunaFlow.Scripting` + `LuaOneD20WorldModule` built, demo script diff-verified against
`DefaultOneD20WorldModule`, both fail-fast layers and the sandbox escape demonstrated, full session
plays correctly, full suite green (155/155 across both test projects, self-run — not yet an
Arryn-run `latest.trx`). Follow-up filed as [0066](0066-lua-world-module-maintained-test-coverage.md) to Nadia — now **resolved**
(15 new tests, 77/77 + 93/93, independently re-verified). Sending to Kofi to verify and close 0065.

## Details

### Context

Phase 2.9 ([`phase-2-9.md`](../../../Roadmap/phase-2-9.md)) is design-complete and ready to build —
Amara's design additions (`design/world-module.md`'s "Two authoring paths into one contract",
`design/scripting-content.md`'s refinement note) and buildable spec
([`lua-world-module-spec.md`](../../../Specs/lua-world-module-spec.md)) are in place, per ticket
[0064](0064-phase-2-9-ready-and-phase-4-scope-correction.md). No content-design role needed — the
proof script is a verbatim transcription of `DefaultOneD20WorldModule`'s already-shipped content,
not new game content.

### Ask

Build [`lua-world-module-spec.md`](../../../Specs/lua-world-module-spec.md) in full:

1. **`LunaFlow.Scripting` project** — a new project hosting a sandboxed MoonSharp execution
   primitive.
2. **`LuaOneD20WorldModule`** — marshals a Lua table into `IOneD20WorldModule`'s shape.
3. **A demonstration script** reproducing `DefaultOneD20WorldModule`'s content verbatim —
   diff-verified against it, same bar 0060 held for `DefaultOneD20WorldModule` itself.
4. **Two-layer fail-fast** — marshal errors surface clearly, then the existing, unchanged
   `OneD20WorldModuleValidator` runs unmodified on top. No changes to the validator, the
   `IOneD20WorldModule` contract, or `OneD20SessionAssembler`.
5. **A demonstrated sandbox-escape rejection** — show the MoonSharp sandbox actually blocks an
   escape attempt (filesystem/process access or similar), not just that well-formed scripts work.

Same acceptance bar Phase 2.8 held: a full session (move/look/check/fight/flee/inventory) runs
correctly against the Lua-authored module, with zero changes to `IOneD20WorldModule`,
`OneD20WorldModuleValidator`, `OneD20SessionAssembler`, or any ROM — proving a third, differently-
authored implementation against the same unmodified contract.

If content/implementation surfaces a real framework gap the spec's toolbox can't express, that's a
ticket to Amara, not a workaround — same discipline the last two content phases held.

Once built: full suite green, read from an Arryn-run `latest.trx`, per the spec's own acceptance
criteria.

### Response

Built in full. Plan reviewed by Amara before any code was written —
[`CodePlans/0065-lua-world-module-implementation-plan.md`](../../../Roadmap/CodePlans/0065-lua-world-module-implementation-plan.md)
has the full design and the one correction she made (a `sourceName` parameter, so a marshal/execution
failure names *which* script broke, not just what — the spec's Mechanics section requires this and the
original draft didn't wire it to anything concrete).

**Built:**
- `src/LunaFlow.Scripting/` (new project) — `ISandboxedScriptHost`/`SandboxedScriptHost`, backed by
  MoonSharp 2.0.0's `CoreModules.Preset_HardSandbox`. Verified against the MoonSharp source at the exact
  `v2.0.0.0` tag (not `master`, which has since moved to an unreleased 3.0 line) that this preset excludes
  `IO`/`OS_System`/`OS_Time`/`LoadMethods`/`Debug` — stricter than the spec's stated minimum.
- `src/LunaFlow.Rulebook/OneD20/LuaOneD20WorldModule.cs` — marshals a Lua table into
  `IOneD20WorldModule`'s exact shape, fail-fast on the first marshal error, every thrown
  `InvalidOperationException` naming the source and the specific field/room/challenge/monster at fault.
  `FromEmbeddedResource()`/`FromFile()` static factories.
- `src/LunaFlow.Rulebook/OneD20/Content/DefaultOneD20WorldModule.lua` — verbatim transcription of
  `DefaultOneD20WorldModule`'s shipped content (9 rooms, 9 challenges, 5 monsters, the loot table, PC
  stats/ability scores, all 62 flavor-text entries), embedded as a resource. Diff-verified field-for-field
  against the C# source twice — once by the agent that wrote it (which caught and fixed its own
  em-dash-vs-double-hyphen slip in ~20 flavor-text values before finishing), once independently by me
  reading both sides side by side. Both passes: exact match, 62/62 flavor-text entries.
- `src/MyLunaFlowServer/Program.cs` — one commented swap-line added (`LuaOneD20WorldModule.FromEmbeddedResource()`),
  matching the existing `HollowKeepWorldModule` swap-line convention. `DefaultOneD20WorldModule` stays
  active by default.
- Zero changes to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, `OneD20SessionAssembler`, or any ROM.

**Proofs (per spec's acceptance criteria):**
- Malformed-script rejection — syntax error, missing `rooms` key, `dc` as a string, unrecognized
  `challengeType` — all fail loudly at construction, naming the source and the exact field:
  ```
  syntax-error: InvalidOperationException: Lua World Module script 'syntax-error' failed to execute: unexpected symbol near 'is'
  missing-rooms-key: InvalidOperationException: ...invalid: top level is missing required table field 'rooms' (got Nil).
  dc-as-string: InvalidOperationException: ...invalid: Challenge 'search' is missing required number field 'dc' (got String).
  unrecognized-challengetype: InvalidOperationException: ...invalid: Room 1's challengeType ('Dance-Off') is not a recognized OneD20ChallengeType.
  ```
- Sandbox-escape rejection — `io.open(...)` and `os.execute(...)` both fail, because `io`/`os` simply
  don't exist as globals under `Preset_HardSandbox`:
  ```
  hostile-io-open: InvalidOperationException: ...failed to execute: attempt to index a nil value
  hostile-os-execute: InvalidOperationException: ...failed to execute: attempt to index a nil value
  ```
  (Both run via a throwaway probe project, not checked into `src/` or `tests/` — see Decision 5 in the plan.)
- Full session against `LuaOneD20WorldModule.FromEmbeddedResource()` (swap-line temporarily activated,
  reverted after): move (incl. boundary rejection), look, check (3 skill checks, correct DC/rolls), fight
  (3 monsters defeated with correct HP/damage/loot — brawler→knuckles, duelist→rapier, enforcer→vest),
  flee (correctly reports "Not in combat." outside combat), inventory (final 3 items correct). Win fired
  via the pre-existing `WinLineTopRowRule` (rooms 1/2/3 solved) — confirmed by reading
  `WinLineRules.cs` that this is unmodified, topology-only NineRooms/GridMap logic, not something this
  spec touches.
- `dotnet build LunaFlow_A.sln`: 0 warnings, 0 errors.
- `dotnet test` both projects (self-run, not yet Arryn's `latest.trx`): `LunaFlow.EngineTests` 93/93,
  `LunaFlow.ContentTests` 62/62 — nothing broke. I didn't add coverage to either suite myself, per the
  `tests/` territory line in `dev.md`; filed [0066](0066-lua-world-module-maintained-test-coverage.md) to
  Nadia for the maintained regression coverage (malformed/hostile-script rejection, full-session
  integration test).

Ready for Kofi to verify and route to Lothar for the official gate once Nadia's follow-up lands.

### Resolution

Verified against `lua-world-module-spec.md`'s acceptance criteria and shape-checked against the
generated API reference: `LunaFlow.Scripting` exists with `ISandboxedScriptHost`/
`SandboxedScriptHost.Execute(string)`; `LuaOneD20WorldModule`'s constructor is
`(string, string, ISandboxedScriptHost)`, confirming the `sourceName` correction from Amara's
reviewed plan actually landed. Every acceptance-criteria line item is addressed by the Response
above — content-diff verification, two-layer fail-fast with named failures, sandbox-escape
rejection, full-session run — except the spec's own last line, "full suite green, read from an
Arryn-run `latest.trx`," which 0065 itself flags as self-run only. Not closing that gap myself —
routing to Lothar as the standing QA gate ([0068](0068-phase-2-9-qa-gate.md)), same pattern as
0056/0063. Ticket closed; phase stays `pending` until Lothar's verdict lands.
