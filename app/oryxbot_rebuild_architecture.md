# OryxBot Rebuild — Architecture & Domain Knowledge Extraction

> This document is designed to be fed to multiple agents to rebuild OryxBot from the ground up. It contains everything an agent needs: the existing domain knowledge, the current architecture's strengths and weaknesses, game-specific data, and the complete new architecture proposal.

---

## Table of Contents

1. [Domain Context](#1-domain-context)
2. [Current Architecture Analysis](#2-current-architecture-analysis)
3. [New Architecture Overview](#3-new-architecture-overview)
4. [Project Structure](#4-project-structure)
5. [Core Infrastructure](#5-core-infrastructure)
6. [RouteTaker Bot](#6-routetaker-bot)
7. [Navigation AI](#7-navigation-ai)
8. [Trade Mission Bot](#8-trade-mission-bot)
9. [Game Data & Domain Models](#9-game-data--domain-models)
10. [Network Protocol Layer](#10-network-protocol-layer)
11. [Input Abstraction Layer](#11-input-abstraction-layer)
12. [Remote Desktop Layer](#12-remote-desktop-layer)
13. [API & Remote Control Layer](#13-api--remote-control-layer)
14. [Cross-Cutting Concerns](#14-cross-cutting-concerns)
15. [Pipeline Flow Diagrams](#15-pipeline-flow-diagrams)
16. [File-by-File Build Order](#16-file-by-file-build-order)
17. [Testing Framework](#17-testing-framework)

---

## 1. Domain Context

### What is OryxBot?

OryxBot is a game bot for **Albion Online** that automates **trade missions** — repeatable quests where a player:

1. Goes to a **bank** to deposit reward items and withdraw mission tokens
2. Runs to a **Faction Leader NPC** in a city to accept a trade mission contract
3. Follows a recorded **route** through multiple map zones (clusters) to reach a **Faction Emissary NPC** in the wilderness
4. Interacts with the emissary to **progress** the quest
5. Follows a recorded route **back** to the origin city
6. Interacts with the Faction Leader NPC to **complete** the quest
7. Loops back to step 1

### How the Bot Drives the Game

The game runs in a **Linux VM** with VNC. The bot:
- **Reads the game state** by sniffing the game's Photon network protocol (UDP packets) using SharpPcap — this gives the character's position, movement, cluster changes, NPC interactions, death events, and quest progress
- **Controls the game** by sending mouse/keyboard commands through an HTTP bridge to a Java VNC client (`localhost:8010`) — this simulates right-click movement, left-click NPC interaction, drag-and-drop for banking, and UI navigation

### Key Game Concepts

| Concept | Description |
|---------|-------------|
| **City** | Major hub cities: Caerleon, Thetford, Fort Sterling, Lymhurst, Bridgewatch, Martlock |
| **Region** | Geographic zones — includes cities plus wilderness areas (SnapshaftTrough, DeadveinGully, etc.) |
| **Cluster** | An individual map zone; changing clusters triggers a loading screen (~8-10s) |
| **Faction Leader** | NPC in each city that gives/completes trade missions. Has fixed world positions |
| **Faction Emissary** | NPC in the wilderness where you progress the quest. Has fixed world positions per region |
| **Trade Mission Contract** | A quest with 3 tiers: Minor (3 hearts), Medium (7 hearts), Major (15 hearts). Requires splitting bank tokens accordingly |
| **Route** | A pre-recorded series of move positions and cluster change markers, stored as CSV |

---

## 2. Current Architecture Analysis

### Project Structure (Current)

```
OryxBot.sln
├── OryxBot.Shared          — Contracts, game data (City/Region/Npc), design utilities
├── OryxBot.Albion           — Photon protocol parsing (operation + event codes)
├── OryxBot.Client.Linux     — Main bot application (all business logic)
└── OryxBot.Client.Windows   — Empty/abandoned
```

### Strengths to Keep

1. **Network packet sniffing** for game state — zero-injection, safe approach
2. **Recorded CSV routes** for navigation — flexible, user-recordable
3. **Responsive UI coordinates** ([ResponsivePoint](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Shared/Design/ResponsivePoint.cs#59-62)) — handles different game resolutions
4. **TwoWayEnumerator** — bidirectional route traversal for position skipping and recovery
5. **Movement state tracking** with idle detection, speed calculation, position prediction
6. **Step state machine** pattern for the trade mission pipeline
7. **Cluster change handling** with configurable wait durations

### Problems to Fix

1. **Tight coupling everywhere** — [TradeMissionRun](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/TradeMissionRun.cs#20-169), [TradeMissionStep](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/TradeMissionStep.cs#19-32), `TradeMissionSteps` are all partial classes inside each other. Inner classes can't be swapped or tested
2. **God object**: [LocalCharacter](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/Game/LocalCharacter.cs#10-149) is a singleton with events for move, cluster change, interaction, death, quest progress — everything subscribes directly to it
3. **Static references** — `FileLogger.Common`, `Vnc.*`, `Program.*` are used directly instead of through DI
4. **No separation of route vs trade mission** — the route-following logic is trapped inside [TradeMissionRun](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/TradeMissionRun.cs#20-169)
5. **Thread.Sleep everywhere** — blocking delays should be async, with cancellation token support
6. **ActionFactory contract** is trade-mission-specific (has [BankRewardItems](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/Services/InputActionFactory.cs#123-127), [NpcQuestProgress](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/Services/InputActionFactory.cs#177-181), etc.) instead of being a generic game action layer
7. **Custom service container** wraps `System.ComponentModel.Design.ServiceContainer` in a brittle way — should use `Microsoft.Extensions.DependencyInjection`
8. **VNC management** is a static class with global mutable state

---

## 3. New Architecture Overview

### Design Principles

1. **Service Container** — Use `Microsoft.Extensions.DependencyInjection` for proper DI with lifetimes
2. **Event Bus** — Decouple components via a `MediatR`-style event bus instead of direct event subscriptions
3. **Pipeline Pattern** — Bot execution reads like a pipeline of composable steps
4. **Two-Layer Bot** — RouteTaker (route execution + navigation AI) is independent of Trade Mission logic
5. **Navigation AI as Module** — The route-following intelligence lives in its own module with a clear state machine whose decisions are serializable for UI display
6. **Platform Agnostic** — abstractions for input, remote desktop, and logging that can have Linux/Windows/Mac implementations
7. **Async-First** — All delays use `Task.Delay` with `CancellationToken` instead of `Thread.Sleep`
8. **No Singletons** — [LocalCharacter](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/Game/LocalCharacter.cs#10-149) becomes a scoped service, not a static `Instance`

### Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| DI Framework | `Microsoft.Extensions.DependencyInjection` | Industry standard, no custom container needed |
| Event System | Custom `IEventBus` with `IEventHandler<T>` | Lightweight, avoids MediatR dependency, easy to understand |
| Logging | `Microsoft.Extensions.Logging` + Serilog | Standard .NET logging facade |
| Config | `Microsoft.Extensions.Configuration` + JSON files | Structured config, no hard-coded values |
| Async Pattern | [Task](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Client.Linux/Bot/Game/MovementStateTracker.cs#70-76)-based with `CancellationToken` throughout | Non-blocking, graceful shutdown |
| Serialization | `System.Text.Json` | Built-in, fast, no Newtonsoft dependency |
| Build Target | `net8.0` (or latest LTS) | Modern .NET, cross-platform |

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
│   │   ├── Pipeline/
│   │   │   ├── IStep.cs                   — Base step interface (Name, IsComplete, TickAsync)
│   │   │   ├── PipelineExecutor.cs        — Runs steps sequentially
│   │   │   └── PipelineContext.cs
│   │   └── Abstractions/
│   │       ├── IGameController.cs         — High-level game input (move, click, interact)
│   │       ├── IInputAdapter.cs           — Low-level platform input (mouse, keyboard)
│   │       ├── ICharacterTracker.cs       — Character state (position, cluster, moving)
│   │       └── IPacketInterceptor.cs      — Network packet sniffing
│   │
│   ├── OryxBot.GameData/                  — Albion game knowledge (pure data, no logic)
│   │   ├── Cities.cs
│   │   ├── Regions.cs
│   │   ├── NpcPositions.cs
│   │   ├── UiCoordinates.cs
│   │   └── ContractTypes.cs
│   │
│   ├── OryxBot.Protocol/                  — Photon packet sniffing & game event publishing
│   │   ├── PacketInterceptor.cs           — SharpPcap listener, replaces PhotonSnifferService
│   │   ├── Handlers/
│   │   │   ├── MoveHandler.cs
│   │   │   ├── ClusterChangeHandler.cs
│   │   │   ├── InteractionHandler.cs
│   │   │   ├── DeathHandler.cs
│   │   │   └── QuestProgressHandler.cs
│   │   ├── Events/
│   │   │   ├── CharacterMovedEvent.cs
│   │   │   ├── ClusterChangedEvent.cs
│   │   │   ├── CharacterDiedEvent.cs
│   │   │   ├── InteractionChangedEvent.cs
│   │   │   └── QuestProgressedEvent.cs
│   │   └── Constants/
│   │       ├── OperationCodes.cs
│   │       └── EventCodes.cs
│   │
│   ├── OryxBot.Input/                     — Input abstraction layer
│   │   ├── GameController.cs              — MoveTowards, InteractWith, ClickUi, StopMovement
│   │   ├── CoordinateTranslator.cs        — Game-world → screen coordinate math
│   │   ├── Http/
│   │   │   └── HttpInputAdapter.cs        — HTTP bridge to VNC Java client
│   │   └── ResponsivePoint.cs             — Resolution-agnostic screen coordinates
│   │
│   ├── OryxBot.RemoteDesktop/             — VNC/Remote desktop management
│   │   ├── VncManager.cs                  — Process lifecycle, connection, screen dimensions
│   │   └── VncConfiguration.cs
│   │
│   ├── OryxBot.GameState/                 — Character state tracking
│   │   ├── CharacterTracker.cs            — Replaces LocalCharacter singleton
│   │   ├── MotionDetector.cs              — Idle/running detection with timeout
│   │   ├── PositionPredictor.cs           — Velocity-based position extrapolation
│   │   └── Events/
│   │       ├── CharacterBecameIdleEvent.cs
│   │       └── CharacterStateChangedEvent.cs
│   │
│   ├── OryxBot.Routes/                    — Route recording, storage, and traversal
│   │   ├── Models/
│   │   │   ├── Route.cs
│   │   │   ├── Waypoint.cs                — Base: MoveWaypoint, PortalWaypoint, MarkerWaypoint
│   │   │   └── RouteMetadata.cs
│   │   ├── Recording/
│   │   │   └── RouteRecorder.cs
│   │   ├── Storage/
│   │   │   ├── IRouteStore.cs
│   │   │   └── JsonRouteStore.cs           — JSON-only storage
│   │   └── Traversal/
│   │       └── RouteCursor.cs             — Bidirectional traversal + skip-ahead
│   │
│   ├── OryxBot.Navigation/               — Navigation AI (behavioral state machine)
│   │   ├── NavigationController.cs        — The brain: evaluates state, produces decisions
│   │   ├── NavigationDecision.cs          — Serializable output per tick (for UI)
│   │   ├── NavigationSnapshot.cs          — Full serializable state for debugging/UI
│   │   ├── States/
│   │   │   ├── INavigationState.cs        — State interface
│   │   │   ├── FollowingRouteState.cs     — Default: following waypoints
│   │   │   ├── CorrectingCourseState.cs   — Drifted off, steering back
│   │   │   ├── UnstickingState.cs         — Stuck against obstacle, rotating
│   │   │   ├── TransitioningState.cs      — Cluster portal loading screen
│   │   │   ├── EvadingState.cs            — Responding to attack
│   │   │   ├── LostState.cs              — Unable to recover onto route (failure)
│   │   │   ├── KilledState.cs            — Character died (failure)
│   │   │   └── DisconnectedState.cs      — No position updates received (failure)
│   │   └── ObstacleDetector.cs            — Idle-timeout stuck detection
│   │
│   ├── OryxBot.RouteTaker/               — Route execution engine (Bot #1)
│   │   ├── RouteTakerEngine.cs            — Orchestrates recording and route execution
│   │   └── RouteExecutor.cs               — Ticks the NavigationController against a route
│   │
│   ├── OryxBot.TradeMission/             — Trade mission bot (Bot #2, built on RouteTaker)
│   │   ├── TradeMissionEngine.cs          — Orchestrates the full trade mission loop
│   │   ├── Steps/                         — Each step has a clear type
│   │   │   ├── TradeMissionStep.cs        — Base class with step type enum
│   │   │   ├── RouteFollowStep.cs         — Type: Route — delegates to RouteExecutor
│   │   │   │   ├── FollowRouteToDestination.cs
│   │   │   │   └── FollowRouteBack.cs
│   │   │   ├── NavigateToStep.cs           — Type: Navigate — move to a world position
│   │   │   │   ├── NavigateToBank.cs
│   │   │   │   └── NavigateToQuestNpc.cs
│   │   │   └── NpcInteractionStep.cs      — Type: Interaction — interact with NPC + UI
│   │   │       ├── BankItems.cs
│   │   │       ├── AcceptQuest.cs
│   │   │       ├── ProgressQuest.cs
│   │   │       └── CompleteQuest.cs
│   │   ├── TradeMissionRouteProvider.cs
│   │   └── TradeMissionConfiguration.cs
│   │
│   └── OryxBot.Api/                      — Remote control API (optional)
│       ├── ApiGateway.cs                  — HTTP client with auth + encryption
│       ├── StatusReporter.cs              — Pushes state updates to dashboard
│       ├── RemoteCommandListener.cs       — Pusher WebSocket command receiver
│       └── PayloadEncryptor.cs            — AES-256-CBC
│
├── hosts/
│   └── OryxBot.Host/                     — Entry point, DI composition root
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── OryxBot.Core.Tests/
│   ├── OryxBot.Routes.Tests/
│   ├── OryxBot.Navigation.Tests/          — Navigation AI state machine tests
│   ├── OryxBot.GameState.Tests/
│   ├── OryxBot.Input.Tests/
│   └── OryxBot.TradeMission.Tests/
│
└── tests/e2e/
    ├── OryxBot.Simulation/                — Game world simulation engine
    │   ├── WorldSimulator.cs              — Physics/position simulation
    │   ├── InputRecorder.cs               — Captures all input commands
    │   ├── Profiles/
    │   │   ├── ISimulationProfile.cs
    │   │   ├── PerfectProfile.cs
    │   │   ├── RealisticProfile.cs
    │   │   ├── HighLatencyProfile.cs
    │   │   └── AdversarialProfile.cs
    │   ├── Obstacles/
    │   │   ├── WallObstacle.cs
    │   │   └── SlowZone.cs
    │   └── Fixtures/
    │       ├── RouteFixtures.cs
    │       └── SimulationBuilder.cs
    └── OryxBot.Simulation.Tests/
        ├── RouteFollowingTests.cs
        ├── CourseCorrectionTests.cs
        ├── ClusterChangeTests.cs
        ├── StuckRecoveryTests.cs
        ├── DeathRecoveryTests.cs
        └── FullTradeMissionTests.cs
```

---

## 5. Core Infrastructure

### 5.1 Event Bus

Replaces direct event handler subscriptions (`+=`) with a decoupled publish/subscribe model.

```csharp
// OryxBot.Core/Events/IEventBus.cs
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : IEvent;
}

public interface IEvent { }

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
```

Implementation uses `ConcurrentDictionary<Type, List<object>>` to store handlers. Registration happens automatically via DI scanning.

### 5.2 Pipeline Executor

All bot actions are expressed as steps. The executor ticks each step until it completes, then moves to the next.

```csharp
// OryxBot.Core/Pipeline/IStep.cs
public interface IStep
{
    string Name { get; }
    bool IsComplete { get; }
    Task TickAsync(PipelineContext context, CancellationToken ct);
}

// OryxBot.Core/Pipeline/PipelineExecutor.cs
public class PipelineExecutor
{
    private readonly ILogger<PipelineExecutor> _logger;
    private readonly IEventBus _eventBus;

    public async Task RunAsync(
        IReadOnlyList<IStep> steps,
        PipelineContext context,
        CancellationToken ct)
    {
        foreach (var step in steps)
        {
            _logger.LogInformation("Starting step: {Step}", step.Name);
            await _eventBus.PublishAsync(new StepChangedEvent(step.Name), ct);

            while (!step.IsComplete && !ct.IsCancellationRequested)
            {
                await step.TickAsync(context, ct);
                await Task.Delay(step is IHasTickInterval d ? d.TickInterval : 10, ct);
            }
        }
    }
}
```

### 5.3 Host Composition Root

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddOryxBotCore();
        services.AddOryxBotProtocol();
        services.AddOryxBotGameState();
        services.AddOryxBotInput();
        services.AddOryxBotRoutes();
        services.AddOryxBotNavigation();
        services.AddOryxBotRouteTaker();
        services.AddOryxBotTradeMission();
        services.AddOryxBotApi(); // Optional
    })
    .Build();
```

---

## 6. RouteTaker Bot

The standalone route bot — records routes and replays them using the Navigation AI. No knowledge of trade missions.

### 6.1 Route Model

```csharp
public class Route
{
    public RouteMetadata Metadata { get; set; } = new();
    public List<Waypoint> Waypoints { get; set; } = new();
}

public class RouteMetadata
{
    public string Name { get; set; } = "";
    public string? OriginCity { get; set; }
    public string? DestinationRegion { get; set; }
    public DateTime RecordedAt { get; set; }
}

// OryxBot.Routes/Models/Waypoint.cs
public abstract record Waypoint(string Type);
public record MoveWaypoint(float X, float Y) : Waypoint("move");
public record PortalWaypoint(string ClusterName, string? Alias = null) : Waypoint("portal");
public record MarkerWaypoint(string MarkerName) : Waypoint("marker");
```

### 6.2 Route Cursor

Tracks position within a route. Supports bidirectional traversal and skip-ahead.

```csharp
// OryxBot.Routes/Traversal/RouteCursor.cs
public class RouteCursor
{
    private readonly IReadOnlyList<Waypoint> _waypoints;
    private int _index = -1;

    public Waypoint? Current => ...;
    public int Index => _index;
    public float PercentComplete => ...;

    public bool MoveNext() { ... }
    public bool MovePrevious() { ... }
    public bool TrySkipAhead(Position currentPos, float maxDistance, int maxSkip = 4) { ... }
    public bool ResumeFromCluster(string alias, Position approxPos) { ... }
}
```

### 6.3 Route Executor

Feeds waypoints from the `RouteCursor` into the `NavigationController` each tick.

```csharp
// OryxBot.RouteTaker/RouteExecutor.cs
public class RouteExecutor
{
    private readonly NavigationController _navigation;
    private readonly ICharacterTracker _character;
    private RouteCursor? _cursor;

    public float PercentComplete => _cursor?.PercentComplete ?? 0;
    public bool IsComplete { get; private set; }
    public NavigationSnapshot Snapshot => _navigation.TakeSnapshot();

    public void LoadRoute(Route route) { ... }

    public async Task TickAsync(CancellationToken ct)
    {
        var decision = _navigation.Evaluate(_cursor, _character);
        await _navigation.Execute(decision, ct);
        
        if (_cursor.IsAtEnd)
            IsComplete = true;
    }
}
```

### 6.4 Route Recorder

```csharp
// OryxBot.Routes/Recording/RouteRecorder.cs
public class RouteRecorder : IEventHandler<CharacterMovedEvent>,
                              IEventHandler<ClusterChangedEvent>
{
    private readonly List<Waypoint> _recorded = new();

    public void StartRecording() { ... }
    public Route StopRecording() { ... }
    public void AddMarker(string name) { ... }
}
```

### 6.5 Route Storage (JSON only)

```csharp
// OryxBot.Routes/Storage/IRouteStore.cs
public interface IRouteStore
{
    Task<Route?> LoadAsync(string name, CancellationToken ct = default);
    Task SaveAsync(Route route, CancellationToken ct = default);
    Task<IReadOnlyList<RouteMetadata>> ListAsync(CancellationToken ct = default);
}
```

JSON format:
```json
{
  "metadata": {
    "name": "FortSterling to Lymhurst",
    "originCity": "fort-sterling",
    "destinationRegion": "aspenwood",
    "recordedAt": "2026-03-09T12:00:00Z"
  },
  "waypoints": [
    { "type": "move", "x": 12.5, "y": -34.2 },
    { "type": "move", "x": 15.1, "y": -33.8 },
    { "type": "portal", "clusterName": "SomeMapName" },
    { "type": "move", "x": 5.2, "y": 10.1 },
    { "type": "marker", "markerName": "quest_progress" },
    { "type": "move", "x": -5.0, "y": 15.3 }
  ]
}
```

---

## 7. Navigation AI

The navigation intelligence lives in `OryxBot.Navigation` as a **behavioral state machine**. It is the brain of the RouteTaker bot — it decides what to do each tick based on the character's state relative to the route.

Every decision is captured in a serializable `NavigationDecision`, making the AI's behavior fully reproducible, debuggable, and displayable in a UI.

### 7.1 State Machine

```
┌─────────────────────────────────────────────────────────────────────┐
│                   OPERATIONAL STATES                         │
│                                                               │
│  ┌───────────────┐     drift > threshold    ┌────────────────┐   │
│  │ FollowingRoute │ ────────────────→ │ CorrectingCourse │   │
│  │  (default)     │ ←──────────────── │                  │   │
│  └───────────────┘  back within range    └────────────────┘   │
│       │                                                       │
│       │ no movement for 1.3s    ┌───────────────┐               │
│       └───────────────────────→ │  Unsticking   │               │
│                                │ (rotating)    │               │
│                                └───────────────┘               │
│       │ portal reached           ┌───────────────┐               │
│       └───────────────────────→ │ Transitioning │               │
│                                │ (loading)     │               │
│                                └───────────────┘               │
│       │ attack detected          ┌───────────────┐               │
│       └───────────────────────→ │   Evading     │               │
│                                │ (fleeing)     │               │
│                                └───────────────┘               │
└─────────────────────────────────────────────────────────────────────┘
         │ │ │
         │ │ │  FAILURE STATES (terminal — require external intervention)
         │ │ │
         │ │ └─── unstick failed 5x ───→ [Lost]           — unable to recover
         │ └───── death event ─────────→ [Killed]         — character died
         └─────── no packets 15s ──────→ [Disconnected]   — connection lost
```

### 7.2 Navigation Controller

```csharp
// OryxBot.Navigation/NavigationController.cs
public class NavigationController
{
    private readonly IGameController _gameController;
    private readonly ICharacterTracker _character;
    private readonly ObstacleDetector _obstacleDetector;
    private readonly IEventBus _eventBus;
    private readonly ILogger<NavigationController> _logger;

    private INavigationState _currentState;
    public NavigationStateName CurrentStateName => _currentState.Name;

    /// Evaluate the current situation and produce a decision
    public NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker character)
    {
        // 1. Check failure conditions (death, disconnect, max stuck attempts)
        // 2. Check reactive conditions (attack detected, stuck)
        // 3. Check operational conditions (at portal, drifted off course)
        // 4. Delegate to current state's Evaluate method
        // 5. Handle state transitions
        var decision = _currentState.Evaluate(cursor, character);
        
        if (decision.TransitionTo != null)
        {
            _logger.LogInformation("Navigation: {From} → {To}",
                _currentState.Name, decision.TransitionTo);
            _currentState = ResolveState(decision.TransitionTo.Value);
        }
        
        return decision;
    }

    /// Execute the decision (send input commands)
    public async Task Execute(NavigationDecision decision, CancellationToken ct)
    {
        switch (decision.Action)
        {
            case NavAction.MoveTowards:
                await _gameController.MoveTowards(decision.TargetPosition!.Value, ct);
                break;
            case NavAction.MoveInDirection:
                await _gameController.MoveInDirection(decision.Direction!.Value, ct);
                break;
            case NavAction.Stop:
                await _gameController.StopMovement(ct);
                break;
            case NavAction.Wait:
                await Task.Delay(decision.WaitMs, ct);
                break;
            case NavAction.None:
                break;
        }
    }

    /// Serializable snapshot of full AI state for UI/debugging
    public NavigationSnapshot TakeSnapshot() => new()
    {
        StateName = _currentState.Name,
        LastDecision = _lastDecision,
        CurrentWaypointIndex = ...,
        PercentComplete = ...,
        DeviationFromRoute = ...,
        StuckAttempts = _obstacleDetector.AttemptCount,
        CharacterPosition = _character.Position,
        CharacterState = _character.State,
    };
}
```

### 7.3 Navigation Decision (serializable output)

```csharp
// OryxBot.Navigation/NavigationDecision.cs
public record NavigationDecision
{
    public NavAction Action { get; init; }            // What to do
    public Position? TargetPosition { get; init; }    // For MoveTowards
    public Vector2? Direction { get; init; }           // For MoveInDirection
    public int WaitMs { get; init; }                   // For Wait
    public NavigationStateName? TransitionTo { get; init; } // State change
    public string Reason { get; init; } = "";          // Human-readable explanation
}

public enum NavAction { None, MoveTowards, MoveInDirection, Stop, Wait }

public enum NavigationStateName
{
    FollowingRoute,
    CorrectingCourse,
    Unsticking,
    Transitioning,
    Evading,
    Lost,       // Failure
    Killed,     // Failure
    Disconnected // Failure
}
```

### 7.4 Navigation Snapshot (for UI)

```csharp
// OryxBot.Navigation/NavigationSnapshot.cs
public record NavigationSnapshot
{
    public NavigationStateName StateName { get; init; }
    public NavigationDecision? LastDecision { get; init; }
    public int CurrentWaypointIndex { get; init; }
    public float PercentComplete { get; init; }
    public float DeviationFromRoute { get; init; }
    public int StuckAttempts { get; init; }
    public Position CharacterPosition { get; init; }
    public string CharacterState { get; init; } = "";
    public bool IsFailureState => StateName is NavigationStateName.Lost
        or NavigationStateName.Killed or NavigationStateName.Disconnected;
}
```

### 7.5 State Implementations

Each state implements `INavigationState`:

```csharp
public interface INavigationState
{
    NavigationStateName Name { get; }
    NavigationDecision Evaluate(RouteCursor cursor, ICharacterTracker character);
}
```

| State | Behavior | Transition Out |
|-------|----------|----------------|
| **FollowingRoute** | Move towards current waypoint; advance cursor on arrival; skip-ahead if past intermediate waypoints | → CorrectingCourse (drift > 6.0), → Unsticking (idle > 1.3s), → Transitioning (portal waypoint), → Killed (death event) |
| **CorrectingCourse** | Steer back towards nearest upcoming waypoint; re-evaluate distance each tick | → FollowingRoute (deviation < 3.0), → Lost (deviation > 30.0 for 10s) |
| **Unsticking** | Apply rotation: -π/3 after cluster change, -2π/3 otherwise; hold for 1.5s | → FollowingRoute (movement detected), → Lost (5 attempts in 30s window) |
| **Transitioning** | Wait for cluster change event; after cluster loads, skip cursor to next move waypoint after portal | → FollowingRoute (cluster loaded), → Disconnected (no event in 30s) |
| **Evading** | Move away from threat position; re-evaluate threat state each tick | → CorrectingCourse (threat cleared) |
| **Lost** | Stopped. Publishes `NavigationFailedEvent`. Awaits external reset | Terminal — requires restart |
| **Killed** | Stopped. Publishes `CharacterKilledEvent`. Awaits respawn handling | Terminal — handled by TradeMissionEngine |
| **Disconnected** | Stopped. No position packets received for 15s. Publishes `ConnectionLostEvent` | Terminal — requires reconnection |

### 7.6 Obstacle Detector

```csharp
// OryxBot.Navigation/ObstacleDetector.cs
public class ObstacleDetector
{
    private readonly Queue<DateTime> _stuckTimestamps = new();
    public int AttemptCount => _stuckTimestamps.Count;
    public bool IsStuck => AttemptCount >= MaxAttempts; // MaxAttempts = 5

    public void RecordAttempt() { ... }
    public void PurgeExpired(TimeSpan window) { ... }  // 30-second window
    public void Reset() { ... }
}
```

---

## 8. Trade Mission Bot

Built **on top** of the RouteTaker. Uses `RouteExecutor` for navigation but adds trade-mission-specific steps.

### 8.1 Step Type Abstraction

Every trade mission step inherits from a base class that declares its **type**, making it clear what category of work each step performs:

```csharp
// OryxBot.TradeMission/Steps/TradeMissionStep.cs
public enum StepType { Route, Navigate, Interaction }

public abstract class TradeMissionStep : IStep
{
    public abstract StepType Type { get; }
    public abstract string Name { get; }
    public bool IsComplete { get; protected set; }
    public abstract Task TickAsync(PipelineContext context, CancellationToken ct);
}
```

| Step Type | Description | Base Class |
|-----------|-------------|------------|
| **Route** | Follow a recorded route using `RouteExecutor` + Navigation AI | `RouteFollowStep` |
| **Navigate** | Move to a specific world position (short distance, no recorded route) | `NavigateToStep` |
| **Interaction** | Interact with an NPC and navigate game UI | `NpcInteractionStep` |

### 8.2 Pipeline

```
┌─→ NavigateToBank        [Navigate]     — walk to bank NPC
│   ↓
│   BankItems             [Interaction]  — deposit rewards, withdraw tokens
│   ↓
│   NavigateToQuestNpc    [Navigate]     — walk to quest NPC
│   ↓
│   AcceptQuest           [Interaction]  — open quest UI, select contract
│   ↓
│   FollowRouteToDestination [Route]     — follow recorded route via RouteExecutor
│   ↓
│   ProgressQuest         [Interaction]  — interact with emissary NPC
│   ↓
│   FollowRouteBack       [Route]        — follow recorded route back
│   ↓
│   CompleteQuest         [Interaction]  — turn in quest at NPC
│   ↓
└───┘ (loop)
```

### 8.3 Route Follow Steps (delegate to RouteExecutor)

```csharp
// OryxBot.TradeMission/Steps/RouteFollowStep.cs
public abstract class RouteFollowStep : TradeMissionStep
{
    public override StepType Type => StepType.Route;
    protected readonly RouteExecutor _executor;

    public override bool IsComplete => _executor.IsComplete;

    public override Task TickAsync(PipelineContext ctx, CancellationToken ct)
        => _executor.TickAsync(ct);
}

// Concrete:
public class FollowRouteToDestination : RouteFollowStep { ... }
public class FollowRouteBack : RouteFollowStep { ... }
```

### 8.4 Navigate-To Steps (short distance movement)

```csharp
// OryxBot.TradeMission/Steps/NavigateToStep.cs
public abstract class NavigateToStep : TradeMissionStep
{
    public override StepType Type => StepType.Navigate;
    protected abstract Position TargetPosition { get; }
    protected readonly IGameController _gameController;
    protected readonly ICharacterTracker _character;
    protected float ArrivalThreshold => 3f;

    public override async Task TickAsync(PipelineContext ctx, CancellationToken ct)
    {
        var dist = Position.Distance(_character.Position, TargetPosition);
        if (dist < ArrivalThreshold)
        {
            await _gameController.StopMovement(ct);
            IsComplete = true;
        }
        else
        {
            await _gameController.MoveTowards(TargetPosition, ct);
        }
    }
}

// Concrete:
public class NavigateToBank : NavigateToStep { ... }
public class NavigateToQuestNpc : NavigateToStep { ... }
```

### 8.5 NPC Interaction Steps (UI automation)

```csharp
// OryxBot.TradeMission/Steps/NpcInteractionStep.cs
public abstract class NpcInteractionStep : TradeMissionStep
{
    public override StepType Type => StepType.Interaction;
    protected readonly IGameController _gameController;
    protected readonly ICharacterTracker _character;

    protected abstract Task PerformInteraction(CancellationToken ct);

    public override async Task TickAsync(PipelineContext ctx, CancellationToken ct)
    {
        if (!_character.IsInteracting)
        {
            // Walk to NPC and click to open interaction panel
            await AttemptInteraction(ct);
        }
        else
        {
            await PerformInteraction(ct);
            IsComplete = true;
        }
    }
}

// Concrete:
public class AcceptQuest : NpcInteractionStep
{
    protected override async Task PerformInteraction(CancellationToken ct)
    {
        // 1. Click Trade Missions tab
        // 2. Click destination contract tab (calculated by city ordering)
        // 3. Click contract tier (Minor/Medium/Major)
        // 4. Click Accept
    }
}
public class BankItems : NpcInteractionStep { ... }
public class ProgressQuest : NpcInteractionStep { ... }
public class CompleteQuest : NpcInteractionStep { ... }
```

### 8.6 Trade Mission Route Provider

```csharp
public class TradeMissionRouteProvider
{
    private readonly IRouteStore _routeStore;

    // Routes per city (JSON files):
    //   route_{city}_trademission.json  → split at "quest" marker into (toNpc, back)
    //   route_{city}_bank_to_quest.json
    //   route_{city}_quest_to_bank.json
    
    public async Task<TradeMissionRoutes> GetRoutesForCity(City city, CancellationToken ct)
    {
        var mainRoute = await _routeStore.LoadAsync($"route_{city.Code}_trademission", ct);
        var (toNpc, back) = SplitAtQuestMarker(mainRoute);
        ...
    }
}
```

---

## 9. Game Data & Domain Models

All hard-coded game knowledge lives in `OryxBot.GameData`. This data changes when the game updates.

### 9.1 Cities

```csharp
public enum City { Caerleon, Thetford, FortSterling, Lymhurst, Bridgewatch, Martlock }

public static class CityData
{
    // Display names: "Fort Sterling", etc.
    // Slug codes: "fort-sterling", etc.
    // Mission ordering per city (which destination appears in which tab position):
    //   Caerleon     → [Thetford, Lymhurst, FortSterling, Martlock, Bridgewatch]
    //   Bridgewatch  → [Caerleon, Lymhurst, Martlock, FortSterling, Thetford]
    //   FortSterling → [Caerleon, Thetford, Lymhurst, Bridgewatch, Martlock]
    //   Lymhurst     → [Caerleon, Bridgewatch, FortSterling, Thetford, Martlock]
    //   Martlock     → [Caerleon, Bridgewatch, Thetford, FortSterling, Lymhurst]
    //   Thetford     → [Caerleon, Martlock, FortSterling, Lymhurst, Bridgewatch]
}
```

### 9.2 NPC Positions (World Coordinates)

(Content unchanged — same position data)

### 9.3 UI Coordinates

All UI positions are expressed as [ResponsivePoint](file:///a:/Projects/RiderProjects/OryxBot/OryxBot.Shared/Design/ResponsivePoint.cs#59-62) — coordinates relative to a reference resolution, scaled to current.

### 9.4 Contract Types

```csharp
public enum ContractType
{
    Minor = 3,    // split 2 times (value - 1)
    Medium = 7,   // split 6 times
    Major = 15    // split 14 times
}

---

## 10. Network Protocol Layer

### 10.1 How It Works

Albion Online uses **Photon** over UDP. The `PacketInterceptor` sniffs via **SharpPcap** on `eth*`, filtering UDP ports `5056`, `5055`, `4535`.

### 10.2 Handlers

| Handler | Type | Code | Action |
|---------|------|------|--------|
| **MoveHandler** | Request | `OperationCodes.Move` | Update character position |
| **ClusterChangeHandler** | Request | `OperationCodes.ChangeCluster` | Update cluster, set `IgnoreMovePackets = 5` |
| **InteractStartHandler** | Request | `OperationCodes.RegisterToObject` | Set `Interacting = true` |
| **InteractEndHandler** | Request | `OperationCodes.UnRegisterFromObject` | Set `Interacting = false` |
| **QuestProgressHandler** | Request | `OperationCodes.QuestGiverRequest` | Publish `QuestProgressedEvent` |
| **DeathHandler** | Event | `EventCodes.Died` | Wait 2s, publish `CharacterDiedEvent` |

### 10.3 Move Packet Suppression

After cluster change, first 5 move packets are ignored (stale positions). Counter only decrements when `msSinceClusterChange < 1000`.

### 10.4 Protocol Constants

Existing `OperationCodes.cs` (~350 values) and `EventCodes.cs` (~500 values) kept as-is.

---

## 11. Input Abstraction Layer

### 11.1 Architecture

```
IGameController (high-level game actions)
    │
    ├── MoveTowards(Position target)
    ├── MoveInDirection(Vector2 direction)
    ├── MoveAwayFrom(Position target)
    ├── InteractAt(Position target)
    ├── ClickUi(ResponsivePoint point)
    ├── DragAndDrop(ResponsivePoint from, ResponsivePoint to)
    ├── StopMovement()
    └── Respawn()
        │
        ↓
    CoordinateTranslator (math)
        │
        ├── World positions → screen-relative directions
        ├── Isometric rotation (-π/4 for Albion camera)
        │
        ↓
    IInputAdapter (platform-specific)
        │
        ├── SetCursorPosition, LeftClick, RightMouseDown/Up
        └── KeyPress, KeyDown, KeyUp
```

### 11.2 HTTP Input Adapter

Sends HTTP requests to a local Java process at `localhost:8010`:

```
GET /mouse/move?x={x}&y={y}
GET /mouse/click?x={x}&y={y}&mouse=left|right
GET /mouse/down?x={x}&y={y}&mouse=left|right
GET /mouse/up?x={x}&y={y}&mouse=left|right
GET /key/press?key={key}
```

---

## 12. Remote Desktop Layer

### 12.1 VNC Manager

Manages a Java VNC client process. Key behaviors:
- Starts Java process with GhostAWT toolkit (headless rendering)
- Reads stdout for connection status and screen dimensions
- Auto-restarts on non-zero exit codes
- Sets `ResponsivePoint.CurrentResolution` based on VNC dimensions × scaling
- Connection timeout: 15 seconds

---

## 13. API & Remote Control Layer

(API endpoints, WebSocket commands, auth, encryption — all unchanged, just renamed `ApiClient` → `ApiGateway`, `ApiNotifier` → `StatusReporter`, `WebSocketCommandReceiver` → `RemoteCommandListener`, `EncryptionService` → `PayloadEncryptor`)

---

## 14. Cross-Cutting Concerns

### 14.1 Logging Architecture

Use `Microsoft.Extensions.Logging` with **Serilog**. Each module gets its own named logger for clean filtering.

| Logger Category | What It Logs | Level |
|----------------|-------------|-------|
| `OryxBot.Navigation` | State transitions, decisions (reason), deviation metrics | Info |
| `OryxBot.Navigation.Decision` | Every `NavigationDecision` per tick (high volume) | Debug |
| `OryxBot.Protocol` | Parsed packet events (move, cluster, death, interact) | Debug |
| `OryxBot.Protocol.Raw` | Raw packet hex dump for post-mortem replay | Trace |
| `OryxBot.GameState` | Character state changes (idle/moving), position updates | Debug |
| `OryxBot.Input` | Commands sent (cursor position, clicks, drags) | Debug |
| `OryxBot.Routes` | Route load/save, cursor advance, skip-ahead, waypoint reached | Info |
| `OryxBot.TradeMission` | Step transitions, quest accept/complete, banking actions | Info |
| `OryxBot.Api` | API requests, WebSocket commands received, auth events | Info |
| `OryxBot.RemoteDesktop` | VNC connection state, screen dimension changes, restarts | Info |
| `OryxBot.Pipeline` | Step started/completed, pipeline loop count | Info |

Serilog sinks:
- **File** — rolling daily, structured JSON for machine parsing
- **Console** — human-readable, colored output
- **Remote** (optional) — forward to API via `StatusReporter` for dashboard log view

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/oryxbot-.json", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .CreateLogger();
```

### 14.2 Packet Recording for Post-Mortem

A dedicated `PacketRecorder` captures all raw network packets during each run for later analysis:

```csharp
// OryxBot.Protocol/PacketRecorder.cs
public class PacketRecorder : IPacketInterceptorObserver
{
    private readonly string _outputDirectory;
    private string? _currentRunFile;

    public void StartRecording(string runId) { ... }  // Creates run_{runId}_{timestamp}.pcap
    public void OnPacket(byte[] rawPacket) { ... }     // Appends to pcap file
    public void StopRecording() { ... }                // Flushes and closes
}
```

Each test run (simulation or live) produces:
- `run_{id}_packets.pcap` — raw network capture
- `run_{id}_decisions.jsonl` — every NavigationDecision in order
- `run_{id}_positions.jsonl` — character position timeline
- `run_{id}_events.jsonl` — all events published to the event bus

### 14.3 Error Recovery

| Scenario | Behavior |
|----------|----------|
| **Character death** | Navigation AI enters `Killed` state → `TradeMissionEngine` handles respawn + pipeline reset |
| **Route stuck** | Navigation AI enters `Unsticking` → up to 5 retry rotations → `Lost` state on failure |
| **Connection lost** | Navigation AI enters `Disconnected` after 15s no packets → awaits reconnection |
| **VNC disconnect** | `VncManager` auto-restarts process |
| **API disconnect** | `RemoteCommandListener` retries Pusher with 10s delay |

### 14.4 Movement State Machine

```
┌──────────┐   position change    ┌──────────┐
│   Idle   │ ──────────────────→ │ Running  │
│          │ ←────────────────── │          │
└──────────┘  no move for 1.3s   └──────────┘
```

---

## 14. Pipeline Flow Diagrams

### Trade Mission Complete Flow

```
Program Start
    │
    ├── Load configuration (appsettings.json, /etc/oryxbot.apikey)
    ├── Build DI container
    ├── Start network sniffer (SharpPcap on eth* ports 5056,5055,4535)
    ├── Authenticate with API
    ├── Connect to Pusher WebSocket
    ├── Start VNC client
    │
    ↓
Trade Mission Pipeline (loops)
    │
    ├── [1] RunToBank
    │    └── RoutePlayerService plays "quest_to_bank" route
    │
    ├── [2] BankItems
    │    ├── Walk to bank position (0,0)
    │    ├── Interact with bank
    │    ├── Drag inventory item → bank slot 3
    │    ├── Click bank slot 1
    │    ├── Split N times (contractType - 1)
    │    ├── Close split dialog
    │    └── Drag bank slot 2 → inventory
    │
    ├── [3] RunToQuestNpc
    │    └── RoutePlayerService plays "bank_to_quest" route
    │
    ├── [4] AcceptQuest
    │    ├── Walk to Faction Leader position
    │    ├── Interact (click)
    │    ├── Click Trade Missions tab
    │    ├── Click destination contract tab (calculated offset)
    │    ├── Click contract tier selection (calculated offset)
    │    └── Click Accept
    │
    ├── [5] RunRouteToDestination
    │    └── RoutePlayerService plays "to NPC" portion of trade mission route
    │
    ├── [6] ProgressQuest
    │    ├── Walk to nearest Faction Emissary
    │    ├── Interact
    │    └── Click Progress
    │
    ├── [7] RunRouteBack
    │    └── RoutePlayerService plays "back" portion of trade mission route
    │
    └── [8] CompleteQuest
         ├── Walk to Faction Leader position
         ├── Interact
         └── Click Progress (same UI button)
```

### NPC Interaction Sub-Flow

```
AttemptInteraction()
    │
    ├── Is near NPC (distance ≤ 3.0)?
    │    ├── NO → MoveTowards NPC position
    │    └── YES
    │         ├── Stop all movement
    │         ├── Wait 500ms
    │         ├── Click on NPC position
    │         ├── Wait 1500ms
    │         ├── Still not interacting?
    │         │    ├── MoveAwayFrom NPC (unstuck)
    │         │    └── Wait 1000ms, retry
    │         └── Interacting!
    │              └── Execute DoInteractions()
    │                   └── Wait 500ms, center cursor, wait 500ms
    │
    Done
```

---

## 15. File-by-File Build Order

Build in this order to satisfy dependency chains:

### Phase 1: Core Infrastructure
1. `OryxBot.Core` — IEventBus, EventBus, IPipelineStep, PipelineRunner, PipelineContext, IHasTickDelay
2. `OryxBot.GameData` — City, Region, CityData, RegionData, NpcPositions, UiCoordinates, ContractType

### Phase 2: Platform Abstractions
3. `OryxBot.Input` — IInputDriver, HttpInputDriver, ResponsivePoint, IGameInputService, GameInputService, InputTranslator
4. `OryxBot.RemoteDesktop` — IRemoteDesktopService, VncService, VncConfiguration
5. `OryxBot.Protocol` — PhotonSnifferService, all packet handlers, domain events (CharacterMovedEvent, etc.)

### Phase 3: Game State
6. `OryxBot.GameState` — CharacterStateService, MovementTracker, MovementPredictor, state events

### Phase 4: Route System
7. `OryxBot.Routes` — Route/RouteStep models, IRouteRepository, CsvRouteRepository, RouteNavigator, StuckDetector, RoutePlayerService, RouteRecorderService

### Phase 5: Bots
8. `OryxBot.RouteTaker` — RouteTakerBot, PlayRoutePipelineStep, RecordRoutePipelineStep
9. `OryxBot.TradeMission` — All pipeline steps, interaction classes, TradeMissionBot, TradeMissionRouteProvider

### Phase 6: API & Host
10. `OryxBot.Api` — ApiClient, ApiNotifier, WebSocketCommandReceiver, EncryptionService
11. `OryxBot.Host` — Program.cs, appsettings.json, DI composition

### Phase 7: Tests
12. Route parsing, navigation, stuck detection
13. Movement state tracking
14. Trade mission pipeline step transitions

---

## Critical Implementation Notes

### Things That Seem Simple But Aren't

1. **Cluster change timing**: After a cluster change, there's an 8-10 second loading screen. The bot must sleep during this AND ignore the first 5 move packets after resuming (stale data).

2. **Quest NPC UI tab calculation**: The destination city appears in a different tab position depending on the origin city. The offset is calculated from a per-city ordering table. The contract tier adds an additional vertical offset.

3. **Right-mouse movement**: The character is moved by holding right-click and positioning the cursor relative to the character. The bot tracks `rightMouseIsDown` state to avoid redundant clicks, but must re-click if the character isn't moving (force reclick).

4. **Bank token splitting**: Different contract types require different split counts. The values (3, 7, 15) represent the number of hearts, and you split `value - 1` times. The UI involves clicking increase buttons in a split dialog repeatedly.

5. **Route skip-ahead**: The navigator looks up to 4 steps ahead to see if the character is already past intermediate waypoints. This avoids backtracking in linear routes. It checks distance from each future move step and jumps to the farthest one within `maxDistance` (6.0 units).

6. **Emissary NPC selection**: When progressing a quest, the nearest Faction Emissary is found by sorting all emissary positions by distance from the character's current position. This handles routes that go through multiple regions.

7. **Death recovery**: When the character dies, the pipeline resets to the first step (RunToBank), and a respawn button is clicked. This must wait for the character to stop moving (up to 2 seconds after the death event) before sending the event.

### NuGet Dependencies to Use

| Package | Purpose |
|---------|---------|
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft.Extensions.Hosting` | Generic host for lifecycle management |
| `Microsoft.Extensions.Logging` | Logging abstractions |
| `Serilog.Extensions.Hosting` | Serilog integration |
| `SharpPcap` | Network packet capture |
| `PacketDotNet` | Packet parsing |
| `Albion.Network` | Photon protocol parsing (existing dependency) |
| `System.Text.Json` | JSON serialization |
| `PusherClient` | WebSocket communication (existing dependency, optional) |
| `xunit` | Test framework |
| `FluentAssertions` | Readable test assertions |
| `NSubstitute` | Mocking framework |

---

## 16. Testing Framework

### 16.1 Philosophy

The testing framework is built around a **game simulator** — a deterministic physics engine that models how the game responds to the bot's input commands. Instead of mocking individual services, we simulate the entire feedback loop:

```
Bot decides direction → Simulator applies movement with drift/lag → Bot receives new position → Bot course-corrects
```

This lets us validate that the AI actually converges on the route despite realistic (and adversarial) conditions.

### 16.2 Test Project Structure

```
tests/
├── OryxBot.Core.Tests/           — Unit tests for event bus, pipeline runner
├── OryxBot.Routes.Tests/         — Unit tests for route parsing, navigator, stuck detection
├── OryxBot.GameState.Tests/      — Unit tests for movement tracker, predictor, state machine
├── OryxBot.Input.Tests/          — Unit tests for coordinate translation, responsive point
├── OryxBot.TradeMission.Tests/   — Unit tests for pipeline step transitions
│
tests/e2e/
├── OryxBot.Simulation/           — Shared simulation infrastructure (class library)
└── OryxBot.Simulation.Tests/     — E2E test cases using the simulator
```

### 16.3 Game Simulator — Core Engine

The `GameSimulator` replaces the real network sniffer and input driver. It closes the loop: the bot sends movement commands to the simulator, the simulator calculates where the character actually ends up, and publishes position events back to the bot.

```csharp
// tests/e2e/OryxBot.Simulation/GameSimulator.cs
public class GameSimulator
{
    private readonly ISimulationProfile _profile;
    private readonly IEventBus _eventBus;
    private Position _actualPosition;
    private Vector2 _currentDirection;
    private float _speed;
    private string _currentCluster;
    private readonly List<Obstacle> _obstacles;
    private readonly Random _rng;
    
    // Track all positions the character visited for assertions
    public List<SimulatedTick> TickHistory { get; } = new();
    
    /// Called by the simulated input driver when the bot issues a move command
    public async Task OnBotMovedCursor(Vector2 screenDirection, CancellationToken ct)
    {
        // 1. Convert screen direction back to game-world direction
        //    (reverse the -π/4 isometric rotation the bot applies)
        var gameDirection = ReverseIsometricTransform(screenDirection);
        
        // 2. Apply the simulation profile's drift
        gameDirection = _profile.ApplyDirectionDrift(gameDirection, _rng);
        _currentDirection = gameDirection;
        
        // 3. Calculate elapsed time since last tick
        //    (determined by profile — simulates how fast packets arrive)
        var elapsed = _profile.NextPacketInterval(_rng);
        
        // 4. Calculate new position = old + direction * speed * elapsed
        _speed = _profile.CalculateSpeed(_speed, _rng);
        var displacement = _currentDirection * _speed * (float)elapsed.TotalSeconds;
        var candidatePosition = new Position(
            _actualPosition.X + displacement.X,
            _actualPosition.Y + displacement.Y);
        
        // 5. Apply obstacle collision (walls, slow zones)
        _actualPosition = ApplyObstacles(candidatePosition);
        
        // 6. Record tick for assertions
        TickHistory.Add(new SimulatedTick(
            _actualPosition, _currentDirection, _speed, elapsed));
        
        // 7. Publish the position as if it came from a network packet
        await _eventBus.PublishAsync(
            new CharacterMovedEvent(_actualPosition.X, _actualPosition.Y), ct);
    }
    
    /// Simulate a cluster change (portal transition)
    public async Task SimulateClusterChange(string newCluster, CancellationToken ct)
    {
        _currentCluster = newCluster;
        await _eventBus.PublishAsync(
            new ClusterChangedEvent(newCluster), ct);
        
        // Simulate loading screen delay
        await Task.Delay(_profile.ClusterChangeDelay(_rng), ct);
        
        // After cluster change, publish a few stale positions (like the real game)
        for (int i = 0; i < _profile.StalePacketsAfterClusterChange; i++)
        {
            await _eventBus.PublishAsync(
                new CharacterMovedEvent(_actualPosition.X, _actualPosition.Y), ct);
        }
    }
}

public record SimulatedTick(
    Position Position,
    Vector2 Direction,
    float Speed,
    TimeSpan Elapsed);
```

### 16.4 Simulation Profiles

Profiles control how "difficult" the simulation is for the bot's AI.

```csharp
public interface ISimulationProfile
{
    /// How much to rotate/offset the actual movement direction from what the bot intended
    Vector2 ApplyDirectionDrift(Vector2 intendedDirection, Random rng);
    
    /// Time between simulated position packets (simulates network update rate)
    TimeSpan NextPacketInterval(Random rng);
    
    /// Character speed with optional jitter
    float CalculateSpeed(float currentSpeed, Random rng);
    
    /// How long the cluster change loading screen takes
    TimeSpan ClusterChangeDelay(Random rng);
    
    /// How many stale move packets arrive after a cluster change
    int StalePacketsAfterClusterChange { get; }
}
```

| Profile | Direction Drift | Packet Interval | Speed Jitter | Cluster Delay | Description |
|---------|----------------|-----------------|--------------|---------------|-------------|
| **PerfectProfile** | 0° | 200ms fixed | 7.0 fixed | 8s fixed | Baseline — bot should follow route exactly |
| **RealisticProfile** | ±5° random | 150-350ms | 6.5-7.5 | 7-10s | Normal gameplay conditions |
| **HighLatencyProfile** | ±15° random | 300-800ms | 5.0-9.0 | 8-15s | Bad connection, large position jumps |
| **AdversarialProfile** | ±25° random | 500-1500ms | 3.0-12.0 | 10-20s | Worst case — stress test for course correction |

### 16.5 Simulated Input Capture

Replaces the real `IInputDriver` and records every command the bot sends for assertion.

```csharp
// tests/e2e/OryxBot.Simulation/SimulatedInputCapture.cs
public class SimulatedInputCapture : IInputDriver
{
    private readonly GameSimulator _simulator;
    public List<InputCommand> CommandLog { get; } = new();
    
    public async Task SetCursorPosition(int x, int y)
    {
        CommandLog.Add(new CursorMoveCommand(x, y, DateTime.UtcNow));
        // Feed the cursor direction back into the simulator
        var direction = CalculateDirectionFromScreenPos(x, y);
        await _simulator.OnBotMovedCursor(direction, CancellationToken.None);
    }
    
    public Task RightMouseDown() { CommandLog.Add(new RightMouseDownCommand()); return Task.CompletedTask; }
    public Task RightMouseUp() { CommandLog.Add(new RightMouseUpCommand()); return Task.CompletedTask; }
    public Task LeftClick() { CommandLog.Add(new LeftClickCommand()); return Task.CompletedTask; }
    // ... etc
}

public abstract record InputCommand(DateTime Timestamp);
public record CursorMoveCommand(int X, int Y, DateTime Timestamp) : InputCommand(Timestamp);
public record RightMouseDownCommand() : InputCommand(DateTime.UtcNow);
public record LeftClickCommand() : InputCommand(DateTime.UtcNow);
```

### 16.6 Test Builder (Fluent API)

```csharp
// tests/e2e/OryxBot.Simulation/Fixtures/SimulatorBuilder.cs
public class SimulatorBuilder
{
    public SimulatorBuilder WithRoute(Route route) { ... }
    public SimulatorBuilder WithProfile(ISimulationProfile profile) { ... }
    public SimulatorBuilder WithProfile<T>() where T : ISimulationProfile, new() { ... }
    public SimulatorBuilder StartingAt(Position position) { ... }
    public SimulatorBuilder StartingInCluster(string cluster) { ... }
    public SimulatorBuilder WithObstacle(Obstacle obstacle) { ... }
    public SimulatorBuilder WithSeed(int seed) { ... }  // Deterministic randomness
    public SimulatorBuilder WithMaxTicks(int maxTicks) { ... }  // Safety timeout
    
    /// Builds the simulator AND wires up DI with all mocked services
    public (GameSimulator Simulator, RoutePlayerService Player, 
            SimulatedInputCapture Input, IServiceProvider Services) Build();
}
```

### 16.7 Route Fixtures

```csharp
// tests/e2e/OryxBot.Simulation/Fixtures/RouteFixtures.cs
public static class RouteFixtures
{
    /// 10 waypoints in a straight line (easiest possible route)
    public static Route StraightLine(float length = 100f) { ... }
    
    /// Route with 90° turns every 20 units
    public static Route ZigZag(int segments = 5) { ... }
    
    /// Smooth curve (many small waypoints forming an arc)
    public static Route Curve(float radius = 50f, float angleDegrees = 180f) { ... }
    
    /// Route that passes through 3 cluster changes
    public static Route MultiCluster() { ... }
    
    /// Route with a narrow corridor (walls on both sides)
    public static Route NarrowCorridor(float width = 8f) { ... }
    
    /// Route that doubles back on itself (U-turn)
    public static Route UTurn() { ... }
    
    /// Very long route (1000+ waypoints, realistic trade mission length)
    public static Route LongTradeMission() { ... }
    
    /// Route where waypoints are spaced far apart (tests large skip-ahead)
    public static Route SparseWaypoints(float spacing = 20f) { ... }
    
    /// Route loaded from an actual recorded CSV file (production data)
    public static Route FromCsvFile(string path) { ... }
}
```

---

### 16.8 E2E Test Cases — Route Following

```csharp
// tests/e2e/OryxBot.Simulation.Tests/RouteFollowingTests.cs

[Fact]
public async Task StraightRoute_PerfectConditions_CompletesWithin5PercentError()
{
    // The bot follows a straight-line route with no drift.
    // Assert: reaches the end, max deviation from any waypoint < 5% of route length.
    var (sim, player, input, _) = new SimulatorBuilder()
        .WithRoute(RouteFixtures.StraightLine())
        .WithProfile<PerfectProfile>()
        .StartingAt(new Position(0, 0))
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player);
    
    sim.TickHistory.Should().AllSatisfy(tick =>
        RouteDeviation(tick.Position, RouteFixtures.StraightLine())
            .Should().BeLessThan(5f));
}

[Theory]
[InlineData(typeof(RealisticProfile))]
[InlineData(typeof(HighLatencyProfile))]
public async Task ZigZagRoute_WithDrift_CourseCorrectsBackToRoute(
    Type profileType)
{
    // The bot follows a zig-zag route under drift.
    // Assert: after each turn, the bot's deviation decreases within 3 ticks.
    var profile = (ISimulationProfile)Activator.CreateInstance(profileType)!;
    var (sim, player, input, _) = new SimulatorBuilder()
        .WithRoute(RouteFixtures.ZigZag())
        .WithProfile(profile)
        .StartingAt(new Position(0, 0))
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player);
    
    // Verify convergence: deviation should trend downward after corrections
    AssertConvergenceAfterTurns(sim.TickHistory, RouteFixtures.ZigZag());
}

[Fact]
public async Task CurvedRoute_Realistic_StaysWithinMaxDeviation()
{
    // On a curved route, the bot should never be more than MaxDistance (6.0)
    // away from the nearest upcoming waypoint.
    var (sim, player, _, _) = new SimulatorBuilder()
        .WithRoute(RouteFixtures.Curve())
        .WithProfile<RealisticProfile>()
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player);
    
    var maxDeviation = sim.TickHistory
        .Max(t => NearestWaypointDistance(t.Position, RouteFixtures.Curve()));
    maxDeviation.Should().BeLessThan(12f); // 2x the skip-ahead threshold
}

[Fact]
public async Task AdversarialDrift_EventuallyCompletesRoute()
{
    // Even under worst-case drift, the bot should eventually finish.
    // May take longer but must not get permanently stuck.
    var (sim, player, _, _) = new SimulatorBuilder()
        .WithRoute(RouteFixtures.StraightLine(50f))
        .WithProfile<AdversarialProfile>()
        .WithMaxTicks(10000)
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player, timeout: TimeSpan.FromSeconds(30));
    
    player.IsComplete.Should().BeTrue();
}

[Fact]
public async Task SparseWaypoints_SkipAheadWorks_DoesNotBacktrack()
{
    // With widely-spaced waypoints, the navigator should skip ahead
    // when the character passes through intermediate points.
    var (sim, player, _, _) = new SimulatorBuilder()
        .WithRoute(RouteFixtures.SparseWaypoints(spacing: 20f))
        .WithProfile<PerfectProfile>()
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player);
    
    // The navigator index should only ever increase (no backtracking)
    // (we can track this via a custom event handler)
}
```

---

### 16.9 E2E Test Cases — Course Correction Validation

These are the most important tests — they validate the core AI behavior.

```csharp
// tests/e2e/OryxBot.Simulation.Tests/CourseCorrectionTests.cs

[Fact]
public async Task DriftingRight_BotCorrectedLeft_WithinReasonableTime()
{
    // Start the character 3 units to the right of the route.
    // Assert: within 5 ticks, the bot's movement direction has a leftward component.
    var route = RouteFixtures.StraightLine();
    var startOffset = new Position(3f, 0f); // Offset from first waypoint
    
    var (sim, player, input, _) = new SimulatorBuilder()
        .WithRoute(route)
        .WithProfile<PerfectProfile>()
        .StartingAt(startOffset)
        .WithSeed(42)
        .Build();
    
    // Run for 5 ticks
    await RunForTicks(player, 5);
    
    // The cursor commands should show the bot steering left
    var lastCursorMoves = input.CommandLog
        .OfType<CursorMoveCommand>().TakeLast(3);
    // Direction component should be correcting toward the route
    AssertDirectionConvergesToRoute(lastCursorMoves, route);
}

[Fact]
public async Task ConstantPerpendicularDrift_BotMaintainsApproximatePath()
{
    // Simulation always pushes character 2° perpendicular to intended.
    // Assert: the character stays within an acceptable corridor.
    var route = RouteFixtures.StraightLine(200f);
    var profile = new ConstantDriftProfile(driftAngleDegrees: 2f);
    
    var (sim, player, _, _) = new SimulatorBuilder()
        .WithRoute(route)
        .WithProfile(profile)
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player);
    
    var maxDeviation = sim.TickHistory
        .Max(t => PerpendicularDistanceToRouteLine(t.Position, route));
    maxDeviation.Should().BeLessThan(8f); // Should stay close to route
}

[Fact]
public async Task SuddenLargePositionJump_BotRecovers()
{
    // Mid-route, the character teleports 15 units off-course (simulating
    // a lag spike followed by a big position correction from the game).
    var route = RouteFixtures.StraightLine(100f);
    var profile = new PositionJumpProfile(jumpAtTick: 50, jumpDistance: 15f);
    
    var (sim, player, _, _) = new SimulatorBuilder()
        .WithRoute(route)
        .WithProfile(profile)
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player);
    
    player.IsComplete.Should().BeTrue();
    // After the jump, deviation should decrease over subsequent ticks
    var ticksAfterJump = sim.TickHistory.Skip(50).Take(20).ToList();
    var deviations = ticksAfterJump.Select(t => 
        NearestWaypointDistance(t.Position, route)).ToList();
    deviations.Last().Should().BeLessThan(deviations.First());
}

[Fact]
public async Task NarrowCorridor_WithObstacles_NavigatesWithoutGettingStuck()
{
    // Route passes through a narrow corridor with walls.
    // Character can bounce off walls. Bot must navigate through.
    var route = RouteFixtures.NarrowCorridor(width: 8f);
    var (sim, player, _, _) = new SimulatorBuilder()
        .WithRoute(route)
        .WithProfile<RealisticProfile>()
        .WithObstacle(new WallObstacle(/* corridor walls */))
        .WithSeed(42)
        .Build();
    
    await RunUntilComplete(player, timeout: TimeSpan.FromSeconds(30));
    
    player.IsComplete.Should().BeTrue();
}
```

---

### 16.10 E2E Test Cases — Cluster Changes

```csharp
// tests/e2e/OryxBot.Simulation.Tests/ClusterChangeTests.cs

[Fact]
public async Task ClusterChange_NavigatorAdvancesPastChangeStep()
{
    // Route has: move, move, cluster, move, move.
    // After cluster change event fires, the navigator should be on the 
    // first MoveStep after the cluster marker.
    var route = RouteFixtures.MultiCluster();
    // ... setup and validate navigator index after cluster events
}

[Fact]
public async Task ClusterChange_StalePacketsIgnored()
{
    // After cluster change, 5 stale move packets arrive with old positions.
    // The character state service should ignore them.
    // Assert: actual position doesn't jump back to pre-cluster position.
}

[Fact]
public async Task ClusterChange_BotWaitsForLoadingScreen()
{
    // During loading (8-10s), the bot should not issue movement commands.
    // Assert: no CursorMoveCommands during loading period.
}

[Fact]
public async Task MultipleClusterChanges_RouteCompletesCorrectly()
{
    // Full route with 3 cluster changes.
    // Assert: all cluster steps traversed in order, route completes.
}
```

---

### 16.11 E2E Test Cases — Stuck Detection & Recovery

```csharp
// tests/e2e/OryxBot.Simulation.Tests/StuckRecoveryTests.cs

[Fact]
public async Task CharacterNotMoving_StuckDetected_AntiStuckRotationApplied()
{
    // Simulation holds character position fixed for 1+ second.
    // Assert: bot applies anti-stuck rotation (direction change),
    //   and the stuck detector fires after MaxTriesToGetUnstuckWithinTimeout.
}

[Fact]
public async Task CharacterStuckAgainstWall_AlternatesUnstuckStrategies()
{
    // Character hits a wall and can't move in the intended direction.
    // Assert: the bot tries -π/3 rotation first, then -2π/3,
    //   and the character eventually moves past the obstacle.
}

[Fact]
public async Task StuckTimeout_EventPublished_BotPauses()
{
    // After 5 stuck attempts within the 30-second window,
    // a CharacterBecameStuckEvent is published.
    // Assert: the pipeline pauses (or publishes appropriate event).
}

[Fact]
public async Task RecentlyChangedCluster_UsesShallowRotation()
{
    // When stuck shortly after a cluster change, use -π/3 rotation.
    // When stuck in normal conditions, use -2π/3 rotation.
    // Assert: correct rotation angle is used based on context.
}
```

---

### 16.12 E2E Test Cases — Death Recovery

```csharp
// tests/e2e/OryxBot.Simulation.Tests/DeathRecoveryTests.cs

[Fact]
public async Task CharacterDies_PipelineResetsToRunToBank()
{
    // Mid-route, simulate CharacterDiedEvent.
    // Assert: pipeline resets to RunToBank step.
}

[Fact]
public async Task CharacterDies_RespawnButtonClicked()
{
    // After death event, bot should click the respawn button.
    // Assert: input log contains click at RespawnButton coordinates.
}

[Fact]
public async Task DeathWhileMoving_WaitsForStopBeforeRespawn()
{
    // Death fires while character is still moving (death animation).
    // Bot should wait up to 2 seconds for movement to stop before respawning.
}
```

---

### 16.13 Unit Test Cases — Route Navigator

```csharp
// tests/OryxBot.Routes.Tests/RouteNavigatorTests.cs

[Fact]
public void MoveNext_AdvancesIndex()
[Fact]
public void MovePrevious_AtStart_ReturnsFalse()
[Fact]
public void MovePrevious_AfterAdvancing_GoesBack()

[Fact]
public void TrySkipAhead_CharacterPastWaypoints_SkipsCorrectly()
// Character is at position that's past waypoints 1,2,3.
// TrySkipAhead should advance to waypoint 3.

[Fact]
public void TrySkipAhead_CharacterBehind_NoSkip()
// Character is still near waypoint 0. No skip should happen.

[Fact]
public void TrySkipAhead_MaxSkipLimitRespected()
// Even if character is past 10 waypoints, skip at most 4.

[Fact]
public void ResumeFromCluster_FindsClosestMoveStep()
// Given a cluster alias and approximate position, navigator resumes
// from the closest MoveStep after that cluster marker.

[Fact]
public void ResumeFromCluster_UnknownAlias_ReturnsFalse()

[Fact]
public void PercentComplete_CalculatedCorrectly()
```

### 16.14 Unit Test Cases — Movement State Tracking

```csharp
// tests/OryxBot.GameState.Tests/MovementTrackerTests.cs

[Fact]
public void PositionChange_SetsMovingTrue()
[Fact]
public void NoPositionChangeFor1300ms_SetsMovingFalse()
[Fact]
public void SmallPositionChange_BelowThreshold_SetsMovingFalse()
// MinDistanceConsideredAsMove = 0.2f

[Fact]
public void IdleWatch_ResetsWhenMovingStarts()
[Fact]
public void IdleWatch_StartsWhenMovingStops()

// tests/OryxBot.GameState.Tests/MovementPredictorTests.cs
[Fact]
public void Speed_CalculatedFromDistanceAndTime()
[Fact]
public void PredictedPosition_ExtrapolatesForward()
[Fact]
public void ClusterChange_ResetsSpeed()
[Fact]
public void HighSpeed_CappedAt30_IgnoredAsTeleport()
```

### 16.15 Unit Test Cases — Stuck Detector

```csharp
// tests/OryxBot.Routes.Tests/StuckDetectorTests.cs

[Fact]
public void NoAttempts_IsStuckFalse()
[Fact]
public void FiveAttemptsWithinWindow_IsStuckTrue()
[Fact]
public void AttemptsExpire_IsStuckFalse()
[Fact]
public void PurgeExpired_RemovesOldTimestamps()
[Fact]
public void Reset_ClearsAllAttempts()
```

### 16.16 Unit Test Cases — Route Parsing

```csharp
// tests/OryxBot.Routes.Tests/CsvRouteRepositoryTests.cs

[Fact]
public async Task ParsesCsvWithMetadata()
// metadata,origin:fort-sterling,destination:aspenwood,name:Test
// move,12.5,-34.2
// cluster,SomeMap
// Assert: metadata populated, correct step types

[Fact]
public async Task ParsesCsvWithQuestMarker_SplitsIntoTwoRoutes()
// move,...
// quest
// move,...
// Assert: returns TradeMissionRoute with toNpc and back sub-routes

[Fact]
public async Task FloatParsing_InvariantCulture()
// Positions like "12.5" must parse correctly regardless of system locale.

[Fact]
public async Task EmptyCsv_ReturnsEmptyRoute()

[Fact]
public async Task SaveAndReload_Roundtrips()
// Save a route, reload it, compare all fields.
```

### 16.17 Unit Test Cases — Pipeline Runner

```csharp
// tests/OryxBot.Core.Tests/PipelineRunnerTests.cs

[Fact]
public async Task RunsStepsInOrder()
[Fact]
public async Task StopsOnCancellation()
[Fact]
public async Task PublishesStepChangedEvents()
[Fact]
public async Task RespectsTickDelay()
```

### 16.18 Unit Test Cases — Input Coordinate Translation

```csharp
// tests/OryxBot.Input.Tests/InputTranslatorTests.cs

[Fact]
public void IsometricRotation_NorthInGame_MapsToUpRightOnScreen()
// Game direction (0, -1) after -π/4 rotation should be approximately (0.707, -0.707)

[Fact]
public void MoveTowards_CursorPlacedAtCorrectScreenPosition()
// Given character at center, target to the north-east.
// Cursor should be offset from center by (direction * screenHeight/10).

[Fact]
public void AntiStuck_NormalConditions_RotatesBy120Degrees()
// -2π/3 rotation

[Fact]
public void AntiStuck_RecentClusterChange_RotatesBy60Degrees()
// -π/3 rotation

// tests/OryxBot.Input.Tests/ResponsivePointTests.cs

[Theory]
[InlineData(1920, 1080)]  // Reference resolution
[InlineData(2560, 1440)]  // 2K
[InlineData(3840, 2160)]  // 4K
public void LeftAnchor_ScalesCorrectlyAcrossResolutions(int width, int height)

[Fact]
public void RightAnchor_MirrorsLeftAnchor()

[Fact]
public void CenterAnchor_OffsetsFromScreenCenter()
```

### 16.19 Unit Test Cases — Trade Mission Pipeline Transitions

```csharp
// tests/OryxBot.TradeMission.Tests/PipelineTransitionTests.cs

[Fact]
public void AfterRunToBank_NextStepIsBankItems()
[Fact]
public void AfterBankItems_NextStepIsRunToQuest()
[Fact]
public void AfterAcceptQuest_NextStepIsRunRouteToDestination()
[Fact]
public void AfterRunRouteBack_NextStepIsCompleteQuest()
[Fact]
public void AfterCompleteQuest_LoopsBackToRunToBank()

[Fact]
public void ResumeFromAlias_SkipsToCorrectStep()
// When resuming with a ResumeFromAlias config, the route should
// skip to the specified cluster and nearest position.

[Fact]
public void ResumeWithProgressedQuest_StartsFromRouteBack()
// If the quest was already progressed, start from RunRouteBack.
```

### 16.20 NPC Interaction Test Cases

```csharp
// tests/OryxBot.TradeMission.Tests/NpcInteractionTests.cs

[Fact]
public async Task FarFromNpc_WalksToward()
// Character is 20 units from NPC → bot moves toward NPC.

[Fact]
public async Task NearNpc_StopsAndClicks()
// Character is within 3 units → stop movement, click to interact.

[Fact]
public async Task InteractionFails_MovesAwayThenRetries()
// Click didn't trigger interaction → move away from NPC briefly, retry.

[Fact]
public async Task QuestTabCalculation_CorrectForAllCityPairs()
// For every (origin, destination) pair, verify the tab offset matches
// the FactionLeaderCityMissionOrdering table.
```

---

### 16.21 Running the Tests

```bash
# All unit tests
dotnet test tests/OryxBot.Core.Tests/
dotnet test tests/OryxBot.Routes.Tests/
dotnet test tests/OryxBot.GameState.Tests/
dotnet test tests/OryxBot.Input.Tests/
dotnet test tests/OryxBot.TradeMission.Tests/

# E2E simulation tests
dotnet test tests/e2e/OryxBot.Simulation.Tests/

# All tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test class
dotnet test --filter "FullyQualifiedName~RouteFollowingTests"

# Run with a specific profile (parameterized)
dotnet test --filter "CourseCorrectionTests"
```

### 16.22 Test Metrics & Assertions

For route-following tests, define these standard metrics:

| Metric | Formula | Acceptable Range |
|--------|---------|------------------|
| **Max Deviation** | Max distance from any tick position to nearest route waypoint | < 12 units (2x skip threshold) |
| **Average Deviation** | Mean distance from tick positions to nearest waypoint | < 4 units |
| **Convergence Rate** | How quickly deviation decreases after a correction | < 5 ticks to halve deviation |
| **Completion Rate** | Whether the route finishes | 100% for Perfect/Realistic profiles |
| **Completion Time Ratio** | Actual ticks / theoretical minimum ticks | < 1.5x for Realistic, < 3x for Adversarial |
| **Backtrack Count** | Number of times navigator index decreased | 0 for normal routes |
