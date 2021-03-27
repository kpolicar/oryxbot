using System.Collections.Generic;
using OryxBot.Shared.Game;

namespace OryxBot.Client.Windows.Bot
{
    public class Route : LinkedList<TradeMissionRecord.RecordableStep>
    {
        public Region Origin;
        public Region Destination;
    }
}
