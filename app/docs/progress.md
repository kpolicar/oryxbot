# Progress — What's Done & What's Next

## Documents Created

| Document | Status | Description |
|----------|--------|-------------|
| [00-architecture-overview.md](00-architecture-overview.md) | ✅ Done | Master architecture — Pathfinder/Pilot split, project structure, all interfaces |
| [phase-1-core-skeleton.md](phase-1-core-skeleton.md) | ✅ Done | Solution setup, interfaces, implementable components, config POCOs, Host, tests |
| [phase-1b-game-data-tooling.md](phase-1b-game-data-tooling.md) | ✅ Done | CLI extractor (.bin → JSON), world graph builder, web map viewer |
| [phase-2-simulation-tests.md](phase-2-simulation-tests.md) | ✅ Done | GameSimulator, profiles, SimulatorBuilder, Pathfinder A*, NavigationController, E2E tests |
| [phase-3-and-beyond.md](phase-3-and-beyond.md) | ✅ Done | Phases 3–8: protocol, input bridge, pilot loop, API, hardening, backlog |
| [projectbrief.md](projectbrief.md) | ✅ Done | Core requirements and goals |
| [systemPatterns.md](systemPatterns.md) | ✅ Done | Architecture decisions and patterns |
| [techContext.md](techContext.md) | ✅ Done | Stack, packages, tooling, constants |
| [activeContext.md](activeContext.md) | ✅ Done | Current work state |
| [progress.md](progress.md) | ✅ Done | This file |

## Implementation Status

### Phase 1 — Core & Skeleton
- [ ] Solution created with all projects
- [ ] Core interfaces defined (IEventBus, IGameController, IInputAdapter, ICharacterTracker, IPathfinder, etc.)
- [ ] EventBus implemented + tested
- [ ] Position record struct implemented + tested
- [ ] TickContext record struct defined
- [ ] NavigationOptions + other config POCOs defined
- [ ] ObstacleDetector implemented + tested (uses TimeProvider)
- [ ] RouteCursor implemented + tested
- [ ] JsonRouteStore implemented + tested
- [ ] Waypoint JSON converter implemented
- [ ] ResponsivePoint ported + tested
- [ ] CoordinateTranslator implemented + tested
- [ ] MotionDetector implemented + tested (uses TimeProvider)
- [ ] PositionPredictor implemented + tested (uses TimeProvider)
- [ ] OperationCodes / EventCodes ported from existing codebase
- [ ] Protocol domain events defined (CharacterMovedEvent, ClusterChangedEvent, etc.)
- [ ] Host composition root with DI + Serilog
- [ ] All stubs compile (NavigationController, PilotEngine, Pathfinder, etc.)

### Phase 1b — Game Data Extraction
- [ ] CLI project created (System.CommandLine)
- [ ] BinDecryptor (AES-256-CBC decryption)
- [ ] ClusterExtractor (XML → ClusterDefinition)
- [ ] WorldGraphBuilder (clusters → adjacency graph)
- [ ] JSON output writer
- [ ] Web map viewer (static HTML + Cytoscape.js / vis-network)
- [ ] Extractor tests with fixture XML files

### Phase 2 — Simulation Engine & Tests
- [ ] GameSimulator (deterministic physics)
- [ ] SimulatedInputCapture
- [ ] Simulation profiles (Perfect, Realistic, HighLatency, Adversarial)
- [ ] SimulatorBuilder fluent API
- [ ] RouteFixtures library
- [ ] Pathfinder A* implementation + tests
- [ ] NavigationController state machine + all states
- [ ] E2E route following tests
- [ ] E2E course correction tests
- [ ] E2E cluster change tests
- [ ] E2E stuck recovery tests
- [ ] E2E death recovery tests

### Phase 3 — Live Protocol Integration
- [ ] Not started

### Phase 4 — Input Bridge & VNC
- [ ] Not started

### Phase 5 — End-to-End Pilot Loop
- [ ] Not started

### Phase 6+ — API, Hardening, Advanced
- [ ] Not started
