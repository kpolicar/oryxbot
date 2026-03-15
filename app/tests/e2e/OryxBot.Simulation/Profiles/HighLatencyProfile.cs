using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public class HighLatencyProfile : ISimulationProfile
{
    public string Name => "HighLatency";

    public Vector2 ApplyDirectionDrift(Vector2 dir, Random rng)
    {
        var angle = (float)(rng.NextDouble() * 2 - 1) * MathF.PI / 12f; // ±15°
        return RotateVector(dir, angle);
    }

    public TimeSpan NextPacketInterval(Random rng) =>
        TimeSpan.FromMilliseconds(300 + rng.NextDouble() * 500); // 300–800ms

    public float CalculateSpeed(float current, Random rng) =>
        5.0f + (float)rng.NextDouble() * 4.0f; // 5.0–9.0

    public TimeSpan ClusterChangeDelay(Random rng) =>
        TimeSpan.FromSeconds(8 + rng.NextDouble() * 7); // 8–15s

    public int StalePacketsAfterClusterChange => 5;

    private static Vector2 RotateVector(Vector2 v, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }
}
