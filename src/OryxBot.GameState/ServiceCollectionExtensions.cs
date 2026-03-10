using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.Core.Abstractions;

namespace OryxBot.GameState;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotGameState(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<PositionHistory>();
        services.AddSingleton<MotionDetector>();
        services.AddSingleton<PositionPredictor>();
        services.AddSingleton<CharacterTracker>();
        services.AddSingleton<ICharacterTracker>(sp => sp.GetRequiredService<CharacterTracker>());
        return services;
    }
}
