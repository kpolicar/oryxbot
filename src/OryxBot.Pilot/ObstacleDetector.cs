using Microsoft.Extensions.Options;
using OryxBot.Core.Configuration;

namespace OryxBot.Pilot;

public sealed class ObstacleDetector
{
    private readonly Queue<DateTimeOffset> _attempts = new();
    private readonly TimeProvider _timeProvider;
    private readonly NavigationOptions _options;

    public ObstacleDetector(TimeProvider timeProvider, IOptions<NavigationOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public void RecordAttempt()
    {
        PurgeExpired();
        _attempts.Enqueue(_timeProvider.GetUtcNow());
    }

    public void PurgeExpired()
    {
        var cutoff = _timeProvider.GetUtcNow() - _options.StuckWindow;
        while (_attempts.Count > 0 && _attempts.Peek() < cutoff)
            _attempts.Dequeue();
    }

    public bool IsStuck
    {
        get
        {
            PurgeExpired();
            return _attempts.Count >= _options.MaxStuckAttempts;
        }
    }

    public int AttemptCount
    {
        get
        {
            PurgeExpired();
            return _attempts.Count;
        }
    }

    public void Reset() => _attempts.Clear();
}
