using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class UnstickingState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.Unsticking;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        throw new NotImplementedException("UnstickingState implementation is Phase 2");
}
