# Tech Context — Stack & Tooling

## Runtime & Language

| | Choice |
|---|---|
| **Runtime** | .NET 10 |
| **Language** | Latest C# (14+) |
| **Platform** | Linux-first (bot runs in Linux VMs) |

## Core Packages

| Package | Purpose |
|---------|---------|
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft.Extensions.Hosting` | Generic host for lifecycle management |
| `Microsoft.Extensions.Configuration` | JSON config binding, `IOptions<T>` |
| `System.TimeProvider` | Abstracted clock (built-in since .NET 8) |
| `Microsoft.Extensions.Time.Testing` | `FakeTimeProvider` for deterministic tests |
| `Serilog` + `Serilog.Extensions.Hosting` | Structured logging (native `Serilog.ILogger`) |
| `System.Text.Json` | JSON serialization |

## Network & Protocol

| Package | Purpose |
|---------|---------|
| `SharpPcap` | Network packet capture (UDP sniffing) |
| `PacketDotNet` | Packet parsing |
| `Albion.Network` / `PhotonPackageParser` | Photon protocol deserialization |

Photon UDP traffic sniffed on ports **5056**, **5055**, **4535**.

## Input

HTTP bridge to a Java VNC client at `localhost:8010`:
- `POST /cursor/{x}/{y}` — move cursor
- `POST /mouse/right/down` | `up` — right mouse button
- `POST /mouse/left/click` — left click
- `POST /key/{keycode}` — key press

## Testing

| Package | Purpose |
|---------|---------|
| `xunit` | Test framework |
| `FluentAssertions` | Readable assertions |
| `NSubstitute` | Mocking |

## Game Data Extraction

| Source | Description |
|--------|-------------|
| [ao-data/ao-bin-dumps](https://github.com/ao-data/ao-bin-dumps) | Pre-dumped XML/JSON from game data (~1700+ cluster files) |
| [ao-data/albiondata-bin-dumper](https://github.com/ao-data/albiondata-bin-dumper) | C# tool that decrypts `.bin` → XML/JSON |
| Game install path | `AlbionOnline/game/Albion-Online_Data/StreamingAssets/GameData/` |

Cluster files follow naming: `{id}_{type}_{biome}_{season}_{tier}_{faction}_{zone}.cluster.xml`

## Web Map Viewer

Static HTML/JS/CSS. No backend. Uses Cytoscape.js or vis-network for graph rendering. Loads extracted `world-graph.json`.

## Key Constants

| Constant | Value | Context |
|----------|-------|---------|
| Idle timeout | 1.3s | Character considered stuck |
| Skip-ahead | 4 waypoints / 6.0 units | Route cursor optimization |
| Unstick rotation (normal) | -2π/3 (120°) | Anti-stuck direction change |
| Unstick rotation (post-cluster) | -π/3 (60°) | Shallower after cluster load |
| Max stuck attempts | 5 in 30s window | Before declaring Lost |
| Stale packets after cluster | 5 | Ignored (contain old positions) |
| Isometric rotation | -π/4 (45°) | World → screen coordinate transform |
| Photon ports | 5056, 5055, 4535 | UDP sniffing |
| VNC bridge | localhost:8010 | HTTP input adapter |
