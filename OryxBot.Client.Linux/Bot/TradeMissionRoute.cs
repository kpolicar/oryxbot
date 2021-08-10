using OryxBot.Shared.Game;

namespace OryxBot.Client.Linux.Bot
{
    public class TradeMissionRoute : Route
    {
        public readonly Route RouteToNpc;
        public readonly Route RouteBack;
        public override string Name => RouteToNpc.Name;
        public override Region? Origin => RouteToNpc.Origin;
        public override Region? Destination => RouteToNpc.Destination;

        public TradeMissionRoute(Route route, Route routeBack) =>
            (RouteToNpc, RouteBack) = (route, routeBack);
    }
}
