using System.Text.Json;
using System.Text.Json.Serialization;
using OryxBot.GameDataExtractor.Models;

namespace OryxBot.GameDataExtractor.Output;

public static class JsonOutputWriter
{
    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions MinifiedOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task WriteAsync(string outputPath, string fileName, object data, bool isPretty)
    {
        Directory.CreateDirectory(outputPath);
        
        var filePath = Path.Combine(outputPath, fileName);
        var options = isPretty ? PrettyOptions : MinifiedOptions;
        
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, data, options);
        
        Console.WriteLine($"Wrote {filePath}");
    }
}
