using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Routes.Models;

namespace OryxBot.Simulation.Recording;

public record SimulationRecording(
    SimulationMetadata Metadata,
    IReadOnlyList<SimulationFrame> Frames,
    IReadOnlyList<SimulationEvent> Events,
    IReadOnlyList<SimulationLog> Logs,
    IReadOnlyList<Waypoint> RouteWaypoints,
    IReadOnlyList<RecordedObstacleHit> ObstacleHits,
    IReadOnlyList<RecordedObstacle> Obstacles);

public record SimulationMetadata(
    string TestName,
    string Profile,
    string RouteName,
    int Seed,
    string StartCluster,
    DateTimeOffset RecordedAt,
    int TotalTicks,
    double DurationMs,
    Dictionary<string, double> StateDistribution);

public record SimulationFrame(
    int Tick,
    double TimeMs,
    string ClusterId,
    Position Position,
    NavigationStateName State,
    int WaypointIndex,
    Position? TargetPosition);

public record SimulationEvent(
    int Tick,
    double TimeMs,
    string Type,
    Dictionary<string, object?> Data);

public record SimulationLog(
    int Tick,
    double TimeMs,
    string Level,
    string Message);

public record RecordedObstacleHit(
    int Tick,
    double TimeMs,
    string ClusterId,
    Position Position,
    float DirectionX,
    float DirectionY);

public record RecordedObstacle(
    string Type,
    float X1,
    float Y1,
    float X2,
    float Y2);
