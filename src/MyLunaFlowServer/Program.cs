// See https://aka.ms/new-console-template for more information
using LunaFlow.Engine;

Console.WriteLine("Welcom to MyLunaFlowServer, powered by LunaFlow_A");
Console.WriteLine("Starting server");
var engine = new EngineHost()
    .ConfigureGameEngine(opts =>
    {
        opts.GameName = "My LunaFlow Game";
        opts.EnableRulesEngine = true;
    });

engine.Run();
Console.WriteLine("Server stopped.");