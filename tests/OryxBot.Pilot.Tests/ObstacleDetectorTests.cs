using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OryxBot.Core.Configuration;

namespace OryxBot.Pilot.Tests;

public class ObstacleDetectorTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ObstacleDetector _detector;

    public ObstacleDetectorTests()
    {
        var options = Options.Create(new NavigationOptions());
        _detector = new ObstacleDetector(_timeProvider, options);
    }

    [Fact]
    public void NoAttempts_IsStuckFalse()
    {
        _detector.IsStuck.Should().BeFalse();
    }

    [Fact]
    public void FiveAttemptsWithinWindow_IsStuckTrue()
    {
        for (var i = 0; i < 5; i++)
        {
            _detector.RecordAttempt();
            _timeProvider.Advance(TimeSpan.FromSeconds(1));
        }

        _detector.IsStuck.Should().BeTrue();
    }

    [Fact]
    public void FourAttempts_IsStuckFalse()
    {
        for (var i = 0; i < 4; i++)
        {
            _detector.RecordAttempt();
            _timeProvider.Advance(TimeSpan.FromSeconds(1));
        }

        _detector.IsStuck.Should().BeFalse();
    }

    [Fact]
    public void AttemptsExpire_IsStuckFalse()
    {
        for (var i = 0; i < 5; i++)
        {
            _detector.RecordAttempt();
            _timeProvider.Advance(TimeSpan.FromSeconds(1));
        }

        _detector.IsStuck.Should().BeTrue();

        // Advance past the 30s stuck window
        _timeProvider.Advance(TimeSpan.FromSeconds(30));

        _detector.IsStuck.Should().BeFalse();
    }

    [Fact]
    public void PurgeExpired_RemovesOldTimestamps()
    {
        _detector.RecordAttempt();
        _timeProvider.Advance(TimeSpan.FromSeconds(31));
        _detector.RecordAttempt();

        _detector.PurgeExpired();
        _detector.AttemptCount.Should().Be(1);
    }

    [Fact]
    public void Reset_ClearsAllAttempts()
    {
        for (var i = 0; i < 5; i++)
            _detector.RecordAttempt();

        _detector.Reset();

        _detector.IsStuck.Should().BeFalse();
        _detector.AttemptCount.Should().Be(0);
    }
}
