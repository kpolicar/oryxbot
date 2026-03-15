using FluentAssertions;
using NSubstitute;
using OryxBot.Core.Models;
using OryxBot.WorldGraph.Models;

namespace OryxBot.WorldGraph.Tests;

public class PathfinderTests
{
    private static readonly Position DefaultPos = new(0, 0);

    private static Cluster MakeCluster(string id, int tier = 3, ClusterType type = ClusterType.World, params ClusterExit[] exits) =>
        new(id, id, type, tier, "Forest", exits);

    private static ClusterExit Exit(string target) =>
        new(target, DefaultPos, DefaultPos);

    private static (Pathfinder Pathfinder, Models.WorldGraph Graph) BuildGraph(params Cluster[] clusters)
    {
        var dict = clusters.ToDictionary(c => c.Id);
        var edges = clusters
            .SelectMany(c => c.Exits.Select(e => (c.Id, e.TargetClusterId, e)))
            .ToList();
        var graph = new Models.WorldGraph(dict, edges);
        var provider = Substitute.For<IWorldGraphProvider>();
        provider.GetWorldGraph().Returns(graph);
        return (new Pathfinder(provider), graph);
    }

    [Fact]
    public void FindPath_DirectNeighbors_ReturnsSingleHop()
    {
        var (pf, _) = BuildGraph(
            MakeCluster("A", exits: Exit("B")),
            MakeCluster("B", exits: Exit("A")));

        var path = pf.FindPath("A", "B");

        path.Should().NotBeNull();
        path!.Hops.Should().HaveCount(2);
        path.Hops[0].ClusterId.Should().Be("A");
        path.Hops[0].ExitToNext!.TargetClusterId.Should().Be("B");
        path.Hops[1].ClusterId.Should().Be("B");
        path.Hops[1].ExitToNext.Should().BeNull();
    }

    [Fact]
    public void FindPath_MultipleClusters_ReturnsShortestPath()
    {
        // A->B->D (2 hops) vs A->C->E->D (3 hops)
        var (pf, _) = BuildGraph(
            MakeCluster("A", exits: [Exit("B"), Exit("C")]),
            MakeCluster("B", exits: [Exit("A"), Exit("D")]),
            MakeCluster("C", exits: [Exit("A"), Exit("E")]),
            MakeCluster("D", exits: [Exit("B"), Exit("E")]),
            MakeCluster("E", exits: [Exit("C"), Exit("D")]));

        var path = pf.FindPath("A", "D");

        path.Should().NotBeNull();
        path!.Hops.Select(h => h.ClusterId).Should().Equal("A", "B", "D");
    }

    [Fact]
    public void FindPath_NoPathExists_ReturnsNull()
    {
        var (pf, _) = BuildGraph(
            MakeCluster("A"),
            MakeCluster("B"));

        var path = pf.FindPath("A", "B");

        path.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithAvoidClusters_RoutesAround()
    {
        // A->B->D (short but B avoided), A->C->D (longer but allowed)
        var (pf, _) = BuildGraph(
            MakeCluster("A", exits: [Exit("B"), Exit("C")]),
            MakeCluster("B", exits: [Exit("A"), Exit("D")]),
            MakeCluster("C", exits: [Exit("A"), Exit("D")]),
            MakeCluster("D", exits: [Exit("B"), Exit("C")]));

        var path = pf.FindPath("A", "D", new PathOptions(AvoidClusters: ["B"]));

        path.Should().NotBeNull();
        path!.Hops.Select(h => h.ClusterId).Should().Equal("A", "C", "D");
    }

    [Fact]
    public void FindPath_SameCluster_ReturnsEmptyPath()
    {
        var (pf, _) = BuildGraph(MakeCluster("A"));

        var path = pf.FindPath("A", "A");

        path.Should().NotBeNull();
        path!.Hops.Should().BeEmpty();
        path.TotalClusters.Should().Be(0);
    }

    [Fact]
    public void FindPath_UnknownCluster_ReturnsNull()
    {
        var (pf, _) = BuildGraph(MakeCluster("A"));

        pf.FindPath("A", "UNKNOWN").Should().BeNull();
        pf.FindPath("UNKNOWN", "A").Should().BeNull();
    }

    [Fact]
    public void FindPath_WithMaxTier_SkipsHighTierClusters()
    {
        // A->B(T7)->D vs A->C(T4)->D, MaxTier=5 blocks B
        var (pf, _) = BuildGraph(
            MakeCluster("A", tier: 3, exits: [Exit("B"), Exit("C")]),
            MakeCluster("B", tier: 7, exits: [Exit("A"), Exit("D")]),
            MakeCluster("C", tier: 4, exits: [Exit("A"), Exit("D")]),
            MakeCluster("D", tier: 4, exits: [Exit("B"), Exit("C")]));

        var path = pf.FindPath("A", "D", new PathOptions(MaxTier: 5));

        path.Should().NotBeNull();
        path!.Hops.Select(h => h.ClusterId).Should().Equal("A", "C", "D");
    }

    [Fact]
    public void FindPath_LargeGraph_CompletesQuickly()
    {
        // Generate 1700 clusters in a chain with random cross-links
        var rng = new Random(42);
        var clusters = new List<Cluster>();
        for (int i = 0; i < 1700; i++)
        {
            var exits = new List<ClusterExit>();
            if (i > 0) exits.Add(Exit((i - 1).ToString()));
            if (i < 1699) exits.Add(Exit((i + 1).ToString()));
            // Random cross-link
            if (i > 10)
            {
                var target = rng.Next(0, i - 1);
                exits.Add(Exit(target.ToString()));
            }
            clusters.Add(MakeCluster(i.ToString(), exits: exits.ToArray()));
        }

        var (pf, _) = BuildGraph(clusters.ToArray());

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var path = pf.FindPath("0", "1699");
        sw.Stop();

        path.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }
}
