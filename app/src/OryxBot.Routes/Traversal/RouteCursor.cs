using OryxBot.Core.Models;
using OryxBot.Routes.Models;

namespace OryxBot.Routes.Traversal;

public sealed class RouteCursor
{
    private readonly IReadOnlyList<Waypoint> _waypoints;
    private int _index;

    public RouteCursor(IReadOnlyList<Waypoint> waypoints)
    {
        _waypoints = waypoints;
        _index = 0;
    }

    public Waypoint? Current => _index >= 0 && _index < _waypoints.Count ? _waypoints[_index] : null;
    public int Index => _index;
    public int Count => _waypoints.Count;
    public bool IsAtEnd => _index >= _waypoints.Count;
    public float PercentComplete => _waypoints.Count == 0 ? 100f : (float)_index / _waypoints.Count * 100f;

    public bool MoveNext()
    {
        if (_index >= _waypoints.Count)
            return false;
        _index++;
        return _index < _waypoints.Count;
    }

    public bool MovePrevious()
    {
        if (_index <= 0)
            return false;
        _index--;
        return true;
    }

    /// <summary>
    /// Skips ahead to the farthest waypoint that is still within maxDistance of currentPos,
    /// limited by maxSteps from the current index.
    /// </summary>
    public bool TrySkipAhead(Position currentPos, float maxDistance, int maxSteps)
    {
        var bestIndex = _index;
        var limit = Math.Min(_index + maxSteps, _waypoints.Count - 1);

        for (var i = _index + 1; i <= limit; i++)
        {
            if (_waypoints[i] is not MoveWaypoint move)
                break; // Don't skip past portal/marker waypoints

            var dist = Position.Distance(currentPos, new Position(move.X, move.Y));
            if (dist <= maxDistance)
                bestIndex = i;
            else
                break;
        }

        if (bestIndex > _index)
        {
            _index = bestIndex;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds the closest MoveWaypoint in a cluster segment and resumes from there.
    /// </summary>
    public bool ResumeFromCluster(string clusterAlias, Position approxPos)
    {
        // Find the portal waypoint for this cluster, then find the closest move waypoint after it
        int portalIndex = -1;
        for (var i = 0; i < _waypoints.Count; i++)
        {
            if (_waypoints[i] is PortalWaypoint portal &&
                string.Equals(portal.ClusterName, clusterAlias, StringComparison.OrdinalIgnoreCase))
            {
                portalIndex = i;
            }
        }

        // Find the closest move waypoint to approxPos, preferring those after the portal
        var searchStart = portalIndex >= 0 ? portalIndex : 0;
        var bestIndex = -1;
        var bestDist = float.MaxValue;

        for (var i = searchStart; i < _waypoints.Count; i++)
        {
            if (_waypoints[i] is MoveWaypoint move)
            {
                var dist = Position.Distance(approxPos, new Position(move.X, move.Y));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }

            // Stop searching if we hit the next portal (different cluster segment)
            if (i > searchStart && _waypoints[i] is PortalWaypoint)
                break;
        }

        if (bestIndex >= 0)
        {
            _index = bestIndex;
            return true;
        }

        return false;
    }

    public Waypoint? Peek(int offset)
    {
        var i = _index + offset;
        return i >= 0 && i < _waypoints.Count ? _waypoints[i] : null;
    }

    public void Reset() => _index = 0;

    public void SetIndex(int index) => _index = Math.Clamp(index, 0, _waypoints.Count);
}
