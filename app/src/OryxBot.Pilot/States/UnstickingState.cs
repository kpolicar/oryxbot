using System.Numerics;
using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class UnstickingState(
    ObstacleDetector obstacleDetector,
    IOptions<NavigationOptions> options,
    IObstacleMap obstacleMap) : INavigationState
{
    private readonly NavigationOptions _options = options.Value;

    private enum UnstickPhase { BackingUp, Circling, Probing }

    private UnstickPhase _phase;
    private DateTimeOffset _phaseStartedAt;
    private Vector2 _originalDirection;
    private Vector2 _circleDirection;
    private int _circleFlipCount;
    private float _lastDistToWaypoint;
    private int _progressTicks;
    private DateTimeOffset _lastMovingTime;
    private bool _initialized;
    private string _clusterId = "";
    private float _obstacleRadius;

    public NavigationStateName Name => NavigationStateName.Unsticking;

    public void OnEnter(TickContext tick)
    {
        obstacleDetector.RecordAttempt();

        // Scale assumed obstacle radius based on attempt count
        // First attempt: base radius, each subsequent attempt grows
        var attempt = obstacleDetector.AttemptCount;
        _obstacleRadius = _options.AssumedObstacleRadius + (attempt - 1) * _options.ObstacleRadiusGrowth;

        _phase = UnstickPhase.BackingUp;
        _phaseStartedAt = tick.Timestamp;
        _originalDirection = Vector2.Zero;
        _circleDirection = Vector2.Zero;
        _circleFlipCount = 0;
        _lastDistToWaypoint = float.MaxValue;
        _progressTicks = 0;
        _lastMovingTime = tick.Timestamp;
        _initialized = false;
    }

    public void OnExit() { }

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        if (obstacleDetector.IsStuck)
            return new(NavAction.None, null, null, null, NavigationStateName.Lost,
                "Too many stuck attempts");

        // Initialize on first Evaluate (we need cursor/tracker which aren't in OnEnter)
        if (!_initialized)
        {
            _initialized = true;
            _originalDirection = GetBaseDirection(cursor, tracker);
            _clusterId = tracker.CurrentCluster;

            // Record obstacle hit
            obstacleMap.RecordHit(_clusterId, tracker.Position, _originalDirection, tick.Timestamp);
        }

        if (tracker.IsMoving)
            _lastMovingTime = tick.Timestamp;

        return _phase switch
        {
            UnstickPhase.BackingUp => EvaluateBackingUp(cursor, tracker, tick),
            UnstickPhase.Circling => EvaluateCircling(cursor, tracker, tick),
            UnstickPhase.Probing => EvaluateProbing(cursor, tracker, tick),
            _ => new(NavAction.None, null, null, null, NavigationStateName.Lost, "Invalid unstick phase")
        };
    }

    /// <summary>
    /// Backup and circle durations scale with assumed obstacle radius.
    /// Larger obstacle = need to back up further and arc wider.
    /// </summary>
    private TimeSpan ScaledBackupDuration =>
        _options.UnstickBackupDuration * (_obstacleRadius / _options.AssumedObstacleRadius);

    private TimeSpan ScaledCircleDuration =>
        _options.UnstickCircleDuration * (_obstacleRadius / _options.AssumedObstacleRadius);

    private NavigationDecision EvaluateBackingUp(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        var elapsed = tick.Timestamp - _phaseStartedAt;

        if (elapsed >= ScaledBackupDuration)
        {
            // Transition to Circling — pick side using obstacle map
            _phase = UnstickPhase.Circling;
            _phaseStartedAt = tick.Timestamp;
            _lastMovingTime = tick.Timestamp;

            var clockwise = RotateVector(_originalDirection, MathF.PI / 2f);
            var counterClockwise = RotateVector(_originalDirection, -MathF.PI / 2f);

            var cwBlocked = obstacleMap.IsDirectionBlocked(_clusterId, tracker.Position, clockwise);
            var ccwBlocked = obstacleMap.IsDirectionBlocked(_clusterId, tracker.Position, counterClockwise);

            _circleDirection = cwBlocked && !ccwBlocked ? counterClockwise
                : !cwBlocked && ccwBlocked ? clockwise
                : clockwise; // default to clockwise if both or neither blocked

            return new(NavAction.MoveInDirection, null, _circleDirection, null, null,
                $"Backing up complete (r={_obstacleRadius:F1}), starting circle");
        }

        return new(NavAction.MoveInDirection, null, -_originalDirection, null, null,
            $"Backing up from obstacle ({elapsed.TotalMilliseconds:F0}ms, r={_obstacleRadius:F1})");
    }

    private NavigationDecision EvaluateCircling(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        var elapsed = tick.Timestamp - _phaseStartedAt;
        var circleDuration = ScaledCircleDuration;

        // Check if stuck during circling — hit another obstacle
        var stuckDuring = tick.Timestamp - _lastMovingTime > TimeSpan.FromMilliseconds(500);
        if (stuckDuring)
        {
            _circleFlipCount++;
            if (_circleFlipCount > _options.UnstickMaxCircleFlips)
                return new(NavAction.None, null, null, null, NavigationStateName.Lost,
                    $"Stuck while circling after {_circleFlipCount} flips");

            // Flip direction and restart circling
            _circleDirection = -_circleDirection;
            _phaseStartedAt = tick.Timestamp;
            _lastMovingTime = tick.Timestamp;
            _progressTicks = 0;
            return new(NavAction.MoveInDirection, null, _circleDirection, null, null,
                "Flipping circle direction");
        }

        // After halfway through the arc, check if making progress toward waypoint
        var t = (float)(elapsed / circleDuration);
        t = Math.Clamp(t, 0f, 1f);

        if (t > 0.5f && cursor.Current is MoveWaypoint move)
        {
            var dist = Position.Distance(tracker.Position, new Position(move.X, move.Y));
            if (dist < _lastDistToWaypoint - 0.5f)
                _progressTicks++;
            else
                _progressTicks = 0;
            _lastDistToWaypoint = dist;

            if (_progressTicks >= 3)
            {
                _phase = UnstickPhase.Probing;
                _phaseStartedAt = tick.Timestamp;
                _lastMovingTime = tick.Timestamp;
                return new(NavAction.MoveTowards, new Position(move.X, move.Y), null, null, null,
                    "Progress detected, probing toward waypoint");
            }
        }

        // Time's up — transition to probing
        if (elapsed >= circleDuration)
        {
            _phase = UnstickPhase.Probing;
            _phaseStartedAt = tick.Timestamp;
            _lastMovingTime = tick.Timestamp;
        }

        // Semicircle sweep: rotate from perpendicular (0°) through forward (90°)
        // to opposite-perpendicular (180°), arcing fully around the obstacle
        var dir = RotateVector(_circleDirection, -t * MathF.PI);
        var len = dir.Length();
        if (len > 0.001f) dir /= len;

        return new(NavAction.MoveInDirection, null, dir, null, null,
            $"Circling around obstacle (t={t:F2}, r={_obstacleRadius:F1})");
    }

    private NavigationDecision EvaluateProbing(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        var elapsed = tick.Timestamp - _phaseStartedAt;

        // Moving successfully — back to route
        if (elapsed > TimeSpan.FromMilliseconds(200) && tracker.IsMoving)
            return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute,
                "Movement confirmed after probe, resuming route");

        // Not moving after probe duration — try unsticking again (with larger assumed radius)
        if (elapsed >= _options.UnstickProbeDuration)
            return new(NavAction.None, null, null, null, NavigationStateName.Unsticking,
                $"Probe failed, retrying with larger assumed radius (was r={_obstacleRadius:F1})");

        // Move toward current waypoint
        if (cursor.Current is MoveWaypoint move)
        {
            var target = new Position(move.X, move.Y);
            return new(NavAction.MoveTowards, target, null, null, null,
                $"Probing toward waypoint ({elapsed.TotalMilliseconds:F0}ms)");
        }

        return new(NavAction.MoveInDirection, null, _originalDirection, null, null,
            "Probing in original direction");
    }

    private static Vector2 GetBaseDirection(RouteCursor cursor, ICharacterTracker tracker)
    {
        if (cursor.Current is MoveWaypoint move)
        {
            var target = new Position(move.X, move.Y);
            var dir = target - tracker.Position;
            var len = dir.Length();
            return len > 0.001f ? dir / len : Vector2.UnitX;
        }
        return Vector2.UnitX;
    }

    private static Vector2 RotateVector(Vector2 v, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }
}
