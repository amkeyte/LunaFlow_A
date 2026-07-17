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
