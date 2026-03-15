using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public class AdversarialProfile : ISimulationProfile
{
    public string Name => "Adversarial";

    public Vector2 ApplyDirectionDrift(Vector2 dir, Random rng)
    {
        var angle = (float)(rng.NextDouble() * 2 - 1) * MathF.PI / 7.2f; // ±25°
        return RotateVector(dir, angle);
    }

    public TimeSpan NextPacketInterval(Random rng) =>
        TimeSpan.FromMilliseconds(500 + rng.NextDouble() * 1000); // 500–1500ms

    public float CalculateSpeed(float current, Random rng) =>
        3.0f + (float)rng.NextDouble() * 9.0f; // 3.0–12.0

    public TimeSpan ClusterChangeDelay(Random rng) =>
        TimeSpan.FromSeconds(10 + rng.NextDouble() * 10); // 10–20s

    public int StalePacketsAfterClusterChange => 5;

    private static Vector2 RotateVector(Vector2 v, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }
}
