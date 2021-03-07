using System.Collections.Generic;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord
    {
        class TradeMissionRecordState
        {
            public LinkedList<RecordableStep> RecordedSteps = new();
        }
    }
}
