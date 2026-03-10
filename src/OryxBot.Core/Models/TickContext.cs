namespace OryxBot.Core.Models;

public readonly record struct TickContext(TimeSpan Elapsed, long TickNumber, DateTimeOffset Timestamp);
