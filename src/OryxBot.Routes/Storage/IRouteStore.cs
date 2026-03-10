using OryxBot.Routes.Models;

namespace OryxBot.Routes.Storage;

public interface IRouteStore
{
    Task<Route?> LoadAsync(string name, CancellationToken ct = default);
    Task SaveAsync(Route route, CancellationToken ct = default);
    Task<IReadOnlyList<RouteMetadata>> ListAsync(CancellationToken ct = default);
}
