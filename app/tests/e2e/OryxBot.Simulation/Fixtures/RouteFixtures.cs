using OryxBot.Routes.Models;

namespace OryxBot.Simulation.Fixtures;

public static class RouteFixtures
{
    public static Route StraightLine(float length = 100f, int waypoints = 10)
    {
        var spacing = length / waypoints;
        var wps = Enumerable.Range(0, waypoints)
            .Select(i => (Waypoint)new MoveWaypoint(i * spacing, 0))
            .ToList();
        return new Route(new RouteMetadata("StraightLine", DateTimeOffset.UtcNow), wps);
    }

    public static Route ZigZag(int segments = 5, float segmentLength = 20f)
    {
        var wps = new List<Waypoint>();
        for (var i = 0; i < segments; i++)
        {
            var x = i * segmentLength;
            var y = (i % 2 == 0) ? 0f : segmentLength;
            wps.Add(new MoveWaypoint(x, y));
        }
        return new Route(new RouteMetadata("ZigZag", DateTimeOffset.UtcNow), wps);
    }

    public static Route Curve(float radius = 50f, float angleDegrees = 180f, int points = 20)
    {
        var wps = new List<Waypoint>();
        var totalRadians = angleDegrees * MathF.PI / 180f;
        for (var i = 0; i < points; i++)
        {
            var angle = totalRadians * i / (points - 1);
            wps.Add(new MoveWaypoint(
                radius * MathF.Cos(angle),
                radius * MathF.Sin(angle)));
        }
        return new Route(new RouteMetadata("Curve", DateTimeOffset.UtcNow), wps);
    }

    public static Route MultiCluster()
    {
        // Real route: 0208 (Stumprot Swamp) → 0218 (Slithertail Marsh) → 0207 (Nightcreak Marsh)
        // Exit/entrance positions from albionLocations.json
        // Uses interpolated waypoints every ~20 units for smooth navigation
        var wps = new List<Waypoint>();

        // Cluster 0208: center (0,0) → exit toward 0218 at (-373.5, 59.5)
        AddInterpolated(wps, 0, 0, -373.5f, 59.5f, spacing: 20f);
        wps.Add(new PortalWaypoint("0218"));

        // Cluster 0218: entrance from 0208 (373.5, -69.5) → exit toward 0207 (170.5, 380.5)
        AddInterpolated(wps, 373.5f, -69.5f, 170.5f, 380.5f, spacing: 20f);
        wps.Add(new PortalWaypoint("0207"));

        // Cluster 0207: entrance from 0218 (149.5, -380.5) → center (0, 0)
        AddInterpolated(wps, 149.5f, -380.5f, 0, 0, spacing: 20f);

        return new Route(new RouteMetadata("MultiCluster", DateTimeOffset.UtcNow), wps);
    }

    private static void AddInterpolated(List<Waypoint> wps, float x0, float y0, float x1, float y1, float spacing)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        var steps = Math.Max(1, (int)(dist / spacing));
        for (var i = 0; i <= steps; i++)
        {
            var t = (float)i / steps;
            wps.Add(new MoveWaypoint(x0 + dx * t, y0 + dy * t));
        }
    }

    public static Route NarrowCorridor(float width = 8f, float length = 100f, int waypoints = 20)
    {
        var spacing = length / waypoints;
        var wps = Enumerable.Range(0, waypoints)
            .Select(i => (Waypoint)new MoveWaypoint(i * spacing, width / 2f))
            .ToList();
        return new Route(new RouteMetadata("NarrowCorridor", DateTimeOffset.UtcNow), wps);
    }

    public static Route UTurn(float length = 40f)
    {
        var wps = new List<Waypoint>
        {
            new MoveWaypoint(0, 0),
            new MoveWaypoint(length, 0),
            new MoveWaypoint(length, 10),
            new MoveWaypoint(0, 10),
        };
        return new Route(new RouteMetadata("UTurn", DateTimeOffset.UtcNow), wps);
    }

    public static Route LongRoute(int waypoints = 1000, float spacing = 1f)
    {
        var rng = new Random(42);
        var x = 0f;
        var y = 0f;
        var wps = new List<Waypoint>();
        for (var i = 0; i < waypoints; i++)
        {
            x += spacing;
            y += (float)(rng.NextDouble() * 2 - 1) * 0.5f;
            wps.Add(new MoveWaypoint(x, y));
        }
        return new Route(new RouteMetadata("LongRoute", DateTimeOffset.UtcNow), wps);
    }

    public static Route SparseWaypoints(float spacing = 20f, int count = 10)
    {
        var wps = Enumerable.Range(0, count)
            .Select(i => (Waypoint)new MoveWaypoint(i * spacing, 0))
            .ToList();
        return new Route(new RouteMetadata("SparseWaypoints", DateTimeOffset.UtcNow), wps);
    }
}
