namespace OryxBot.Core.Abstractions;

public interface IPacketInterceptor
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}
