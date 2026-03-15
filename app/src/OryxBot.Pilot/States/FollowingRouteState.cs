using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class FollowingRouteState(IOptions<NavigationOptions> options) : INavigationState
{
    private readonly NavigationOptions _options = options.Value;
    private DateTimeOffset _lastMovingTime;

    public NavigationStateName Name => NavigationStateName.FollowingRoute;

    public void OnEnter(TickContext tick)
    {
        _lastMovingTime = tick.Timestamp;
    }

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        if (cursor.IsAtEnd)
            return new(NavAction.Stop, null, null, null, null, "Route complete");

        cursor.TrySkipAhead(tracker.Position, _options.CorrectionEnterThreshold, 4);

        if (cursor.IsAtEnd)
            return new(NavAction.Stop, null, null, null, null, "Route complete after skip");

        var current = cursor.Current;

        if (current is PortalWaypoint)
            return new(NavAction.Stop, null, null, null, NavigationStateName.Transitioning, "Approaching portal");

        if (current is not MoveWaypoint move)
        {
            cursor.MoveNext();
            return Evaluate(cursor, tracker, tick);
        }

        var target = new Position(move.X, move.Y);
        var distance = Position.Distance(tracker.Position, target);

        if (distance < _options.ArrivalDistance)
        {
            cursor.MoveNext();
            return Evaluate(cursor, tracker, tick);
        }

        if (tracker.IsMoving)
            _lastMovingTime = tick.Timestamp;

        var idleTime = tick.Timestamp - _lastMovingTime;
        if (idleTime > _options.IdleTimeout)
            return new(NavAction.None, null, null, null, NavigationStateName.Unsticking, "Idle timeout exceeded");

        var deviation = ComputeMinDeviation(cursor, tracker.Position);
        if (deviation > _options.CorrectionEnterThreshold)
            return new(NavAction.None, null, null, null, NavigationStateName.CorrectingCourse,
                $"Deviation {deviation:F1} exceeds threshold");

        return new(NavAction.MoveTowards, target, null, null, null, "Following route");
    }

    private static float ComputeMinDeviation(RouteCursor cursor, Position pos)
    {
        var bestDist = float.MaxValue;

        // Collect upcoming move waypoints for segment distance checks
        var waypoints = new List<Position>();
        for (var offset = 0; offset <= 5; offset++)
        {
            if (cursor.Peek(offset) is MoveWaypoint wp)
                waypoints.Add(new Position(wp.X, wp.Y));
            else break;
        }

        // Check distance to each segment between consecutive waypoints
        for (var i = 0; i < waypoints.Count - 1; i++)
        {
            var dist = Position.DistanceToSegment(pos, waypoints[i], waypoints[i + 1]);
            if (dist < bestDist) bestDist = dist;
        }

        // Also check distance to first waypoint (in case we're behind it)
        if (waypoints.Count > 0)
        {
            var dist = Position.Distance(pos, waypoints[0]);
            if (dist < bestDist) bestDist = dist;
        }

        return bestDist;
    }
}
