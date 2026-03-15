using Microsoft.Extensions.Options;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Models;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;

namespace OryxBot.Pilot.States;

public class TransitioningState(IOptions<NavigationOptions> options) : INavigationState
{
    private readonly NavigationOptions _options = options.Value;
    private DateTimeOffset _enteredAt;
    private string? _expectedCluster;
    private string? _previousCluster;
    private TransitionPhase _phase;
    private int _stalePacketsRemaining;

    public NavigationStateName Name => NavigationStateName.Transitioning;

    public void OnEnter(TickContext tick)
    {
        _enteredAt = tick.Timestamp;
        _phase = TransitionPhase.WaitingForClusterChange;
        _expectedCluster = null;
        _previousCluster = null;
        _stalePacketsRemaining = _options.StalePacketsAfterClusterChange;
    }

    public void OnExit()
    {
        _phase = TransitionPhase.WaitingForClusterChange;
    }

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker tracker, TickContext tick)
    {
        switch (_phase)
        {
            case TransitionPhase.WaitingForClusterChange:
                // Extract expected cluster from the portal waypoint
                if (_expectedCluster is null && cursor.Current is PortalWaypoint portal)
                {
                    _expectedCluster = portal.ClusterName;
                    _previousCluster = tracker.CurrentCluster;
                }

                // Check timeout
                if (tick.Timestamp - _enteredAt > _options.ClusterLoadTimeout)
                    return new(NavAction.None, null, null, null, NavigationStateName.Disconnected,
                        "Cluster change timeout");

                // Detect cluster change
                if (_previousCluster is not null && tracker.CurrentCluster != _previousCluster)
                {
                    _phase = TransitionPhase.SkippingStalePackets;
                    _stalePacketsRemaining = _options.StalePacketsAfterClusterChange;
                }

                return new(NavAction.Stop, null, null, null, null, "Waiting for cluster change");

            case TransitionPhase.SkippingStalePackets:
                _stalePacketsRemaining--;
                if (_stalePacketsRemaining <= 0)
                {
                    _phase = TransitionPhase.Resuming;
                }
                return new(NavAction.Wait, null, null, 200, null, "Skipping stale packets");

            case TransitionPhase.Resuming:
                // Advance cursor past the portal waypoint
                cursor.MoveNext();

                // Try to resume from cluster position
                if (_expectedCluster is not null)
                    cursor.ResumeFromCluster(_expectedCluster, tracker.Position);

                return new(NavAction.None, null, null, null, NavigationStateName.FollowingRoute,
                    "Cluster loaded, resuming route");

            default:
                return new(NavAction.None, null, null, null, NavigationStateName.Lost, "Invalid transition phase");
        }
    }

    private enum TransitionPhase
    {
        WaitingForClusterChange,
        SkippingStalePackets,
        Resuming
    }
}
