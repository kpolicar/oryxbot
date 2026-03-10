using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OryxBot.Routes.Configuration;
using OryxBot.Routes.Models;
using OryxBot.Routes.Storage;

namespace OryxBot.Routes.Tests;

public class JsonRouteStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly JsonRouteStore _store;

    public JsonRouteStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"oryxbot_test_{Guid.NewGuid():N}");
        var options = Options.Create(new RouteOptions { Directory = _testDir });
        _store = new JsonRouteStore(options);
    }

    [Fact]
    public async Task SaveAndReload_Roundtrips()
    {
        var route = new Route(
            new RouteMetadata("test-route", DateTimeOffset.UtcNow),
            [
                new MoveWaypoint(1.5f, 2.5f),
                new PortalWaypoint("SomeCluster"),
                new MoveWaypoint(3.0f, 4.0f),
                new MarkerWaypoint("checkpoint")
            ]);

        await _store.SaveAsync(route);
        var loaded = await _store.LoadAsync("test-route");

        loaded.Should().NotBeNull();
        loaded!.Waypoints.Should().HaveCount(4);
        loaded.Waypoints[0].Should().BeOfType<MoveWaypoint>()
            .Which.Should().BeEquivalentTo(new { X = 1.5f, Y = 2.5f });
        loaded.Waypoints[1].Should().BeOfType<PortalWaypoint>()
            .Which.ClusterName.Should().Be("SomeCluster");
        loaded.Waypoints[3].Should().BeOfType<MarkerWaypoint>()
            .Which.MarkerName.Should().Be("checkpoint");
    }

    [Fact]
    public async Task Load_InvalidName_ReturnsNull()
    {
        var result = await _store.LoadAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_ReturnsMetadata()
    {
        var route = new Route(
            new RouteMetadata("list-test", DateTimeOffset.UtcNow),
            [new MoveWaypoint(0, 0)]);

        await _store.SaveAsync(route);
        var list = await _store.ListAsync();

        list.Should().ContainSingle().Which.Name.Should().Be("list-test");
    }

    [Fact]
    public async Task ListAsync_EmptyDirectory_ReturnsEmpty()
    {
        var list = await _store.ListAsync();

        list.Should().BeEmpty();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }
}
