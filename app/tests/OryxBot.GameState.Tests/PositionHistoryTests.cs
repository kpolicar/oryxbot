using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using OryxBot.Core.Models;

namespace OryxBot.GameState.Tests;

public class PositionHistoryTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly PositionHistory _history;

    public PositionHistoryTests()
    {
        _history = new PositionHistory(_timeProvider, capacity: 10);
    }

    [Fact]
    public void Record_IncrementsCount()
    {
        _history.Record(new Position(1, 1));

        _history.Count.Should().Be(1);
    }

    [Fact]
    public void Record_CircularEviction_OldEntriesRemoved()
    {
        for (var i = 0; i < 15; i++)
        {
            _history.Record(new Position(i, i));
            _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        }

        _history.Count.Should().Be(10); // Capacity is 10
    }

    [Fact]
    public void GetRecent_ReturnsInReverseOrder()
    {
        _history.Record(new Position(1, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        _history.Record(new Position(2, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        _history.Record(new Position(3, 0));

        var recent = _history.GetRecent(3);

        recent.Should().HaveCount(3);
        recent[0].Position.X.Should().Be(3);
        recent[1].Position.X.Should().Be(2);
        recent[2].Position.X.Should().Be(1);
    }

    [Fact]
    public void GetTrail_ReturnsEntriesWithinWindow()
    {
        _history.Record(new Position(1, 0));
        _timeProvider.Advance(TimeSpan.FromSeconds(5));
        _history.Record(new Position(2, 0));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _history.Record(new Position(3, 0));

        var trail = _history.GetTrail(TimeSpan.FromSeconds(3));

        trail.Should().HaveCount(2);
        trail[0].Position.X.Should().Be(3);
        trail[1].Position.X.Should().Be(2);
    }

    [Fact]
    public void GetBacktrackPath_DeduplicatesClosePositions()
    {
        _history.Record(new Position(0, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        _history.Record(new Position(0.1f, 0)); // Too close to previous
        _timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        _history.Record(new Position(2, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        _history.Record(new Position(5, 0));

        var path = _history.GetBacktrackPath(10).ToList();

        path.Should().HaveCount(3); // (5,0), (2,0), (0,0) or (0.1,0) — skips duplicate
    }

    [Fact]
    public void GetBacktrackPath_RespectsMaxSteps()
    {
        for (var i = 0; i < 10; i++)
        {
            _history.Record(new Position(i * 5, 0));
            _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        }

        var path = _history.GetBacktrackPath(3).ToList();

        path.Should().HaveCount(3);
    }

    [Fact]
    public void Clear_ResetsHistory()
    {
        _history.Record(new Position(1, 1));
        _history.Record(new Position(2, 2));

        _history.Clear();

        _history.Count.Should().Be(0);
        _history.GetRecent(10).Should().BeEmpty();
    }
}
