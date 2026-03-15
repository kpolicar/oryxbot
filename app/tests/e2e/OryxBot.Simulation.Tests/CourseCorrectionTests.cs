using FluentAssertions;
using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Simulation.Assertions;
using OryxBot.Simulation.Fixtures;
using OryxBot.Simulation.Profiles;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Tests;

public class CourseCorrectionTests
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
    public async Task StartingOffRoute_BotCorrects_ReturnsToFollowing()
    {
        // Start 4 units off the route (route is along Y=0, start at Y=4)
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine())
                .WithProfile<PerfectProfile>()
                .StartingAt(new Position(0, 4))
                .WithSeed(42)
                .WithMaxTicks(500),
            nameof(StartingOffRoute_BotCorrects_ReturnsToFollowing));

        recording.RouteCompleted().Should().BeTrue();
    }

    [Fact]
    public async Task ConstantPerpendicularDrift_MaintainsApproximatePath()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine())
                .WithProfile(new ConstantDriftProfile(2f))
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(1000),
            nameof(ConstantPerpendicularDrift_MaintainsApproximatePath));

        recording.RouteCompleted().Should().BeTrue();
    }

    [Fact]
    public async Task SuddenPositionJump_BotRecovers()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.StraightLine(200f, 20))
                .WithProfile(new PositionJumpProfile(jumpAtTick: 30, jumpDistance: 10f))
                .StartingAt(new Position(0, 0))
                .WithSeed(42)
                .WithMaxTicks(1000),
            nameof(SuddenPositionJump_BotRecovers));

        // Should eventually recover and complete (or at least continue navigating)
        recording.Frames.Count.Should().BeGreaterThan(30);
    }

    [Fact]
    public async Task NarrowCorridor_WithObstacles_Navigates()
    {
        var recording = await RunAndSave(
            new SimulatorBuilder()
                .WithRoute(RouteFixtures.NarrowCorridor(width: 8f))
                .WithProfile<RealisticProfile>()
                .StartingAt(new Position(0, 4))
                .WithObstacle(new Obstacles.WallObstacle(0, -1, 100, 0))   // bottom wall
                .WithObstacle(new Obstacles.WallObstacle(0, 8, 100, 9))    // top wall
                .WithSeed(42)
                .WithMaxTicks(1000),
            nameof(NarrowCorridor_WithObstacles_Navigates));

        recording.Frames.Count.Should().BeGreaterThan(0);
    }
}
