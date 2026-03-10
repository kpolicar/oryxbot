using System.Drawing;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;

namespace OryxBot.Input.Tests;

public class CoordinateTranslatorTests
{
    private readonly CoordinateTranslator _translator;

    public CoordinateTranslatorTests()
    {
        var display = Options.Create(new DisplayOptions { ScreenWidth = 1920, ScreenHeight = 1080 });
        var navigation = Options.Create(new NavigationOptions());
        _translator = new CoordinateTranslator(display, navigation);
    }

    [Fact]
    public void WorldToScreen_SamePosition_ReturnsCenter()
    {
        var result = _translator.WorldToScreen(new Position(100, 100), new Position(100, 100));

        result.X.Should().BeCloseTo(960, 1);
        result.Y.Should().BeCloseTo(540, 1);
    }

    [Fact]
    public void WorldToScreen_Offset_AppliesIsometricRotation()
    {
        var result = _translator.WorldToScreen(new Position(110, 100), new Position(100, 100));

        // With -45° rotation, (10, 0) becomes approximately (7.07, -7.07)
        // Screen center is (960, 540)
        result.X.Should().BeGreaterThan(960);
    }

    [Fact]
    public void DirectionToScreen_ReturnsOffsetFromCenter()
    {
        var result = _translator.DirectionToScreen(new System.Numerics.Vector2(1, 0));

        result.X.Should().BeGreaterThan(960); // Should be offset right of center
    }
}
