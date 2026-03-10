# Project Brief — OryxBot Rebuild

## Vision

Rebuild OryxBot from scratch as a **pure Pilot bot** for Albion Online. Given a destination, it autonomously navigates the character there — through portals, across multiple map zones, recovering from failures.

**No trade missions.** The original OryxBot was a trade mission bot with navigation embedded inside. This rebuild strips away all trade mission logic and focuses entirely on navigation as a first-class capability. Trade missions may be layered back on top in a future phase.

## Core Requirements

1. **Pathfinder** — Given a source and destination cluster, compute the shortest cluster-to-cluster path through the world graph using A*/Dijkstra. Pure computation, no I/O, deterministic.
2. **Pilot** — Given a ClusterPath or pre-recorded Route, execute the movement: follow waypoints, handle portal transitions, course-correct, unstick from obstacles, and recover from failures. Driven by a behavioral state machine.
3. **Game Data Extraction** — CLI tool that reads Albion Online's encrypted `.bin` game files (or pre-dumped XML from ao-data/ao-bin-dumps) and outputs structured JSON: cluster definitions, portal connections, world graph.
4. **Map Viewer** — Lightweight web app to visualize the extracted world graph.
5. **Simulation Engine** — Deterministic game physics simulator that closes the bot input/output loop for E2E testing without a live game.

## How the Bot Works

- **Reads game state** by sniffing Photon UDP packets (SharpPcap) — character position, movement, cluster changes, death events
- **Controls the game** by sending HTTP requests to a Java VNC bridge (`localhost:8010`) — mouse movement, clicks, key presses
- Game runs in a **Linux VM** with VNC

## Non-Goals (for now)

- Trade mission pipeline (banking, NPC interaction, quest UI)
- Multi-bot orchestration
- Combat / PvP
- Item management

## Success Criteria

- Bot can follow a pre-recorded route from city A to city B across multiple cluster transitions
- Pathfinder computes valid shortest paths on the full cluster world graph
- Navigation AI handles drift, stuck detection, cluster loading, and death recovery
- All behaviors validated by the simulation engine without needing a live game
