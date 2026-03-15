using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public class ConstantDriftProfile(float driftDegrees = 2f) : ISimulationProfile
{
    private readonly float _driftRadians = driftDegrees * MathF.PI / 180f;

    public string Name => "ConstantDrift";

    public Vector2 ApplyDirectionDrift(Vector2 dir, Random rng)
    {
        var cos = MathF.Cos(_driftRadians);
        var sin = MathF.Sin(_driftRadians);
        return new Vector2(dir.X * cos - dir.Y * sin, dir.X * sin + dir.Y * cos);
    }

    public TimeSpan NextPacketInterval(Random rng) => TimeSpan.FromMilliseconds(200);
    public float CalculateSpeed(float current, Random rng) => 7.0f;
    public TimeSpan ClusterChangeDelay(Random rng) => TimeSpan.FromSeconds(8);
    public int StalePacketsAfterClusterChange => 5;
}
