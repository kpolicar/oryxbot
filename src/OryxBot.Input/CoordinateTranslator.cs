using System.Drawing;
using System.Numerics;
using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;

namespace OryxBot.Input;

public sealed class CoordinateTranslator
{
    private readonly DisplayOptions _display;
    private readonly NavigationOptions _navigation;

    public CoordinateTranslator(IOptions<DisplayOptions> display, IOptions<NavigationOptions> navigation)
    {
        _display = display.Value;
        _navigation = navigation.Value;
    }

    /// <summary>
    /// Translates a world position to screen coordinates relative to the character's position.
    /// Applies isometric rotation (-45°) as the game uses an isometric camera.
    /// </summary>
    public Point WorldToScreen(Position worldTarget, Position characterPos)
    {
        // World-space offset from character to target
        var dx = worldTarget.X - characterPos.X;
        var dy = worldTarget.Y - characterPos.Y;

        // Apply isometric rotation
        var cos = MathF.Cos(_navigation.IsometricRotation);
        var sin = MathF.Sin(_navigation.IsometricRotation);
        var rotatedX = dx * cos - dy * sin;
        var rotatedY = dx * sin + dy * cos;

        // Scale and center on screen
        var screenX = (int)(_display.ScreenWidth / 2f + rotatedX);
        var screenY = (int)(_display.ScreenHeight / 2f + rotatedY);

        return new Point(screenX, screenY);
    }

    /// <summary>
    /// Translates a direction vector to screen coordinates relative to screen center.
    /// </summary>
    public Point DirectionToScreen(Vector2 direction, float magnitude = 200f)
    {
        var normalized = Vector2.Normalize(direction) * magnitude;

        var cos = MathF.Cos(_navigation.IsometricRotation);
        var sin = MathF.Sin(_navigation.IsometricRotation);
        var rotatedX = normalized.X * cos - normalized.Y * sin;
        var rotatedY = normalized.X * sin + normalized.Y * cos;

        return new Point(
            (int)(_display.ScreenWidth / 2f + rotatedX),
            (int)(_display.ScreenHeight / 2f + rotatedY));
    }
}
