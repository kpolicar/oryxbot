using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using OryxBot.Core.Models;

namespace OryxBot.GameState.Tests;

public class PositionPredictorTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly PositionPredictor _predictor;

    public PositionPredictorTests()
    {
        _predictor = new PositionPredictor(_timeProvider);
    }

    [Fact]
    public void PredictedPosition_WithVelocity_Extrapolates()
    {
        _predictor.Update(new Position(0, 0));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _predictor.Update(new Position(10, 0)); // velocity = (10, 0) per second

        _timeProvider.Advance(TimeSpan.FromSeconds(1));

        // Should predict 1 second ahead: (10, 0) + (10, 0) = (20, 0)
        var predicted = _predictor.PredictedPosition;
        predicted.X.Should().BeApproximately(20f, 1f);
        predicted.Y.Should().BeApproximately(0f, 1f);
    }

    [Fact]
    public void PredictedPosition_Stationary_NoExtrapolation()
    {
        _predictor.Update(new Position(5, 5));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _predictor.Update(new Position(5, 5)); // No movement

        _timeProvider.Advance(TimeSpan.FromSeconds(1));

        var predicted = _predictor.PredictedPosition;
        predicted.X.Should().BeApproximately(5f, 0.1f);
        predicted.Y.Should().BeApproximately(5f, 0.1f);
    }

    [Fact]
    public void Reset_ClearsVelocity()
    {
        _predictor.Update(new Position(0, 0));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _predictor.Update(new Position(10, 0));

        _predictor.Reset();

        _predictor.Velocity.X.Should().Be(0f);
        _predictor.Velocity.Y.Should().Be(0f);
    }
}
