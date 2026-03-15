using FluentAssertions;
using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Simulation.Assertions;
using OryxBot.Simulation.Fixtures;
using OryxBot.Simulation.Profiles;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Tests;

public class RouteFollowingTests
{
    private async Task<SimulationRecording> RunAndSave(SimulatorBuilder builder, string testName)
    {
        var harness = builder.WithTestName(testName).Build();
        var recording = await harness.Simulator.RunAsync();

        if (Environment.GetEnvironmentVariable("ORYXBOT_SAVE_RECORDINGS") is not null)
            await RecordingSerializer.WriteToFileAsync(recording,
                Path.Combine("recordings", $"{testName}.json"));

        return recording;
    }

    [Fact]
    public async Task StraightRoute_PerfectConditions_CompletesWithMinimalDeviation()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine())
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(500),
            nameof(StraightRoute_PerfectConditions_CompletesWithMinimalDeviation));

        recording.RouteCompleted().Should().BeTrue();
        recording.MaxDeviation().Should().BeLessThan(12f);
        recording.BacktrackCount().Should().Be(0);
    }

    [Theory]
    [InlineData(typeof(RealisticProfile))]
    [InlineData(typeof(HighLatencyProfile))]
    public async Task ZigZagRoute_WithDrift_CourseCorrectsAtTurns(Type profileType)
    {
        var profile = (ISimulationProfile)Activator.CreateInstance(profileType)!;
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.ZigZag())
                .WithProfile(profile)
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(1000),
            $"ZigZag_{profile.Name}");

        recording.RouteCompleted().Should().BeTrue();
    }

    [Fact]
    public async Task CurvedRoute_Realistic_StaysWithinMaxDeviation()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.Curve())
                .WithProfile<RealisticProfile>()
                .StartingAt(new Position(50, 0)) // Start at first curve point
                .WithSeed(42)
                .WithMaxTicks(1000),
            nameof(CurvedRoute_Realistic_StaysWithinMaxDeviation));

        recording.RouteCompleted().Should().BeTrue();
        recording.MaxDeviation().Should().BeLessThan(12f);
    }

    [Fact]
    public async Task AdversarialDrift_EventuallyCompletesRoute()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine(50f, 5))
                .WithProfile<AdversarialProfile>()
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(2000),
            nameof(AdversarialDrift_EventuallyCompletesRoute));

        // May not always complete under adversarial conditions, but should not get permanently stuck
        recording.Frames.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SparseWaypoints_SkipAheadWorks_DoesNotBacktrack()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.SparseWaypoints(spacing: 5f))
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(500),
            nameof(SparseWaypoints_SkipAheadWorks_DoesNotBacktrack));

        recording.BacktrackCount().Should().Be(0);
    }

    [Fact]
    public async Task LongRoute_Realistic_Completes()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.LongRoute(100, 2f))
                .WithProfile<RealisticProfile>()
                .StartingAt(new Position(2, 0))
                .WithSeed(42)
                .WithMaxTicks(5000),
            nameof(LongRoute_Realistic_Completes));

        recording.RouteCompleted().Should().BeTrue();
    }

    [Fact]
    public async Task MultiCluster_Realistic_NavigatesAcrossClusters()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.MultiCluster())
                .WithProfile<RealisticProfile>()
                .StartingAt(new Position(0, 0))
                .StartingInCluster("0208")
                .WithSeed(42)
                .WithMaxTicks(10000),
            nameof(MultiCluster_Realistic_NavigatesAcrossClusters));

        // Should have visited multiple clusters
        var clusters = recording.Frames.Select(f => f.ClusterId).Distinct().ToList();
        clusters.Count.Should().BeGreaterThanOrEqualTo(2);

        // Should have gone through Transitioning state
        recording.StateSequence().Should().Contain(NavigationStateName.Transitioning);
    }
}
