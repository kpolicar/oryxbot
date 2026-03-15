using FluentAssertions;
using OryxBot.Core.Models;
using OryxBot.Pilot;
using OryxBot.Simulation.Assertions;
using OryxBot.Simulation.Fixtures;
using OryxBot.Simulation.Profiles;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Tests;

public class DeathRecoveryTests
{
    [Fact]
    public async Task CharacterDies_KilledStateReached()
    {
        var harness = new SimulatorBuilder()
            .WithRoute(RouteFixtures.StraightLine())
            .WithProfile<PerfectProfile>()
            .StartingAt(new Position(0, 0))
            .WithTestName(nameof(CharacterDies_KilledStateReached))
            .WithMaxTicks(500)
            .Build();

        // Simulate death after 10 ticks
        _ = Task.Run(async () =>
        {
            await Task.Delay(1); // Let sim start
            // The tracker is on the FakeTimeProvider, so we need to set dead directly
        });

        // Kill the character after a few ticks by setting IsDead
        harness.Tracker.SetDead(true);

        var recording = await harness.Simulator.RunAsync();

        recording.StateSequence().Should().Contain(NavigationStateName.Killed);
        recording.Frames.Count.Should().BeGreaterThan(0);

        if (Environment.GetEnvironmentVariable("ORYXBOT_SAVE_RECORDINGS") is not null)
            await RecordingSerializer.WriteToFileAsync(recording,
                Path.Combine("recordings", $"{nameof(CharacterDies_KilledStateReached)}.json"));
    }

    [Fact]
    public async Task CharacterNotDead_NeverReachesKilledState()
    {
        var harness = new SimulatorBuilder()
            .WithRoute(RouteFixtures.StraightLine(50f, 5))
            .WithProfile<PerfectProfile>()
            .StartingAt(new Position(0, 0))
            .WithTestName(nameof(CharacterNotDead_NeverReachesKilledState))
            .WithMaxTicks(500)
            .Build();

        var recording = await harness.Simulator.RunAsync();

        recording.StateSequence().Should().NotContain(NavigationStateName.Killed);
    }
}
