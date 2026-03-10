# Phase 1 — Core & Skeleton

> **Goal:** Set up the solution structure, core abstractions, and interfaces.  
> Everything compiles, nothing runs. This is the foundation all other phases build on.  
> **Target:** .NET 10 · Latest C# · Linux-first  
> **Depends on:** Nothing  
> **Unlocks:** Phase 1b (game data extraction), Phase 2 (simulation engine)

---

## Deliverables

1. Solution file with all projects created (empty implementations)
2. All core interfaces and abstractions defined
3. Event bus implemented and tested
4. Domain models (Position, Waypoint, Route, Cluster, etc.)
5. Configuration POCOs for all tunable constants (`IOptions<T>` pattern)
6. DI registration extensions for each project
7. Host project with composition root (wires everything up, does nothing yet)
8. Unit test projects created with test infrastructure

---

## 1. Solution Setup

Create the .NET 10 solution with all projects:

```bash
# Solution
dotnet new sln -n OryxBot

# Source projects (all classlib)
dotnet new classlib -n OryxBot.Core -o src/OryxBot.Core -f net10.0
dotnet new classlib -n OryxBot.WorldGraph -o src/OryxBot.WorldGraph -f net10.0
dotnet new classlib -n OryxBot.Pilot -o src/OryxBot.Pilot -f net10.0
dotnet new classlib -n OryxBot.Routes -o src/OryxBot.Routes -f net10.0
dotnet new classlib -n OryxBot.Protocol -o src/OryxBot.Protocol -f net10.0
dotnet new classlib -n OryxBot.Input -o src/OryxBot.Input -f net10.0
dotnet new classlib -n OryxBot.GameState -o src/OryxBot.GameState -f net10.0
dotnet new classlib -n OryxBot.RemoteDesktop -o src/OryxBot.RemoteDesktop -f net10.0

# Host (console app)
dotnet new console -n OryxBot.Host -o hosts/OryxBot.Host -f net10.0

# Test projects
dotnet new xunit -n OryxBot.Core.Tests -o tests/OryxBot.Core.Tests -f net10.0
dotnet new xunit -n OryxBot.WorldGraph.Tests -o tests/OryxBot.WorldGraph.Tests -f net10.0
dotnet new xunit -n OryxBot.Pilot.Tests -o tests/OryxBot.Pilot.Tests -f net10.0
dotnet new xunit -n OryxBot.Routes.Tests -o tests/OryxBot.Routes.Tests -f net10.0
dotnet new xunit -n OryxBot.GameState.Tests -o tests/OryxBot.GameState.Tests -f net10.0
dotnet new xunit -n OryxBot.Input.Tests -o tests/OryxBot.Input.Tests -f net10.0

# Add all to solution
dotnet sln add src/**/*.csproj hosts/**/*.csproj tests/**/*.csproj
```

### Project Dependencies (reference graph)

```
OryxBot.Core          → (no dependencies — leaf)
OryxBot.WorldGraph    → OryxBot.Core
OryxBot.Routes        → OryxBot.Core
OryxBot.Protocol      → OryxBot.Core
OryxBot.GameState     → OryxBot.Core
OryxBot.Input         → OryxBot.Core
OryxBot.RemoteDesktop → OryxBot.Core
OryxBot.Pilot         → OryxBot.Core, OryxBot.WorldGraph, OryxBot.Routes
OryxBot.Host          → All src/ projects
```

---

## 2. OryxBot.Core — Interfaces & Abstractions

This is the **leaf dependency** — everything references Core. Define all shared abstractions here.

### 2.1 Events

```
src/OryxBot.Core/
├── Events/
│   ├── IEvent.cs
│   ├── IEventBus.cs
│   ├── IEventHandler.cs
│   └── EventBus.cs              — ConcurrentDictionary<Type, List<object>> implementation
```

**IEvent** — Marker interface for all domain events.

**IEventBus** — Publish/subscribe. `PublishAsync<T>()` fans out to all registered `IEventHandler<T>`. Registration via DI scanning (all types implementing `IEventHandler<T>` auto-registered).

**EventBus** — Implementation. Thread-safe via `ConcurrentDictionary`. Handlers invoked sequentially (not parallel) to preserve ordering guarantees. Catches and logs handler exceptions without stopping propagation to other handlers.

### 2.2 Abstractions

```
src/OryxBot.Core/
├── Abstractions/
│   ├── IGameController.cs       — MoveTowards, MoveInDirection, MoveAwayFrom, StopMovement, Respawn
│   ├── IInputAdapter.cs         — SetCursorPosition, LeftClick, RightMouseDown, RightMouseUp, KeyPress
│   ├── IRemoteConnection.cs     — ConnectAsync, DisconnectAsync, IsConnected, ScreenSize
│   ├── ICharacterTracker.cs     — Position, PredictedPosition, CurrentCluster, IsMoving, RecentlyChangedCluster,
│   │                              IsDead, IsDisconnected, Speed, LastPositionUpdate
│   └── IPacketInterceptor.cs    — Start, Stop, event callbacks
```

**IRemoteConnection** — Abstracts the remote desktop transport (VNC, RDP, or any future backend). The rest of the system never knows what protocol is used underneath. In tests, mock this alongside `IInputAdapter` to simulate game interaction without any real connection.

**IInputAdapter** — Transport-agnostic input sending. Implementations translate calls into whatever the remote connection backend expects (HTTP API, raw VNC messages, etc.). Fully mockable — test the bot's decision-making without sending real input.

**CancellationToken convention:** Every async method on every interface must accept `CancellationToken ct = default` as its last parameter. This is trivial to add now but painful to retrofit across 30+ methods later. Enforce this from day one.

**Logging convention:** All non-trivial classes take `Serilog.ILogger` via constructor injection. Use Serilog throughout with its native `ILogger` — source projects reference the `Serilog` package directly. Use `Log.ForContext<T>()` or constructor injection via DI. Structured logging with message templates (`{PropertyName}`) everywhere.

### 2.3 Models

```
src/OryxBot.Core/
├── Models/
│   ├── Position.cs              — readonly record struct Position(float X, float Y)
│   │                              with Distance(), DistanceSquared(), operator overloads
│   └── TickContext.cs           — readonly record struct TickContext(TimeSpan Elapsed, long TickNumber, DateTimeOffset Timestamp)
```

`TickContext` is passed into `NavigationController.Evaluate()` and any component that needs elapsed time. In production, the pilot loop creates it from real wall-clock deltas. In Phase 2 simulation, the simulator fabricates it with controlled values. This keeps all time-dependent logic deterministic under test.

**Position** should include:
- `static float Distance(Position a, Position b)` 
- `static float DistanceSquared(Position a, Position b)` — for comparisons without sqrt
- `operator +(Position, Vector2)`, `operator -(Position, Position)` returning Vector2

### 2.4 TimeProvider

Use .NET's built-in `System.TimeProvider` (available since .NET 8) for all timestamp needs:

```
src/OryxBot.Core/
├── Time/
│   └── (no custom files needed — use System.TimeProvider directly)
```

- Inject `TimeProvider` into `ObstacleDetector`, `MotionDetector`, `PositionPredictor`, and anything else that reads the current time
- In production DI: `services.AddSingleton(TimeProvider.System)`
- In tests: use `Microsoft.Extensions.Time.Testing.FakeTimeProvider` to control time deterministically
- **Never call `DateTime.UtcNow` or `DateTimeOffset.UtcNow` directly** — always go through the injected `TimeProvider`

This is critical for Phase 2: the simulation engine must be able to advance time in controlled steps.

### 2.5 Configuration POCOs

All tunable constants live in typed options classes, bound via `IOptions<T>` from `appsettings.json`:

```
src/OryxBot.Core/
├── Configuration/
│   └── NavigationOptions.cs     — All navigation magic numbers in one place
```

```csharp
public class NavigationOptions
{
    public float ArrivalDistance { get; set; } = 2.0f;
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMilliseconds(1300);
    public float UnstickRotationNormal { get; set; } = -2f * MathF.PI / 3f;  // -120°
    public float UnstickRotationPostCluster { get; set; } = -MathF.PI / 3f;   // -60°
    public TimeSpan UnstickHoldDuration { get; set; } = TimeSpan.FromMilliseconds(1500);
    public int MaxStuckAttempts { get; set; } = 5;
    public TimeSpan StuckWindow { get; set; } = TimeSpan.FromSeconds(30);
    public float CorrectionEnterThreshold { get; set; } = 6.0f;
    public float CorrectionExitThreshold { get; set; } = 3.0f;
    public float LostThreshold { get; set; } = 30.0f;
    public TimeSpan LostTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public int StalePacketsAfterClusterChange { get; set; } = 5;
    public TimeSpan ClusterLoadTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(15);
    public float IsometricRotation { get; set; } = -MathF.PI / 4f;           // -45°
}
```

Additional options classes defined in their respective projects:

```csharp
// src/OryxBot.Routes/Configuration/RouteOptions.cs
public class RouteOptions
{
    public string Directory { get; set; } = "./routes";
}

// src/OryxBot.Protocol/Configuration/ProtocolOptions.cs
public class ProtocolOptions
{
    public int[] Ports { get; set; } = [5056, 5055, 4535];
}

// src/OryxBot.Core/Configuration/DisplayOptions.cs
public class DisplayOptions
{
    public int ScreenWidth { get; set; } = 1920;
    public int ScreenHeight { get; set; } = 1080;
}

// src/OryxBot.RemoteDesktop/Configuration/VncOptions.cs
public class VncOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8010;
}

// src/OryxBot.WorldGraph/Configuration/WorldGraphOptions.cs
public class WorldGraphOptions
{
    public string DataDirectory { get; set; } = "./gamedata";
}
```

### 2.6 DI Extension

```csharp
// src/OryxBot.Core/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOryxBotCore(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton(TimeProvider.System);
        services.Configure<NavigationOptions>(config.GetSection("Navigation"));
        services.Configure<DisplayOptions>(config.GetSection("Display"));
        return services;
    }
}
```

### 2.7 Tests: OryxBot.Core.Tests

```
tests/OryxBot.Core.Tests/
├── Events/
│   ├── EventBusTests.cs         — Publish reaches subscribers, multiple handlers, exception isolation
│   └── EventBusSubscriptionTests.cs
└── Models/
    └── PositionTests.cs         — Distance, equality, operators
```

**Key test cases:**
- `PublishAsync_WithSubscriber_HandlerReceivesEvent`
- `PublishAsync_MultipleHandlers_AllReceive`
- `PublishAsync_HandlerThrows_OtherHandlersStillCalled`
- `PublishAsync_NoSubscribers_DoesNotThrow`
- `Position_Distance_CalculatedCorrectly`
- `Position_DistanceSquared_NoSqrt`
- `Position_Equality_ValueSemantics`

---

## 3. OryxBot.WorldGraph — Pathfinder Abstractions

### 3.1 Models

```
src/OryxBot.WorldGraph/
├── Models/
│   ├── Cluster.cs               — Id, DisplayName, Type, Tier, Biome, Exits
│   ├── ClusterType.cs           — enum: City, World, Dungeon, Island, Arena, etc.
│   ├── ClusterExit.cs           — TargetClusterId, PositionInCluster, ArrivalPosition
│   ├── WorldGraph.cs            — Dictionary<string, Cluster>, adjacency, GetExits(), GetCluster()
│   └── ClusterPath.cs           — List<ClusterHop>, TotalClusters
│   └── ClusterHop.cs            — ClusterId, ExitToNext
│   └── PathOptions.cs           — AvoidPvpZones, MaxTier, AvoidClusters
```

### 3.2 Interfaces

```
src/OryxBot.WorldGraph/
├── IPathfinder.cs               — FindPath(from, to), FindPath(from, to, options)
├── IWorldGraphProvider.cs       — GetWorldGraph() — loads/caches the graph
```

### 3.3 Stub Implementations

Create stub/skeleton implementations that throw `NotImplementedException`. The real implementations come in Phase 2.

```csharp
// Stub — replaced in Phase 1b when game data extraction is built
public class Pathfinder : IPathfinder
{
    public ClusterPath? FindPath(string from, string to) => throw new NotImplementedException();
    public ClusterPath? FindPath(string from, string to, PathOptions options) => throw new NotImplementedException();
}
```

### 3.4 Tests: OryxBot.WorldGraph.Tests

Create test file stubs with `[Fact]` attributes and `Skip` reasons:

```csharp
[Fact(Skip = "Phase 2 — Pathfinder implementation")]
public void FindPath_DirectNeighbors_ReturnsSingleHop() { }

[Fact(Skip = "Phase 2 — Pathfinder implementation")]
public void FindPath_MultipleClusters_ReturnsShortestPath() { }

[Fact(Skip = "Phase 2 — Pathfinder implementation")]
public void FindPath_NoPathExists_ReturnsNull() { }

[Fact(Skip = "Phase 2 — Pathfinder implementation")]
public void FindPath_WithAvoidClusters_RoutesAround() { }
```

---

## 4. OryxBot.Pilot — Navigation Abstractions

### 4.1 State Machine Interfaces

```
src/OryxBot.Pilot/
├── States/
│   └── INavigationState.cs      — Name, Evaluate(RouteCursor, ICharacterTracker) → NavigationDecision
├── NavigationDecision.cs        — Action, TargetPosition, Direction, WaitMs, TransitionTo, Reason
├── NavigationSnapshot.cs        — StateName, LastDecision, WaypointIndex, PercentComplete, etc.
├── NavigationStateName.cs       — enum: FollowingRoute, CorrectingCourse, Unsticking, etc.
├── NavAction.cs                 — enum: None, MoveTowards, MoveInDirection, Stop, Wait
```

### 4.2 Pilot Engine Interface

```
src/OryxBot.Pilot/
├── IPilotEngine.cs              — NavigateToCluster(), FollowRoute(), Snapshot, IsComplete, HasFailed
├── PilotEngine.cs               — Stub implementation
├── NavigationController.cs      — Stub: Evaluate(), Execute(), TakeSnapshot()
├── ObstacleDetector.cs          — Implementable now: sliding window stuck counter (5 attempts / 30s)
```

**ObstacleDetector** can be fully implemented (it's pure logic). Takes `TimeProvider` and `IOptions<NavigationOptions>` via constructor:
- `RecordAttempt()` — adds `TimeProvider.GetUtcNow()` timestamp to queue
- `PurgeExpired()` — removes timestamps outside the configured `StuckWindow` (default 30s)
- `IsStuck` — true if ≥ `MaxStuckAttempts` (default 5) attempts in window
- `Reset()` — clears all

### 4.3 Tests: OryxBot.Pilot.Tests

```
tests/OryxBot.Pilot.Tests/
├── ObstacleDetectorTests.cs     — Fully testable now
│   ├── NoAttempts_IsStuckFalse
│   ├── FiveAttemptsWithinWindow_IsStuckTrue
│   ├── AttemptsExpire_IsStuckFalse
│   ├── PurgeExpired_RemovesOldTimestamps
│   └── Reset_ClearsAllAttempts
└── NavigationDecisionTests.cs   — Record serialization
    └── Decision_RoundtripsJson
```

---

## 5. OryxBot.Routes — Route Models

### 5.1 Models

```
src/OryxBot.Routes/
├── Models/
│   ├── Route.cs                 — Metadata + List<Waypoint>
│   ├── RouteMetadata.cs         — Name, RecordedAt
│   ├── Waypoint.cs              — abstract record Waypoint(string Type)
│   │                              ├── MoveWaypoint(float X, float Y) : Waypoint("move")
│   │                              ├── PortalWaypoint(string ClusterName) : Waypoint("portal")
│   │                              └── MarkerWaypoint(string MarkerName) : Waypoint("marker")
```

### 5.2 Interfaces

```
src/OryxBot.Routes/
├── Storage/
│   └── IRouteStore.cs           — LoadAsync, SaveAsync, ListAsync
├── Traversal/
│   └── RouteCursor.cs           — MoveNext, MovePrevious, TrySkipAhead, ResumeFromCluster
├── Recording/
│   └── RouteRecorder.cs         — StartRecording, StopRecording, AddMarker (stub)
```

**RouteCursor** can be fully implemented in Phase 1 — it's pure logic with no external dependencies:
- Wraps `IReadOnlyList<Waypoint>` with an index
- `MoveNext()` / `MovePrevious()` 
- `TrySkipAhead(Position currentPos, float maxDistance, int maxSteps)` — caller computes distance/steps from speed + last position packet
- `ResumeFromCluster(string alias, Position approxPos)` 
- `Current`, `Index`, `PercentComplete`, `IsAtEnd`

**Skip-ahead is dynamically computed, not configured.** The pilot loop calculates `maxDistance` and `maxSteps` each tick based on the character's current speed and the elapsed time since the last position packet. Faster movement → larger skip window. This makes route following predictive and self-adjusting rather than relying on static thresholds that only work at one speed.

### 5.3 Storage — JsonRouteStore

Can be fully implemented:
```csharp
public class JsonRouteStore : IRouteStore
{
    private readonly string _directory;  // From configuration
    
    public Task<Route?> LoadAsync(string name, CancellationToken ct);
    public Task SaveAsync(Route route, CancellationToken ct);
    public Task<IReadOnlyList<RouteMetadata>> ListAsync(CancellationToken ct);
}
```

Uses `System.Text.Json` with a custom `JsonConverter` for the `Waypoint` discriminated union (based on `Type` property).

### 5.4 Tests: OryxBot.Routes.Tests

```
tests/OryxBot.Routes.Tests/
├── RouteCursorTests.cs          — Fully testable
│   ├── MoveNext_AdvancesIndex
│   ├── MovePrevious_AtStart_ReturnsFalse
│   ├── TrySkipAhead_CharacterPastWaypoints_SkipsCorrectly
│   ├── TrySkipAhead_MaxSkipLimitRespected
│   ├── TrySkipAhead_CharacterBehind_NoSkip
│   ├── ResumeFromCluster_FindsClosestMoveStep
│   └── PercentComplete_CalculatedCorrectly
├── JsonRouteStoreTests.cs       — Fully testable  
│   ├── SaveAndReload_Roundtrips
│   ├── Load_InvalidName_ReturnsNull
│   ├── WaypointSerialization_AllTypes
│   └── FloatParsing_InvariantCulture
└── WaypointSerializationTests.cs
    ├── MoveWaypoint_SerializesAndDeserializes
    ├── PortalWaypoint_SerializesAndDeserializes
    └── MarkerWaypoint_SerializesAndDeserializes
```

---

## 6. OryxBot.Protocol — Constants Only

In Phase 1, just port the existing operation/event codes:

```
src/OryxBot.Protocol/
├── Constants/
│   ├── OperationCodes.cs        — Port from existing OryxBot.Albion (all ~350 values)
│   └── EventCodes.cs            — Port from existing OryxBot.Albion (all ~500 values)
├── Events/
│   ├── CharacterMovedEvent.cs   — record : IEvent { float X, float Y }
│   ├── ClusterChangedEvent.cs   — record : IEvent { string ClusterName }
│   ├── CharacterDiedEvent.cs    — record : IEvent { }
│   └── InteractionChangedEvent.cs — record : IEvent { bool IsInteracting }
```

Packet handlers are stubbed — real implementation in Phase 2.

---

## 7. Remaining Projects — Stubs

### OryxBot.Input

```
src/OryxBot.Input/
├── GameController.cs            — Implements IGameController (stub)
├── CoordinateTranslator.cs      — Stub, but define the interface:
│                                  WorldToScreen(Position world, Position character, Size screen) → Point
├── ResponsivePoint.cs           — Port from existing codebase (it's pure math)
```

**ResponsivePoint** and **CoordinateTranslator** can be fully implemented — they're pure math. `GameController` depends on `IInputAdapter` (injected), never on a specific transport — it doesn't know or care whether input goes over VNC, RDP, or a test mock.

### OryxBot.GameState

```
src/OryxBot.GameState/
├── CharacterTracker.cs          — Implements ICharacterTracker (stub, updated via events)
├── MotionDetector.cs            — Idle detection logic (implementable: uses TimeProvider)
├── PositionPredictor.cs         — Velocity extrapolation (implementable: uses TimeProvider)
├── PositionHistory.cs           — Circular buffer of recent positions (implementable: pure data structure)
```

`MotionDetector` and `PositionPredictor` both take `TimeProvider` via constructor. They call `TimeProvider.GetUtcNow()` instead of `DateTime.UtcNow` so Phase 2's simulation can control time advancement.

**PositionHistory** — bounded circular buffer that records timestamped positions as the character moves. Fully implementable in Phase 1 (pure data structure, no external dependencies). Takes `TimeProvider` for timestamps.

```csharp
public class PositionHistory
{
    private readonly int _capacity;  // e.g. 500 entries
    
    public void Record(Position position);                          // Adds with current timestamp
    public IReadOnlyList<TimestampedPosition> GetRecent(int count); // Most recent N entries
    public IReadOnlyList<TimestampedPosition> GetTrail(TimeSpan window); // Entries within time window
    public IEnumerable<Position> GetBacktrackPath(int maxSteps);    // Recent positions in reverse order,
                                                                     // deduplicated (skip entries < 1.0 apart)
    public void Clear();                                             // Reset (e.g. after cluster change)
    public int Count { get; }
}

public readonly record struct TimestampedPosition(Position Position, DateTimeOffset Timestamp);
```

Use cases:
- **Backtracking** — When stuck, the Unsticking state can retrieve the last N distinct positions in reverse and generate temporary waypoints to retrace steps, rather than blindly rotating
- **Trail visualization** — Debug UI can render the character's recent path
- **Loop detection** — If the character keeps visiting the same positions, it's going in circles (future enhancement)
- **Cleared on cluster change** — Old-cluster positions are meaningless in the new cluster

### OryxBot.RemoteDesktop

This project is the **VNC concrete implementation** of `IRemoteConnection` and `IInputAdapter` from Core. Nothing outside this project references VNC types directly — everything goes through the Core interfaces.

```
src/OryxBot.RemoteDesktop/
├── VncConnection.cs             — Implements IRemoteConnection (stub): connect/disconnect lifecycle
├── VncInputAdapter.cs           — Implements IInputAdapter (stub): translates input calls to VNC protocol
├── Configuration/
│   └── VncOptions.cs            — Host, Port (VNC-specific config)
├── ServiceCollectionExtensions.cs — Registers VncConnection as IRemoteConnection, VncInputAdapter as IInputAdapter
```

In production, `AddOryxBotRemoteDesktop` wires these implementations. In tests, mock `IRemoteConnection` and `IInputAdapter` directly — no VNC dependency needed.

---

## 8. Host — Composition Root

```csharp
// hosts/OryxBot.Host/Program.cs
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;
        services.AddOryxBotCore(config);
        services.AddOryxBotWorldGraph(config);
        services.AddOryxBotPilot(config);
        services.AddOryxBotRoutes(config);
        services.AddOryxBotProtocol(config);
        services.AddOryxBotInput(config);
        services.AddOryxBotGameState(config);
        services.AddOryxBotRemoteDesktop(config);
    })
    .UseSerilog((ctx, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration);
    })
    .Build();

await host.RunAsync();
```

```json
// hosts/OryxBot.Host/appsettings.json
{
  "Navigation": {
    "ArrivalDistance": 2.0,
    "IdleTimeout": "00:00:01.300",
    "MaxStuckAttempts": 5,
    "StuckWindow": "00:00:30",
    "DisconnectTimeout": "00:00:15"
  },
  "Display": {
    "ScreenWidth": 1920,
    "ScreenHeight": 1080
  },
  "Vnc": {
    "Host": "localhost",
    "Port": 8010
  },
  "Routes": {
    "Directory": "./routes"
  },
  "WorldGraph": {
    "DataDirectory": "./gamedata"
  },
  "Protocol": {
    "Ports": [5056, 5055, 4535]
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/oryxbot-.log", "rollingInterval": "Day" } }
    ]
  }
}
```

---

## 9. What's Implementable vs Stub in Phase 1

| Component | Status | Notes |
|-----------|--------|-------|
| EventBus | **Implement** | Full implementation + tests |
| Position | **Implement** | Full record struct + tests |
| ObstacleDetector | **Implement** | Pure sliding-window logic + tests |
| RouteCursor | **Implement** | Pure traversal logic + tests |
| JsonRouteStore | **Implement** | JSON read/write + tests |
| ResponsivePoint | **Implement** | Port from existing (pure math) + tests |
| CoordinateTranslator | **Implement** | Isometric math + tests |
| MotionDetector | **Implement** | Timer-based idle detection (via TimeProvider) |
| PositionPredictor | **Implement** | Velocity extrapolation math (via TimeProvider) |
| PositionHistory | **Implement** | Circular buffer + backtrack path generation + tests |
| NavigationOptions | **Implement** | Config POCO with all tunable constants |
| RouteOptions / DisplayOptions / VncOptions / etc. | **Implement** | Config POCOs per project |
| Waypoint JSON converter | **Implement** | Polymorphic serialization |
| All interfaces | **Define** | Just the signatures |
| NavigationController | **Stub** | Needs Phase 2 simulation to validate |
| PilotEngine | **Stub** | Needs NavigationController |
| Pathfinder | **Stub** | Needs game data (Phase 1b) |
| PacketInterceptor | **Stub** | Network code, tested manually |
| GameController | **Stub** | Needs IInputAdapter implementation |
| VncConnection | **Stub** | Implements IRemoteConnection (needs Linux environment) |
| VncInputAdapter | **Stub** | Implements IInputAdapter over VNC protocol |

---

## 10. Acceptance Criteria

- [ ] `dotnet build` succeeds for entire solution
- [ ] `dotnet test` passes all non-skipped tests
- [ ] EventBus tests pass (publish, subscribe, exception isolation)
- [ ] Position tests pass (distance, equality, operators)
- [ ] ObstacleDetector tests pass (sliding window, expiry, max attempts)
- [ ] RouteCursor tests pass (traverse, skip-ahead, resume)
- [ ] JsonRouteStore tests pass (save, load, roundtrip)
- [ ] ResponsivePoint tests pass (anchor scaling across resolutions)
- [ ] CoordinateTranslator tests pass (isometric rotation)
- [ ] PositionHistory tests pass (record, circular eviction, backtrack path, clear on cluster change)
- [ ] Host starts without crashing (logs "started" via Serilog, then idles)
- [ ] All projects have `ServiceCollectionExtensions` with `AddOryxBot{Module}(IConfiguration)`
- [ ] No direct calls to `DateTime.UtcNow` or `DateTimeOffset.UtcNow` — all via `TimeProvider`
- [ ] All async interface methods accept `CancellationToken ct = default`
- [ ] `NavigationOptions` binds from `appsettings.json` and defaults are sane
- [ ] Serilog writes to both console and rolling file
