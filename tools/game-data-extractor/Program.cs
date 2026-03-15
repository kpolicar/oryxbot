using System.CommandLine;
using OryxBot.GameDataExtractor.Extraction;
using OryxBot.GameDataExtractor.Models;
using OryxBot.GameDataExtractor.Output;

namespace OryxBot.GameDataExtractor;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("OryxBot Game Data Extractor")
        {
            Name = "oryxbot-extract"
        };

        var gameDirOption = new Option<string>(
            name: "--game-dir",
            description: "Path to Albion Online/game directory (extracts encrypted .bin files)."
        );

        var xmlDirOption = new Option<string>(
            name: "--xml-dir",
            description: "Path to directory containing pre-dumped XML files."
        );

        var outputOption = new Option<string>(
            name: "--output",
            description: "Output directory for JSON files.",
            getDefaultValue: () => "./gamedata"
        );

        var modeOption = new Option<string>(
            name: "--mode",
            description: "What to extract (clusters, worldgraph, localization, all).",
            getDefaultValue: () => "all"
        );

        var formatOption = new Option<string>(
            name: "--format",
            description: "Output format (json, json-pretty).",
            getDefaultValue: () => "json-pretty"
        );

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Log every file processed.",
            getDefaultValue: () => false
        );

        rootCommand.AddOption(gameDirOption);
        rootCommand.AddOption(xmlDirOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(modeOption);
        rootCommand.AddOption(formatOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(async (gameDir, xmlDir, outputDir, mode, format, verbose) =>
        {
            if (string.IsNullOrEmpty(gameDir) && string.IsNullOrEmpty(xmlDir))
            {
                Console.WriteLine("Error: Must provide either --game-dir or --xml-dir");
                return;
            }

            var isEncrypted = !string.IsNullOrEmpty(gameDir);
            var searchDir = isEncrypted ? gameDir : xmlDir;
            var isPretty = format == "json-pretty";

            Console.WriteLine($"Starting extraction from {searchDir}");
            Console.WriteLine($"Options: Mode={mode}, Encrypted={isEncrypted}, Format={format}, Output={outputDir}");

            var extractor = new ClusterExtractor();
            var clusters = new List<ClusterDefinition>();
            
            await foreach (var cluster in extractor.ExtractAllAsync(searchDir!, isEncrypted))
            {
                if (verbose) Console.WriteLine($"Extracted cluster: {cluster.Id} - {cluster.DisplayName}");
                clusters.Add(cluster);
            }

            Console.WriteLine($"Extracted {clusters.Count} clusters.");

            if (mode == "all" || mode == "clusters")
            {
                var dict = clusters
                    .GroupBy(c => c.Id)
                    .ToDictionary(g => g.Key, g => g.First());
                
                if (clusters.Count > dict.Count)
                {
                    Console.WriteLine($"[WARNING] Filtered out {clusters.Count - dict.Count} duplicate cluster IDs.");
                }

                var clustersOutput = new
                {
                    ExtractedAt = DateTime.UtcNow,
                    GameVersion = "unknown",
                    TotalClusters = clusters.Count,
                    Clusters = dict
                };
                await JsonOutputWriter.WriteAsync(outputDir, "clusters.json", clustersOutput, isPretty);
            }

            if (mode == "all" || mode == "worldgraph")
            {
                var builder = new WorldGraphBuilder();
                var worldGraph = builder.Build(clusters);
                
                var graphOutput = new
                {
                    ExtractedAt = DateTime.UtcNow,
                    TotalClusters = worldGraph.TotalClusters,
                    TotalEdges = worldGraph.TotalEdges,
                    Clusters = worldGraph.Clusters,
                    Edges = worldGraph.Edges
                };
                await JsonOutputWriter.WriteAsync(outputDir, "world-graph.json", graphOutput, isPretty);
                Console.WriteLine($"Built world graph with {worldGraph.TotalClusters} clusters and {worldGraph.TotalEdges} edges.");
            }

        }, gameDirOption, xmlDirOption, outputOption, modeOption, formatOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }
}
