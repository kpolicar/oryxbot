using System.Collections.Generic;
using OryxBot.Shared.Design;

namespace OryxBot.Bot.Contracts
{
    public interface TradeMissionRouteProvider
    {
        LinkedList<Position>? Route();
    }
}
