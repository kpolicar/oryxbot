namespace OryxBot.Core.Abstractions;

public interface IInputAdapter
{
    Task SetCursorPosition(int x, int y, CancellationToken ct = default);
    Task LeftClick(int x, int y, CancellationToken ct = default);
    Task RightMouseDown(int x, int y, CancellationToken ct = default);
    Task RightMouseUp(int x, int y, CancellationToken ct = default);
    Task KeyPress(int keyCode, CancellationToken ct = default);
}
