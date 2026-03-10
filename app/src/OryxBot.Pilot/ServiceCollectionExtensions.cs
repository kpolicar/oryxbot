using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OryxBot.Pilot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotPilot(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<ObstacleDetector>();
        services.AddSingleton<NavigationController>();
        services.AddSingleton<IPilotEngine, PilotEngine>();
        return services;
    }
}
