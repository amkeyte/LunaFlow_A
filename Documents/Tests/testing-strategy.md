# LunaFlow Testing Strategy & Gaps Analysis

## Current Testing Coverage

### What's Currently Tested
- **RuleTests.cs**: 2 test methods only
  - `SessionFiresExpectedRules()` - Validates rule compilation and matching with NRules
  - `Test2()` - Tests counter-based threshold logic (MatchThresholdReachedRule)

### Test Framework
- MSTest 3.6.4 with Microsoft.NET.Test.Sdk 17.12.0
- Target: .NET 9.0
- Only tests the rules engine in isolation

---

## Critical Testing Gaps

### 1. **GameEngineCore (Turn Loop & Threading)**
**Why it matters**: The core engine manages a complex turn-based loop with thread-safe command dispatch, cancellation, and state transitions. This is mission-critical for game stability.

**What to test**:
- Engine start/stop lifecycle (including restart scenarios)
- Command queue behavior (blocking, non-blocking, ordering)
- Turn advancement logic (TurnFinished gate, turn counter increment)
- Thread safety: concurrent command submissions from multiple threads
- Cancellation token handling during shutdown
- RunLoop edge cases: command execution during turn advance, race conditions
- TurnTick event subscription/unsubscription and count management
- Message posting event fires correctly
- Options application at startup

**Value**: High—prevents deadlocks, race conditions, and state corruption during concurrent gameplay

---

### 2. **Command System** (Parser, Dispatcher, Handlers)
**Why it matters**: Commands are the input layer for all player actions. Bugs here affect every game interaction.

**What to test**:

#### DefaultCommandParser
- Parsing valid commands (name + args)
- Whitespace handling (leading/trailing, multiple spaces)
- Empty input handling
- Case sensitivity for command names
- Malformed command edge cases

#### DefaultCommandDispatcher
- Handler registration (duplicate keys, null checks)
- Command dispatch to correct handler
- Unknown command handling
- Case-insensitive lookup
- GetCommandList() returns all registered handlers
- Handler execution order (if multi-handler support is added)

#### Command Handlers (existing: Echo, Help, Quit)
- EchoCommandHandler echoes input correctly
- HelpCommandHandler returns command list
- QuitCommandHandler stops engine
- Custom handler registration and dispatch
- Handler error propagation

#### DefaultCommand
- Command object creation and data binding
- Immutability of command once created

**Value**: Medium-high—prevents UI/input layer failures that block testing of game logic

---

### 3. **GameWorld & Session Management**
**Why it matters**: World state tracks active sessions and entities. Bugs affect multiplayer scenarios and persistence.

**What to test**:
- AddSession() uniqueness (Guid generation)
- RemoveSession() correctness (null handling, already-removed)
- Session.Id uniqueness across multiple sessions
- Session.ConnectedAt timestamp accuracy
- Multiple concurrent sessions
- Session lifecycle (creation, removal, isolation)

**Value**: Medium—becomes critical for multiplayer or save/load features

---

### 4. **GameState & Persistence**
**Why it matters**: GameState holds turn number and future game state. Critical for save/load and turn logic.

**What to test**:
- TurnNumber increments correctly across turn cycles
- State snapshots (once implemented)
- State rollback (once implemented)
- Thread safety of state updates

**Value**: Medium—ensures replay/undo features work correctly

---

### 5. **RuleEngine Integration**
**Why it matters**: Rules are the game's core logic layer. Current tests are isolated; integration tests would catch wiring failures.

**What to test**:
- LoadRulesFromAssembly() with real game assemblies
- LoadRulesFromTypes() with rule collections
- Clear() resets state properly
- Insert/Retract/Update fact lifecycle
- Query<T>() returns correct facts
- Multiple Fire() cycles with stateful facts
- Rules firing in correct order (priority, dependencies)
- Session reuse across Fire() calls
- Memory cleanup on Clear()

#### Integration with GameEngineCore
- Rules loaded by engine startup
- Facts injected by game logic
- Rule-driven state changes affect game world

**Value**: Very high—rules are the game; integration failures are silent

---

### 6. **ConsoleEngine**
**Why it matters**: The entry point for text-based games. Bugs here prevent any game from running.

**What to test**:
- Configuration via fluent API (ConfigureGameEngine, ConfigureCommand)
- Game initialization sequence
- Command-line I/O loop
- Message event handling
- Graceful shutdown on stdin EOF
- Option propagation to underlying engine
- Command parser/dispatcher integration
- TurnTick handler invocation (currently a stub)

**Value**: High—full-stack entry point

---

### 7. **DispatchGameCommand**
**Why it matters**: Bridges console input → engine command execution. Failure here breaks input flow.

**What to test**:
- Command execution in engine context
- Completion signal (ManualResetEventSlim) fires
- Exception handling during command execution
- Chaining multiple commands in sequence

**Value**: Medium—prevents command deadlocks

---

## High-Priority Test Scenarios

### Unit Tests (Each Component in Isolation)
1. GameEngineCore: Start/Stop, Command Queue, Turn Loop, Events
2. DefaultCommandParser: Input parsing edge cases
3. DefaultCommandDispatcher: Handler lookup and dispatch
4. RuleEngine: Load/Clear/Fire/Query cycles
5. GameWorld: Session management
6. GameState: Turn counter logic

### Integration Tests (2+ Components)
1. **Engine + Commands**: Submit command → Execute → Signal completion
2. **Engine + Rules**: Load rules → Insert facts → Fire → Verify state
3. **ConsoleEngine + CommandDispatcher**: Parse input → Dispatch → Handle → Message event
4. **Engine Lifecycle**: Start → Commands → Turns → Stop → Restart

### Concurrent/Stress Tests
1. **Multi-threaded Command Submission**: 10+ threads submitting commands, verify ordering
2. **Turn Loop Under Load**: Rapid Fire() calls + command submissions
3. **Engine Stop During Command Execution**: Graceful shutdown
4. **Rule Engine Session Reuse**: Repeated fact insertions, memory behavior

### Scenario/Acceptance Tests
1. **NineRooms Game Flow**: Start game → Move → Look → Answer → Win (end-to-end)
2. **Error Recovery**: Invalid command → Error message → Continue playing
3. **Multi-session**: Two sessions connected simultaneously, commands don't cross

---

## Implementation Priority

### Phase 1 (Foundation)
- [ ] GameEngineCore unit tests (lifecycle, command dispatch, turn loop basics)
- [ ] DefaultCommandParser unit tests
- [ ] DefaultCommandDispatcher unit tests
- [ ] RuleEngine integration tests (with NineRooms rules)

### Phase 2 (Integration)
- [ ] ConsoleEngine integration tests
- [ ] Command dispatch → Execution end-to-end
- [ ] Engine + Rules integration

### Phase 3 (Advanced)
- [ ] Concurrent command submission tests
- [ ] Stress tests (high-frequency turns/commands)
- [ ] Full game scenario tests (NineRooms end-to-end)

### Phase 4 (Polish)
- [ ] Performance benchmarks
- [ ] Code coverage analysis and gaps
- [ ] Chaos/resilience tests (exception injection)

---

## Recommended New Test Class Structure

```
tests/LunaFlow.EngineTests/
├── Core/
│   ├── GameEngineCoreTests.cs
│   ├── GameWorldTests.cs
│   └── GameStateTests.cs
├── Commands/
│   ├── DefaultCommandParserTests.cs
│   ├── DefaultCommandDispatcherTests.cs
│   └── CommandHandlerTests.cs
├── Rules/
│   ├── RuleEngineTests.cs (expand existing)
│   └── RuleIntegrationTests.cs
├── Integration/
│   ├── EngineCommandsIntegrationTests.cs
│   ├── EngineRulesIntegrationTests.cs
│   └── ConsoleEngineIntegrationTests.cs
├── Scenarios/
│   └── NineRoomsGameFlowTests.cs
└── Mocks/ (already exists)
```

---

## Testing Best Practices for LunaFlow

1. **Isolate Threading**: Use `ManualResetEventSlim` in tests to synchronize with engine thread
2. **Mock Facts for Rules**: Create simple test facts (like NumberPair) rather than full game entities
3. **Verify Events**: Subscribe to engine events and assert they fire
4. **Clean State**: Call `RuleEngine.Clear()` and reset engine between tests
5. **Test Exceptions**: Verify error handling in command handlers and rules
6. **Concurrent Safety**: Use ThreadSafeCounter or similar patterns for thread-safe verification
