using System.Numerics;
using OryxBot.Core.Models;

namespace OryxBot.Pilot;

public record NavigationDecision(
    NavAction Action,
    Position? TargetPosition,
    Vector2? Direction,
    int? WaitMs,
    NavigationStateName? TransitionTo,
    string Reason);
