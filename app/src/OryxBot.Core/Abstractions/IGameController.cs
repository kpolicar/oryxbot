using System.Numerics;
using OryxBot.Core.Models;

namespace OryxBot.Core.Abstractions;

public interface IGameController
{
    Task MoveTowards(Position target, CancellationToken ct = default);
    Task MoveInDirection(Vector2 direction, CancellationToken ct = default);
    Task MoveAwayFrom(Position target, CancellationToken ct = default);
    Task StopMovement(CancellationToken ct = default);
    Task Respawn(CancellationToken ct = default);
}
