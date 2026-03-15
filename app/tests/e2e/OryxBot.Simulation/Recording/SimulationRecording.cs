using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Routes.Models;

namespace OryxBot.Simulation.Recording;

public record SimulationRecording(
    SimulationMetadata Metadata,
    IReadOnlyList<SimulationFrame> Frames,
    IReadOnlyList<SimulationEvent> Events,
    IReadOnlyList<SimulationLog> Logs,
    IReadOnlyList<Waypoint> RouteWaypoints);

public record SimulationMetadata(
    string TestName,
    string Profile,
    string RouteName,
    int Seed,
    string StartCluster,
    DateTimeOffset RecordedAt,
    int TotalTicks,
    double DurationMs);

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
