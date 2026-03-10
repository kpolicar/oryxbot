using Microsoft.Extensions.Hosting;
using OryxBot.Core;
using OryxBot.GameState;
using OryxBot.Input;
using OryxBot.Pilot;
using OryxBot.Protocol;
using OryxBot.RemoteDesktop;
using OryxBot.Routes;
using OryxBot.WorldGraph;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;
        services.AddOryxBotCore(config);
        services.AddOryxBotWorldGraph(config);
        services.AddOryxBotPilot(config);
        services.AddOryxBotRoutes(config);
        services.AddOryxBotProtocol(config);
        services.AddOryxBotInput(config);
        services.AddOryxBotGameState(config);
        services.AddOryxBotRemoteDesktop(config);
    })
    .UseSerilog((ctx, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration);
    })
    .Build();

Log.Information("OryxBot started");

await host.RunAsync();
