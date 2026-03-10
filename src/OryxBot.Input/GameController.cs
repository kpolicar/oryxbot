using System.Numerics;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;

namespace OryxBot.Input;

/// <summary>
/// Stub — real implementation depends on IInputAdapter being wired to a real transport.
/// </summary>
public sealed class GameController : IGameController
{
    private readonly IInputAdapter _input;
    private readonly ICharacterTracker _tracker;
    private readonly CoordinateTranslator _coordinates;

    public GameController(IInputAdapter input, ICharacterTracker tracker, CoordinateTranslator coordinates)
    {
        _input = input;
        _tracker = tracker;
        _coordinates = coordinates;
    }

    public Task MoveTowards(Position target, CancellationToken ct = default)
    {
        var screen = _coordinates.WorldToScreen(target, _tracker.Position);
        return _input.RightMouseDown(screen.X, screen.Y, ct);
    }

    public Task MoveInDirection(Vector2 direction, CancellationToken ct = default)
    {
        var screen = _coordinates.DirectionToScreen(direction);
        return _input.RightMouseDown(screen.X, screen.Y, ct);
    }

    public Task MoveAwayFrom(Position target, CancellationToken ct = default)
    {
        var away = _tracker.Position - target;
        var screen = _coordinates.DirectionToScreen(away);
        return _input.RightMouseDown(screen.X, screen.Y, ct);
    }

    public Task StopMovement(CancellationToken ct = default)
    {
        return _input.RightMouseUp(0, 0, ct);
    }

    public Task Respawn(CancellationToken ct = default)
    {
        // Stub — respawn requires clicking specific UI elements
        throw new NotImplementedException("Respawn implementation is Phase 2");
    }
}
