using System;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState;

namespace OryxBot.Bot.Attributes
{
    public class CallOnUnregisterFromObjectAttribute : Attribute
    {
        public TradeMissionAction RequiredState {
            get;
            init;
        }
    }
}
