# Phase 2 — Simulation Engine & Test Suite

> **Goal:** Build a deterministic game simulator that closes the feedback loop  
> (bot sends input → simulator moves character → bot reads new position),  
> then use it to validate all navigation AI behaviors end-to-end.  
> **Target:** .NET 10 · Latest C# · xUnit · FluentAssertions · NSubstitute  
> **Depends on:** Phase 1 (core interfaces, RouteCursor, ObstacleDetector), Phase 1b (world graph data for cluster change tests)  
> **Unlocks:** Phase 3 (live protocol integration — confidence that the AI works before wiring to real game)

---

## Deliverables

1. `GameSimulator` — deterministic physics engine modeling game movement
2. `SimulatedInputCapture` — fake `IInputAdapter` that records commands and feeds them to the simulator
3. Simulation profiles (Perfect, Realistic, HighLatency, Adversarial, custom)
4. `SimulatorBuilder` — fluent API to wire up test scenarios
5. `RouteFixtures` — library of canned routes for testing
6. **Pathfinder implementation** — A* on the world graph (pure computation)
7. **NavigationController implementation** — the full state machine
8. **State implementations** — FollowingRoute, CorrectingCourse, Unsticking, Transitioning, Evading, Lost, Killed, Disconnected
9. Comprehensive E2E and unit test suites

---

## 1. Test Project Structure

```
tests/
├── OryxBot.Core.Tests/                     — Unit: EventBus, Position (Phase 1)
├── OryxBot.WorldGraph.Tests/               — Unit: Pathfinder, WorldGraph
├── OryxBot.Pilot.Tests/                    — Unit: ObstacleDetector, NavigationDecision
├── OryxBot.Routes.Tests/                   — Unit: RouteCursor, JsonRouteStore
├── OryxBot.GameState.Tests/                — Unit: MotionDetector, PositionPredictor
├── OryxBot.Input.Tests/                    — Unit: CoordinateTranslator, ResponsivePoint
│
tests/e2e/
├── OryxBot.Simulation/                     — Shared library (not a test project itself)
│   ├── GameSimulator.cs
│   ├── SimulatedInputCapture.cs
│   ├── Profiles/
│   │   ├── ISimulationProfile.cs
│   │   ├── PerfectProfile.cs
│   │   ├── RealisticProfile.cs
│   │   ├── HighLatencyProfile.cs
│   │   ├── AdversarialProfile.cs
│   │   ├── ConstantDriftProfile.cs
│   │   └── PositionJumpProfile.cs
│   ├── Obstacles/
│   │   ├── Obstacle.cs
│   │   └── WallObstacle.cs
│   ├── Fixtures/
│   │   ├── SimulatorBuilder.cs
│   │   └── RouteFixtures.cs
│   └── Assertions/
│       └── SimulationAssertions.cs         — Custom FluentAssertions for deviation, convergence
│
└── OryxBot.Simulation.Tests/              — E2E tests using the simulator
    ├── RouteFollowingTests.cs
    ├── CourseCorrectionTests.cs
    ├── ClusterChangeTests.cs
    ├── StuckRecoveryTests.cs
    ├── DeathRecoveryTests.cs
    └── PathfinderTests.cs
```

---

## 2. GameSimulator — Deterministic Physics

The simulator replaces the real network sniffer and input driver. It closes the loop: bot sends movement commands → simulator calculates character position → publishes position events back to bot.

```csharp
// tests/e2e/OryxBot.Simulation/GameSimulator.cs
public class GameSimulator
{
    private readonly ISimulationProfile _profile;
    private readonly IEventBus _eventBus;
    private readonly Random _rng;
    private readonly List<Obstacle> _obstacles;
    
    private Position _actualPosition;
    private Vector2 _currentDirection;
    private float _speed;
    private string _currentCluster;
    
    public List<SimulatedTick> TickHistory { get; } = [];

    /// Called by SimulatedInputCapture when the bot issues a move command.
    public async Task OnBotMovedCursor(Vector2 screenDirection, CancellationToken ct)
    {
        // 1. Reverse the -π/4 isometric rotation the bot applies
        var gameDirection = ReverseIsometricTransform(screenDirection);
        
        // 2. Apply profile's drift
        gameDirection = _profile.ApplyDirectionDrift(gameDirection, _rng);
        _currentDirection = gameDirection;
        
        // 3. Simulate elapsed time (packet interval)
        var elapsed = _profile.NextPacketInterval(_rng);
        
        // 4. Calculate displacement
        _speed = _profile.CalculateSpeed(_speed, _rng);
        var displacement = _currentDirection * _speed * (float)elapsed.TotalSeconds;
        var candidate = new Position(
            _actualPosition.X + displacement.X,
            _actualPosition.Y + displacement.Y);
        
        // 5. Collision resolution
        _actualPosition = ApplyObstacles(candidate);
        
        // 6. Record for assertions
        TickHistory.Add(new SimulatedTick(_actualPosition, _currentDirection, _speed, elapsed));
        
        // 7. Publish position event (feeds back to bot)
        await _eventBus.PublishAsync(new CharacterMovedEvent(_actualPosition.X, _actualPosition.Y), ct);
    }

    /// Simulate a cluster change (portal transition)
    public async Task SimulateClusterChange(string newCluster, CancellationToken ct)
    {
        _currentCluster = newCluster;
        await _eventBus.PublishAsync(new ClusterChangedEvent(newCluster), ct);
        
        // Loading screen delay
        await Task.Delay(_profile.ClusterChangeDelay(_rng), ct);
        
        // Stale packets (real game sends ~5 packets with old positions after cluster change)
        for (int i = 0; i < _profile.StalePacketsAfterClusterChange; i++)
        {
            await _eventBus.PublishAsync(
                new CharacterMovedEvent(_actualPosition.X, _actualPosition.Y), ct);
        }
    }
    
    private static Vector2 ReverseIsometricTransform(Vector2 screen)
    {
        // Reverse the -π/4 rotation: apply +π/4
        const float angle = MathF.PI / 4f;
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Vector2(
            screen.X * cos - screen.Y * sin,
            screen.X * sin + screen.Y * cos);
    }
    
    private Position ApplyObstacles(Position candidate)
    {
        foreach (var obstacle in _obstacles)
        {
            candidate = obstacle.Resolve(_actualPosition, candidate);
        }
        return candidate;
    }
}

public record SimulatedTick(Position Position, Vector2 Direction, float Speed, TimeSpan Elapsed);
```

---

## 3. Simulation Profiles

```csharp
public interface ISimulationProfile
{
    Vector2 ApplyDirectionDrift(Vector2 intendedDirection, Random rng);
    TimeSpan NextPacketInterval(Random rng);
    float CalculateSpeed(float currentSpeed, Random rng);
    TimeSpan ClusterChangeDelay(Random rng);
    int StalePacketsAfterClusterChange { get; }
}
```

| Profile | Direction Drift | Packet Interval | Speed | Cluster Delay | Use Case |
|---------|----------------|-----------------|-------|---------------|----------|
| **PerfectProfile** | 0° | 200ms fixed | 7.0 fixed | 8s fixed | Baseline — bot must follow route exactly |
| **RealisticProfile** | ±5° random | 150–350ms | 6.5–7.5 | 7–10s | Normal gameplay |
| **HighLatencyProfile** | ±15° random | 300–800ms | 5.0–9.0 | 8–15s | Poor connection |
| **AdversarialProfile** | ±25° random | 500–1500ms | 3.0–12.0 | 10–20s | Stress test |
| **ConstantDriftProfile** | Fixed N° perpendicular | 200ms fixed | 7.0 fixed | 8s fixed | Tests correction convergence |
| **PositionJumpProfile** | 0° (normal), then teleport at tick N | 200ms fixed | 7.0 fixed | 8s fixed | Tests recovery from lag spikes |

---

## 4. SimulatedInputCapture

Replaces the real `IInputAdapter`. Records every command and feeds movement back to the simulator.

```csharp
public class SimulatedInputCapture : IInputAdapter
{
    private readonly GameSimulator _simulator;
    public List<InputCommand> CommandLog { get; } = [];
    
    public async Task SetCursorPosition(int x, int y)
    {
        CommandLog.Add(new CursorMoveCommand(x, y, DateTime.UtcNow));
        var direction = CalculateDirectionFromScreenPos(x, y);
        await _simulator.OnBotMovedCursor(direction, CancellationToken.None);
    }
    
    public Task RightMouseDown() { CommandLog.Add(new RightMouseDownCommand()); return Task.CompletedTask; }
    public Task RightMouseUp() { CommandLog.Add(new RightMouseUpCommand()); return Task.CompletedTask; }
    public Task LeftClick() { CommandLog.Add(new LeftClickCommand()); return Task.CompletedTask; }
    public Task KeyPress(string key) { CommandLog.Add(new KeyPressCommand(key)); return Task.CompletedTask; }
}

public abstract record InputCommand(DateTime Timestamp);
public record CursorMoveCommand(int X, int Y, DateTime Timestamp) : InputCommand(Timestamp);
public record RightMouseDownCommand() : InputCommand(DateTime.UtcNow);
public record RightMouseUpCommand() : InputCommand(DateTime.UtcNow);
public record LeftClickCommand() : InputCommand(DateTime.UtcNow);
public record KeyPressCommand(string Key) : InputCommand(DateTime.UtcNow);
```

---

## 5. SimulatorBuilder — Fluent API

```csharp
public class SimulatorBuilder
{
    public SimulatorBuilder WithRoute(Route route) { ... }
    public SimulatorBuilder WithProfile(ISimulationProfile profile) { ... }
    public SimulatorBuilder WithProfile<T>() where T : ISimulationProfile, new() { ... }
    public SimulatorBuilder StartingAt(Position position) { ... }
    public SimulatorBuilder StartingInCluster(string cluster) { ... }
    public SimulatorBuilder WithObstacle(Obstacle obstacle) { ... }
    public SimulatorBuilder WithSeed(int seed) { ... }
    public SimulatorBuilder WithMaxTicks(int maxTicks) { ... }

    /// Builds the full DI container with simulator wired in place of real services.
    public (GameSimulator Simulator, IPilotEngine Pilot,
            SimulatedInputCapture Input, IServiceProvider Services) Build();
}
```

The builder:
1. Creates `IServiceCollection`
2. Registers all real services (NavigationController, RouteCursor, CharacterTracker, etc.)
3. Replaces `IInputAdapter` with `SimulatedInputCapture`
4. Replaces `IPacketInterceptor` with a no-op (simulator pushes events directly)
5. Wires the `GameSimulator` to the `SimulatedInputCapture` feedback loop
6. Configures the `IWorldGraphProvider` with test fixture data
7. Builds `IServiceProvider` and resolves the pilot engine

---

## 6. RouteFixtures

```csharp
public static class RouteFixtures
{
    /// 10 waypoints in a straight line
    public static Route StraightLine(float length = 100f) { ... }
    
    /// Route with 90° turns every 20 units
    public static Route ZigZag(int segments = 5) { ... }
    
    /// Smooth arc (many small waypoints)
    public static Route Curve(float radius = 50f, float angleDegrees = 180f) { ... }
    
    /// Route passing through 3 cluster changes (with PortalWaypoints)
    public static Route MultiCluster() { ... }
    
    /// Narrow corridor with walls on both sides
    public static Route NarrowCorridor(float width = 8f) { ... }
    
    /// U-turn (route doubles back on itself)
    public static Route UTurn() { ... }
    
    /// Very long route (1000+ waypoints, realistic production length)
    public static Route LongRoute() { ... }
    
    /// Widely-spaced waypoints (tests skip-ahead logic)
    public static Route SparseWaypoints(float spacing = 20f) { ... }
}
```

---

## 7. Pathfinder Implementation

The Pathfinder is **pure computation** — no I/O, no side effects, fully deterministic.

### 7.1 Algorithm

Use **A\*** with Euclidean heuristic (or Dijkstra if cluster positions aren't available) on the `WorldGraph` adjacency structure.

```csharp
// src/OryxBot.WorldGraph/Pathfinder.cs
public class Pathfinder : IPathfinder
{
    private readonly IWorldGraphProvider _graphProvider;
    
    public ClusterPath? FindPath(string from, string to)
        => FindPath(from, to, PathOptions.Default);
    
    public ClusterPath? FindPath(string from, string to, PathOptions options)
    {
        var graph = _graphProvider.GetWorldGraph();
        if (!graph.Contains(from) || !graph.Contains(to))
            return null;
        
        // A* with optional node filtering (avoid PvP, max tier, blocked clusters)
        var openSet = new PriorityQueue<string, float>();
        var cameFrom = new Dictionary<string, (string ClusterId, ClusterExit Exit)>();
        var gScore = new Dictionary<string, float> { [from] = 0 };
        
        openSet.Enqueue(from, 0);
        
        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == to)
                return ReconstructPath(cameFrom, from, to);
            
            foreach (var exit in graph.GetExits(current))
            {
                if (options.ShouldAvoid(exit.TargetClusterId, graph))
                    continue;
                
                var tentativeG = gScore[current] + 1;  // Uniform edge cost
                if (tentativeG < gScore.GetValueOrDefault(exit.TargetClusterId, float.MaxValue))
                {
                    cameFrom[exit.TargetClusterId] = (current, exit);
                    gScore[exit.TargetClusterId] = tentativeG;
                    var fScore = tentativeG + Heuristic(exit.TargetClusterId, to, graph);
                    openSet.Enqueue(exit.TargetClusterId, fScore);
                }
            }
        }
        
        return null;  // No path exists
    }
}
```

### 7.2 PathOptions

```csharp
public record PathOptions
{
    public static readonly PathOptions Default = new();
    
    public bool AvoidPvpZones { get; init; }
    public int? MaxTier { get; init; }
    public HashSet<string> AvoidClusters { get; init; } = [];
    
    public bool ShouldAvoid(string clusterId, WorldGraph graph)
    {
        if (AvoidClusters.Contains(clusterId)) return true;
        var cluster = graph.GetCluster(clusterId);
        if (cluster is null) return true;
        if (AvoidPvpZones && cluster.IsPvpZone) return true;
        if (MaxTier.HasValue && cluster.Tier > MaxTier.Value) return true;
        return false;
    }
}
```

### 7.3 Pathfinder Tests

```csharp
// tests/OryxBot.WorldGraph.Tests/PathfinderTests.cs

[Fact]
public void FindPath_DirectNeighbors_ReturnsSingleHop()
// A→B directly connected. Result: [A→B] with 1 hop.

[Fact]
public void FindPath_ThroughIntermediate_ReturnsShortestPath()
// A→B→C, A→D→C (longer). Result: [A→B→C] (2 hops, shorter).

[Fact]
public void FindPath_NoPathExists_ReturnsNull()
// A is disconnected from B. Result: null.

[Fact]
public void FindPath_SameCluster_ReturnsEmptyPath()
// From == To. Result: path with 0 hops.

[Fact]
public void FindPath_WithAvoidClusters_RoutesAround()
// A→B→D, A→C→D. Avoid B → result: [A→C→D].

[Fact]
public void FindPath_WithMaxTier_SkipsHighTier()
// Path through T7 zone exists but MaxTier=5. Takes longer T5 route.

[Fact]
public void FindPath_LargeGraph_CompletesReasonably()
// 1700+ clusters (realistic size). Should complete in <100ms.
```

---

## 8. NavigationController — State Machine Implementation

### 8.1 Controller

```csharp
// src/OryxBot.Pilot/NavigationController.cs
public class NavigationController
{
    private INavigationState _currentState;
    private NavigationDecision? _lastDecision;
    
    public NavigationStateName CurrentStateName => _currentState.Name;

    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker character)
    {
        // 1. Check terminal conditions
        if (character.IsDead)
            return TransitionTo(NavigationStateName.Killed, "Character died");
        if (character.IsDisconnected)
            return TransitionTo(NavigationStateName.Disconnected, "Connection lost");
        
        // 2. Delegate to current state
        var decision = _currentState.Evaluate(cursor, character);
        
        // 3. Handle transitions
        if (decision.TransitionTo is { } next)
        {
            _currentState = ResolveState(next);
        }
        
        _lastDecision = decision;
        return decision;
    }
    
    public async Task Execute(NavigationDecision decision, IGameController controller, CancellationToken ct)
    {
        switch (decision.Action)
        {
            case NavAction.MoveTowards:
                await controller.MoveTowards(decision.TargetPosition!.Value, ct);
                break;
            case NavAction.MoveInDirection:
                await controller.MoveInDirection(decision.Direction!.Value, ct);
                break;
            case NavAction.Stop:
                await controller.StopMovement(ct);
                break;
            case NavAction.Wait:
                await Task.Delay(decision.WaitMs, ct);
                break;
        }
    }
    
    public NavigationSnapshot TakeSnapshot() => new()
    {
        StateName = _currentState.Name,
        LastDecision = _lastDecision,
        // ... populate from cursor and character tracker
    };
}
```

### 8.2 State Implementations

| State | Key Logic | Transition Rules |
|-------|-----------|-----------------|
| **FollowingRoute** | Move towards `cursor.Current`; call `cursor.TrySkipAhead()` each tick; advance on arrival (distance < 2.0) | → CorrectingCourse: deviation > 6.0 from nearest upcoming waypoint |
| | | → Unsticking: idle > 1.3s (no movement detected) |
| | | → Transitioning: `cursor.Current` is a `PortalWaypoint` |
| **CorrectingCourse** | Steer towards nearest upcoming waypoint (not just current — look ahead 2–3) | → FollowingRoute: deviation < 3.0 |
| | | → Lost: deviation > 30.0 for 10+ seconds |
| **Unsticking** | Apply rotation to last movement direction: `-π/3` if recently changed cluster, `-2π/3` otherwise. Hold for 1.5s. Record attempt in `ObstacleDetector`. | → FollowingRoute: movement detected |
| | | → Lost: `ObstacleDetector.IsStuck` (5 attempts in 30s) |
| **Transitioning** | Stop movement. Wait for `ClusterChangedEvent`. After event, wait for loading to finish (configurable, ~8–10s). Skip stale packets (5). Resume cursor past `PortalWaypoint`. | → FollowingRoute: cluster loaded + cursor resumed |
| | | → Disconnected: no cluster change event in 30s |
| **Evading** | Move away from threat position (180° from threat bearing). Re-evaluate each tick. | → CorrectingCourse: threat cleared |
| **Lost** | Terminal. Publish `NavigationFailedEvent`. No further actions. | — |
| **Killed** | Terminal. Publish `CharacterKilledEvent`. | — |
| **Disconnected** | Terminal. Publish `ConnectionLostEvent`. | — |

### 8.3 Critical Constants

| Constant | Value | Used By |
|----------|-------|---------|
| Arrival distance | 2.0 | FollowingRoute — waypoint reached |
| Skip-ahead max steps | 4 | RouteCursor.TrySkipAhead |
| Skip-ahead max distance | 6.0 | RouteCursor.TrySkipAhead |
| Idle timeout | 1.3s | FollowingRoute → Unsticking |
| Unstick rotation (normal) | -2π/3 (120°) | Unsticking state |
| Unstick rotation (post-cluster) | -π/3 (60°) | Unsticking state |
| Unstick hold duration | 1.5s | Unsticking state |
| Max stuck attempts | 5 | ObstacleDetector |
| Stuck window | 30s | ObstacleDetector |
| Correction threshold (enter) | 6.0 | FollowingRoute → CorrectingCourse |
| Correction threshold (exit) | 3.0 | CorrectingCourse → FollowingRoute |
| Lost threshold | 30.0 for 10s | CorrectingCourse → Lost |
| Stale packets after cluster | 5 | Transitioning state |
| Cluster load timeout | 30s | Transitioning → Disconnected |
| Disconnect timeout | 15s of no packets | → Disconnected |
| Isometric rotation | -π/4 | CoordinateTranslator |

---

## 9. E2E Test Cases

### 9.1 Route Following

```csharp
// tests/e2e/OryxBot.Simulation.Tests/RouteFollowingTests.cs

[Fact]
public async Task StraightRoute_PerfectConditions_CompletesWithMinimalDeviation()

[Theory]
[InlineData(typeof(RealisticProfile))]
[InlineData(typeof(HighLatencyProfile))]
public async Task ZigZagRoute_WithDrift_CourseCorrectsAtTurns(Type profileType)

[Fact]
public async Task CurvedRoute_Realistic_StaysWithinMaxDeviation()
// Max deviation < 12 units (2x skip threshold)

[Fact]
public async Task AdversarialDrift_EventuallyCompletesRoute()
// May take longer but must not get permanently stuck

[Fact]
public async Task SparseWaypoints_SkipAheadWorks_DoesNotBacktrack()
// Navigator index only ever increases

[Fact]
public async Task LongRoute_Realistic_CompletesWithinTimeRatio()
// Actual ticks < 1.5× theoretical minimum
```

### 9.2 Course Correction

```csharp
// tests/e2e/OryxBot.Simulation.Tests/CourseCorrectionTests.cs

[Fact]
public async Task DriftingRight_BotCorrects_WithinReasonableTime()
// Start 3 units off-route. Within 5 ticks, movement direction corrects.

[Fact]
public async Task ConstantPerpendicularDrift_MaintainsApproximatePath()
// 2° constant drift. Character stays within 8-unit corridor.

[Fact]
public async Task SuddenLargePositionJump_BotRecovers()
// Teleport 15 units off-course at tick 50. Deviation decreases afterward.

[Fact]
public async Task NarrowCorridor_WithObstacles_NavigatesWithoutGettingStuck()
// Wall collision. Bot navigates through.
```

### 9.3 Cluster Changes

```csharp
// tests/e2e/OryxBot.Simulation.Tests/ClusterChangeTests.cs

[Fact]
public async Task ClusterChange_CursorAdvancesPastPortalWaypoint()
// Route: move, move, portal, move, move. After cluster event, cursor is on first move after portal.

[Fact]
public async Task ClusterChange_StalePacketsIgnored()
// After cluster change, 5 stale packets with old positions. Character state doesn't jump back.

[Fact]
public async Task ClusterChange_BotWaitsForLoadingScreen()
// No CursorMoveCommands during loading period.

[Fact]
public async Task MultipleClusterChanges_RouteCompletesCorrectly()
// Full route with 3 cluster changes in sequence.
```

### 9.4 Stuck Detection & Recovery

```csharp
// tests/e2e/OryxBot.Simulation.Tests/StuckRecoveryTests.cs

[Fact]
public async Task NotMoving_StuckDetected_RotationApplied()
// Hold position fixed for >1.3s. Assert: anti-stuck rotation triggered.

[Fact]
public async Task StuckAgainstWall_AlternatesStrategies()
// Wall blocks movement. Bot tries rotations and eventually passes.

[Fact]
public async Task StuckTimeout_LostStateReached()
// 5 stuck attempts in 30s. NavigationStateName transitions to Lost.

[Fact]
public async Task RecentlyChangedCluster_UsesShallowRotation()
// -π/3 after cluster change (not -2π/3).
```

### 9.5 Death Recovery

```csharp
// tests/e2e/OryxBot.Simulation.Tests/DeathRecoveryTests.cs

[Fact]
public async Task CharacterDies_KilledStateReached()
// Death event fired. State → Killed. NavigationFailedEvent published.

[Fact]
public async Task DeathWhileMoving_WaitsBeforeTransition()
// Death fires mid-movement. Bot waits up to 2s for movement to stop.
```

---

## 10. Unit Test Cases (Non-E2E)

These test individual components in isolation.

### 10.1 Movement State Tracking

```csharp
// tests/OryxBot.GameState.Tests/MotionDetectorTests.cs

[Fact] public void PositionChange_SetsMovingTrue()
[Fact] public void NoPositionChangeFor1300ms_SetsMovingFalse()
[Fact] public void SmallPositionChange_BelowThreshold_SetsMovingFalse()
// MinDistanceConsideredAsMove = 0.2f
[Fact] public void IdleWatch_ResetsWhenMovingStarts()

// tests/OryxBot.GameState.Tests/PositionPredictorTests.cs

[Fact] public void Speed_CalculatedFromDistanceAndTime()
[Fact] public void PredictedPosition_ExtrapolatesForward()
[Fact] public void ClusterChange_ResetsSpeed()
[Fact] public void HighSpeed_CappedAt30_IgnoredAsTeleport()
```

### 10.2 Input Coordinate Translation

```csharp
// tests/OryxBot.Input.Tests/CoordinateTranslatorTests.cs

[Fact] public void IsometricRotation_NorthInGame_MapsToUpRightOnScreen()
// Direction (0, -1) after -π/4 → approximately (0.707, -0.707)

[Fact] public void MoveTowards_CursorPlacedAtCorrectScreenPosition()
// Target to northeast. Cursor offset = direction × screenHeight/10.

[Fact] public void AntiStuck_NormalConditions_RotatesBy120Degrees()  // -2π/3
[Fact] public void AntiStuck_RecentClusterChange_RotatesBy60Degrees()  // -π/3

// tests/OryxBot.Input.Tests/ResponsivePointTests.cs

[Theory]
[InlineData(1920, 1080)]
[InlineData(2560, 1440)]
[InlineData(3840, 2160)]
public void LeftAnchor_ScalesCorrectlyAcrossResolutions(int w, int h)

[Fact] public void RightAnchor_MirrorsLeftAnchor()
[Fact] public void CenterAnchor_OffsetsFromScreenCenter()
```

---

## 11. Test Metrics & Acceptance Thresholds

For route-following E2E tests, define these standard metrics:

| Metric | Formula | Acceptable Range |
|--------|---------|------------------|
| **Max Deviation** | Max distance from any tick position to nearest route waypoint | < 12 units (2× skip threshold) |
| **Average Deviation** | Mean distance from tick positions to nearest waypoint | < 4 units |
| **Convergence Rate** | Ticks to halve deviation after a correction | < 5 ticks |
| **Completion Rate** | Route finishes successfully | 100% for Perfect/Realistic profiles |
| **Completion Time Ratio** | Actual ticks / theoretical minimum | < 1.5× Realistic, < 3× Adversarial |
| **Backtrack Count** | Times cursor index decreased | 0 for all normal routes |

---

## 12. Acceptance Criteria

- [ ] `dotnet test` passes all E2E and unit tests
- [ ] Pathfinder finds correct shortest paths on test graphs
- [ ] Pathfinder completes on 1700+ cluster real world graph in <100ms
- [ ] NavigationController transitions through all states correctly
- [ ] StraightLine + PerfectProfile: max deviation < 5 units
- [ ] ZigZag + RealisticProfile: completes with convergence after turns
- [ ] MultiCluster route: all cluster changes handled, stale packets ignored
- [ ] Stuck detection fires after 5 attempts in 30s window
- [ ] Unsticking uses correct rotation angles based on context
- [ ] ObstacleDetector resets correctly
- [ ] ConstantDrift: character stays within 8-unit corridor
- [ ] PositionJump: deviation decreases after the jump
- [ ] All simulation profiles are deterministic with the same seed
