using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class DisconnectedState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.Disconnected;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        new(NavAction.None, null, null, null, null, "Connection lost");
}
