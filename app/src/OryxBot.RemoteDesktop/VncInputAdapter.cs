using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.RemoteDesktop.Configuration;

namespace OryxBot.RemoteDesktop;

/// <summary>
/// Stub — translates input calls to HTTP requests to the VNC Java client bridge.
/// </summary>
public sealed class VncInputAdapter : IInputAdapter
{
    private readonly VncOptions _options;

    public VncInputAdapter(IOptions<VncOptions> options)
    {
        _options = options.Value;
    }

    public Task SetCursorPosition(int x, int y, CancellationToken ct = default) =>
        throw new NotImplementedException("VncInputAdapter implementation is Phase 2");

    public Task LeftClick(int x, int y, CancellationToken ct = default) =>
        throw new NotImplementedException("VncInputAdapter implementation is Phase 2");

    public Task RightMouseDown(int x, int y, CancellationToken ct = default) =>
        throw new NotImplementedException("VncInputAdapter implementation is Phase 2");

    public Task RightMouseUp(int x, int y, CancellationToken ct = default) =>
        throw new NotImplementedException("VncInputAdapter implementation is Phase 2");

    public Task KeyPress(int keyCode, CancellationToken ct = default) =>
        throw new NotImplementedException("VncInputAdapter implementation is Phase 2");
}
