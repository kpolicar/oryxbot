namespace OryxBot.Core.Abstractions;

public interface IRemoteConnection
{
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    bool IsConnected { get; }
    (int Width, int Height) ScreenSize { get; }
}
