using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public class RealisticProfile : ISimulationProfile
{
    public string Name => "Realistic";

    public Vector2 ApplyDirectionDrift(Vector2 dir, Random rng)
    {
        var angle = (float)(rng.NextDouble() * 2 - 1) * MathF.PI / 36f; // ±5°
        return RotateVector(dir, angle);
    }

    public TimeSpan NextPacketInterval(Random rng) =>
        TimeSpan.FromMilliseconds(150 + rng.NextDouble() * 200); // 150–350ms

    public float CalculateSpeed(float current, Random rng) =>
        6.5f + (float)rng.NextDouble(); // 6.5–7.5

    public TimeSpan ClusterChangeDelay(Random rng) =>
        TimeSpan.FromSeconds(7 + rng.NextDouble() * 3); // 7–10s

    public int StalePacketsAfterClusterChange => 5;

    private static Vector2 RotateVector(Vector2 v, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }
}
