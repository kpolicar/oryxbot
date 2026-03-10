# Phase 3 & Beyond — Proposed Next Steps

> **Goal:** After Phases 1, 1b, and 2, the bot has a working navigation AI validated by simulation.  
> These phases connect it to the real game and add operational features.  
> Each phase is independent — they can be reordered based on priority.

---

## Phase 3 — Live Protocol Integration

**Depends on:** Phase 1, Phase 2  
**Goal:** Wire the bot to real Albion Online network traffic.

### Scope

1. **PacketInterceptor** — SharpPcap + PacketDotNet listener on UDP ports 5056, 5055, 4535
2. **PhotonPacketHandler** — Deserializes Photon protocol commands using `Albion.Network` (or a rewrite)
3. **OperationHandler / EventHandler** — Maps operation/event codes to domain events:
   - `OperationCode.Move (21)` → `CharacterMovedEvent`
   - `EventCode.ChangeCluster` → `ClusterChangedEvent`
   - `EventCode.Died` → `CharacterDiedEvent`
   - `OperationCode.RegisterToObject (39)` / `UnRegisterFromObject (40)` → `InteractionChangedEvent`
4. **Stale packet filter** — Drop first 5 move packets after cluster change (they contain pre-transition positions)
5. **Integration test** — Record a PCAP from a real session, replay it, and verify the correct domain events are published

### Key Decisions

- Port the existing ~350 OperationCodes and ~500 EventCodes from `OryxBot.Albion`
- Keep SharpPcap for sniffing (zero-injection, safe approach)
- Run as `CAP_NET_RAW` on Linux (no root required with proper capabilities)

### Deliverables

- `src/OryxBot.Protocol/PacketInterceptor.cs` — real implementation
- `src/OryxBot.Protocol/Handlers/MoveHandler.cs`, `ClusterChangeHandler.cs`, `DeathHandler.cs`, etc.
- Integration tests with recorded PCAP replay

---

## Phase 4 — Input Bridge & VNC Integration

**Depends on:** Phase 1, Phase 3  
**Goal:** Send mouse/keyboard commands to the game running in a Linux VM with VNC.

### Scope

1. **HttpInputAdapter** — Sends HTTP requests to a Java VNC bridge at `localhost:8010`:
   - `POST /cursor/{x}/{y}` — move cursor
   - `POST /mouse/right/down` — right mouse button down
   - `POST /mouse/right/up` — right mouse button up
   - `POST /mouse/left/click` — left click
   - `POST /key/{keycode}` — key press
2. **GameController** — Implements `IGameController` using `IInputAdapter` + `CoordinateTranslator`:
   - `MoveTowards(Position target)` → calculate screen direction (isometric rotation), set cursor, hold right-click
   - `MoveInDirection(Vector2 dir)` → apply rotation, set cursor offset from center
   - `StopMovement()` → release right mouse
   - `Respawn()` → click respawn button at `ResponsivePoint` coordinates
3. **VncManager** — Lifecycle management: connect, disconnect, health check, reconnect
4. **CoordinateTranslator** — Full implementation:
   - `WorldToScreen(Position world, Position character, Size screen) → Point`
   - Applies -π/4 isometric rotation
   - Offsets from screen center by `direction × screenHeight / 10`

### Deliverables

- `src/OryxBot.Input/Http/HttpInputAdapter.cs`
- `src/OryxBot.Input/GameController.cs`
- `src/OryxBot.Input/CoordinateTranslator.cs` (full implementation if not done in Phase 1)
- `src/OryxBot.RemoteDesktop/VncManager.cs`
- Integration tests with a mock HTTP server

---

## Phase 5 — End-to-End Pilot Loop

**Depends on:** Phases 3, 4  
**Goal:** The bot can navigate a real route in a live game session.

### Scope

1. **PilotEngine** — Full implementation:
   - Loads a route from `IRouteStore`
   - Creates a `RouteCursor`
   - Runs a tick loop: `Evaluate()` → `Execute()` → sleep → repeat
   - Publishes `NavigationSnapshot` each tick for observability
   - Handles terminal states (Lost, Killed, Disconnected) with appropriate events
2. **Route recorder** — `RouteRecorder` captures live movement into a new route:
   - Subscribe to `CharacterMovedEvent` and `ClusterChangedEvent`
   - Emit `MoveWaypoint` for significant position changes (>2 units from last waypoint)
   - Emit `PortalWaypoint` for cluster changes
   - Save to `IRouteStore` on stop
3. **CLI commands** for the host:
   - `pilot run <route-name>` — follow a saved route
   - `pilot record <route-name>` — record a new route from live movement
   - `pilot list` — list available routes
   - `pilot status` — show current navigation snapshot

### Deliverables

- `src/OryxBot.Pilot/PilotEngine.cs` — real implementation
- `src/OryxBot.Routes/Recording/RouteRecorder.cs` — real implementation
- CLI commands in `hosts/OryxBot.Host/`
- First successful live test: bot follows a recorded route from city A to city B

---

## Phase 6 — API & Dashboard

**Depends on:** Phase 5  
**Goal:** Remote monitoring and control via HTTP API + web dashboard.

### Scope

1. **REST API** (ASP.NET Core minimal APIs, hosted in `OryxBot.Host`):
   - `GET /api/status` — current navigation snapshot
   - `GET /api/routes` — list routes
   - `POST /api/pilot/start` — start a route
   - `POST /api/pilot/stop` — stop navigation
   - `POST /api/pilot/record/start` — begin recording
   - `POST /api/pilot/record/stop` — stop recording
   - `GET /api/events` — SSE (Server-Sent Events) stream of domain events
2. **Web Dashboard** (static SPA, optional):
   - Real-time navigation state (position, state, deviation, waypoint progress)
   - Route visualization (list of waypoints with current position marker)
   - Event log
   - Start/stop/record controls

### Deliverables

- `src/OryxBot.Api/` — minimal API project
- `web/OryxBot.Dashboard/` — static web UI (optional, could be Phase 7)
- API integration tests

---

## Phase 7 — Hardening & Production Readiness

**Depends on:** Phase 6  
**Goal:** Make the bot reliable for long unattended sessions.

### Scope

1. **Automatic reconnection** — detect disconnect, wait, reconnect VNC, resume route
2. **Death handling** — click respawn, navigate back to route start, resume
3. **Crash recovery** — persist state to disk, resume from last known position on restart
4. **Health monitoring** — periodic checks: VNC alive, packets flowing, character responding
5. **Structured logging** — Serilog with structured properties, log rotation, configurable verbosity
6. **Configuration validation** — fail fast on startup if config is missing or invalid
7. **Graceful shutdown** — `CancellationToken` propagation through all layers, clean resource disposal

### Deliverables

- Reconnection loop in VncManager
- State persistence (JSON checkpoint file)
- Health check background service
- Production `appsettings.Production.json`
- Deployment guide (systemd unit file, Docker compose, etc.)

---

## Phase 8 — Advanced Features (Backlog)

Ideas for future development, unprioritized:

| Feature | Description |
|---------|-------------|
| **Dynamic pathfinding** | Pathfinder reacts to PvP zone danger (player counts from network data) |
| **Multi-bot orchestration** | Run multiple bot instances across VMs, coordinated by a central service |
| **Route optimization** | Analyze recorded routes and suggest shorter paths using the world graph |
| **Map viewer integration** | Phase 1b web viewer shows live bot position on the map |
| **Combat avoidance** | Detect nearby hostile players via network events, trigger Evading state |
| **Resource tracking** | Track silver/items gathered per session |
| **Trade mission support** | Re-add trade mission pipeline on top of the Pilot (the original bot functionality) |
| **Discord integration** | Send notifications (route complete, death, disconnect) to Discord webhook |

---

## Phase Dependency Graph

```
Phase 1  ─────────────────────┬────────→ Phase 3 ──→ Phase 4 ──→ Phase 5 ──→ Phase 6 ──→ Phase 7
(Core)                        │          (Protocol)   (Input)     (Pilot)     (API)       (Harden)
                              │
Phase 1b ─────────────────────┤
(Game Data)                   │
                              │
Phase 2  ─────────────────────┘
(Simulation)
```

Phases 1, 1b, and 2 can be developed in parallel (1b and 2 depend on Phase 1 but not on each other).  
Phases 3–7 are sequential — each builds on the previous.  
Phase 8 items are independent backlog items.
