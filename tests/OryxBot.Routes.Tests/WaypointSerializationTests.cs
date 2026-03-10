using System.Text.Json;
using FluentAssertions;
using OryxBot.Routes.Models;

namespace OryxBot.Routes.Tests;

public class WaypointSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void MoveWaypoint_SerializesAndDeserializes()
    {
        Waypoint original = new MoveWaypoint(1.5f, 2.5f);

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Waypoint>(json, JsonOptions);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void PortalWaypoint_SerializesAndDeserializes()
    {
        Waypoint original = new PortalWaypoint("TestCluster");

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Waypoint>(json, JsonOptions);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void MarkerWaypoint_SerializesAndDeserializes()
    {
        Waypoint original = new MarkerWaypoint("checkpoint1");

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Waypoint>(json, JsonOptions);

        deserialized.Should().Be(original);
    }

    [Fact]
    public void FloatParsing_InvariantCulture()
    {
        Waypoint original = new MoveWaypoint(1234.5678f, 9876.5432f);

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Waypoint>(json, JsonOptions);

        deserialized.Should().BeOfType<MoveWaypoint>()
            .Which.Should().BeEquivalentTo(new { X = 1234.5678f, Y = 9876.5432f },
                opt => opt.Using<float>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.01f))
                          .WhenTypeIs<float>());
    }
}
