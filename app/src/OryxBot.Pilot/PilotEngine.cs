using OryxBot.Routes.Models;

namespace OryxBot.Pilot;

/// <summary>
/// Stub — real implementation in Phase 2.
/// </summary>
public class PilotEngine : IPilotEngine
{
    public bool IsComplete => false;
    public bool HasFailed => false;

    public Task NavigateToClusterAsync(string targetClusterId, CancellationToken ct = default) =>
        throw new NotImplementedException("PilotEngine implementation is Phase 2");

    public Task FollowRouteAsync(Route route, CancellationToken ct = default) =>
        throw new NotImplementedException("PilotEngine implementation is Phase 2");

    public NavigationSnapshot TakeSnapshot() =>
        throw new NotImplementedException("PilotEngine implementation is Phase 2");
}
