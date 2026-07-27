# 0023 — Cross-ROM fact mediation: what actually lets one ROM's rule react to another ROM's fact?

**Status:** rescinded — superseded by [Phase 2.6.5](../../../Roadmap/phase-2-6-5.md); administrative
close by Kofi (PM), 2026-07-22.
**Opened by:** Game Designer (Juno)
**Opened:** 2026-07-21
**For:** Architect (Amara)

## Context

Working the actual content split for [Phase 2.7](../../../Roadmap/phase-2-7.md): splitting
`LunaFlow.Rom.D20System` into a new Grid ROM (adjacency, `PlayerPosition`/`RoomSolved`/`WinMarker`,
the win-line rule family) and a trimmed D20System ROM (`Room`/`CombatantStats`/etc., `check`/
`fight`/`flee`/`look`) — the "one Rulebook, two ROMs" shape you and Arryn confirmed replaces the
stale Nine-Rooms-retrofit framing still sitting in `phase-2-7.md`'s Deliverables/Done-When (that
correction is worth a pass separately; not this ticket's concern).

Redesigning `MoveCommandHandler`'s destination-description announcement surfaced the real question.
Original plan: keep `Move` generic in Grid ROM, constructor-inject a description-lookup delegate
wired by the composition root. Arryn correctly called that a smell — it's exactly the single-
condition/single-action shape `ChallengeRule`/`WinLineRule` already handle declaratively
(`D20SystemRuleGroup.cs`), so it should be a rule instead: D20System gets its own unconditional rule
that matches `PlayerPosition`, joins it against its own `Room` on `RoomId`, and announces
`DescriptionKey` — `Match<PlayerPosition>(() => pos)` / `Match<Room>(() => room, r => r.Id ==
pos.RoomId)`, a real bound-variable join, cleaner even than the existing per-room-enumerated
pattern.

That only works if D20System's rule code can reference the `PlayerPosition` type at all. I first
proposed relocating `PlayerPosition`/`RoomSolved`/`WinMarker` into `LunaFlow.Rules` to sidestep a
direct ROM-to-ROM reference — Arryn corrected that too: `LunaFlow.Rules` is the framework/authoring
namespace (`ARule`, `IHasLunaFlowReference`, `IMechanic`, `GeneratorServices`), not a place for
domain content, and nothing in this codebase currently puts concrete fact records there.

So: the facts stay in whichever ROM produces them (Grid ROM, in this case). `phase-2-7.md`'s own
Done-When bar says the two ROMs' "only interaction being fact/marker-mediated (no project reference
between them)." But a rule can't `Match<T>` a type it can't see — D20System needs `PlayerPosition`
resolvable at compile time to write the join above, which means either a project reference to Grid
ROM (seemingly what "no project reference between them" rules out) or some mechanism I'm not seeing.

## Ask

What's the actual mechanism that lets a ROM's declarative rule (or handler) react to a fact type
owned by a different ROM, given both constraints hold: content facts live in ROM/Rulebook only
(not `LunaFlow.Rules`), and the two ROMs have zero project reference between them? Concretely, for
this case: how does D20System's rule join against Grid-ROM-owned `PlayerPosition` without
referencing Grid ROM's assembly?

If "no project reference between them" is meant more narrowly than I'm reading it — e.g. it bars
mutual/circular reference or one ROM calling into another's handlers/logic, but tolerates a single
one-way reference where a real conceptual dependency exists (D20System's content only makes sense
inside a room Grid ROM establishes) — that's a different, much simpler answer, and worth just
saying so. If it's meant literally, that's presumably what 2.7's "shared, ROM-agnostic marker
vocabulary" deliverable is for, in which case: what does that actually look like, concretely, for
`PlayerPosition` specifically (a marker/interface contract? something else?) — not a hypothetical
future system, since this case needs it now to finish the Move redesign.

Blocks: finishing the Grid/D20System split design and the `1-d20-spec.md` update that goes with it
(mine to write per Arryn — noted here so the dependency's visible, not asking you to touch the spec).

## Response

_Never filled through the normal flow._ The ask was answered by design work, not in this ticket:
Amara and Arryn took it straight into [Phase 2.6.5](../../../Roadmap/phase-2-6-5.md), which builds the
actual mechanism (`IRom`/`IInteropRom`/`Capability`/`IHasDependencies`) that lets one ROM's rule react
to another ROM's fact with no assembly reference between them.

## Resolution

Administratively rescinded by Kofi, 2026-07-22 — not resolved through Juno's normal close, because
the question was absorbed into a phase rather than answered in place.

The reasoning captured above (the `IRoomLocated`/`ISpatialPosition` interface proposals, the
relocate-facts-into-`LunaFlow.Rules` idea, the narrow-vs-literal reading of "no project reference,"
the one-way-reference fork) is **exploration that got us to 2.6.5, not live guidance**. Once 2.6.5
lands, most of it is historical — the capability-mediation answer replaces the fork this ticket was
weighing, and the "how does D20System see `PlayerPosition`" question is answered by `IInteropRom`, not
by any option enumerated here. Kept intact for lineage (supersede-don't-delete), not to act on.

**This ticket is closed and should not be referenced going forward.** When Phase 2.7 resumes — the
real Grid/D20System content split — fresh tickets get issued against 2.6.5's shipped mechanism as
needed. Don't reopen or extend this one.
