using OryxBot.Core.Models;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Obstacles;

public abstract class Obstacle
{
    public abstract Position Resolve(Position from, Position candidate);
    public abstract RecordedObstacle ToRecorded();
}
