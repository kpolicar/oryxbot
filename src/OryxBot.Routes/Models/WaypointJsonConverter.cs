using System.Text.Json;
using System.Text.Json.Serialization;

namespace OryxBot.Routes.Models;

public sealed class WaypointJsonConverter : JsonConverter<Waypoint>
{
    public override Waypoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var type = root.GetProperty("type").GetString();
        return type switch
        {
            "move" => new MoveWaypoint(
                root.GetProperty("x").GetSingle(),
                root.GetProperty("y").GetSingle()),
            "portal" => new PortalWaypoint(
                root.GetProperty("clusterName").GetString()!),
            "marker" => new MarkerWaypoint(
                root.GetProperty("markerName").GetString()!),
            _ => throw new JsonException($"Unknown waypoint type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Waypoint value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);

        switch (value)
        {
            case MoveWaypoint move:
                writer.WriteNumber("x", move.X);
                writer.WriteNumber("y", move.Y);
                break;
            case PortalWaypoint portal:
                writer.WriteString("clusterName", portal.ClusterName);
                break;
            case MarkerWaypoint marker:
                writer.WriteString("markerName", marker.MarkerName);
                break;
        }

        writer.WriteEndObject();
    }
}
