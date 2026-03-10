using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.Protocol.Configuration;

namespace OryxBot.Protocol;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotProtocol(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ProtocolOptions>(config.GetSection("Protocol"));
        return services;
    }
}
