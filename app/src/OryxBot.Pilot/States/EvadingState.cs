using System.Numerics;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class EvadingState : INavigationState
{
    private Position? _threatPosition;

    public NavigationStateName Name => NavigationStateName.Evading;

    public void SetThreat(Position threatPosition)
    {
        _threatPosition = threatPosition;
    }

    public void ClearThreat()
    {
        _threatPosition = null;
    }

    public void OnEnter(TickContext tick) { }

    public void OnExit()
    {
        _threatPosition = null;
    }

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        if (_threatPosition is null)
            return new(NavAction.None, null, null, null, NavigationStateName.CorrectingCourse,
                "Threat cleared");

        // Move 180° from threat
        var awayDir = tracker.Position - _threatPosition.Value;
        var len = awayDir.Length();
        if (len > 0.001f)
            awayDir = Vector2.Normalize(awayDir);
        else
            awayDir = Vector2.UnitX;

        return new(NavAction.MoveInDirection, null, awayDir, null, null, "Evading threat");
    }
}
