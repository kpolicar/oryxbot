# OryxBot — Repository Structure

This monorepo contains the bot application, website, and supporting tools.

## `app/` — OryxBot Application

The bot itself. A .NET 10 (C# 14+) application that autonomously navigates a character through Albion Online without interacting with the game process — it reads game state via network packet sniffing (Photon/UDP) and controls the game by moving the cursor through VNC.

- **Solution:** `app/OryxBot.slnx`
- **Source:** `app/src/` — 8 projects (Core, WorldGraph, Pilot, Routes, Protocol, Input, GameState, RemoteDesktop)
- **Host:** `app/hosts/OryxBot.Host/` — DI composition root, entry point
- **Tests:** `app/tests/` — xUnit + FluentAssertions + NSubstitute
- **Docs:** `app/docs/` — architecture overview, phase plans, system patterns
- **Build:** `dotnet build app/OryxBot.slnx`
- **Test:** `dotnet test app/OryxBot.slnx`

Key architectural decisions: event bus (no MediatR), pure Pathfinder (no side effects), reactive Pilot state machine, all decisions serializable for replay.

## `website/` — OryxBot Website

Two static SPAs (Vite + React + TypeScript + Tailwind) for the public-facing product pages.

- **Docs:** `website/docs/` — full page breakdowns, shared component specs, design tokens
- `oryxbot.com` — platform landing, marketplace, developer pitch, auth (Clerk)
- `pilot.oryxbot.com` — Pilot product page with interactive dashboard demo

The website has no backend. All dashboard/marketplace data is mocked. Auth via Clerk client-side SDK.

## `tools/` — Supporting Tools

### `tools/map-viewer/`
Interactive Leaflet-based world map viewer. Open `index.html` to use. Consumes tile and JSON data from its `data/` directory.

### `tools/map-extractor/`
Python CLI for downloading and color-grading Albion Online map tiles and minimap overlays. Outputs into `tools/map-viewer/data/` by default.

```bash
cd tools/map-extractor
pip install -r requirements.txt
python cli.py all            # full pipeline
python cli.py --help         # see all commands
```

### `tools/game-data-extractor/`
.NET CLI tool for extracting Albion Online world graph data from game files or ao-bin-dumps XML. Standalone solution with its own tests.

```bash
dotnet build tools/game-data-extractor/OryxBot.GameDataExtractor.slnx
dotnet run --project tools/game-data-extractor -- --help
```

### `tools/game/`
Raw game data (ao-bin-dumps) and Python Unity asset extraction scripts (DumpTextureNames.py, ExtractMinimaps.py).
