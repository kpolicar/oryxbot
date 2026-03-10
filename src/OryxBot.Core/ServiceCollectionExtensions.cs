using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.Core.Configuration;
using OryxBot.Core.Events;

namespace OryxBot.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotCore(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton(TimeProvider.System);
        services.Configure<NavigationOptions>(config.GetSection("Navigation"));
        services.Configure<DisplayOptions>(config.GetSection("Display"));
        return services;
    }
}
