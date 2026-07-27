# 0034: Phase 2.7 Test Refactor

**Owner:** Test Dev (Nadia)  
**Status:** Pending  
**Phase:** 2.7  
**Related:** [0027](0027-implement-phase-2-6-5.md) (Phase 2.6.5), Phase 2.7 implementation complete

---

## Summary

Phase 2.7 split the monolithic D20System ROM into two peer ROMs (GridMap and D20System) coordinating through a broker ROM (D20SystemInterop). This architecture change breaks the existing test suite because:

1. **Handler constructors changed**: All handlers now require ROM instances via constructor injection instead of parameterless constructors relying on static `D20SystemContent`
2. **Facts scattered across ROMs**: PlayerPosition/PreviousRoomId/WinMarker owned by GridMap; RoomChallenge/CombatRound owned by D20System
3. **Cross-ROM capability mediation**: Service-shaped contracts like `IPlayerMovement` are now resolved from the broker, not directly accessed

**Status:** Production code builds and runs successfully (verified with manual server test). Tests are disabled pending this refactor.

---

## Disabled Test Files

The following test classes are marked `[Ignore]` and need refactoring:

- `tests/LunaFlow.EngineTests/Rom/D20System/D20SystemHandlerTests.cs` (handler-level coverage)
- `tests/LunaFlow.EngineTests/Rom/D20System/D20SystemRuleFixtureTests.cs` (rule-level coverage)

Both test classes were written for the Phase 2.6 single-ROM architecture and fail to compile when enabled.

---

## Key Architecture Changes

### Constructor Injection Pattern

**Old (Phase 2.6):**
```csharp
// Static holder configured once
D20SystemContent.Configure(rooms, flavor, pc, monster, scores, startId);

// Handlers instantiated with no args
new MoveCommandHandler()
new CheckCommandHandler()
```

**New (Phase 2.7):**
```csharp
// ROMs instantiated with constructor-injected data
var gridMap = new GridMapRom(gridRoomData, startRoomId);
var d20System = new D20SystemRom(pc, monster, challenges, abilityScores);
var interop = new D20SystemInteropRom();

// Providers registered with broker
interop.RegisterProvider("PlayerLocation", gridMap);
interop.RegisterProvider("PlayerMovement", new GridMovementService());
interop.RegisterProvider("ChallengeSolved", d20System);
interop.RegisterProvider("CombatStatus", d20System);

// Handlers injected with their dependencies
new MoveCommandHandler(gridMap)
new CheckCommandHandler(d20System, mechanic)
new FleeCommandHandler(interop.Resolve<IPlayerMovement>("PlayerMovement"))
```

### Fact Ownership

| Fact | Owner | Used By |
|------|-------|---------|
| `PlayerPosition(int RoomId)` | GridMap | Move handler, Check/Fight handlers (cross-ROM Query) |
| `PreviousRoomId(int RoomId)` | GridMap | Flee handler (cross-ROM service call) |
| `WinMarker(int[] Line)` | GridMap | GridMap rules |
| `RoomChallenge(...)` | D20System | Check/Fight handlers |
| `CombatRound(...)` | D20System | Look handler (cross-ROM Query) |
| `RoomFailed(int RoomId)` | D20System | D20System rules |
| `RoomSolved(int RoomId)` | D20System | GridMap rules (cross-ROM Query via IChallengeSolved) |

### Contract Interfaces

**Fact-shaped** (matched directly in rules via `Query<T>`):
- `IPlayerLocation { int RoomId }` — implemented by GridMap's `PlayerPosition`
- `IChallengeSolved { int RoomId }` — implemented by D20System's `RoomSolved`
- `ICombatStatus { int RoomId, int MonsterHp, int PlayerHp }` — implemented by D20System's `CombatRound`

**Service-shaped** (resolved from broker, constructor-injected):
- `IPlayerMovement { bool ReturnToPreviousRoom() }` — implemented by GridMap's `GridMovementService`

---

## Test Refactoring Approach

### 1. Update Test Fixtures

The `StartSession()` method in `D20SystemHandlerTests.cs` is the core setup. It needs to:

```csharp
private void StartSession(D20ResolutionMechanic? fightMechanic = null, D20ResolutionMechanic? checkMechanic = null)
{
    // Instantiate ROMs with test data
    var gridMapData = OneD20RulebookContent.Rooms.Values
        .Select(r => new GridRoomData(r.Id, r.Row, r.Col, r.DescriptionKey))
        .ToList();
    var gridMap = new GridMapRom(gridMapData, OneD20RulebookContent.StartRoomId);

    var d20Challenges = OneD20RulebookContent.Rooms.Values
        .Select(r => new RoomChallenge(r.Id, r.ChallengeType, r.Skill, r.SkillAbility, r.DC, r.SuccessKey, r.FailureKey, r.PromptKey))
        .ToDictionary(c => c.Id);
    var d20System = new D20SystemRom(
        OneD20RulebookContent.PlayerCharacter,
        OneD20RulebookContent.Monster,
        d20Challenges,
        OneD20RulebookContent.PlayerAbilityScores,
        fightMechanic ?? new D20ResolutionMechanic());

    var interop = new D20SystemInteropRom();
    interop.RegisterProvider("PlayerLocation", gridMap);
    interop.RegisterProvider("PlayerMovement", new GridMovementService());
    interop.RegisterProvider("ChallengeSolved", d20System);
    interop.RegisterProvider("CombatStatus", d20System);

    // Register handlers with injected dependencies
    _dispatcher.RegisterHandler("move", new MoveCommandHandler(gridMap));
    _dispatcher.RegisterHandler("look", new LookCommandHandler());
    _dispatcher.RegisterHandler("check", new CheckCommandHandler(d20System, checkMechanic));
    _dispatcher.RegisterHandler("fight", new FightCommandHandler(d20System, fightMechanic));
    _dispatcher.RegisterHandler("flee", new FleeCommandHandler(interop.Resolve<IPlayerMovement>("PlayerMovement")));

    // ... rest of setup (subscribe to messages, compile rules, start engine)
}
```

### 2. Compile Rules from Both ROMs

```csharp
options.Rules
    .FromAssembly(RuleExport.Compile(typeof(GridMapRuleGroup)))
    .FromAssembly(RuleExport.Compile(typeof(D20SystemRuleGroup)))
    // ... seed with initial facts
```

### 3. Seed Initial Facts

Must seed facts from both ROMs:

```csharp
.Seed(() => new object[]
{
    new PlayerPosition(OneD20RulebookContent.StartRoomId),  // GridMap
    OneD20RulebookContent.PlayerCharacter,                   // D20System
    OneD20RulebookContent.Monster,                           // D20System
})
```

### 4. Query Cross-ROM Facts

Rules and handlers query contract types, not concrete types:

```csharp
// In handlers
var current = rules.Query<IPlayerLocation>().FirstOrDefault();
var combatStatus = rules.Query<ICombatStatus>().FirstOrDefault();

// In rule assertions
Assert.AreEqual(1, _engine.Rules.Query<RoomSolved>().Count());  // Still concrete in assertions
```

---

## Key Testing Patterns

### Handler Tests

Each handler should:
1. Set up the ROMs with test data
2. Instantiate the handler with injected ROM
3. Submit a command through the dispatcher
4. Assert on messages or fact state

**Example: MoveCommandHandler should insert PlayerPosition in new room**

```csharp
[TestMethod]
public void MoveCommand_ValidDirection_UpdatesPlayerPosition()
{
    StartSession();
    var before = _engine.Rules.Query<PlayerPosition>().First();
    Assert.AreEqual(5, before.RoomId);  // Start in room 5

    Submit("move north");  // 5 → 2

    var after = _engine.Rules.Query<PlayerPosition>().First();
    Assert.AreEqual(2, after.RoomId);
}
```

### Rule Tests

Each rule fixture should:
1. Compile both rule groups
2. Seed initial facts
3. Insert/retract facts to trigger rules
4. Assert on rule-inserted facts or announcements

**Example: WinLineRule should fire when three rooms in a line are solved**

```csharp
[TestMethod]
public void WinLineRule_AllRoomsSolvedInLine_InsertsWinMarker()
{
    // Seed initial state
    _engine.Rules.Insert(new RoomSolved(1));
    _engine.Rules.Insert(new RoomSolved(2));
    
    // Trigger rule
    _engine.Rules.Insert(new RoomSolved(3));
    
    var markers = _engine.Rules.Query<WinMarker>().ToList();
    Assert.IsTrue(markers.Any(m => m.Line.SequenceEqual(new[] { 1, 2, 3 })));
}
```

---

## Troubleshooting Checklist

- [ ] All ROMs instantiated with constructor-injected data (not static holders)
- [ ] Broker (`D20SystemInteropRom`) registered as a component
- [ ] Capability providers registered with broker before handler instantiation
- [ ] Handlers instantiated with their dependencies (e.g., `new MoveCommandHandler(gridMap)`)
- [ ] Rules compiled from both `GridMapRuleGroup` and `D20SystemRuleGroup`
- [ ] Initial facts seeded from all ROMs
- [ ] Cross-ROM queries use contract types (e.g., `Query<IPlayerLocation>()`)
- [ ] ComponentRegistry validation passes (no circular dependencies)

---

## Reference

- **Phase 2.7 spec**: Handoff 0027 (implementation, now complete)
- **Architecture**: [design/engine-core.md](../../design/engine-core.md), [design/architecture.md](../../design/architecture.md)
- **Capability mediation**: Cross-ROM capability broker pattern
- **Verified:** Production build succeeds, manual server test passes (move → north, look, quit)

---

## Next Steps

1. Remove `[Ignore]` attributes from test classes
2. Update `StartSession()` and test fixtures per patterns above
3. Verify ComponentRegistry validation passes
4. Run full test suite: `dotnet test tests/LunaFlow.EngineTests`
5. Ensure handler coverage (move, look, check, fight, flee) and rule coverage (win-lines, game-end)

