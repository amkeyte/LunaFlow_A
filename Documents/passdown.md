# Passdown Notes

Quick, working handoff notes between role-scoped chat sessions — not a design doc, and not
authoritative once a finding here has been actioned. If something graduates into an actual
decision, move it into the relevant `design/`/`Specs/` doc and delete the entry below.

## `LunaFlow.Rom.[ModuleName]` for all game-domain content, for now

**For:** whoever next touches ROM or Rulebook content.

**Decision, not yet graduated:** Nine Rooms' rules live under `LunaFlow.Rom.NineRooms` even
though some of that content is Rulebook-tier (rules deciding when a challenge fires) rather than
ROM-tier (reusable tools) — see `design/architecture.md`'s System/setting/session split. Staying
that way on purpose: all game-domain content, whichever tier it conceptually belongs to, lives
under `LunaFlow.Rom.[ModuleName]` until Rulebook infrastructure is real. Reasoning: the shared
namespace is a free uniqueness check — two conflicting definitions of the same module (e.g. two
incompatible `MyGame.Room` types) collide at compile time instead of silently coexisting later.
**Revisit** once there's a real Rulebook boundary to split into — not before. If it holds up past
that point, promote it into `design/architecture.md` as a real convention rather than leaving it
here.

## Engine core — `GameEngineCore` accessibility blocks Rom-side wiring — GRADUATED

Resolved as [`Specs/engine-host-spec.md`](Specs/engine-host-spec.md): a minimal public
`IEngineHost` (`Rules` + `Announce` only), reached via a new `Engine.Host` static gateway.
`GameEngineCore` stays `internal sealed` — the earlier proposed fix here (widen the whole class
to `public`) turned out to also expose `FinishTurn`/`Stop`/`RequestStop`, which contradicts
`engine-core.md`'s "turn-ending is Rulebook authority, not ROM's." See the spec for the full
reasoning and the excluded-members list. `nine-rooms-spec.md` items 1–3 are superseded by it;
that spec still needs updating to reference `IEngineHost` instead of re-deriving the surface.

## Project scaffolding — `LunaFlow.Rom` isn't wired into the build yet — PARTIALLY DONE

**For:** whoever starts Nine Rooms implementation.

**Checked against code:** `LunaFlow.Rom` is now correctly at `src/LunaFlow.Rom/` and is in
`LunaFlow_A.sln` — that part's fixed. Still missing, verified against the actual `.csproj` files:

- `LunaFlow.Rom.csproj` has no `ProjectReference` to `LunaFlow.Rules` (needed for `ARule`).
- `LunaFlow.Rom.csproj` has no `ProjectReference` to `LunaFlow.Engine` either — this one wasn't
  in the original finding, but `Specs/engine-host-spec.md` (new) confirms it's needed too, since
  Nine Rooms content calls `Engine.Host`/`IEngineHost` members directly.
- `MyLunaFlowServer.csproj` still only references `LunaFlow.Engine`, not `LunaFlow.Rom`.

**Remaining fix:** add `ProjectReference`s for `LunaFlow.Rom → LunaFlow.Rules`,
`LunaFlow.Rom → LunaFlow.Engine`, and `MyLunaFlowServer → LunaFlow.Rom` — same first-commit
timing as originally proposed.

