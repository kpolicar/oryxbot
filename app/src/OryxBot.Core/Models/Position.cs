using System.Numerics;

namespace OryxBot.Core.Models;

public readonly record struct Position(float X, float Y)
{
    public static float Distance(Position a, Position b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float DistanceSquared(Position a, Position b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

    public static Position operator +(Position pos, Vector2 vec) =>
        new(pos.X + vec.X, pos.Y + vec.Y);

    public static Vector2 operator -(Position a, Position b) =>
        new(a.X - b.X, a.Y - b.Y);

    /// <summary>Distance from point P to the line segment AB.</summary>
    public static float DistanceToSegment(Position p, Position a, Position b)
    {
        var ab = b - a;
        var ap = p - a;
        var lenSq = ab.X * ab.X + ab.Y * ab.Y;
        if (lenSq < 1e-6f) return Distance(p, a);
        var t = Math.Clamp((ap.X * ab.X + ap.Y * ab.Y) / lenSq, 0f, 1f);
        var proj = new Position(a.X + t * ab.X, a.Y + t * ab.Y);
        return Distance(p, proj);
    }

    public override string ToString() => $"({X:F1}, {Y:F1})";
}
