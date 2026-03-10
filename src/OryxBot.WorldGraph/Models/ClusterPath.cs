namespace OryxBot.WorldGraph.Models;

public record ClusterPath(
    IReadOnlyList<ClusterHop> Hops,
    int TotalClusters,
    float EstimatedTravelTime);
