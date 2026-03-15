using FluentAssertions;
using OryxBot.GameDataExtractor.Extraction;
using OryxBot.GameDataExtractor.Models;

namespace OryxBot.GameDataExtractor.Tests;

public class WorldGraphBuilderTests
{
    private readonly WorldGraphBuilder _builder;

    public WorldGraphBuilderTests()
    {
        _builder = new WorldGraphBuilder();
    }

    [Fact]
    public void Build_TwoClusters_WithExitsToBoth_CreatesEdges()
    {
        // Arrange
        var clusters = new List<ClusterDefinition>
        {
            new()
            {
                Id = "4205",
                DisplayName = "Test 1",
                Type = "WORLD",
                Exits = new List<ExitDefinition>
                {
                    new()
                    {
                        ExitId = "exit_north",
                        TargetClusterId = "4206",
                        TargetExitId = "exit_south",
                        PositionX = 128.5f,
                        PositionY = 0f
                    }
                }
            },
            new()
            {
                Id = "4206",
                DisplayName = "Test 2",
                Type = "WORLD",
                Exits = new List<ExitDefinition>
                {
                    new()
                    {
                        ExitId = "exit_south",
                        TargetClusterId = "4205",
                        TargetExitId = "exit_north",
                        PositionX = 128.5f,
                        PositionY = 255f
                    }
                }
            }
        };

        // Act
        var result = _builder.Build(clusters);

        // Assert
        result.TotalClusters.Should().Be(2);
        
        // Edge is deduplicated so we only have 1 edge between these two nodes
        result.TotalEdges.Should().Be(1);
        
        var edge = result.Edges.Single();
        edge.FromClusterId.Should().BeOneOf("4205", "4206");
        edge.ToClusterId.Should().BeOneOf("4205", "4206");
        edge.FromClusterId.Should().NotBe(edge.ToClusterId);
    }

    [Fact]
    public void Build_ExitToNonexistentCluster_LogsWarningAndSkipsEdge()
    {
        // Arrange
        var clusters = new List<ClusterDefinition>
        {
            new()
            {
                Id = "4205",
                DisplayName = "Test 1",
                Type = "WORLD",
                Exits = new List<ExitDefinition>
                {
                    new()
                    {
                        ExitId = "exit_north",
                        TargetClusterId = "9999", // Does not exist
                        TargetExitId = "exit_south",
                        PositionX = 128.5f,
                        PositionY = 0f
                    }
                }
            }
        };

        // Act
        var result = _builder.Build(clusters);

        // Assert
        result.TotalClusters.Should().Be(1);
        result.TotalEdges.Should().Be(0); // Cannot link to a node that isn't in cluster list
    }
}
