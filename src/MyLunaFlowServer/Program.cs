// See https://aka.ms/new-console-template for more information
using LunaFlow.Engine;
using LunaFlow.Engine.Commands;
using LunaFlow.Engine.Components;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rom.GridMap.Handlers;
using LunaFlow.Rom.D20SystemInterop;
using LunaFlow.Rulebook.OneD20;
using LunaFlow.Rules.GeneratorServices;

Console.WriteLine("Welcome to MyLunaFlowServer, powered by LunaFlow_A");
Console.WriteLine("Starting server");

// RoslynRuleCompiler only sees already-loaded, file-backed assemblies (see its own doc comment)
// — since 1.d20 has no hand-written ARule anywhere in this process to pull these in as a side
// effect the way Nine Rooms' rules did, force them before the first RuleExport.Compile call. Same
// gotcha the pipeline's own tests hit and work around (RulePipelineGeneratorServiceTests.cs).
_ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
_ = typeof(System.Linq.Expressions.Expression).Assembly;
_ = typeof(NRules.RuleModel.IContext).Assembly;
_ = typeof(NRules.ISession).Assembly;

// Phase 2.7: Instantiate the three ROMs with constructor-injected data.
var gridMapRoomData = OneD20RulebookContent.Rooms.Values
    .Select(r => new GridRoomData(r.Id, r.Row, r.Col, r.DescriptionKey))
    .ToList();
var gridMap = new GridMapRom(gridMapRoomData, OneD20RulebookContent.StartRoomId);

var d20Challenges = OneD20RulebookContent.Rooms.Values
    .Select(r => new LunaFlow.Rom.D20System.RoomChallenge(
        r.Id, r.ChallengeType, r.Skill, r.SkillAbility, r.DC, r.SuccessKey, r.FailureKey, r.PromptKey))
    .ToDictionary(c => c.Id);
var d20System = new D20SystemRom(
    OneD20RulebookContent.PlayerCharacter,
    OneD20RulebookContent.Monster,
    d20Challenges,
    OneD20RulebookContent.PlayerAbilityScores);

var interop = new D20SystemInteropRom();

// Register capability providers with the broker.
interop.RegisterProvider("PlayerLocation", gridMap);
interop.RegisterProvider("PlayerMovement", new GridMovementService());
interop.RegisterProvider("ChallengeSolved", d20System);
interop.RegisterProvider("CombatStatus", d20System);

// Phase 2.6.5: Register and validate ROM/Rulebook components.
var registry = new ComponentRegistry();
registry.Register(gridMap, EngineTier.Rom, "GridMap");
registry.Register(d20System, EngineTier.Rom, "D20System");
registry.Register(interop, EngineTier.Rom, "D20SystemInterop");
registry.Register(new OneD20RuleBook(), EngineTier.Rulebook, "OneD20");

try
{
    registry.Validate();
    Console.WriteLine("Component registry validated successfully.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Component registry validation failed: {ex.Message}");
    throw;
}

var engine = new ConsoleEngine()
    .ConfigureGameEngine(opts =>
    {
        opts.GameName = "My LunaFlow Game";
        opts.EnableRulesEngine = true;
        opts.Rules
            .FromAssembly(RuleExport.Compile(typeof(GridMapRuleGroup)))
            .FromAssembly(RuleExport.Compile(typeof(D20SystemRuleGroup)))
            .Seed(() => gridMapRoomData)
            .Seed(() =>
            {
                LunaFlowEngine.Host.Announce("Starting in room 5...");
                return new object[]
                {
                    new PlayerPosition(OneD20RulebookContent.StartRoomId),
                    OneD20RulebookContent.PlayerCharacter,
                    OneD20RulebookContent.Monster,
                };
            });
    })
    .ConfigureCommand(opts =>
    {
        var mechanic = d20System.Mechanic;
        opts.CommandDispatcher.RegisterHandler("quit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("exit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("echo", new EchoCommandHandler(), "Print back the given arguments.");
        opts.CommandDispatcher.RegisterHandler("help", new HelpCommandHandler(opts.CommandDispatcher), "List available commands.");
        opts.CommandDispatcher.RegisterHandler("move", new MoveCommandHandler(gridMap), "Move: move north/south/east/west.");
        opts.CommandDispatcher.RegisterHandler("look", new LookCommandHandler(), "Redisplay the current room's description.");
        opts.CommandDispatcher.RegisterHandler("check", new CheckCommandHandler(d20System, mechanic), "Attempt the current room's skill check.");
        opts.CommandDispatcher.RegisterHandler("fight", new FightCommandHandler(d20System, mechanic), "Attack the monster in the current room.");
        opts.CommandDispatcher.RegisterHandler("flee", new FleeCommandHandler(interop.Resolve<IPlayerMovement>("PlayerMovement")), "Retreat from the current fight.");
    });

engine.Run();
Console.WriteLine("Server stopped.");
