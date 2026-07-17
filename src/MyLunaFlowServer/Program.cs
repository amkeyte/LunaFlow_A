// See https://aka.ms/new-console-template for more information
using LunaFlow.Engine;
using LunaFlow.Engine.Commands;

Console.WriteLine("Welcome to MyLunaFlowServer, powered by LunaFlow_A");
Console.WriteLine("Starting server");

var engine = new ConsoleEngine()
    .ConfigureGameEngine(opts =>
    {
        opts.GameName = "My LunaFlow Game";
        opts.EnableRulesEngine = true;
    })
    .ConfigureCommand(opts =>
    {
        opts.CommandDispatcher.RegisterHandler("quit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("exit", new QuitCommandHandler(), "Stop the engine and exit.");
        opts.CommandDispatcher.RegisterHandler("echo", new EchoCommandHandler(), "Print back the given arguments.");
        opts.CommandDispatcher.RegisterHandler("help", new HelpCommandHandler(opts.CommandDispatcher), "List available commands.");
    });

engine.Run();
Console.WriteLine("Server stopped.");
