using System.Numerics;
using OryxBot.Core.Models;

namespace OryxBot.GameState;

/// <summary>
/// Velocity-based position extrapolation. Predicts where the character will be
/// based on the last known position and velocity.
/// </summary>
public sealed class PositionPredictor
{
    private readonly TimeProvider _timeProvider;
    private Position _lastPosition;
    private Vector2 _velocity;
    private DateTimeOffset _lastUpdate;

    public PositionPredictor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _lastUpdate = timeProvider.GetUtcNow();
    }

    public Vector2 Velocity => _velocity;

    public void Update(Position newPosition)
    {
        var now = _timeProvider.GetUtcNow();
        var elapsed = (float)(now - _lastUpdate).TotalSeconds;

        if (elapsed > 0.001f)
        {
            _velocity = new Vector2(
                (newPosition.X - _lastPosition.X) / elapsed,
                (newPosition.Y - _lastPosition.Y) / elapsed);
        }

        _lastPosition = newPosition;
        _lastUpdate = now;
    }

    /// <summary>
    /// Returns the predicted position at the current time based on last known velocity.
    /// </summary>
    public Position PredictedPosition
    {
        get
        {
            var elapsed = (float)(_timeProvider.GetUtcNow() - _lastUpdate).TotalSeconds;
            return new Position(
                _lastPosition.X + _velocity.X * elapsed,
                _lastPosition.Y + _velocity.Y * elapsed);
        }
    }

    public void Reset()
    {
        _lastPosition = default;
        _velocity = Vector2.Zero;
        _lastUpdate = _timeProvider.GetUtcNow();
    }
}
