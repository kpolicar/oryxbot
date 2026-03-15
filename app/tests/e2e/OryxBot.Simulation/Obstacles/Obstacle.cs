using OryxBot.Core.Models;

namespace OryxBot.Simulation.Obstacles;

public abstract class Obstacle
{
    public abstract Position Resolve(Position from, Position candidate);
}
