using System.Text.Json.Serialization;

namespace OryxBot.Routes.Models;

[JsonConverter(typeof(WaypointJsonConverter))]
public abstract record Waypoint(string Type);

public record MoveWaypoint(float X, float Y) : Waypoint("move");

public record PortalWaypoint(string ClusterName) : Waypoint("portal");

public record MarkerWaypoint(string MarkerName) : Waypoint("marker");
