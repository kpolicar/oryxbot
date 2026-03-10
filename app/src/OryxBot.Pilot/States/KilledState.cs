using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class KilledState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.Killed;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        throw new NotImplementedException("KilledState implementation is Phase 2");
}
