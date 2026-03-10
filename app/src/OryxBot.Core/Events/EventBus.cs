using System.Collections.Concurrent;
using Serilog;

namespace OryxBot.Core.Events;

public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();
    private readonly ILogger _logger = Log.ForContext<EventBus>();

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var handlers = _handlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (handlers)
        {
            handlers.Add(handler);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IEvent
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
            return;

        List<object> snapshot;
        lock (handlers)
        {
            snapshot = [.. handlers];
        }

        foreach (var handler in snapshot)
        {
            try
            {
                await ((IEventHandler<TEvent>)handler).HandleAsync(@event, ct);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Event handler {HandlerType} failed for {EventType}",
                    handler.GetType().Name, typeof(TEvent).Name);
            }
        }
    }
}
