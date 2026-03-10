using System.Text.Json;
using Microsoft.Extensions.Options;
using OryxBot.Routes.Configuration;
using OryxBot.Routes.Models;
using Serilog;

namespace OryxBot.Routes.Storage;

public sealed class JsonRouteStore : IRouteStore
{
    private readonly string _directory;
    private readonly ILogger _logger = Log.ForContext<JsonRouteStore>();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonRouteStore(IOptions<RouteOptions> options)
    {
        _directory = options.Value.Directory;
    }

    public async Task<Route?> LoadAsync(string name, CancellationToken ct = default)
    {
        var path = GetFilePath(name);
        if (!File.Exists(path))
        {
            _logger.Warning("Route file not found: {Path}", path);
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Route>(stream, JsonOptions, ct);
    }

    public async Task SaveAsync(Route route, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_directory);
        var path = GetFilePath(route.Metadata.Name);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, route, JsonOptions, ct);

        _logger.Information("Route saved: {Name} ({WaypointCount} waypoints)", route.Metadata.Name, route.Waypoints.Count);
    }

    public Task<IReadOnlyList<RouteMetadata>> ListAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists(_directory))
            return Task.FromResult<IReadOnlyList<RouteMetadata>>([]);

        var files = Directory.GetFiles(_directory, "*.json");
        var results = new List<RouteMetadata>();

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var info = new FileInfo(file);
            results.Add(new RouteMetadata(name, new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero)));
        }

        return Task.FromResult<IReadOnlyList<RouteMetadata>>(results);
    }

    private string GetFilePath(string name)
    {
        // Sanitize the name to prevent path traversal
        var sanitized = Path.GetFileName(name);
        return Path.Combine(_directory, $"{sanitized}.json");
    }
}
