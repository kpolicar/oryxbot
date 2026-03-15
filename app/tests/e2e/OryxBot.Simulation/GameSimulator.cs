using System.Numerics;
using Microsoft.Extensions.Time.Testing;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;
using OryxBot.GameState;
using OryxBot.Pilot;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;
using OryxBot.Simulation.Obstacles;
using OryxBot.Simulation.Profiles;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation;

public class GameSimulator
{
    private readonly ISimulationProfile _profile;
    private readonly NavigationController _controller;
    private readonly RouteCursor _cursor;
    private readonly CharacterTracker _tracker;
    private IGameController _gameController;
    private readonly SimulationRecorder _recorder;
    private readonly FakeTimeProvider _timeProvider;
    private readonly List<Obstacle> _obstacles;
    private readonly Random _rng;
    private readonly int _maxTicks;
    private readonly DisplayOptions _displayOptions;

    private Position _actualPosition;
    private string _currentCluster;
    private float _speed;
    private int? _clusterChangeCountdown;
    private string? _pendingCluster;
    private Position _pendingArrival;

    public bool IsComplete { get; private set; }
    public bool HasFailed { get; private set; }
    public List<SimulatedTick> TickHistory { get; } = [];

    public GameSimulator(
        ISimulationProfile profile,
        NavigationController controller,
        RouteCursor cursor,
        CharacterTracker tracker,
        IGameController gameController,
        SimulationRecorder recorder,
        FakeTimeProvider timeProvider,
        DisplayOptions displayOptions,
        List<Obstacle> obstacles,
        Position startPosition,
        string startCluster,
        int seed,
        int maxTicks)
    {
        _profile = profile;
        _controller = controller;
        _cursor = cursor;
        _tracker = tracker;
        _gameController = gameController;
        _recorder = recorder;
        _timeProvider = timeProvider;
        _displayOptions = displayOptions;
        _obstacles = obstacles;
        _actualPosition = startPosition;
        _currentCluster = startCluster;
        _speed = 7.0f;
        _rng = new Random(seed);
        _maxTicks = maxTicks;

        // Initialize tracker with starting state
        _tracker.UpdateCluster(startCluster);
        _tracker.UpdatePosition(startPosition);
    }

    public async Task<SimulationRecording> RunAsync(CancellationToken ct = default)
    {
        var startTime = _timeProvider.GetUtcNow();

        for (var tick = 0; tick < _maxTicks && !IsComplete && !HasFailed; tick++)
        {
            var interval = _profile.NextPacketInterval(_rng);
            _timeProvider.Advance(interval);

            var elapsed = _timeProvider.GetUtcNow() - startTime;
            var tickContext = new TickContext(interval, tick, _timeProvider.GetUtcNow());

            // Simulate cluster change when in Transitioning state
            if (_controller.CurrentStateName == NavigationStateName.Transitioning && _pendingCluster is null)
            {
                // Find the portal waypoint to determine target cluster
                if (_cursor.Current is PortalWaypoint portal)
                {
                    _pendingCluster = portal.ClusterName;
                    // Find arrival position: first MoveWaypoint after the portal
                    _pendingArrival = FindArrivalPosition();
                    var delay = _profile.ClusterChangeDelay(_rng);
                    var tickInterval = _profile.NextPacketInterval(_rng);
                    _clusterChangeCountdown = Math.Max(1, (int)(delay.TotalMilliseconds / tickInterval.TotalMilliseconds));
                }
            }

            if (_clusterChangeCountdown.HasValue)
            {
                _clusterChangeCountdown--;
                if (_clusterChangeCountdown <= 0 && _pendingCluster is not null)
                {
                    SimulateClusterChange(_pendingCluster, _pendingArrival);
                    _recorder.Log(tick, elapsed.TotalMilliseconds, "Info",
                        $"Cluster changed: {_currentCluster}");
                    _pendingCluster = null;
                    _pendingArrival = new Position(0, 0);
                    _clusterChangeCountdown = null;
                }
            }

            var decision = _controller.Evaluate(_cursor, _tracker, tickContext);
            await _controller.ExecuteAsync(decision, _gameController, ct);

            // Record frame
            _recorder.RecordFrame(
                tick,
                elapsed.TotalMilliseconds,
                _currentCluster,
                _actualPosition,
                _controller.CurrentStateName,
                _cursor.Index,
                decision.TargetPosition);

            // Log the decision reason
            if (decision.Reason is not null)
                _recorder.Log(tick, elapsed.TotalMilliseconds, "Debug", $"[{_controller.CurrentStateName}] {decision.Reason}");

            // Check completion
            if (_cursor.IsAtEnd && _controller.CurrentStateName == NavigationStateName.FollowingRoute)
            {
                IsComplete = true;
                _recorder.Log(tick, elapsed.TotalMilliseconds, "Info", "Route completed successfully");
            }

            if (_controller.CurrentStateName is NavigationStateName.Lost or NavigationStateName.Killed or NavigationStateName.Disconnected)
            {
                HasFailed = true;
                _recorder.Log(tick, elapsed.TotalMilliseconds, "Warn", $"Navigation failed: {_controller.CurrentStateName}");
            }
        }

        return _recorder.ToRecording();
    }

    /// <summary>
    /// Called by SimulatedInputCapture when the bot issues a right-click (movement command).
    /// Converts screen coordinates back to a world direction, applies physics, and updates position.
    /// </summary>
    public Task OnBotInput(int screenX, int screenY, CancellationToken ct)
    {
        // Reverse the coordinate translation: screen coords → world direction
        var centerX = _displayOptions.ScreenWidth / 2f;
        var centerY = _displayOptions.ScreenHeight / 2f;
        var dx = screenX - centerX;
        var dy = screenY - centerY;

        // Reverse the isometric rotation (+π/4 to undo -π/4)
        const float angle = MathF.PI / 4f;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        var worldDx = dx * cos - dy * sin;
        var worldDy = dx * sin + dy * cos;

        var direction = new Vector2(worldDx, worldDy);
        var len = direction.Length();
        if (len > 0.001f)
            direction /= len;

        // Apply profile drift
        direction = _profile.ApplyDirectionDrift(direction, _rng);

        // Calculate speed and displacement
        _speed = _profile.CalculateSpeed(_speed, _rng);
        var interval = _profile.NextPacketInterval(_rng);
        var displacement = direction * _speed * (float)interval.TotalSeconds;

        var candidate = _actualPosition + displacement;

        // Apply obstacles
        foreach (var obstacle in _obstacles)
            candidate = obstacle.Resolve(_actualPosition, candidate);

        _actualPosition = candidate;

        // Record tick for assertions
        TickHistory.Add(new SimulatedTick(_actualPosition, direction, _speed, interval));

        // Feed back to character tracker
        _tracker.UpdatePosition(_actualPosition);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Simulate a cluster change (for testing portal transitions).
    /// </summary>
    public void SetGameController(IGameController controller)
    {
        _gameController = controller;
    }

    private Position FindArrivalPosition()
    {
        // Look ahead in the route for the first MoveWaypoint after the current portal
        for (var offset = 1; offset <= 5; offset++)
        {
            var wp = _cursor.Peek(offset);
            if (wp is MoveWaypoint move)
                return new Position(move.X, move.Y);
        }
        return new Position(0, 0);
    }

    public void SimulateClusterChange(string newCluster, Position arrivalPosition)
    {
        _currentCluster = newCluster;
        _actualPosition = arrivalPosition;
        _tracker.UpdateCluster(newCluster);
        _tracker.UpdatePosition(arrivalPosition);
    }
}

public record SimulatedTick(Position Position, Vector2 Direction, float Speed, TimeSpan Elapsed);
