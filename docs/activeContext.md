# Active Context — Current Work

## Current Phase

**Planning complete.** All architecture and phase documents have been written. No implementation has started yet.

## What Was Just Completed

- Rewrote the architecture from scratch, removing all trade mission logic
- Defined the Pathfinder/Pilot split as the core design pattern
- Created phased implementation plan across 5 documents
- Added TimeProvider injection, TickContext, Configuration POCOs, CancellationToken convention, and Serilog logging to Phase 1

## What's Next

**Phase 1 — Core & Skeleton** is ready to be implemented. It involves:

1. Creating the .NET 10 solution with all projects
2. Defining all interfaces (`IEventBus`, `IGameController`, `IInputAdapter`, `ICharacterTracker`, `IPathfinder`, etc.)
3. Implementing pure-logic components: `EventBus`, `Position`, `ObstacleDetector`, `RouteCursor`, `JsonRouteStore`, `ResponsivePoint`, `CoordinateTranslator`, `MotionDetector`, `PositionPredictor`
4. Defining configuration POCOs (`NavigationOptions`, `RouteOptions`, etc.)
5. Setting up the Host composition root with DI and Serilog
6. Creating test projects with tests for all implemented components

Phase 1b (game data extraction CLI + web map viewer) and Phase 2 (simulation engine + tests) can begin once Phase 1 interfaces are defined — they depend on Phase 1 but not on each other.

## Key Decisions Made

- **Serilog** for logging (native `Serilog.ILogger`, not `Microsoft.Extensions.Logging`)
- **System.TimeProvider** injected everywhere — no direct `DateTime.UtcNow`
- **NavigationOptions** POCO holds all tunable constants with sane defaults
- **TickContext** passed into `Evaluate()` so simulation can control time
- All async methods take `CancellationToken ct = default`

## Open Questions

- Exact XML schema of cluster files may vary between game patches — extractor needs resilience
- Whether intra-cluster terrain data from `.bin` files is rich enough for local pathfinding (future enhancement)
