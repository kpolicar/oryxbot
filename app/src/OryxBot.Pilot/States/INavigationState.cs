using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public interface INavigationState
{
    NavigationStateName Name { get; }
    NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick);
}
