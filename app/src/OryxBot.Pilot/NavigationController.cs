using OryxBot.Core.Abstractions;
using OryxBot.Core.Models;
using OryxBot.Pilot.States;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot;

public class NavigationController
{
    private readonly Dictionary<NavigationStateName, INavigationState> _states;
    private INavigationState _currentState;
    private NavigationDecision? _lastDecision;

    public NavigationStateName CurrentStateName => _currentState.Name;

    public NavigationController(IEnumerable<INavigationState> states)
    {
        _states = states.ToDictionary(s => s.Name);
        _currentState = _states[NavigationStateName.FollowingRoute];
        // Note: OnEnter for initial state is called on first Evaluate via _needsInitialOnEnter
    }

    private bool _needsInitialOnEnter = true;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        if (_needsInitialOnEnter)
        {
            _needsInitialOnEnter = false;
            _currentState.OnEnter(tick);
        }

        if (tracker.IsDead)
            return TransitionTo(NavigationStateName.Killed, tick, "Character died");

        if (tracker.IsDisconnected)
            return TransitionTo(NavigationStateName.Disconnected, tick, "Connection lost");

        var decision = _currentState.Evaluate(cursor, tracker, tick);

        if (decision.TransitionTo is { } next && next != _currentState.Name)
        {
            _currentState.OnExit();
            _currentState = _states[next];
            _currentState.OnEnter(tick);
        }

        _lastDecision = decision;
        return decision;
    }

    public async Task ExecuteAsync(NavigationDecision decision, IGameController controller, CancellationToken ct = default)
    {
        switch (decision.Action)
        {
            case NavAction.MoveTowards when decision.TargetPosition.HasValue:
                await controller.MoveTowards(decision.TargetPosition.Value, ct);
                break;
            case NavAction.MoveInDirection when decision.Direction.HasValue:
                await controller.MoveInDirection(decision.Direction.Value, ct);
                break;
            case NavAction.Stop:
                await controller.StopMovement(ct);
                break;
            case NavAction.Wait when decision.WaitMs.HasValue:
                break; // In simulation, time is advanced by the simulator
        }
    }

    public NavigationSnapshot TakeSnapshot(RouteCursor cursor) => new(
        _currentState.Name,
        _lastDecision,
        cursor.Index,
        cursor.PercentComplete,
        null,
        DateTimeOffset.UtcNow);

    private NavigationDecision TransitionTo(NavigationStateName target, TickContext tick, string reason)
    {
        if (_currentState.Name != target)
        {
            _currentState.OnExit();
            _currentState = _states[target];
            _currentState.OnEnter(tick);
        }

        var decision = new NavigationDecision(NavAction.None, null, null, null, target, reason);
        _lastDecision = decision;
        return decision;
    }
}
