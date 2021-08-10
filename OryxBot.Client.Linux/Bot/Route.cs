using System.Collections.Generic;
using OryxBot.Shared.Game;

namespace OryxBot.Client.Linux.Bot
{
    public class Route : LinkedList<TradeMissionRecord.RecordableStep>
    {
        public virtual string Name { get; set; }
        public virtual Region? Origin { get; set; }
        public virtual Region? Destination { get; set; }
    }
}
