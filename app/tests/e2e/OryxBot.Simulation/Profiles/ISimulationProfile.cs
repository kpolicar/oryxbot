using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public interface ISimulationProfile
{
    string Name { get; }
    Vector2 ApplyDirectionDrift(Vector2 intendedDirection, Random rng);
    TimeSpan NextPacketInterval(Random rng);
    float CalculateSpeed(float currentSpeed, Random rng);
    TimeSpan ClusterChangeDelay(Random rng);
    int StalePacketsAfterClusterChange { get; }
}
