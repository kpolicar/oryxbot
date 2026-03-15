using System.Text.Json;
using System.Text.Json.Serialization;

namespace OryxBot.Simulation.Recording;

public static class RecordingSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(SimulationRecording recording) =>
        JsonSerializer.Serialize(recording, Options);

    public static SimulationRecording? Deserialize(string json) =>
        JsonSerializer.Deserialize<SimulationRecording>(json, Options);

    public static async Task WriteToFileAsync(SimulationRecording recording, string path,
        CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
            Directory.CreateDirectory(dir);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, recording, Options, ct);
    }

    public static async Task<SimulationRecording?> ReadFromFileAsync(string path,
        CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<SimulationRecording>(stream, Options, ct);
    }
}
