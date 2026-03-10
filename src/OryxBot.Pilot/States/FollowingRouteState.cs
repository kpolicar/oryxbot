using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class FollowingRouteState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.FollowingRoute;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        throw new NotImplementedException("FollowingRouteState implementation is Phase 2");
}
