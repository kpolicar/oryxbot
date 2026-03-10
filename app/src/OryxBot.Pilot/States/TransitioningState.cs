using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class TransitioningState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.Transitioning;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        throw new NotImplementedException("TransitioningState implementation is Phase 2");
}
