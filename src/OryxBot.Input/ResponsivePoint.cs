using System.Drawing;

namespace OryxBot.Input;

/// <summary>
/// Resolution-agnostic screen coordinate. Stores position relative to an anchor
/// and a reference resolution, then scales to any target resolution.
/// </summary>
public readonly record struct ResponsivePoint(
    float XRatio,
    float YRatio,
    Anchor AnchorX,
    Anchor AnchorY)
{
    /// <summary>
    /// Creates a ResponsivePoint from absolute pixel coordinates at a reference resolution.
    /// </summary>
    public static ResponsivePoint FromAbsolute(int x, int y, int refWidth, int refHeight, Anchor anchorX = Anchor.Start, Anchor anchorY = Anchor.Start)
    {
        var xRatio = anchorX switch
        {
            Anchor.Start => (float)x / refWidth,
            Anchor.Center => (float)(x - refWidth / 2) / refWidth,
            Anchor.End => (float)(refWidth - x) / refWidth,
            _ => (float)x / refWidth
        };

        var yRatio = anchorY switch
        {
            Anchor.Start => (float)y / refHeight,
            Anchor.Center => (float)(y - refHeight / 2) / refHeight,
            Anchor.End => (float)(refHeight - y) / refHeight,
            _ => (float)y / refHeight
        };

        return new ResponsivePoint(xRatio, yRatio, anchorX, anchorY);
    }

    /// <summary>
    /// Resolves this responsive point to absolute pixel coordinates at the given resolution.
    /// </summary>
    public Point Resolve(int targetWidth, int targetHeight)
    {
        var x = AnchorX switch
        {
            Anchor.Start => (int)(XRatio * targetWidth),
            Anchor.Center => (int)(targetWidth / 2f + XRatio * targetWidth),
            Anchor.End => (int)(targetWidth - XRatio * targetWidth),
            _ => (int)(XRatio * targetWidth)
        };

        var y = AnchorY switch
        {
            Anchor.Start => (int)(YRatio * targetHeight),
            Anchor.Center => (int)(targetHeight / 2f + YRatio * targetHeight),
            Anchor.End => (int)(targetHeight - YRatio * targetHeight),
            _ => (int)(YRatio * targetHeight)
        };

        return new Point(x, y);
    }
}

public enum Anchor
{
    Start,
    Center,
    End
}
