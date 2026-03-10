using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OryxBot.Core.Abstractions;
using OryxBot.RemoteDesktop.Configuration;

namespace OryxBot.RemoteDesktop;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotRemoteDesktop(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<VncOptions>(config.GetSection("Vnc"));
        services.AddSingleton<IRemoteConnection, VncConnection>();
        services.AddSingleton<IInputAdapter, VncInputAdapter>();
        return services;
    }
}
