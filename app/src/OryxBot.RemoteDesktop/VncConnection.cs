using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.RemoteDesktop.Configuration;

namespace OryxBot.RemoteDesktop;

/// <summary>
/// Stub — real VNC connection management comes when running on actual Linux VM.
/// </summary>
public sealed class VncConnection : IRemoteConnection
{
    private readonly VncOptions _options;

    public VncConnection(IOptions<VncOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConnected => false;
    public (int Width, int Height) ScreenSize => (1920, 1080);

    public Task ConnectAsync(CancellationToken ct = default) =>
        throw new NotImplementedException("VncConnection implementation requires Linux VM environment");

    public Task DisconnectAsync(CancellationToken ct = default) =>
        throw new NotImplementedException("VncConnection implementation requires Linux VM environment");
}
