using Microsoft.Extensions.Options;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;

namespace OryxBot.GameState;

/// <summary>
/// Detects whether the character is idle (not moving) based on position updates and time.
/// </summary>
public sealed class MotionDetector
{
    private readonly TimeProvider _timeProvider;
    private readonly NavigationOptions _options;
    private Position _lastPosition;
    private DateTimeOffset _lastMoveTime;
    private bool _isMoving;

    public MotionDetector(TimeProvider timeProvider, IOptions<NavigationOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
        _lastMoveTime = timeProvider.GetUtcNow();
    }

    public bool IsMoving => _isMoving;
    public float Speed { get; private set; }

    public void Update(Position newPosition)
    {
        var now = _timeProvider.GetUtcNow();
        var dist = Position.Distance(_lastPosition, newPosition);

        if (dist > 0.01f) // Minimal movement threshold
        {
            var elapsed = (now - _lastMoveTime).TotalSeconds;
            Speed = elapsed > 0 ? (float)(dist / elapsed) : 0f;
            _lastMoveTime = now;
            _isMoving = true;
        }
        else
        {
            var idleDuration = now - _lastMoveTime;
            if (idleDuration >= _options.IdleTimeout)
            {
                _isMoving = false;
                Speed = 0f;
            }
        }

        _lastPosition = newPosition;
    }

    public void Reset()
    {
        _lastPosition = default;
        _lastMoveTime = _timeProvider.GetUtcNow();
        _isMoving = false;
        Speed = 0f;
    }
}
