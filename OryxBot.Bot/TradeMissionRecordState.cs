using System.Collections.Generic;
using OryxBot.Shared.Design;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord
    {
        class TradeMissionRecordState
        {
            public LinkedList<Position> RecordedPositions = new();
        }
    }
}
