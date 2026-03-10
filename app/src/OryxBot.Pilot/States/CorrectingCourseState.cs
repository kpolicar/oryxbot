using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class CorrectingCourseState : INavigationState
{
    public NavigationStateName Name => NavigationStateName.CorrectingCourse;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick) =>
        throw new NotImplementedException("CorrectingCourseState implementation is Phase 2");
}
