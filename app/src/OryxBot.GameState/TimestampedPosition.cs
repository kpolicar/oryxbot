using OryxBot.Core.Models;

namespace OryxBot.GameState;

public readonly record struct TimestampedPosition(Position Position, DateTimeOffset Timestamp);
