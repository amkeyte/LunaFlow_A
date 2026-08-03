// Serial-by-default: GameEngineCore.Instance is a singleton (load-bearing, production-fidelity requirement).
// Mirrors tests/LunaFlow.EngineTests/MSTestSettings.cs — the assembly-level attribute doesn't cross
// project boundaries, so a separate content test project needs its own copy. Content tests
// (D20SystemHandlerTests, InventoryCommandHandlerTests, etc.) touch the same singleton, so the same
// fail-safe default applies here. Pure tests may opt into parallel explicitly:
// [Parallelize(Scope = ExecutionScope.MethodLevel)]
[assembly: DoNotParallelize]
