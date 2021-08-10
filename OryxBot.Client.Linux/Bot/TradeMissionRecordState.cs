using System;
using System.Collections.Generic;

namespace OryxBot.Client.Linux.Bot
{
    public partial class TradeMissionRecord
    {
        public class TradeMissionRecordState
        {
            public EventHandler? StatusChanged;
            
            private BotStatus _status = BotStatus.RecordingWaitingToStartQuest;
            public BotStatus Status {
                internal set {
                    _status = value;
                    StatusChanged?.Invoke(this, EventArgs.Empty);
                }
                get => _status;
            }
            public LinkedList<RecordableStep> RecordedSteps = new();
        }
    }
}
