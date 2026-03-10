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

    public override string ToString() => $"({X:F1}, {Y:F1})";
}
