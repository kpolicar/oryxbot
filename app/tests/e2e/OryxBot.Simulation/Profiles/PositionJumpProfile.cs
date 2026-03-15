using System.Numerics;

namespace OryxBot.Simulation.Profiles;

public class PositionJumpProfile(int jumpAtTick = 50, float jumpDistance = 15f) : ISimulationProfile
{
    private int _tickCount;

    public string Name => "PositionJump";

    public Vector2 ApplyDirectionDrift(Vector2 dir, Random rng)
    {
        _tickCount++;
        if (_tickCount == jumpAtTick)
        {
            // Apply a large perpendicular offset
            var perpendicular = new Vector2(-dir.Y, dir.X);
            return Vector2.Normalize(dir + perpendicular * jumpDistance);
        }
        return dir;
    }

    public TimeSpan NextPacketInterval(Random rng) => TimeSpan.FromMilliseconds(200);
    public float CalculateSpeed(float current, Random rng) => 7.0f;
    public TimeSpan ClusterChangeDelay(Random rng) => TimeSpan.FromSeconds(8);
    public int StalePacketsAfterClusterChange => 5;
}
