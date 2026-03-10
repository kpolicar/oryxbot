namespace OryxBot.WorldGraph.Models;

public record PathOptions(
    bool AvoidPvpZones = false,
    int? MaxTier = null,
    HashSet<string>? AvoidClusters = null);
