using OryxBot.Core.Models;

namespace OryxBot.WorldGraph.Models;

public record ClusterExit(
    string TargetClusterId,
    Position PositionInCluster,
    Position? ArrivalPosition);
