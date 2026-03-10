using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class EvadingState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.Evading;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        throw new NotImplementedException("EvadingState implementation is Phase 2");
}
