// Serial-by-default: GameEngineCore.Instance is a singleton (load-bearing, production-fidelity requirement).
// Parallelism is only safe for tests with no shared state — rare in this suite. Default-serial is fail-safe;
// default-parallel would require every singleton test to remember [DoNotParallelize], creating a trap.
// Pure tests may opt into parallel explicitly: [Parallelize(Scope = ExecutionScope.MethodLevel)]
[assembly: DoNotParallelize]
