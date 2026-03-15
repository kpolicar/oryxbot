using System.Numerics;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;

namespace OryxBot.GameState;

public class ObstacleMap : IObstacleMap
{
    private readonly Dictionary<string, List<ObstacleHit>> _hitsByCluster = [];

    public void RecordHit(string clusterId, Position position, Vector2 blockedDirection, DateTimeOffset timestamp)
    {
        if (!_hitsByCluster.TryGetValue(clusterId, out var hits))
        {
            hits = [];
            _hitsByCluster[clusterId] = hits;
        }

        // Avoid duplicate hits at nearly the same position
        var dominated = hits.Any(h =>
            Position.Distance(h.Position, position) < 2f &&
            Vector2.Dot(Vector2.Normalize(h.BlockedDirection), Vector2.Normalize(blockedDirection)) > 0.8f);

        if (!dominated)
            hits.Add(new ObstacleHit(clusterId, position, blockedDirection, timestamp));
    }

    public IReadOnlyList<ObstacleHit> GetHits(string clusterId) =>
        _hitsByCluster.TryGetValue(clusterId, out var hits) ? hits : [];

    public IReadOnlyList<ObstacleHit> GetAllHits() =>
        _hitsByCluster.Values.SelectMany(h => h).ToList();

    public bool IsDirectionBlocked(string clusterId, Position from, Vector2 direction, float lookAhead = 5f)
    {
        if (!_hitsByCluster.TryGetValue(clusterId, out var hits))
            return false;

        var dirNorm = Vector2.Normalize(direction);

        foreach (var hit in hits)
        {
            var toHit = hit.Position - from;
            var dist = toHit.Length();
            if (dist > lookAhead) continue;

            // Check if the hit is roughly in the direction we want to go
            if (dist > 0.01f)
            {
                var dot = Vector2.Dot(toHit / dist, dirNorm);
                if (dot > 0.5f) // within ~60° cone
                    return true;
            }
        }

        return false;
    }
}
