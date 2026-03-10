using OryxBot.WorldGraph.Models;

namespace OryxBot.WorldGraph;

/// <summary>
/// Stub — real implementation in Phase 1b when game data extraction is built.
/// </summary>
public class Pathfinder : IPathfinder
{
    public ClusterPath? FindPath(string fromClusterId, string toClusterId) =>
        throw new NotImplementedException("Pathfinder implementation is Phase 1b");

    public ClusterPath? FindPath(string fromClusterId, string toClusterId, PathOptions options) =>
        throw new NotImplementedException("Pathfinder implementation is Phase 1b");
}
