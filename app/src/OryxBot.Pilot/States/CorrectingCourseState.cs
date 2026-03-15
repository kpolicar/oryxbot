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

        // Find the closest waypoint by direct distance — head straight for it
        var (bestOffset, bestDist) = FindClosestWaypoint(cursor, tracker.Position);

        if (bestOffset < 0)
            return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute, "No upcoming waypoints");

        // Snap the cursor to the closest waypoint so FollowingRoute resumes from there
        if (bestOffset > 0)
            cursor.SetIndex(cursor.Index + bestOffset);

        var wp = (MoveWaypoint)cursor.Current!;
        var target = new Position(wp.X, wp.Y);

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

        return new(NavAction.MoveTowards, target, null, null, null,
            $"Correcting course toward waypoint {cursor.Index}, deviation {bestDist:F1}");
    }

    /// <summary>
    /// Scans upcoming waypoints and returns the offset and distance of the closest one
    /// by direct point distance. The bot should head straight for the nearest point
    /// on the route rather than looking ahead along segments.
    /// </summary>
    private static (int Offset, float Distance) FindClosestWaypoint(RouteCursor cursor, Position pos)
    {
        var bestOffset = -1;
        var bestDist = float.MaxValue;

        for (var offset = 0; offset <= 10; offset++)
        {
            if (cursor.Peek(offset) is not MoveWaypoint wp) break;

            var dist = Position.Distance(pos, new Position(wp.X, wp.Y));
            if (dist < bestDist)
            {
                bestDist = dist;
                bestOffset = offset;
            }
        }

        return (bestOffset, bestDist);
    }
}
