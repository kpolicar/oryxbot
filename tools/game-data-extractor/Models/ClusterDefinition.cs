namespace OryxBot.GameDataExtractor.Models;

public record ClusterDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Type { get; init; }
    public string? Biome { get; init; }
    public string? Tier { get; init; }
    public string? Faction { get; init; }
    public string? Zone { get; init; }
    public float? WorldX { get; init; }
    public float? WorldY { get; init; }
    public string? InternalName { get; init; }
    public required List<ExitDefinition> Exits { get; init; }
}

public record ExitDefinition
{
    public required string ExitId { get; init; }
    public required string TargetClusterId { get; init; }
    public required string TargetExitId { get; init; }
    public required float PositionX { get; init; }
    public required float PositionY { get; init; }
}
