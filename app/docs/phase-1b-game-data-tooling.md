# Phase 1b — Game Data Extraction & Map Viewer

> **Goal:** Build a CLI tool that extracts Albion Online game data (cluster definitions,  
> portal connections, localization) into structured JSON, and a lightweight web viewer  
> that renders the resulting world graph as an interactive map.  
> **Target:** .NET 10 · Latest C# · Linux-first  
> **Depends on:** Phase 1 (OryxBot.Core models: Position, Cluster)  
> **Unlocks:** Phase 2 (Pathfinder needs the world graph to compute routes)

---

## Deliverables

1. `tools/OryxBot.GameDataExtractor` — CLI tool that reads Albion's game data files and outputs:
   - `clusters.json` — all clusters with IDs, types, tiers, biomes, exits
   - `world-graph.json` — adjacency list of cluster connections (portal → portal)
   - `localization.json` — display names for clusters, NPCs, items (optional but useful)
2. `web/OryxBot.MapViewer` — minimal web app that loads the world graph JSON and renders a navigable map
3. Unit tests for the extraction pipeline

---

## Background: How Albion Game Data Works

### Game Install Layout

```
AlbionOnline/
├── game/
│   └── Albion-Online_Data/
│       └── StreamingAssets/
│           └── GameData/
│               ├── cluster/           ← Encrypted .bin files (one per map zone)
│               ├── world/             ← World definition files
│               ├── localization/      ← Display name translations
│               └── ...               ← Items, spells, mobs, etc.
```

Each `.bin` file is an **AES-encrypted XML document**. The encryption key is embedded in the game client and is publicly known (used by the community data-mining tools).

### Reference: ao-data Ecosystem

| Repository | Purpose |
|------------|---------|
| [ao-data/ao-bin-dumps](https://github.com/ao-data/ao-bin-dumps) | Pre-dumped XML/JSON from game data. ~1700+ cluster files. Updated each patch. |
| [ao-data/albiondata-bin-dumper](https://github.com/ao-data/albiondata-bin-dumper) | C# .NET 7 tool that decrypts `.bin` → XML/JSON. CLI: `-d` (game dir), `-o` (output), `-m` (mode) |

The bin-dumper tool supports these modes:
- `ItemExtraction` — items.json
- `LocationExtraction` — clusters + world data
- `DumpAllXML` — raw XML dump of all `.bin` files
- `Everything` — all of the above

### Cluster File Format

Dumped cluster files follow the naming convention:

```
{id}_{type}_{biome}_{season}_{tier}_{faction}_{zone}.cluster.xml
```

Example: `4205_WRL_MN_AUTO_T5_KPR_ROY.cluster.xml`

| Field | Value | Meaning |
|-------|-------|---------|
| `id` | 4205 | Unique cluster ID |
| `type` | WRL | World (overworld) |
| `biome` | MN | Mountain |
| `season` | AUTO | Auto (matches game season) |
| `tier` | T5 | Tier 5 zone |
| `faction` | KPR | Keeper (NPC faction) |
| `zone` | ROY | Royal Continent |

The XML inside contains the full cluster definition — exits/portals, spawn points, resource nodes, NPC placements, and terrain boundaries.

### Key XML Elements for World Graph

```xml
<cluster id="4205" displayname="@CLUSTER_4205" type="WORLD">
  <exits>
    <exit id="exit_north" targetcluster="4206" targetexitid="exit_south">
      <position x="128.5" y="0.0" />
    </exit>
    <exit id="exit_east" targetcluster="4210" targetexitid="exit_west">
      <position x="255.0" y="128.0" />
    </exit>
  </exits>
</cluster>
```

> **Note:** The exact XML schema varies between game patches. The extractor should be resilient to missing/extra elements and log warnings rather than failing.

---

## 1. CLI Tool: OryxBot.GameDataExtractor

### 1.1 Project Setup

```
tools/OryxBot.GameDataExtractor/
├── OryxBot.GameDataExtractor.csproj   — Console app, net10.0
├── Program.cs                          — CLI entry point (System.CommandLine)
├── Extraction/
│   ├── BinDecryptor.cs                 — AES-256-CBC decryption of .bin files
│   ├── ClusterExtractor.cs             — Parses cluster XML → ClusterDefinition
│   ├── WorldGraphBuilder.cs            — Assembles clusters into WorldGraph
│   └── LocalizationExtractor.cs        — Parses localization files → display names
├── Models/
│   ├── ClusterDefinition.cs            — Raw parsed cluster data
│   ├── ExitDefinition.cs               — Raw parsed exit/portal data
│   └── WorldGraphOutput.cs             — Serialization model for output JSON
└── Output/
    ├── JsonOutputWriter.cs             — Writes structured JSON files
    └── OutputOptions.cs                — Pretty-print, minify, etc.
```

### 1.2 CLI Interface

```bash
# Extract from game install (decrypts .bin files)
oryxbot-extract --game-dir "/path/to/AlbionOnline/game" --output "./gamedata"

# Extract from pre-dumped XML (ao-bin-dumps checkout)
oryxbot-extract --xml-dir "/path/to/ao-bin-dumps/cluster" --output "./gamedata"

# Options
--mode <clusters|worldgraph|localization|all>   # What to extract (default: all)
--format <json|json-pretty>                      # Output format (default: json-pretty)
--verbose                                         # Log every file processed
```

Use `System.CommandLine` for argument parsing.

### 1.3 BinDecryptor

Decrypts `.bin` files from the game install into XML strings.

```csharp
public static class BinDecryptor
{
    // AES-256-CBC with known key + IV (same as albiondata-bin-dumper)
    // Key and IV are publicly known community resources — they decode the game's
    // own data files for statistical/informational purposes.
    
    public static string DecryptBinFile(string binFilePath);
    public static async Task<string> DecryptBinFileAsync(string binFilePath, CancellationToken ct);
}
```

Implementation reference: study the `albiondata-bin-dumper` source for the exact key/IV and decryption approach. The key is a static byte array embedded in the tool.

### 1.4 ClusterExtractor

Parses cluster XML (either decrypted or from pre-dumped files) into a structured model:

```csharp
public class ClusterExtractor
{
    public ClusterDefinition? ExtractFromXml(string xml, string sourceFileName);
    public async IAsyncEnumerable<ClusterDefinition> ExtractAllAsync(
        string directoryPath, 
        bool isEncrypted,
        [EnumeratorCancellation] CancellationToken ct);
}
```

**ClusterDefinition:**
```csharp
public record ClusterDefinition
{
    public required string Id { get; init; }          // "4205"
    public required string DisplayName { get; init; } // "@CLUSTER_4205" (localization key)
    public required string Type { get; init; }        // "WORLD", "CITY", "DUNGEON"
    public string? Biome { get; init; }               // "MN" (mountain), "SW" (swamp), etc.
    public string? Tier { get; init; }                // "T5"
    public string? Faction { get; init; }             // "KPR" (keeper)
    public string? Zone { get; init; }                // "ROY" (royal), "OUT" (outlands)
    public required List<ExitDefinition> Exits { get; init; }
}

public record ExitDefinition
{
    public required string ExitId { get; init; }            // "exit_north"
    public required string TargetClusterId { get; init; }   // "4206"
    public required string TargetExitId { get; init; }      // "exit_south"
    public required float PositionX { get; init; }
    public required float PositionY { get; init; }
}
```

### 1.5 WorldGraphBuilder

Takes all extracted clusters and builds the adjacency graph:

```csharp
public class WorldGraphBuilder
{
    public WorldGraphOutput Build(IReadOnlyList<ClusterDefinition> clusters);
}
```

The builder:
1. Creates a node for each cluster
2. Creates edges from exit definitions (bidirectional — if A→B exists, B→A should too)
3. Validates consistency: if exit references a target cluster that doesn't exist, log a warning
4. Deduplicates edges (same source→target pair)

**WorldGraphOutput:**
```csharp
public record WorldGraphOutput
{
    public required Dictionary<string, ClusterNode> Clusters { get; init; }
    public required List<Edge> Edges { get; init; }
    public int TotalClusters => Clusters.Count;
    public int TotalEdges => Edges.Count;
}

public record ClusterNode
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Type { get; init; }
    public string? Tier { get; init; }
    public string? Biome { get; init; }
    public string? Zone { get; init; }
}

public record Edge
{
    public required string FromClusterId { get; init; }
    public required string FromExitId { get; init; }
    public required float FromX { get; init; }
    public required float FromY { get; init; }
    public required string ToClusterId { get; init; }
    public required string ToExitId { get; init; }
    public required float ToX { get; init; }
    public required float ToY { get; init; }
}
```

### 1.6 Output JSON Schema

**clusters.json:**
```json
{
  "extractedAt": "2025-01-15T10:30:00Z",
  "gameVersion": "unknown",
  "totalClusters": 1723,
  "clusters": {
    "4205": {
      "id": "4205",
      "displayName": "Some Mountain Pass",
      "type": "WORLD",
      "tier": "T5",
      "biome": "MN",
      "zone": "ROY",
      "exits": [
        {
          "exitId": "exit_north",
          "targetClusterId": "4206",
          "targetExitId": "exit_south",
          "positionX": 128.5,
          "positionY": 0.0
        }
      ]
    }
  }
}
```

**world-graph.json:**
```json
{
  "extractedAt": "2025-01-15T10:30:00Z",
  "totalClusters": 1723,
  "totalEdges": 3641,
  "clusters": {
    "4205": {
      "id": "4205",
      "displayName": "Some Mountain Pass",
      "type": "WORLD",
      "tier": "T5",
      "biome": "MN",
      "zone": "ROY"
    }
  },
  "edges": [
    {
      "fromClusterId": "4205",
      "fromExitId": "exit_north",
      "fromX": 128.5,
      "fromY": 0.0,
      "toClusterId": "4206",
      "toExitId": "exit_south",
      "toX": 128.5,
      "toY": 255.0
    }
  ]
}
```

---

## 2. Web Map Viewer: OryxBot.MapViewer

A simple single-page web app that loads the `world-graph.json` and renders it as an interactive graph.

### 2.1 Technology

- **No backend server required** — static HTML/JS/CSS served from disk or a simple file server
- **Rendering:** Use [Cytoscape.js](https://js.cytoscape.org/) (graph visualization library) or [vis-network](https://visjs.github.io/vis-network/docs/network/) for node-edge rendering
- **Layout:** Force-directed or hierarchical layout, with manual position overrides possible
- **Alternative:** If a more map-like experience is desired, use [Leaflet.js](https://leafletjs.com/) with a custom tile layer or SVG overlay

### 2.2 Project Structure

```
web/OryxBot.MapViewer/
├── index.html
├── styles.css
├── app.js
├── lib/                     — Vendored or CDN-linked dependencies
└── data/                    — Symlink or copy of gamedata/ output
    ├── world-graph.json
    └── clusters.json
```

### 2.3 Features (MVP)

1. **Render all clusters as nodes** — colored by type (city = gold, world = green, dungeon = gray)
2. **Render portal connections as edges** — bidirectional edges shown as lines
3. **Click a cluster** → show info panel: ID, display name, type, tier, biome, exits
4. **Search** → type a cluster name or ID to focus the view
5. **Zoom & pan** → standard graph navigation
6. **Filter** → toggle visibility by type (cities only, world only), by tier, by zone

### 2.4 Data Loading

```javascript
// app.js
async function loadWorldGraph() {
    const response = await fetch('data/world-graph.json');
    const graph = await response.json();
    
    const nodes = Object.values(graph.clusters).map(c => ({
        id: c.id,
        label: c.displayName || c.id,
        group: c.type,
        tier: c.tier,
        zone: c.zone
    }));
    
    const edges = graph.edges.map(e => ({
        from: e.fromClusterId,
        to: e.toClusterId,
        arrows: ''  // Bidirectional, no arrows
    }));
    
    renderGraph(nodes, edges);
}
```

### 2.5 Reference: albiononline2d.com Map

The website [albiononline2d.com/en/map](https://albiononline2d.com/en/map) serves as a reference for what a fully-featured map viewer looks like. Our MVP is simpler — a graph visualization of cluster connectivity, not a geographic tile map. Eventually, if cluster position data is available, the viewer could evolve into a proper geographic map.

---

## 3. Integration with the Main Bot

### 3.1 WorldGraph Loading

The OryxBot.WorldGraph project consumes the extracted JSON:

```csharp
// src/OryxBot.WorldGraph/WorldGraphLoader.cs
public class JsonWorldGraphProvider : IWorldGraphProvider
{
    private readonly string _worldGraphPath;  // From config: WorldGraph:DataDirectory
    
    public WorldGraph GetWorldGraph()
    {
        var json = File.ReadAllText(Path.Combine(_worldGraphPath, "world-graph.json"));
        var output = JsonSerializer.Deserialize<WorldGraphOutput>(json);
        return MapToWorldGraph(output);
    }
    
    private WorldGraph MapToWorldGraph(WorldGraphOutput output)
    {
        // Convert WorldGraphOutput (serialization model) → WorldGraph (domain model)
        // Map ClusterNode → Cluster, Edge → ClusterExit
    }
}
```

### 3.2 Pathfinder Integration

Once the world graph is loaded, the Pathfinder (Phase 2) uses it:

```csharp
public class Pathfinder : IPathfinder
{
    private readonly IWorldGraphProvider _graphProvider;
    
    public ClusterPath? FindPath(string from, string to)
    {
        var graph = _graphProvider.GetWorldGraph();
        // A* or Dijkstra on graph.Clusters + graph adjacency
    }
}
```

---

## 4. Testing

### 4.1 Test Projects

```
tests/OryxBot.GameDataExtractor.Tests/
├── ClusterExtractorTests.cs
├── WorldGraphBuilderTests.cs
├── BinDecryptorTests.cs
└── Fixtures/
    ├── sample_cluster_4205.xml       — Real cluster XML (from ao-bin-dumps)
    ├── sample_cluster_city.xml       — City cluster example
    ├── sample_cluster_dungeon.xml    — Dungeon cluster example
    ├── malformed_cluster.xml         — Missing elements, to test resilience
    └── expected_worldgraph.json      — Expected output for fixture set
```

### 4.2 Test Cases

**ClusterExtractorTests:**
- `ExtractFromXml_ValidCluster_ParsesAllFields`
- `ExtractFromXml_MissingExits_ReturnsEmptyExitList`
- `ExtractFromXml_MalformedXml_ReturnsNull`
- `ExtractFromXml_CityCluster_TypeIsCity`
- `ExtractAllAsync_DirectoryWithMultipleFiles_ReturnsAll`

**WorldGraphBuilderTests:**
- `Build_TwoClusters_WithExitsToBoth_CreatesEdges`
- `Build_ExitToNonexistentCluster_LogsWarningAndSkips`
- `Build_DuplicateEdges_Deduplicated`
- `Build_EmptyInput_ReturnsEmptyGraph`
- `Build_LargeDataset_CompletesReasonably` (performance sanity check)

**BinDecryptorTests:**
- `DecryptBinFile_ValidEncryptedFile_ReturnsXml`
- `DecryptBinFile_InvalidFile_ThrowsWithMessage`
- `DecryptBinFile_TruncatedFile_ThrowsWithMessage`

---

## 5. Acceptance Criteria

- [ ] `oryxbot-extract --xml-dir <ao-bin-dumps-path> --output ./gamedata` produces valid JSON
- [ ] `clusters.json` contains >1000 clusters (game has ~1700+)
- [ ] `world-graph.json` contains edges linking clusters bidirectionally
- [ ] Every edge's `toClusterId` exists in the clusters dictionary
- [ ] `index.html` opens in a browser and renders the graph
- [ ] Clicking a cluster node shows its metadata
- [ ] Search finds a cluster by name or ID
- [ ] All extractor tests pass
- [ ] The extracted `world-graph.json` can be loaded by `JsonWorldGraphProvider`
