namespace OryxBot.Core.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent;

    void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : IEvent;
}
