using FluentAssertions;
using OryxBot.GameDataExtractor.Extraction;

namespace OryxBot.GameDataExtractor.Tests;

public class ClusterExtractorTests
{
    private readonly ClusterExtractor _extractor;

    public ClusterExtractorTests()
    {
        _extractor = new ClusterExtractor();
    }

    [Fact]
    public void ExtractFromXml_ValidCluster_ParsesAllFields()
    {
        // Arrange
        var xml = File.ReadAllText("Fixtures/sample_cluster_4205.xml");

        // Act
        var result = _extractor.ExtractFromXml(xml, "4205_WRL_MN_AUTO_T5_KPR_ROY.cluster.xml");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("4205");
        result.DisplayName.Should().Be("@CLUSTER_4205");
        result.Type.Should().Be("WORLD");
        result.Biome.Should().Be("MN");
        result.Tier.Should().Be("T5");
        result.Faction.Should().Be("KPR");
        result.Zone.Should().Be("ROY");

        result.Exits.Should().HaveCount(2);
        
        var northExit = result.Exits.First(e => e.ExitId == "exit_north");
        northExit.TargetClusterId.Should().Be("4206");
        northExit.TargetExitId.Should().Be("exit_south");
        northExit.PositionX.Should().Be(128.5f);
        northExit.PositionY.Should().Be(0.0f);
    }

    [Fact]
    public void ExtractFromXml_MalformedXml_ReturnsNull()
    {
        // Arrange
        var xml = "<notacluster></notacluster>";

        // Act
        var result = _extractor.ExtractFromXml(xml);

        // Assert
        result.Should().BeNull();
    }
}
