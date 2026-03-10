using OryxBot.Core.Models;

namespace OryxBot.WorldGraph.Models;

public record Cluster(
    string Id,
    string DisplayName,
    ClusterType Type,
    int Tier,
    string Biome,
    IReadOnlyList<ClusterExit> Exits);
