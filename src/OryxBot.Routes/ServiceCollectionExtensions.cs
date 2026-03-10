using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.Routes.Configuration;
using OryxBot.Routes.Storage;

namespace OryxBot.Routes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotRoutes(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RouteOptions>(config.GetSection("Routes"));
        services.AddSingleton<IRouteStore, JsonRouteStore>();
        return services;
    }
}
