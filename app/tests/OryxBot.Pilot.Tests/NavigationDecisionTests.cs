using System.Text.Json;
using FluentAssertions;

namespace OryxBot.Pilot.Tests;

public class NavigationDecisionTests
{
    [Fact]
    public void Decision_RoundtripsJson()
    {
        var decision = new NavigationDecision(
            NavAction.MoveTowards,
            new Core.Models.Position(10f, 20f),
            null,
            null,
            NavigationStateName.FollowingRoute,
            "Moving to next waypoint");

        var json = JsonSerializer.Serialize(decision);
        var deserialized = JsonSerializer.Deserialize<NavigationDecision>(json);

        deserialized.Should().Be(decision);
    }
}
