// See https://aka.ms/new-console-template for more information
using LunaFlow.Engine;
using LunaFlow.Engine.Commands;
using LunaFlow.Rom.OneD20;
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

var engine = new ConsoleEngine()
    .ConfigureGameEngine(opts =>
    {
        opts.GameName = "My LunaFlow Game";
        opts.EnableRulesEngine = true;
        opts.Rules
            .FromAssembly(RuleExport.Compile(typeof(OneD20RuleGroup)))
            .Seed(() => OneD20Content.Rooms.Values)
            .Seed(() =>
            {
                // GameEngineCore.Start() runs every Seed() provider (ApplyTo) before the initial-
                // settle Fire() — and MessagePosted is already subscribed by the time Start() runs
                // (ConsoleEngine.Run() wires it up first) — so an Announce() here is the one place
                // content code can guarantee it prints before Fire()'s challenge-prompt rule fires
                // monster-desc for room 5. Program.cs itself runs entirely before engine.Run(), so
                // an Announce() call there would fire with nobody subscribed yet and be lost.
                LunaFlowEngine.Host.Announce(OneD20Flavor.Text[OneD20Content.Rooms[OneD20Content.StartRoomId].DescriptionKey]);
                return new object[]
                {
                    new PlayerPosition(OneD20Content.StartRoomId),
                    OneD20Content.PlayerCharacter,
                    OneD20Content.Monster,
                };
            });
    })
    .ConfigureCommand(opts =>
    {
        opts.CommandDispatcher.RegisterHandler("quit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("exit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("echo", new EchoCommandHandler(), "Print back the given arguments.");
        opts.CommandDispatcher.RegisterHandler("help", new HelpCommandHandler(opts.CommandDispatcher), "List available commands.");
        opts.CommandDispatcher.RegisterHandler("move", new MoveCommandHandler(), "Move: move north/south/east/west.");
        opts.CommandDispatcher.RegisterHandler("look", new LookCommandHandler(), "Redisplay the current room's description.");
        opts.CommandDispatcher.RegisterHandler("check", new CheckCommandHandler(), "Attempt the current room's skill check.");
        opts.CommandDispatcher.RegisterHandler("fight", new FightCommandHandler(), "Attack the monster in the current room.");
        opts.CommandDispatcher.RegisterHandler("flee", new FleeCommandHandler(), "Retreat from the current fight.");
    });

engine.Run();
Console.WriteLine("Server stopped.");
