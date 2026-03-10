namespace OryxBot.Pilot;

/// <summary>
/// Stub — real implementation in Phase 2.
/// </summary>
public class NavigationController
{
    public NavigationDecision Evaluate() =>
        throw new NotImplementedException("NavigationController implementation is Phase 2");

    public Task ExecuteAsync(NavigationDecision decision, CancellationToken ct = default) =>
        throw new NotImplementedException("NavigationController implementation is Phase 2");

    public NavigationSnapshot TakeSnapshot() =>
        throw new NotImplementedException("NavigationController implementation is Phase 2");
}
