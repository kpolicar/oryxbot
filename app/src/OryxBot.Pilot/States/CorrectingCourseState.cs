using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class CorrectingCourseState(IOptions<NavigationOptions> options) : INavigationState
{
    private readonly NavigationOptions _options = options.Value;
    private DateTimeOffset? _lostSince;

    public NavigationStateName Name => NavigationStateName.CorrectingCourse;

    public void OnEnter(TickContext tick)
    {
        _lostSince = null;
    }

    public void OnExit()
    {
        _lostSince = null;
    }

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        if (cursor.IsAtEnd)
            return new(NavAction.Stop, null, null, null, NavigationStateName.FollowingRoute, "Route complete");

        // Find nearest upcoming waypoint (look ahead up to 3)
        var (bestTarget, bestDist) = FindNearestUpcoming(cursor, tracker.Position);

        if (bestTarget is null)
            return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute, "No upcoming waypoints");

        // Check if corrected enough to resume normal following
        if (bestDist < _options.CorrectionExitThreshold)
        {
            _lostSince = null;
            return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute,
                $"Course corrected, deviation {bestDist:F1}");
        }

        // Check if we've been lost too long
        if (bestDist > _options.LostThreshold)
        {
            _lostSince ??= tick.Timestamp;
            if (tick.Timestamp - _lostSince.Value > _options.LostTimeout)
                return new(NavAction.None, null, null, null, NavigationStateName.Lost,
                    $"Lost for {_options.LostTimeout.TotalSeconds}s at deviation {bestDist:F1}");
        }
        else
        {
            _lostSince = null;
        }

        return new(NavAction.MoveTowards, bestTarget.Value, null, null, null,
            $"Correcting course, deviation {bestDist:F1}");
    }

    private static (Position? Target, float Distance) FindNearestUpcoming(RouteCursor cursor, Position pos)
    {
        Position? bestTarget = null;
        var bestDist = float.MaxValue;

        for (var offset = 0; offset <= 3; offset++)
        {
            if (cursor.Peek(offset) is MoveWaypoint wp)
            {
                var target = new Position(wp.X, wp.Y);
                var dist = Position.Distance(pos, target);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = target;
                }
            }
            else break;
        }

        return (bestTarget, bestDist);
    }
}
