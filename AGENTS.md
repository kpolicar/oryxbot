# OryxBot — Repository Structure

This monorepo contains two independent projects:

## `app/` — OryxBot Application

The bot itself. A .NET 10 (C# 14+) application that autonomously navigates a character through the Albion Online without interacting with the game process — it reads game state via network packet sniffing (Photon/UDP) and controls the game by moving the cursor through VNC.

- **Solution:** `app/OryxBot.slnx`
- **Source:** `app/src/` — 8 projects (Core, WorldGraph, Pilot, Routes, Protocol, Input, GameState, RemoteDesktop)
- **Host:** `app/hosts/OryxBot.Host/` — DI composition root, entry point
- **Tests:** `app/tests/` — xUnit + FluentAssertions + NSubstitute
- **Docs:** `app/docs/` — architecture overview, phase plans, system patterns
- **Build:** `dotnet build app/OryxBot.slnx`
- **Test:** `dotnet test app/OryxBot.slnx`

Key architectural decisions: event bus (no MediatR), pure Pathfinder (no side effects), reactive Pilot state machine, all decisions serializable for replay.

## `website/` — OryxBot Website

Two static SPAs (Vite + React + TypeScript + Tailwind) for the public-facing product pages. Not yet scaffolded — currently docs/design only.

- **Docs:** `website/docs/` — full page breakdowns, shared component specs, design tokens
- `oryxbot.com` — platform landing, marketplace, developer pitch, auth (Clerk)
- `pilot.oryxbot.com` — Pilot product page with interactive dashboard demo

The website has no backend. All dashboard/marketplace data is mocked. Auth via Clerk client-side SDK.
