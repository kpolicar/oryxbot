using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class KilledState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.Killed;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        new(NavAction.None, null, null, null, null, "Character killed");
}
