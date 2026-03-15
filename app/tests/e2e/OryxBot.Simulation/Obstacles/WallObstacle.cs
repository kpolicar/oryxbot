using OryxBot.Core.Models;

namespace OryxBot.Simulation.Obstacles;

public class WallObstacle : Obstacle
{
    private readonly float _x1, _y1, _x2, _y2;
    private readonly bool _isHorizontal;

    public WallObstacle(float x1, float y1, float x2, float y2)
    {
        _x1 = Math.Min(x1, x2);
        _y1 = Math.Min(y1, y2);
        _x2 = Math.Max(x1, x2);
        _y2 = Math.Max(y1, y2);
        _isHorizontal = Math.Abs(_y2 - _y1) < Math.Abs(_x2 - _x1);
    }

    public override Position Resolve(Position from, Position candidate)
    {
        if (candidate.X < _x1 || candidate.X > _x2 || candidate.Y < _y1 || candidate.Y > _y2)
            return candidate;

        // Push back to the entry side
        if (_isHorizontal)
        {
            var midY = (_y1 + _y2) / 2f;
            return from.Y < midY
                ? new Position(candidate.X, _y1 - 0.1f)
                : new Position(candidate.X, _y2 + 0.1f);
        }
        else
        {
            var midX = (_x1 + _x2) / 2f;
            return from.X < midX
                ? new Position(_x1 - 0.1f, candidate.Y)
                : new Position(_x2 + 0.1f, candidate.Y);
        }
    }
}
