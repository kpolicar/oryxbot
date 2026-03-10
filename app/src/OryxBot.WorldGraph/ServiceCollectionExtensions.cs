using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.WorldGraph.Configuration;

namespace OryxBot.WorldGraph;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotWorldGraph(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<WorldGraphOptions>(config.GetSection("WorldGraph"));
        services.AddSingleton<IPathfinder, Pathfinder>();
        services.AddSingleton<WorldGraphBuilder>();
        return services;
    }
}
