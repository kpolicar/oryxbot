using OryxBot.Routes.Models;
using OryxBot.WorldGraph.Models;

namespace OryxBot.Pilot;

public interface IPilotEngine
{
    Task NavigateToClusterAsync(string targetClusterId, CancellationToken ct = default);
    Task FollowRouteAsync(Route route, CancellationToken ct = default);
    NavigationSnapshot TakeSnapshot();
    bool IsComplete { get; }
    bool HasFailed { get; }
}
