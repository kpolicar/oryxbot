using System.Numerics;
using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class UnstickingState(
    ObstacleDetector obstacleDetector,
    IOptions<NavigationOptions> options) : INavigationState
{
    private readonly NavigationOptions _options = options.Value;
    private DateTimeOffset _enteredAt;
    private Vector2 _unstickDirection;

    public NavigationStateName Name => NavigationStateName.Unsticking;

    public void OnEnter(TickContext tick)
    {
        _enteredAt = tick.Timestamp;
        obstacleDetector.RecordAttempt();
        _unstickDirection = Vector2.Zero;
    }

    public void OnExit() { }

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        if (obstacleDetector.IsStuck)
            return new(NavAction.None, null, null, null, NavigationStateName.Lost,
                "Too many stuck attempts");

        var elapsed = tick.Timestamp - _enteredAt;

        // If we started moving, return to following route
        if (elapsed > TimeSpan.FromMilliseconds(200) && tracker.IsMoving)
            return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute,
                "Movement detected after unstick");

        // Hold duration exceeded, try returning to route
        if (elapsed > _options.UnstickHoldDuration)
            return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute,
                "Unstick hold duration exceeded");

        // Compute unstick direction on first tick
        if (_unstickDirection == Vector2.Zero)
        {
            var rotation = tracker.RecentlyChangedCluster
                ? _options.UnstickRotationPostCluster
                : _options.UnstickRotationNormal;

            // Get direction to current waypoint and rotate it
            var baseDirection = GetBaseDirection(cursor, tracker);
            _unstickDirection = RotateVector(baseDirection, rotation);
        }

        return new(NavAction.MoveInDirection, null, _unstickDirection, null, null,
            "Applying anti-stuck rotation");
    }

    private static Vector2 GetBaseDirection(RouteCursor cursor, ICharacterTracker tracker)
    {
        if (cursor.Current is Routes.Models.MoveWaypoint move)
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
