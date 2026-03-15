using OryxBot.Core.Abstractions;

namespace OryxBot.Simulation;

public class SimulatedInputCapture : IInputAdapter
{
    private readonly GameSimulator _simulator;
    public List<InputCommand> CommandLog { get; } = [];

    public SimulatedInputCapture(GameSimulator simulator)
    {
        _simulator = simulator;
    }

    public Task SetCursorPosition(int x, int y, CancellationToken ct = default)
    {
        CommandLog.Add(new CursorMoveCommand(x, y));
        return Task.CompletedTask;
    }

    public Task LeftClick(int x, int y, CancellationToken ct = default)
    {
        CommandLog.Add(new LeftClickCommand(x, y));
        return Task.CompletedTask;
    }

    public async Task RightMouseDown(int x, int y, CancellationToken ct = default)
    {
        CommandLog.Add(new RightMouseDownCommand(x, y));
        await _simulator.OnBotInput(x, y, ct);
    }

    public Task RightMouseUp(int x, int y, CancellationToken ct = default)
    {
        CommandLog.Add(new RightMouseUpCommand(x, y));
        return Task.CompletedTask;
    }

    public Task KeyPress(int keyCode, CancellationToken ct = default)
    {
        CommandLog.Add(new KeyPressCommand(keyCode));
        return Task.CompletedTask;
    }
}

public abstract record InputCommand;
public record CursorMoveCommand(int X, int Y) : InputCommand;
public record RightMouseDownCommand(int X, int Y) : InputCommand;
public record RightMouseUpCommand(int X, int Y) : InputCommand;
public record LeftClickCommand(int X, int Y) : InputCommand;
public record KeyPressCommand(int KeyCode) : InputCommand;
