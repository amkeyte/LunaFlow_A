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

## Project scaffolding — DONE

All three `ProjectReference`s (`LunaFlow.Rom → LunaFlow.Rules`, `LunaFlow.Rom → LunaFlow.Engine`,
`MyLunaFlowServer → LunaFlow.Rom`) landed as part of Phase 1.5's first commit, as planned.

## GameEngineCore unconditionally calls Fire() without validating rule initialization — FIXED

Added `RuleEngine.IsInitialized` (`src/LunaFlow.Engine/Game/RuleEngine.cs`) and guarded the
"initial settle" call in `GameEngineCore.Start()`:

```csharp
Rules = new RuleEngine();
Options.Rules.ApplyTo(Rules);
if (Rules.IsInitialized)
    Rules.Fire();
```

`Fire()` itself still throws if called uninitialized elsewhere (rule `Then()` blocks,
`DispatchGameCommand`) — only the automatic startup call now tolerates "no rules configured,"
which is a legitimate state for a test harness or a bare host. Build is clean.

**Not fully resolved:** fixing this unblocked the `RuleEngine.EnsureSessionInitialized` crash,
but running `tests/LunaFlow.EngineTests/Core/*` afterward surfaced a further batch of failures
unrelated to this specific bug. Test team is looking at strategy for those next — this entry can
be deleted once that pass is done, but leaving it until then in case the remaining failures turn
out to be related after all.

## Turn gate semantics — GRADUATED

Both decisions (no queueing on `FinishTurn()`; synchronous resubmit from inside a `TurnTick`
handler is ignored via reset-after-invoke) are now written up as settled, tested semantics in
[`design/engine-core.md`](design/engine-core.md#turn-ending-ordering-not-arbitration), including
the `git log -p` provenance finding on the original comment/behavior mismatch. Nothing further
pending here.

