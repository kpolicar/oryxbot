using FluentAssertions;
using OryxBot.Core.Events;

namespace OryxBot.Core.Tests.Events;

public record TestEvent(string Message) : IEvent;
public record OtherEvent(int Value) : IEvent;

public class EventBusTests
{
    private readonly EventBus _bus = new();

    [Fact]
    public async Task PublishAsync_WithSubscriber_HandlerReceivesEvent()
    {
        var received = new List<TestEvent>();
        var handler = new DelegateHandler<TestEvent>(e => { received.Add(e); return Task.CompletedTask; });
        _bus.Subscribe(handler);

        await _bus.PublishAsync(new TestEvent("hello"));

        received.Should().ContainSingle().Which.Message.Should().Be("hello");
    }

    [Fact]
    public async Task PublishAsync_MultipleHandlers_AllReceive()
    {
        var count = 0;
        var handler1 = new DelegateHandler<TestEvent>(_ => { Interlocked.Increment(ref count); return Task.CompletedTask; });
        var handler2 = new DelegateHandler<TestEvent>(_ => { Interlocked.Increment(ref count); return Task.CompletedTask; });
        _bus.Subscribe(handler1);
        _bus.Subscribe(handler2);

        await _bus.PublishAsync(new TestEvent("test"));

        count.Should().Be(2);
    }

    [Fact]
    public async Task PublishAsync_HandlerThrows_OtherHandlersStillCalled()
    {
        var called = false;
        var throwingHandler = new DelegateHandler<TestEvent>(_ => throw new InvalidOperationException("boom"));
        var goodHandler = new DelegateHandler<TestEvent>(_ => { called = true; return Task.CompletedTask; });
        _bus.Subscribe(throwingHandler);
        _bus.Subscribe(goodHandler);

        await _bus.PublishAsync(new TestEvent("test"));

        called.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_NoSubscribers_DoesNotThrow()
    {
        var act = () => _bus.PublishAsync(new TestEvent("no subscribers"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_OnlyMatchingHandlersCalled()
    {
        var testReceived = false;
        var otherReceived = false;
        var testHandler = new DelegateHandler<TestEvent>(_ => { testReceived = true; return Task.CompletedTask; });
        var otherHandler = new DelegateHandler<OtherEvent>(_ => { otherReceived = true; return Task.CompletedTask; });
        _bus.Subscribe(testHandler);
        _bus.Subscribe(otherHandler);

        await _bus.PublishAsync(new TestEvent("test"));

        testReceived.Should().BeTrue();
        otherReceived.Should().BeFalse();
    }

    private class DelegateHandler<T>(Func<T, Task> action) : IEventHandler<T> where T : IEvent
    {
        public Task HandleAsync(T @event, CancellationToken ct = default) => action(@event);
    }
}
