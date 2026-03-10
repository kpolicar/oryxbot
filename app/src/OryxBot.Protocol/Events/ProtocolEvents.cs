using OryxBot.Core.Events;

namespace OryxBot.Protocol.Events;

public record CharacterMovedEvent(float X, float Y) : IEvent;

public record ClusterChangedEvent(string ClusterName) : IEvent;

public record CharacterDiedEvent : IEvent;

public record InteractionChangedEvent(bool IsInteracting) : IEvent;
