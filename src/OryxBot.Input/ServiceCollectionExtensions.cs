using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.Core.Abstractions;

namespace OryxBot.Input;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotInput(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<CoordinateTranslator>();
        services.AddSingleton<IGameController, GameController>();
        return services;
    }
}
