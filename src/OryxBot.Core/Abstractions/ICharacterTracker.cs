using OryxBot.Core.Models;

namespace OryxBot.Core.Abstractions;

public interface ICharacterTracker
{
    Position Position { get; }
    Position PredictedPosition { get; }
    string CurrentCluster { get; }
    bool IsMoving { get; }
    bool RecentlyChangedCluster { get; }
    bool IsDead { get; }
    bool IsDisconnected { get; }
    float Speed { get; }
    DateTimeOffset LastPositionUpdate { get; }
}
