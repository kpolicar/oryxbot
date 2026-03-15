using System.Numerics;
using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Routes.Models;

namespace OryxBot.Simulation.Recording;

public class SimulationRecorder
{
    private readonly List<SimulationFrame> _frames = [];
    private readonly List<SimulationEvent> _events = [];
    private readonly List<SimulationLog> _logs = [];
    private readonly List<RecordedObstacleHit> _obstacleHits = [];
    private readonly List<RecordedObstacle> _obstacles = [];
    private readonly SimulationMetadata _metadata;
    private readonly IReadOnlyList<Waypoint> _routeWaypoints;
    private NavigationStateName? _lastState;

    public SimulationRecorder(string testName, string profile, string routeName, int seed, string startCluster, IReadOnlyList<Waypoint> routeWaypoints)
    {
        _metadata = new SimulationMetadata(
            testName, profile, routeName, seed, startCluster,
            DateTimeOffset.UtcNow, 0, 0, []);
        _routeWaypoints = routeWaypoints;
    }

    public void AddObstacles(IEnumerable<Obstacles.Obstacle> obstacles)
    {
        foreach (var o in obstacles)
            _obstacles.Add(o.ToRecorded());
    }

    public void RecordFrame(int tick, double timeMs, string clusterId, Position position,
        NavigationStateName state, int waypointIndex, Position? targetPosition)
    {
        // Auto-log state transitions
        if (_lastState.HasValue && _lastState.Value != state)
        {
            Log(tick, timeMs, "Info", $"State: {_lastState.Value} → {state}");
        }
        _lastState = state;

        _frames.Add(new SimulationFrame(tick, timeMs, clusterId, position, state, waypointIndex, targetPosition));
    }

    public void RecordEvent(int tick, double timeMs, string type, Dictionary<string, object?>? data = null)
    {
        _events.Add(new SimulationEvent(tick, timeMs, type, data ?? []));
        Log(tick, timeMs, "Info", $"Event: {type}");
    }

    public void RecordObstacleHit(int tick, double timeMs, string clusterId, Position position, Vector2 direction)
    {
        _obstacleHits.Add(new RecordedObstacleHit(tick, timeMs, clusterId, position, direction.X, direction.Y));
        Log(tick, timeMs, "Info", $"Obstacle hit at {position} heading ({direction.X:F1}, {direction.Y:F1})");
    }

    public void Log(int tick, double timeMs, string level, string message)
    {
        _logs.Add(new SimulationLog(tick, timeMs, level, message));
    }

    public SimulationRecording ToRecording()
    {
        var lastFrame = _frames.Count > 0 ? _frames[^1] : null;
        var total = (double)_frames.Count;
        var stateDistribution = _frames
            .GroupBy(f => f.State)
            .ToDictionary(
                g => char.ToLowerInvariant(g.Key.ToString()[0]) + g.Key.ToString()[1..],
                g => total > 0 ? Math.Round(g.Count() / total * 100, 1) : 0);

        var metadata = _metadata with
        {
            TotalTicks = _frames.Count,
            DurationMs = lastFrame?.TimeMs ?? 0,
            StateDistribution = stateDistribution
        };
        return new SimulationRecording(metadata, _frames, _events, _logs, _routeWaypoints, _obstacleHits, _obstacles);
    }
}
