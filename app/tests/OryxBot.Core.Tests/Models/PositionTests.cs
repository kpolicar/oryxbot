using System.Numerics;
using FluentAssertions;
using OryxBot.Core.Models;

namespace OryxBot.Core.Tests.Models;

public class PositionTests
{
    [Fact]
    public void Distance_CalculatedCorrectly()
    {
        var a = new Position(0, 0);
        var b = new Position(3, 4);

        Position.Distance(a, b).Should().BeApproximately(5f, 0.001f);
    }

    [Fact]
    public void DistanceSquared_NoSqrt()
    {
        var a = new Position(0, 0);
        var b = new Position(3, 4);

        Position.DistanceSquared(a, b).Should().BeApproximately(25f, 0.001f);
    }

    [Fact]
    public void Equality_ValueSemantics()
    {
        var a = new Position(1.5f, 2.5f);
        var b = new Position(1.5f, 2.5f);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Inequality_DifferentValues()
    {
        var a = new Position(1f, 2f);
        var b = new Position(3f, 4f);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Addition_PositionPlusVector()
    {
        var pos = new Position(1f, 2f);
        var vec = new Vector2(3f, 4f);

        var result = pos + vec;

        result.Should().Be(new Position(4f, 6f));
    }

    [Fact]
    public void Subtraction_PositionsYieldVector()
    {
        var a = new Position(5f, 7f);
        var b = new Position(2f, 3f);

        var result = a - b;

        result.Should().Be(new Vector2(3f, 4f));
    }

    [Fact]
    public void Distance_SamePoint_IsZero()
    {
        var a = new Position(5f, 5f);

        Position.Distance(a, a).Should().Be(0f);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var pos = new Position(1.23f, 4.56f);

        pos.ToString().Should().Be("(1.2, 4.6)");
    }
}
