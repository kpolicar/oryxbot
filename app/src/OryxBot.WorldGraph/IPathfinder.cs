using OryxBot.WorldGraph.Models;

namespace OryxBot.WorldGraph;

public interface IPathfinder
{
    ClusterPath? FindPath(string fromClusterId, string toClusterId);
    ClusterPath? FindPath(string fromClusterId, string toClusterId, PathOptions options);
}
