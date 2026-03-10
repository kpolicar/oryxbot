using FluentAssertions;
using OryxBot.Core.Models;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Routes.Tests;

public class RouteCursorTests
{
    private static IReadOnlyList<Waypoint> CreateTestWaypoints() =>
    [
        new MoveWaypoint(0, 0),
        new MoveWaypoint(5, 0),
        new MoveWaypoint(10, 0),
        new PortalWaypoint("DestCluster"),
        new MoveWaypoint(0, 10),
        new MoveWaypoint(5, 10),
        new MoveWaypoint(10, 10),
    ];

    [Fact]
    public void MoveNext_AdvancesIndex()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());

        cursor.MoveNext().Should().BeTrue();
        cursor.Index.Should().Be(1);
    }

    [Fact]
    public void MoveNext_AtEnd_ReturnsFalse()
    {
        var waypoints = new Waypoint[] { new MoveWaypoint(0, 0) };
        var cursor = new RouteCursor(waypoints);

        cursor.MoveNext().Should().BeFalse();
        cursor.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void MovePrevious_AtStart_ReturnsFalse()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());

        cursor.MovePrevious().Should().BeFalse();
    }

    [Fact]
    public void MovePrevious_AfterMoveNext_GoesBack()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        cursor.MoveNext();
        cursor.MoveNext();

        cursor.MovePrevious().Should().BeTrue();
        cursor.Index.Should().Be(1);
    }

    [Fact]
    public void TrySkipAhead_CharacterPastWaypoints_SkipsCorrectly()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        var charPos = new Position(9, 0); // Near waypoint at (10,0)

        cursor.TrySkipAhead(charPos, 10f, 5).Should().BeTrue();
        cursor.Index.Should().Be(2); // Skipped to (10,0)
    }

    [Fact]
    public void TrySkipAhead_MaxSkipLimitRespected()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        var charPos = new Position(9, 0);

        cursor.TrySkipAhead(charPos, 20f, 1).Should().BeTrue();
        cursor.Index.Should().Be(1); // Limited to 1 skip
    }

    [Fact]
    public void TrySkipAhead_CharacterBehind_NoSkip()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        var charPos = new Position(0, 0); // At first waypoint, but next is too far

        cursor.TrySkipAhead(charPos, 1f, 5).Should().BeFalse();
    }

    [Fact]
    public void TrySkipAhead_StopsAtPortalWaypoint()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        var charPos = new Position(5, 5); // Between clusters

        // Even with very high max distance, shouldn't skip past portal
        cursor.TrySkipAhead(charPos, 100f, 10).Should().BeTrue();
        cursor.Index.Should().BeLessThan(3); // Should stop before portal at index 3
    }

    [Fact]
    public void ResumeFromCluster_FindsClosestMoveStep()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        var approxPos = new Position(4, 10);

        cursor.ResumeFromCluster("DestCluster", approxPos).Should().BeTrue();
        cursor.Index.Should().Be(5); // Closest move waypoint after portal: (5,10)
    }

    [Fact]
    public void ResumeFromCluster_UnknownCluster_SearchesFromStart()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        var approxPos = new Position(4, 0);

        cursor.ResumeFromCluster("UnknownCluster", approxPos).Should().BeTrue();
        cursor.Index.Should().Be(1); // Closest to (4,0) from the start
    }

    [Fact]
    public void PercentComplete_CalculatedCorrectly()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());
        cursor.PercentComplete.Should().BeApproximately(0f, 0.1f);

        cursor.MoveNext();
        cursor.MoveNext();
        cursor.MoveNext(); // Index 3 of 7

        cursor.PercentComplete.Should().BeApproximately(3f / 7f * 100f, 0.1f);
    }

    [Fact]
    public void Current_AtValidIndex_ReturnsWaypoint()
    {
        var cursor = new RouteCursor(CreateTestWaypoints());

        cursor.Current.Should().BeOfType<MoveWaypoint>();
    }

    [Fact]
    public void Current_AtEnd_ReturnsNull()
    {
        var waypoints = new Waypoint[] { new MoveWaypoint(0, 0) };
        var cursor = new RouteCursor(waypoints);
        cursor.MoveNext();

        cursor.Current.Should().BeNull();
    }

    [Fact]
    public void EmptyWaypoints_PercentComplete_Is100()
    {
        var cursor = new RouteCursor([]);
        cursor.PercentComplete.Should().Be(100f);
    }
}
