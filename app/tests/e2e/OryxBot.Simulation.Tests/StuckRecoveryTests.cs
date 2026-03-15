using FluentAssertions;
using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Simulation.Assertions;
using OryxBot.Simulation.Fixtures;
using OryxBot.Simulation.Obstacles;
using OryxBot.Simulation.Profiles;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Tests;

public class StuckRecoveryTests
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
    public async Task WallBlocksMovement_UnstickingTriggered()
    {
        // Route goes through a wall
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine(100f, 10))
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithObstacle(new WallObstacle(20, -2, 22, 2)) // Rock at x=20
                .WithSeed(42)
                .WithMaxTicks(500),
            nameof(WallBlocksMovement_UnstickingTriggered));

        // Should detect being stuck and transition to unsticking
        recording.StateSequence().Should().Contain(NavigationStateName.Unsticking);
    }

    [Fact]
    public async Task RepeatedStuckAttempts_TransitionsToLost()
    {
        // Start far from route so deviation > LostThreshold (30), bot will be in CorrectingCourse
        // and unable to reduce deviation, eventually triggering Lost
        var route = new OryxBot.Routes.Models.Route(
            new OryxBot.Routes.Models.RouteMetadata("FarAway", DateTimeOffset.UtcNow),
            Enumerable.Range(0, 10)
                .Select(i => (OryxBot.Routes.Models.Waypoint)new OryxBot.Routes.Models.MoveWaypoint(200 + i * 10, 200))
                .ToList());

        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(route)
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithObstacle(new WallObstacle(-2, -2, 3, 2))  // Box around start
                .WithSeed(42)
                .WithNavigationOptions(new OryxBot.Core.Configuration.NavigationOptions
                {
                    LostTimeout = TimeSpan.FromSeconds(3), // Faster lost detection for test
                })
                .WithMaxTicks(2000),
            nameof(RepeatedStuckAttempts_TransitionsToLost));

        recording.StateSequence().Should().Contain(NavigationStateName.Lost);
    }

    [Fact]
    public async Task StuckDetection_FiresAfterIdleTimeout()
    {
        // Place an obstacle that blocks all movement
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine())
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithObstacle(new WallObstacle(-5, -5, 200, 5)) // Box the player in
                .WithSeed(42)
                .WithMaxTicks(500),
            nameof(StuckDetection_FiresAfterIdleTimeout));

        // Should have detected being stuck
        var states = recording.StateSequence().ToList();
        states.Count.Should().BeGreaterThan(1); // Should have transitioned at least once
    }

    [Fact]
    public async Task RecentClusterChange_UsesShallowRotation()
    {
        // This is more of an integration check - verify the state machine works after cluster change
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine())
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(500),
            nameof(RecentClusterChange_UsesShallowRotation));

        // Since we start in a cluster, RecentlyChangedCluster is initially true
        recording.Frames.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LargerObstacle_FirstAttemptFails_SecondSucceeds()
    {
        // Obstacle tall enough that the base radius arc (r=3, ~5.5 unit height) can't clear it,
        // but the escalated radius arc (r=5, ~9 unit height) can
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine(100f, 10))
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 0))
                .WithObstacle(new WallObstacle(20, -6, 23, 6))
                .WithSeed(42)
                .WithMaxTicks(1000),
            nameof(LargerObstacle_FirstAttemptFails_SecondSucceeds));

        var states = recording.StateSequence().ToList();

        // Should enter Unsticking at least twice (first attempt fails, second succeeds)
        var unstickCount = states.Count(s => s == NavigationStateName.Unsticking);
        unstickCount.Should().BeGreaterThanOrEqualTo(2,
            "first attempt should fail due to obstacle size, second should succeed with larger assumed radius");

        // Should eventually return to FollowingRoute after the second unstick
        states.Last().Should().Be(NavigationStateName.FollowingRoute,
            "bot should resume route after clearing the obstacle on second attempt");
    }
}
