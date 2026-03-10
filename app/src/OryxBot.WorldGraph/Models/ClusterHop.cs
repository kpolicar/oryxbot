namespace OryxBot.WorldGraph.Models;

public record ClusterHop(
    string ClusterId,
    ClusterExit? ExitToNext);
