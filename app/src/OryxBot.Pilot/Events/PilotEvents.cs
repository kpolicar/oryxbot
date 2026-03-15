using OryxBot.Core.Events;

namespace OryxBot.Pilot.Events;

public record NavigationFailedEvent(string Reason) : IEvent;

public record RouteCompletedEvent : IEvent;

public record ConnectionLostEvent : IEvent;
