# System Patterns — Architecture Decisions

## Pathfinder / Pilot Separation

The navigation responsibility is split into two distinct interfaces:

- **IPathfinder** — Pure computation. Takes `(fromCluster, toCluster, WorldGraph)` → returns `ClusterPath`. No I/O, no side effects, deterministic. Lives in `OryxBot.WorldGraph`.
- **IPilotEngine** — Execution engine. Takes a `ClusterPath` or `Route` and drives the character there using input commands. Manages the navigation state machine. Lives in `OryxBot.Pilot`.

They never reference each other. A higher-level orchestrator calls the Pathfinder, then hands the result to the Pilot.

## Navigation State Machine

The Pilot's core is `NavigationController` — a behavioral state machine that produces a `NavigationDecision` each tick:

- **FollowingRoute** → default, moving toward current waypoint
- **CorrectingCourse** → drifted off route, steering back (enter >6.0, exit <3.0)
- **Unsticking** → character idle >1.3s, applying rotation (-π/3 post-cluster, -2π/3 normal)
- **Transitioning** → portal reached, waiting for cluster change event + loading screen
- **Evading** → threat detected, moving away
- **Lost / Killed / Disconnected** → terminal failure states

Every decision is a serializable `NavigationDecision` record — fully loggable and replayable.

## Event Bus (not MediatR)

Custom lightweight `IEventBus` with `IEventHandler<T>`. Handlers invoked sequentially to preserve ordering. Handler exceptions are caught and logged without stopping propagation to other handlers. No external dependency.

## TimeProvider Injection

All time-dependent components (`ObstacleDetector`, `MotionDetector`, `PositionPredictor`) receive `System.TimeProvider` via DI. Never call `DateTime.UtcNow` directly. This enables deterministic time control in Phase 2's simulation engine using `FakeTimeProvider`.

## TickContext for Evaluate Loop

`NavigationController.Evaluate()` receives a `TickContext(TimeSpan Elapsed, long TickNumber, DateTimeOffset Timestamp)` — the caller controls time progression. In production, from wall-clock. In simulation, fabricated.

## Configuration via IOptions\<T\>

All magic numbers live in typed POCOs bound from `appsettings.json`:
- `NavigationOptions` — arrival distance, skip-ahead, idle timeout, stuck thresholds, rotation angles, etc.
- `RouteOptions` — route storage directory
- `ProtocolOptions` — Photon UDP ports
- `VncOptions` — VNC bridge host/port
- `WorldGraphOptions` — game data directory

## CancellationToken Everywhere

Every async method on every interface takes `CancellationToken ct = default` as its last parameter. Enforced from day one.

## No Singletons, No Static Mutable State

All state scoped via DI. No `Thread.Sleep` — use `Task.Delay` with cancellation. No static mutable fields.

## Project Dependency Graph

```
OryxBot.Core          → (leaf — no dependencies)
OryxBot.WorldGraph    → Core
OryxBot.Routes        → Core
OryxBot.Protocol      → Core
OryxBot.GameState     → Core
OryxBot.Input         → Core
OryxBot.RemoteDesktop → Core
OryxBot.Pilot         → Core, WorldGraph, Routes
OryxBot.Host          → all src/ projects
```

## Simulation-First Testing

E2E tests use a `GameSimulator` that replaces the real network sniffer and input driver. The bot sends movement commands → simulator applies physics with drift/lag → publishes position events back. Profiles control difficulty: Perfect, Realistic, HighLatency, Adversarial.
