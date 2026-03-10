using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;

namespace OryxBot.GameState.Tests;

public class MotionDetectorTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly MotionDetector _detector;

    public MotionDetectorTests()
    {
        var options = Options.Create(new NavigationOptions());
        _detector = new MotionDetector(_timeProvider, options);
    }

    [Fact]
    public void InitialState_NotMoving()
    {
        _detector.IsMoving.Should().BeFalse();
    }

    [Fact]
    public void Update_WithMovement_IsMovingTrue()
    {
        _detector.Update(new Position(0, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        _detector.Update(new Position(5, 0));

        _detector.IsMoving.Should().BeTrue();
    }

    [Fact]
    public void Update_NoMovement_AfterIdleTimeout_IsMovingFalse()
    {
        _detector.Update(new Position(5, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        _detector.Update(new Position(10, 0));

        _detector.IsMoving.Should().BeTrue();

        _timeProvider.Advance(TimeSpan.FromSeconds(2)); // Past 1.3s idle timeout
        _detector.Update(new Position(10, 0)); // Same position

        _detector.IsMoving.Should().BeFalse();
    }

    [Fact]
    public void Speed_CalculatedFromMovement()
    {
        _detector.Update(new Position(0, 0));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _detector.Update(new Position(10, 0));

        _detector.Speed.Should().BeApproximately(10f, 0.5f);
    }

    [Fact]
    public void Reset_ClearsState()
    {
        _detector.Update(new Position(0, 0));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(100));
        _detector.Update(new Position(5, 0));

        _detector.Reset();

        _detector.IsMoving.Should().BeFalse();
        _detector.Speed.Should().Be(0f);
    }
}
