using System.Numerics;
using OryxBot.Core.Models;

namespace OryxBot.Core.Abstractions;

public interface IObstacleMap
{
    void RecordHit(string clusterId, Position position, Vector2 blockedDirection, DateTimeOffset timestamp);
    IReadOnlyList<ObstacleHit> GetHits(string clusterId);
    IReadOnlyList<ObstacleHit> GetAllHits();
    bool IsDirectionBlocked(string clusterId, Position from, Vector2 direction, float lookAhead = 5f);
}

public record ObstacleHit(string ClusterId, Position Position, Vector2 BlockedDirection, DateTimeOffset Timestamp);
