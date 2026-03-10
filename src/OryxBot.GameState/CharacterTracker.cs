using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;

namespace OryxBot.GameState;

/// <summary>
/// Implements ICharacterTracker by aggregating MotionDetector, PositionPredictor, and event state.
/// Updated via protocol events through the event bus.
/// </summary>
public sealed class CharacterTracker : ICharacterTracker
{
    private readonly MotionDetector _motionDetector;
    private readonly PositionPredictor _positionPredictor;
    private readonly PositionHistory _positionHistory;
    private readonly TimeProvider _timeProvider;

    private Position _position;
    private string _currentCluster = string.Empty;
    private DateTimeOffset _lastClusterChange;
    private bool _isDead;

    public CharacterTracker(
        MotionDetector motionDetector,
        PositionPredictor positionPredictor,
        PositionHistory positionHistory,
        TimeProvider timeProvider)
    {
        _motionDetector = motionDetector;
        _positionPredictor = positionPredictor;
        _positionHistory = positionHistory;
        _timeProvider = timeProvider;
    }

    public Position Position => _position;
    public Position PredictedPosition => _positionPredictor.PredictedPosition;
    public string CurrentCluster => _currentCluster;
    public bool IsMoving => _motionDetector.IsMoving;
    public bool RecentlyChangedCluster => (_timeProvider.GetUtcNow() - _lastClusterChange).TotalSeconds < 10;
    public bool IsDead => _isDead;
    public bool IsDisconnected => (_timeProvider.GetUtcNow() - LastPositionUpdate).TotalSeconds > 15;
    public float Speed => _motionDetector.Speed;
    public DateTimeOffset LastPositionUpdate { get; private set; }

    public void UpdatePosition(Position position)
    {
        _position = position;
        LastPositionUpdate = _timeProvider.GetUtcNow();
        _motionDetector.Update(position);
        _positionPredictor.Update(position);
        _positionHistory.Record(position);
    }

    public void UpdateCluster(string clusterName)
    {
        _currentCluster = clusterName;
        _lastClusterChange = _timeProvider.GetUtcNow();
        _positionHistory.Clear();
    }

    public void SetDead(bool isDead) => _isDead = isDead;
}
