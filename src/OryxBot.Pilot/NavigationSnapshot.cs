namespace OryxBot.Pilot;

public record NavigationSnapshot(
    NavigationStateName StateName,
    NavigationDecision? LastDecision,
    int WaypointIndex,
    float PercentComplete,
    string? CurrentCluster,
    DateTimeOffset Timestamp);
