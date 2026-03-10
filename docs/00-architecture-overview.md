# OryxBot Rebuild — Architecture Overview

> **Target:** .NET 10 · Latest C# · Linux-first  
> **Scope:** Pilot bot — automated route navigation through the Albion Online world  
> **No trade missions.** The bot is a pure **Pilot**: given a destination, it gets there.

---

## Table of Contents

1. [Domain Context](#1-domain-context)
2. [Core Concepts: Pathfinder & Pilot](#2-core-concepts-pathfinder--pilot)
3. [Architecture Principles](#3-architecture-principles)
4. [Project Structure](#4-project-structure)
5. [Core Infrastructure](#5-core-infrastructure)
6. [Navigation: Pathfinder](#6-navigation-pathfinder)
7. [Navigation: Pilot](#7-navigation-pilot)
8. [Network Protocol Layer](#8-network-protocol-layer)
9. [Input Abstraction Layer](#9-input-abstraction-layer)
10. [Game State Tracking](#10-game-state-tracking)
11. [Route System](#11-route-system)
12. [Remote Desktop Layer](#12-remote-desktop-layer)
13. [Cross-Cutting Concerns](#13-cross-cutting-concerns)
14. [Critical Implementation Notes](#14-critical-implementation-notes)

---

## 1. Domain Context

### What is OryxBot?

OryxBot is a **Pilot bot** for Albion Online. Given a destination (a cluster, a world position, or a named location), it autonomously navigates the character there — through portals, across multiple map zones, avoiding obstacles and recovering from failures.

### How the Bot Drives the Game

The game runs in a **Linux VM** with VNC. The bot:
- **Reads the game state** by sniffing the game's Photon network protocol (UDP packets) using SharpPcap — this gives the character's position, movement, cluster changes, death events
- **Controls the game** by sending mouse/keyboard commands through an HTTP bridge to a Java VNC client (`localhost:8010`)

### Key Game Concepts

| Concept | Description |
|---------|-------------|
| **Cluster** | An individual map zone. Changing clusters triggers a loading screen (~8-10s). Each cluster has exits (portals) to neighboring clusters |
| **Portal/Exit** | A transition point at the edge of a cluster that leads to another cluster |
| **World Graph** | The full graph of all clusters and their portal connections — this is what the Pathfinder uses |
| **Route** | An ordered sequence of waypoints (move positions + portal crossings) that guide the Pilot through the world |
| **City** | Major hub cities: Caerleon, Thetford, Fort Sterling, Lymhurst, Bridgewatch, Martlock |
| **Region** | Geographic zones — includes cities plus wilderness areas |

---

## 2. Core Concepts: Pathfinder & Pilot

The bot's navigation is split into two cleanly separated responsibilities:

### Pathfinder

> "Given where I am and where I want to go, what's the sequence of clusters I need to traverse?"

The **Pathfinder** operates on the **World Graph** — a weighted graph of all clusters and their portal connections. It:
- Loads the world graph from extracted game data (cluster XML files)
- Computes the shortest path between two clusters using graph algorithms (A*, Dijkstra)
- Returns a `ClusterPath` — an ordered list of cluster IDs and the exit/portal to use in each
- Has no knowledge of in-game movement, input, or character state
- Is **pure computation** — deterministic, stateless per query, fully testable without any game connection

```
Pathfinder: (currentCluster, targetCluster, WorldGraph) → ClusterPath
```

### Pilot

> "Given a ClusterPath (or a pre-recorded Route), execute the movement to get there."

The **Pilot** is the execution engine. It:
- Takes a `ClusterPath` from the Pathfinder (or a pre-recorded `Route`) and executes it
- Manages the **Navigation State Machine** — following waypoints, course-correcting, unsticking, handling cluster transitions
- Sends input commands (mouse movement, clicks) to actually move the character
- Reacts to game events (death, disconnect, stuck detection)
- Produces serializable `NavigationSnapshot` objects for UI/debugging

```
Pilot: (ClusterPath | Route, GameState, InputAdapter) → character arrives at destination
```

### How They Work Together

```
User says: "Go to Lymhurst"
    │
    ├─ Pathfinder: currentCluster=FortSterling → targetCluster=Lymhurst
    │              Computes: [FortSterling → 0205 → 0206 → 1201 → Lymhurst]
    │              Returns: ClusterPath with exit positions for each hop
    │
    └─ Pilot: Takes ClusterPath, converts to executable waypoints
              For each cluster hop:
                1. Navigate to the exit portal position
                2. Cross the portal (wait for cluster change event)
                3. Resume in new cluster, navigate to next exit
              Until: arrived at destination cluster
```

---

## 3. Architecture Principles

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Runtime | `.NET 10` | Latest LTS, full Linux support |
| Language | Latest C# (14+) | Primary expressions, collection expressions, etc. |
| Platform | Linux-first | Bot runs on Linux VMs |
| DI Framework | `Microsoft.Extensions.DependencyInjection` | Industry standard |
| Event System | Custom `IEventBus` with `IEventHandler<T>` | Lightweight, no MediatR dependency |
| Logging | `Microsoft.Extensions.Logging` + Serilog | Standard .NET logging facade |
| Config | `Microsoft.Extensions.Configuration` + JSON | Structured config |
| Async | `Task`-based with `CancellationToken` throughout | Non-blocking, graceful shutdown |
| Serialization | `System.Text.Json` | Built-in, fast |
| Tests | xUnit + FluentAssertions + NSubstitute | Modern .NET test stack |

### Design Rules

1. **No singletons** — all state is scoped via DI
2. **No `Thread.Sleep`** — use `Task.Delay` with `CancellationToken`
3. **No static mutable state** — everything is injected
4. **Pathfinder is pure** — no side effects, no I/O, fully deterministic
5. **Pilot is reactive** — driven by game events, produces decisions each tick
6. **All decisions serializable** — every `NavigationDecision` can be logged/replayed

---

## 4. Project Structure

```
OryxBot.sln
│
├── src/
│   ├── OryxBot.Core/                      — Shared abstractions, DI, event bus
│   │   ├── Events/
│   │   │   ├── IEventBus.cs
│   │   │   ├── IEventHandler.cs
│   │   │   └── EventBus.cs
│   │   ├── Abstractions/
│   │   │   ├── IGameController.cs         — High-level game input
│   │   │   ├── IInputAdapter.cs           — Low-level platform input
│   │   │   ├── ICharacterTracker.cs       — Character state queries
│   │   │   └── IPacketInterceptor.cs      — Network packet sniffing
│   │   └── Models/
│   │       └── Position.cs                — 2D world position (float X, float Y)
│   │
│   ├── OryxBot.WorldGraph/               — World graph & Pathfinder (pure computation)
│   │   ├── Models/
│   │   │   ├── Cluster.cs                 — ID, display name, type, tier, biome, exits
│   │   │   ├── ClusterExit.cs             — Portal: target cluster ID, position in source cluster
│   │   │   ├── WorldGraph.cs              — Full graph of clusters + adjacency
│   │   │   └── ClusterPath.cs             — Ordered path result: cluster hops + exit positions
│   │   ├── Pathfinder.cs                  — A*/Dijkstra shortest path on WorldGraph
│   │   ├── WorldGraphBuilder.cs           — Constructs WorldGraph from extracted game data JSON
│   │   └── IPathfinder.cs                 — Interface for testability
│   │
│   ├── OryxBot.Pilot/                    — Pilot execution engine (the brain)
│   │   ├── PilotEngine.cs                 — Orchestrates navigation from ClusterPath or Route
│   │   ├── NavigationController.cs        — Behavioral state machine (per-tick decisions)
│   │   ├── NavigationDecision.cs          — Serializable output per tick
│   │   ├── NavigationSnapshot.cs          — Full state snapshot for UI/debugging
│   │   ├── States/
│   │   │   ├── INavigationState.cs
│   │   │   ├── FollowingRouteState.cs     — Default: following waypoints
│   │   │   ├── CorrectingCourseState.cs   — Drifted off, steering back
│   │   │   ├── UnstickingState.cs         — Stuck against obstacle, rotating
│   │   │   ├── TransitioningState.cs      — Cluster portal loading screen
│   │   │   ├── EvadingState.cs            — Responding to attack
│   │   │   ├── LostState.cs              — Unable to recover (failure)
│   │   │   ├── KilledState.cs            — Character died (failure)
│   │   │   └── DisconnectedState.cs      — No position updates (failure)
│   │   └── ObstacleDetector.cs            — Idle-timeout stuck detection
│   │
│   ├── OryxBot.Routes/                   — Route recording, storage, traversal
│   │   ├── Models/
│   │   │   ├── Route.cs
│   │   │   ├── Waypoint.cs                — MoveWaypoint, PortalWaypoint, MarkerWaypoint
│   │   │   └── RouteMetadata.cs
│   │   ├── Recording/
│   │   │   └── RouteRecorder.cs
│   │   ├── Storage/
│   │   │   ├── IRouteStore.cs
│   │   │   └── JsonRouteStore.cs
│   │   └── Traversal/
│   │       └── RouteCursor.cs             — Bidirectional traversal + skip-ahead
│   │
│   ├── OryxBot.Protocol/                 — Photon packet sniffing & game event publishing
│   │   ├── PacketInterceptor.cs
│   │   ├── Handlers/
│   │   │   ├── MoveHandler.cs
│   │   │   ├── ClusterChangeHandler.cs
│   │   │   ├── InteractionHandler.cs
│   │   │   └── DeathHandler.cs
│   │   ├── Events/
│   │   │   ├── CharacterMovedEvent.cs
│   │   │   ├── ClusterChangedEvent.cs
│   │   │   ├── CharacterDiedEvent.cs
│   │   │   └── InteractionChangedEvent.cs
│   │   └── Constants/
│   │       ├── OperationCodes.cs
│   │       └── EventCodes.cs
│   │
│   ├── OryxBot.Input/                    — Input abstraction layer
│   │   ├── GameController.cs              — MoveTowards, StopMovement, etc.
│   │   ├── CoordinateTranslator.cs        — World → screen coordinate math
│   │   ├── Http/
│   │   │   └── HttpInputAdapter.cs        — HTTP bridge to VNC Java client
│   │   └── ResponsivePoint.cs             — Resolution-agnostic screen coordinates
│   │
│   ├── OryxBot.GameState/               — Character state tracking
│   │   ├── CharacterTracker.cs            — Position, cluster, moving state
│   │   ├── MotionDetector.cs              — Idle/running detection
│   │   └── PositionPredictor.cs           — Velocity-based extrapolation
│   │
│   └── OryxBot.RemoteDesktop/           — VNC management
│       ├── VncManager.cs
│       └── VncConfiguration.cs
│
├── tools/
│   └── OryxBot.GameDataExtractor/        — CLI tool for game file extraction (Phase 1b)
│       └── (see phase-1b doc)
│
├── web/
│   └── OryxBot.MapViewer/               — Simple web map viewer (Phase 1b)
│       └── (see phase-1b doc)
│
├── hosts/
│   └── OryxBot.Host/                    — Entry point, DI composition root
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── OryxBot.Core.Tests/
│   ├── OryxBot.WorldGraph.Tests/          — Pathfinder algorithm tests
│   ├── OryxBot.Pilot.Tests/              — Navigation state machine tests
│   ├── OryxBot.Routes.Tests/
│   ├── OryxBot.GameState.Tests/
│   └── OryxBot.Input.Tests/
│
└── tests/e2e/
    ├── OryxBot.Simulation/               — Game world simulation engine
    └── OryxBot.Simulation.Tests/          — E2E tests using the simulator
```

---

## 5. Core Infrastructure

### 5.1 Event Bus

```csharp
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : IEvent;
}

public interface IEvent;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
```

### 5.2 Core Abstractions

```csharp
public interface IGameController
{
    Task MoveTowards(Position target, CancellationToken ct);
    Task MoveInDirection(Vector2 direction, CancellationToken ct);
    Task MoveAwayFrom(Position target, CancellationToken ct);
    Task StopMovement(CancellationToken ct);
    Task Respawn(CancellationToken ct);
}

public interface ICharacterTracker
{
    Position Position { get; }
    Position PredictedPosition { get; }
    string CurrentCluster { get; }
    bool IsMoving { get; }
    bool RecentlyChangedCluster { get; }
}

public interface IInputAdapter
{
    Task SetCursorPosition(int x, int y, CancellationToken ct);
    Task LeftClick(int x, int y, CancellationToken ct);
    Task RightMouseDown(int x, int y, CancellationToken ct);
    Task RightMouseUp(int x, int y, CancellationToken ct);
}
```

---

## 6. Navigation: Pathfinder

The Pathfinder is **pure computation**. It has zero runtime dependencies on the game.

### 6.1 World Graph Model

```csharp
public record Cluster(
    string Id,                    // e.g. "0205", "4205"
    string DisplayName,           // e.g. "Snapshaft Trough" (from localization)
    ClusterType Type,             // City, World, Dungeon, etc.
    int Tier,                     // T3-T8
    string Biome,                 // Swamp, Forest, Mountain, Highland, Steppe
    IReadOnlyList<ClusterExit> Exits);

public record ClusterExit(
    string TargetClusterId,       // Where this exit leads
    Position PositionInCluster,   // World position of the exit in the source cluster
    Position? ArrivalPosition);   // Approximate spawn position in the target cluster

public class WorldGraph
{
    public IReadOnlyDictionary<string, Cluster> Clusters { get; }
    public IReadOnlyList<(string From, string To, ClusterExit Exit)> Edges { get; }
    
    public IReadOnlyList<ClusterExit> GetExits(string clusterId);
    public Cluster? GetCluster(string id);
}
```

### 6.2 Pathfinder Interface

```csharp
public interface IPathfinder
{
    ClusterPath? FindPath(string fromClusterId, string toClusterId);
    ClusterPath? FindPath(string fromClusterId, string toClusterId, PathOptions options);
}

public record ClusterPath(
    IReadOnlyList<ClusterHop> Hops,
    int TotalClusters,
    float EstimatedTravelTime);

public record ClusterHop(
    string ClusterId,
    ClusterExit? ExitToNext);   // null for the final cluster

public record PathOptions(
    bool AvoidPvpZones = false,
    int? MaxTier = null,
    HashSet<string>? AvoidClusters = null);
```

### 6.3 WorldGraphBuilder

Constructs the `WorldGraph` from extracted game data JSON files (output of the GameDataExtractor CLI tool).

```csharp
public class WorldGraphBuilder
{
    public WorldGraph Build(string gameDataDirectory);
    public WorldGraph BuildFromJson(string worldJsonPath);
}
```

---

## 7. Navigation: Pilot

The Pilot is the execution engine — it takes a path and drives the character along it.

### 7.1 Navigation State Machine

```
┌─────────────────────────────────────────────────────────────────┐
│                   OPERATIONAL STATES                            │
│                                                                 │
│  ┌───────────────┐     drift > threshold    ┌────────────────┐  │
│  │ FollowingRoute │ ──────────────────────→ │CorrectingCourse│  │
│  │  (default)     │ ←──────────────────────  │                │  │
│  └───────────────┘   back within range      └────────────────┘  │
│       │                                                         │
│       │ idle > 1.3s         ┌───────────────┐                   │
│       └────────────────────→│  Unsticking   │                   │
│                             │ (rotating)    │                   │
│                             └───────────────┘                   │
│       │ portal reached      ┌───────────────┐                   │
│       └────────────────────→│ Transitioning │                   │
│                             │ (loading)     │                   │
│                             └───────────────┘                   │
│       │ attack detected     ┌───────────────┐                   │
│       └────────────────────→│   Evading     │                   │
│                             │ (fleeing)     │                   │
│                             └───────────────┘                   │
└─────────────────────────────────────────────────────────────────┘
         │ │ │
         FAILURE STATES (terminal)
         ├── unstick failed 5x ─→ [Lost]
         ├── death event ────────→ [Killed]
         └── no packets 15s ────→ [Disconnected]
```

### 7.2 PilotEngine

```csharp
public class PilotEngine
{
    // Navigate using the Pathfinder (automatic path computation)
    public Task NavigateToCluster(string targetClusterId, CancellationToken ct);
    
    // Navigate using a pre-recorded Route
    public Task FollowRoute(Route route, CancellationToken ct);
    
    // Current state
    public NavigationSnapshot Snapshot { get; }
    public bool IsComplete { get; }
    public bool HasFailed { get; }
}
```

### 7.3 NavigationDecision (serializable)

```csharp
public record NavigationDecision
{
    public NavAction Action { get; init; }
    public Position? TargetPosition { get; init; }
    public Vector2? Direction { get; init; }
    public int WaitMs { get; init; }
    public NavigationStateName? TransitionTo { get; init; }
    public string Reason { get; init; } = "";
}

public enum NavAction { None, MoveTowards, MoveInDirection, Stop, Wait }

public enum NavigationStateName
{
    FollowingRoute, CorrectingCourse, Unsticking,
    Transitioning, Evading,
    Lost, Killed, Disconnected  // Failure states
}
```

### 7.4 State Implementations

| State | Behavior | Transition Out |
|-------|----------|----------------|
| **FollowingRoute** | Move towards current waypoint; advance cursor on arrival; skip-ahead if past intermediate waypoints | → CorrectingCourse (drift > 6.0), → Unsticking (idle > 1.3s), → Transitioning (portal waypoint) |
| **CorrectingCourse** | Steer back towards nearest upcoming waypoint | → FollowingRoute (deviation < 3.0), → Lost (deviation > 30.0 for 10s) |
| **Unsticking** | Apply rotation: -π/3 after cluster change, -2π/3 otherwise; hold for 1.5s | → FollowingRoute (movement detected), → Lost (5 attempts in 30s window) |
| **Transitioning** | Wait for cluster change event; skip cursor past portal waypoint | → FollowingRoute (cluster loaded), → Disconnected (no event in 30s) |
| **Evading** | Move away from threat position | → CorrectingCourse (threat cleared) |
| **Lost/Killed/Disconnected** | Terminal. Publishes failure event | Requires external intervention/restart |

---

## 8. Network Protocol Layer

Albion Online uses **Photon** over UDP. SharpPcap sniffs on `eth*`, ports `5056`, `5055`, `4535`.

| Handler | OpCode | Action |
|---------|--------|--------|
| MoveHandler | `OperationCodes.Move` (21) | Update character position |
| ClusterChangeHandler | `OperationCodes.ChangeCluster` (35) | Update cluster, ignore next 5 move packets |
| InteractStartHandler | `OperationCodes.RegisterToObject` (39) | Set interacting = true |
| InteractEndHandler | `OperationCodes.UnRegisterFromObject` (40) | Set interacting = false |
| DeathHandler | `EventCodes.Died` | Publish `CharacterDiedEvent` |

**Move packet suppression:** After cluster change, first 5 move packets are dropped (stale data). Counter only decrements when `msSinceClusterChange < 1000`.

---

## 9. Input Abstraction Layer

```
IGameController (high-level)
    │
    ├── MoveTowards(Position target)
    ├── MoveInDirection(Vector2 direction)
    ├── MoveAwayFrom(Position target)
    ├── StopMovement()
    └── Respawn()
        │
        ↓
    CoordinateTranslator  
        │  world → screen, isometric rotation (-π/4)
        ↓
    IInputAdapter (platform-specific)
        │
        └── HttpInputAdapter → GET localhost:8010/mouse/...
```

### Key Movement Math

1. Calculate direction: `target - characterPosition`
2. Apply isometric rotation: `Transform(direction, Rotation(-π/4))`
3. Scale to screen: `direction × (screenHeight / 10)`
4. Position cursor relative to screen center
5. Hold right-click to move; re-click if character stops but hasn't arrived

---

## 10. Game State Tracking

### CharacterTracker

Replaces the old `LocalCharacter` singleton. Injected via DI, updated by event handlers.

- **Position**: Updated from `CharacterMovedEvent`
- **PredictedPosition**: Velocity-based extrapolation from recent move history
- **IsMoving**: True if position changed by ≥ 0.2 units within the idle timeout (1.3s)
- **CurrentCluster**: Updated from `ClusterChangedEvent`
- **RecentlyChangedCluster**: True for 3× cluster change duration after a portal crossing

---

## 11. Route System

### Route Model (JSON)

```json
{
  "metadata": {
    "name": "FortSterling to Lymhurst",
    "recordedAt": "2026-03-09T12:00:00Z"
  },
  "waypoints": [
    { "type": "move", "x": 12.5, "y": -34.2 },
    { "type": "portal", "clusterName": "SomeMapName" },
    { "type": "move", "x": 5.2, "y": 10.1 },
    { "type": "marker", "markerName": "poi_bank" }
  ]
}
```

### RouteCursor

Bidirectional traversal with skip-ahead logic (up to 4 waypoints, within 6.0 unit distance).

---

## 12. Remote Desktop Layer

VncManager: manages Java VNC client process lifecycle, reads connection status, sets screen dimensions for `ResponsivePoint` coordinate scaling.

---

## 13. Cross-Cutting Concerns

### Logging

`Microsoft.Extensions.Logging` + Serilog. Structured JSON log files, console output.

| Category | Level |
|----------|-------|
| `OryxBot.Pilot` | Info — state transitions, decisions |
| `OryxBot.Pilot.Decision` | Debug — every decision per tick |
| `OryxBot.Pathfinder` | Info — path computations |
| `OryxBot.Protocol` | Debug — parsed packets |
| `OryxBot.GameState` | Debug — position/state changes |
| `OryxBot.Input` | Debug — commands sent |

### Packet Recording for Post-Mortem

Each run produces:
- `run_{id}_decisions.jsonl` — every NavigationDecision
- `run_{id}_positions.jsonl` — character position timeline
- `run_{id}_events.jsonl` — all events published

---

## 14. Critical Implementation Notes

### Things That Seem Simple But Aren't

1. **Cluster change timing**: 8-10s loading screen. Must ignore first 5 move packets after resume (stale data).

2. **Right-mouse movement**: Character moves by holding right-click with cursor offset from screen center. Bot tracks `rightMouseIsDown` state, re-clicks if character stops moving but hasn't arrived (force reclick).

3. **Route skip-ahead**: Navigator looks up to 4 steps ahead to skip past intermediate waypoints the character has already passed. Checks distance from each future waypoint, jumps to farthest one within 6.0 units.

4. **Anti-stuck rotation**: Uses -π/3 rotation shortly after cluster change (shallow angle), -2π/3 otherwise (wider angle). Max 5 attempts in 30-second window before declaring Lost.

5. **Position prediction**: Between network packets (150-350ms apart), the bot extrapolates position using recent velocity. This prevents stale-position decisions.

6. **Isometric camera**: Albion uses -π/4 isometric rotation. All world→screen coordinate transforms must apply this rotation.

### NuGet Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft.Extensions.Hosting` | Generic host |
| `Microsoft.Extensions.Logging` | Logging abstractions |
| `Serilog.Extensions.Hosting` | Serilog integration |
| `SharpPcap` | Network packet capture |
| `PacketDotNet` | Packet parsing |
| `Albion.Network` | Photon protocol parsing |
| `System.Text.Json` | JSON serialization |
| `xunit` | Test framework |
| `FluentAssertions` | Test assertions |
| `NSubstitute` | Mocking |
