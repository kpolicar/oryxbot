namespace OryxBot.Routes.Models;

public record Route(RouteMetadata Metadata, IReadOnlyList<Waypoint> Waypoints);
