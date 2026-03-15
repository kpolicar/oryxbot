using OryxBot.WorldGraph.Models;

namespace OryxBot.WorldGraph;

public class Pathfinder(IWorldGraphProvider graphProvider) : IPathfinder
{
    public ClusterPath? FindPath(string fromClusterId, string toClusterId) =>
        FindPath(fromClusterId, toClusterId, new PathOptions());

    public ClusterPath? FindPath(string fromClusterId, string toClusterId, PathOptions options)
    {
        var graph = graphProvider.GetWorldGraph();

        if (!graph.Clusters.ContainsKey(fromClusterId) || !graph.Clusters.ContainsKey(toClusterId))
            return null;

        if (fromClusterId == toClusterId)
            return new ClusterPath([], 0, 0f);

        var openSet = new PriorityQueue<string, float>();
        var cameFrom = new Dictionary<string, (string PreviousClusterId, ClusterExit Exit)>();
        var gScore = new Dictionary<string, float> { [fromClusterId] = 0f };

        openSet.Enqueue(fromClusterId, 0f);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == toClusterId)
                return ReconstructPath(cameFrom, fromClusterId, toClusterId);

            var currentG = gScore[current];

            foreach (var exit in graph.GetExits(current))
            {
                var neighbor = exit.TargetClusterId;

                if (ShouldAvoid(neighbor, graph, options))
                    continue;

                var tentativeG = currentG + 1f;

                if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = (current, exit);
                    gScore[neighbor] = tentativeG;
                    openSet.Enqueue(neighbor, tentativeG);
                }
            }
        }

        return null;
    }

    private static bool ShouldAvoid(string clusterId, Models.WorldGraph graph, PathOptions options)
    {
        if (options.AvoidClusters?.Contains(clusterId) == true)
            return true;

        var cluster = graph.GetCluster(clusterId);
        if (cluster is null)
            return true;

        if (options.MaxTier.HasValue && cluster.Tier > options.MaxTier.Value)
            return true;

        if (options.AvoidPvpZones && cluster.Type != ClusterType.City && cluster.Tier >= 5)
            return true;

        return false;
    }

    private static ClusterPath ReconstructPath(
        Dictionary<string, (string PreviousClusterId, ClusterExit Exit)> cameFrom,
        string from,
        string to)
    {
        var hops = new List<ClusterHop>();
        var current = to;

        while (current != from)
        {
            var (prev, exit) = cameFrom[current];
            hops.Add(new ClusterHop(prev, exit));
            current = prev;
        }

        hops.Reverse();
        hops.Add(new ClusterHop(to, null));

        return new ClusterPath(hops, hops.Count, hops.Count - 1);
    }
}
