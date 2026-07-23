# Nine Rooms — Historical Content

> **DEPRECATED** (2026-07-20) — Nine Rooms is a disposable Phase 1.5 test content, kept only for regression coverage. **Do not use as a pattern for new ROMs or Ruleooks.**

## Current pattern

For the current ROM/Rulebook design and split, see:

- **Implementation:** [`../Specs/1-d20/1-d20-spec.md`](../Specs/1-d20/1-d20-spec.md) (D20 System + OneD20 Rulebook)
- **Design rationale:** [`../Roadmap/phase-2-6.md`](../Roadmap/phase-2-6.md) (ROM/Rulebook boundary fortification)
- **Architecture:** [`../design/architecture.md`](../design/architecture.md) (current layers)

## Why Nine Rooms exists

It proved the Phase 1 plumbing (command dispatch → rule firing → turn settling) against something playable and checkable, before the ROM/Rulebook split was designed. It served that purpose; the split now has a real, proven implementation in D20 System / OneD20.

## Still in use

- **Regression coverage:** `tests/LunaFlow.EngineTests/Core/NineRoomsEngineFixtureTests.cs` — engine-fixture tests that exercise the full turn loop end-to-end. This stays in the suite; removing it would lose that coverage.
- **Reference:** Read [`nine-rooms.md`](nine-rooms.md) if you need to understand *why* Phase 1.5 was sequenced where it was, or if you're triaging engine failures in the fixture tests.

**Do not treat Nine Rooms as a current design pattern.** It predates the ROM/Rulebook split and its own layering reflects those constraints, not the split's design. Start with D20 System instead.

## Related

- [Nine Rooms concept doc](nine-rooms.md) — historical context
- [Nine Rooms spec](nine-rooms-spec.md) — the buildable content
- [Nine Rooms facts & rules](nine-rooms-facts.md) — implementation details
- [Nine Rooms gameplay](nine-rooms-gameplay.md) — how to play (for manual testing)
