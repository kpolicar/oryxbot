namespace OryxBot.WorldGraph.Models;

public class WorldGraph
{
    public IReadOnlyDictionary<string, Cluster> Clusters { get; }
    public IReadOnlyList<(string From, string To, ClusterExit Exit)> Edges { get; }

    public WorldGraph(
        IReadOnlyDictionary<string, Cluster> clusters,
        IReadOnlyList<(string From, string To, ClusterExit Exit)> edges)
    {
        Clusters = clusters;
        Edges = edges;
    }

    public IReadOnlyList<ClusterExit> GetExits(string clusterId) =>
        Clusters.TryGetValue(clusterId, out var cluster) ? cluster.Exits : [];

    public Cluster? GetCluster(string id) =>
        Clusters.TryGetValue(id, out var cluster) ? cluster : null;
}
