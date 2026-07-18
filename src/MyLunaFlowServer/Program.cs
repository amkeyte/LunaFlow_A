// See https://aka.ms/new-console-template for more information
using LunaFlow.Engine;
using LunaFlow.Engine.Commands;
using LunaFlow.Rom.NineRooms;

Console.WriteLine("Welcome to MyLunaFlowServer, powered by LunaFlow_A");
Console.WriteLine("Starting server");

NineRoomsContent.Validate();

var engine = new ConsoleEngine()
    .ConfigureGameEngine(opts =>
    {
        opts.GameName = "My LunaFlow Game";
        opts.EnableRulesEngine = true;
        opts.Rules
            .FromAssembly(typeof(NineRoomsContent).Assembly)
            .Seed(() => NineRoomsContent.Rooms.Values)
            .Seed(() => new object[] { new PlayerPosition(NineRoomsContent.StartRoomId) });
    })
    .ConfigureCommand(opts =>
    {
        opts.CommandDispatcher.RegisterHandler("quit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("exit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("echo", new EchoCommandHandler(), "Print back the given arguments.");
        opts.CommandDispatcher.RegisterHandler("help", new HelpCommandHandler(opts.CommandDispatcher), "List available commands.");
        opts.CommandDispatcher.RegisterHandler("go", new MoveCommandHandler(), "Move: go north/south/east/west.");
        opts.CommandDispatcher.RegisterHandler("answer", new AnswerCommandHandler(), "Answer the current room's challenge.");
        opts.CommandDispatcher.RegisterHandler("look", new LookCommandHandler(), "Redisplay the current room's description.");
    });

engine.Run();
Console.WriteLine("Server stopped.");
