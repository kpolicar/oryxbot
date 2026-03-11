namespace OryxBot.GameDataExtractor.Models;

public record WorldGraphOutput
{
    public required Dictionary<string, ClusterNode> Clusters { get; init; }
    public required List<Edge> Edges { get; init; }
    public int TotalClusters => Clusters.Count;
    public int TotalEdges => Edges.Count;
}

public record ClusterNode
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Type { get; init; }
    public string? Tier { get; init; }
    public string? Biome { get; init; }
    public string? Zone { get; init; }
    public float? WorldX { get; init; }
    public float? WorldY { get; init; }
    public string? InternalName { get; init; }
}

public record Edge
{
    public required string FromClusterId { get; init; }
    public required string FromExitId { get; init; }
    public required float FromX { get; init; }
    public required float FromY { get; init; }
    public required string ToClusterId { get; init; }
    public required string ToExitId { get; init; }
    public required float ToX { get; init; }
    public required float ToY { get; init; }
}
