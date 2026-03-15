using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Routes.Models;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Assertions;

public static class SimulationAssertions
{
    public static float MaxDeviation(this SimulationRecording recording)
    {
        var waypoints = recording.RouteWaypoints
            .OfType<MoveWaypoint>()
            .Select(w => new Position(w.X, w.Y))
            .ToList();

        if (waypoints.Count == 0) return 0f;

        return recording.Frames
            .Select(f => waypoints.Min(w => Position.Distance(f.Position, w)))
            .Max();
    }

    public static float AverageDeviation(this SimulationRecording recording)
    {
        var waypoints = recording.RouteWaypoints
            .OfType<MoveWaypoint>()
            .Select(w => new Position(w.X, w.Y))
            .ToList();

        if (waypoints.Count == 0 || recording.Frames.Count == 0) return 0f;

        return recording.Frames
            .Select(f => waypoints.Min(w => Position.Distance(f.Position, w)))
            .Average();
    }

    public static int BacktrackCount(this SimulationRecording recording)
    {
        var count = 0;
        for (var i = 1; i < recording.Frames.Count; i++)
        {
            if (recording.Frames[i].WaypointIndex < recording.Frames[i - 1].WaypointIndex)
                count++;
        }
        return count;
    }

    public static float CompletionTimeRatio(this SimulationRecording recording, int theoreticalMinTicks)
    {
        return theoreticalMinTicks > 0 ? (float)recording.Frames.Count / theoreticalMinTicks : 0f;
    }

    public static bool RouteCompleted(this SimulationRecording recording)
    {
        if (recording.Frames.Count == 0) return false;
        var lastFrame = recording.Frames[^1];
        return lastFrame.State == NavigationStateName.FollowingRoute &&
               lastFrame.WaypointIndex >= recording.RouteWaypoints.Count;
    }

    public static bool ContainsStateTransition(this SimulationRecording recording,
        NavigationStateName from, NavigationStateName to)
    {
        for (var i = 1; i < recording.Frames.Count; i++)
        {
            if (recording.Frames[i - 1].State == from && recording.Frames[i].State == to)
                return true;
        }
        return false;
    }

    public static IEnumerable<NavigationStateName> StateSequence(this SimulationRecording recording)
    {
        NavigationStateName? last = null;
        foreach (var frame in recording.Frames)
        {
            if (frame.State != last)
            {
                yield return frame.State;
                last = frame.State;
            }
        }
    }
}
