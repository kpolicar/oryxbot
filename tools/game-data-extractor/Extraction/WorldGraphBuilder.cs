using OryxBot.GameDataExtractor.Models;

namespace OryxBot.GameDataExtractor.Extraction;

public class WorldGraphBuilder
{
    public WorldGraphOutput Build(IReadOnlyList<ClusterDefinition> clusters)
    {
        var clusterDict = clusters
            .GroupBy(c => c.Id)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var cluster = g.First(); // Take the first cluster if there are duplicates
                    return new ClusterNode
                    {
                        Id = cluster.Id,
                        DisplayName = cluster.DisplayName,
                        Type = cluster.Type,
                        Tier = cluster.Tier,
                        Biome = cluster.Biome,
                        Zone = cluster.Zone,
                        WorldX = cluster.WorldX,
                        WorldY = cluster.WorldY,
                        InternalName = cluster.InternalName
                    };
                }
            );
        var edges = new List<Edge>();
        var processedEdges = new HashSet<string>();

        foreach (var cluster in clusters)
        {
            foreach (var exit in cluster.Exits)
            {
                if (!clusterDict.ContainsKey(exit.TargetClusterId))
                {
                    Console.WriteLine($"Warning: Exit {exit.ExitId} in cluster {cluster.Id} points to nonexistent cluster {exit.TargetClusterId}");
                    continue;
                }

                // Create a canonical edge signature to deduplicate bidirectional edges
                var sortedPair = string.Compare(cluster.Id, exit.TargetClusterId, StringComparison.Ordinal) < 0
                    ? $"{cluster.Id}:{exit.ExitId}-{exit.TargetClusterId}:{exit.TargetExitId}"
                    : $"{exit.TargetClusterId}:{exit.TargetExitId}-{cluster.Id}:{exit.ExitId}";

                if (processedEdges.Add(sortedPair))
                {
                    edges.Add(new Edge
                    {
                        FromClusterId = cluster.Id,
                        FromExitId = exit.ExitId,
                        FromX = exit.PositionX,
                        FromY = exit.PositionY,
                        ToClusterId = exit.TargetClusterId,
                        ToExitId = exit.TargetExitId,
                        // Assuming target position is 0,0 for now as it's not in the source exit definition directly
                        // A more complete implementation might correlate by finding the target cluster's exit
                        ToX = 0,
                        ToY = 0
                    });
                }
            }
        }

        // Second pass to resolve target coordinates if the target exit exists
        foreach (var edge in edges)
        {
            var targetCluster = clusters.FirstOrDefault(c => c.Id == edge.ToClusterId);
            var targetExit = targetCluster?.Exits.FirstOrDefault(e => e.ExitId == edge.ToExitId);
            if (targetExit != null)
            {
                // ToX and ToY shouldn't actually be modified this way unless Edge is mutable, but we used records with 'init'
                // Let's recreate edge or just ignore exact coordinates for target if the record is read-only
                // Wait, record properties are init-only. I'll stick to a simpler implementation where we leave ToX, ToY as 0, 
                // but actually albion map data uses the exit's position in the target map.
                // It's acceptable for MVP to just omit ToX/ToY or set them to 0 if not available.
            }
        }
        
        // Actually let's just make a new list of edges with the updated coordinates
        var enrichedEdges = new List<Edge>(edges.Count);
        foreach (var edge in edges)
        {
            var targetCluster = clusters.FirstOrDefault(c => c.Id == edge.ToClusterId);
            var targetExit = targetCluster?.Exits.FirstOrDefault(e => e.ExitId == edge.ToExitId);
            
            float targetX = 0;
            float targetY = 0;
            if (targetExit != null)
            {
                targetX = targetExit.PositionX;
                targetY = targetExit.PositionY;
            }

            enrichedEdges.Add(new Edge
            {
                FromClusterId = edge.FromClusterId,
                FromExitId = edge.FromExitId,
                FromX = edge.FromX,
                FromY = edge.FromY,
                ToClusterId = edge.ToClusterId,
                ToExitId = edge.ToExitId,
                ToX = targetX,
                ToY = targetY
            });
        }

        return new WorldGraphOutput
        {
            Clusters = clusterDict,
            Edges = enrichedEdges
        };
    }
}
