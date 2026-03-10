using System.Drawing;
using FluentAssertions;

namespace OryxBot.Input.Tests;

public class ResponsivePointTests
{
    [Fact]
    public void FromAbsolute_Start_Resolve_RoundTrips()
    {
        var point = ResponsivePoint.FromAbsolute(960, 540, 1920, 1080);
        var resolved = point.Resolve(1920, 1080);

        resolved.Should().Be(new Point(960, 540));
    }

    [Fact]
    public void Resolve_DifferentResolution_ScalesCorrectly()
    {
        var point = ResponsivePoint.FromAbsolute(960, 540, 1920, 1080);
        var resolved = point.Resolve(3840, 2160);

        resolved.Should().Be(new Point(1920, 1080));
    }

    [Fact]
    public void FromAbsolute_Center_Resolve()
    {
        var point = ResponsivePoint.FromAbsolute(960, 540, 1920, 1080, Anchor.Center, Anchor.Center);
        var resolved = point.Resolve(1920, 1080);

        resolved.X.Should().BeCloseTo(960, 1);
        resolved.Y.Should().BeCloseTo(540, 1);
    }

    [Fact]
    public void FromAbsolute_End_Resolve()
    {
        // 100px from the right edge at 1920
        var point = ResponsivePoint.FromAbsolute(1820, 980, 1920, 1080, Anchor.End, Anchor.End);
        var resolved = point.Resolve(3840, 2160);

        // Should be 200px from right edge at 3840
        resolved.X.Should().BeCloseTo(3640, 2);
        resolved.Y.Should().BeCloseTo(2160 - (int)(100f / 1080f * 2160f), 2);
    }
}
