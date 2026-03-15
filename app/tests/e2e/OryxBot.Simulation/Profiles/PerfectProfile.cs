using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public class PerfectProfile : ISimulationProfile
{
    public string Name => "Perfect";
    public Vector2 ApplyDirectionDrift(Vector2 dir, Random rng) => dir;
    public TimeSpan NextPacketInterval(Random rng) => TimeSpan.FromMilliseconds(200);
    public float CalculateSpeed(float current, Random rng) => 7.0f;
    public TimeSpan ClusterChangeDelay(Random rng) => TimeSpan.FromSeconds(8);
    public int StalePacketsAfterClusterChange => 5;
}
