using OryxBot.Core.Models;

namespace OryxBot.GameState;

/// <summary>
/// Bounded circular buffer that records timestamped positions.
/// </summary>
public sealed class PositionHistory
{
    private readonly TimestampedPosition[] _buffer;
    private readonly TimeProvider _timeProvider;
    private int _head;
    private int _count;

    public PositionHistory(TimeProvider timeProvider, int capacity = 500)
    {
        _timeProvider = timeProvider;
        _buffer = new TimestampedPosition[capacity];
    }

    public int Count => _count;
    public int Capacity => _buffer.Length;

    public void Record(Position position)
    {
        _buffer[_head] = new TimestampedPosition(position, _timeProvider.GetUtcNow());
        _head = (_head + 1) % _buffer.Length;
        if (_count < _buffer.Length)
            _count++;
    }

    public IReadOnlyList<TimestampedPosition> GetRecent(int count)
    {
        count = Math.Min(count, _count);
        var result = new List<TimestampedPosition>(count);

        for (var i = 0; i < count; i++)
        {
            var idx = (_head - 1 - i + _buffer.Length) % _buffer.Length;
            result.Add(_buffer[idx]);
        }

        return result;
    }

    public IReadOnlyList<TimestampedPosition> GetTrail(TimeSpan window)
    {
        var cutoff = _timeProvider.GetUtcNow() - window;
        var result = new List<TimestampedPosition>();

        for (var i = 0; i < _count; i++)
        {
            var idx = (_head - 1 - i + _buffer.Length) % _buffer.Length;
            if (_buffer[idx].Timestamp < cutoff)
                break;
            result.Add(_buffer[idx]);
        }

        return result;
    }

    /// <summary>
    /// Returns recent positions in reverse order, deduplicated (skip entries less than 1.0 apart).
    /// Useful for backtracking when stuck.
    /// </summary>
    public IEnumerable<Position> GetBacktrackPath(int maxSteps)
    {
        Position? last = null;
        var yielded = 0;

        for (var i = 0; i < _count && yielded < maxSteps; i++)
        {
            var idx = (_head - 1 - i + _buffer.Length) % _buffer.Length;
            var pos = _buffer[idx].Position;

            if (last is null || Position.Distance(last.Value, pos) >= 1.0f)
            {
                yield return pos;
                last = pos;
                yielded++;
            }
        }
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
    }
}
