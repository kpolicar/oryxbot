using System.Collections.Generic;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRecord
    {
        class TradeMissionRecordState
        {
            public LinkedList<RecordableStep> RecordedSteps = new();
        }
    }
}
