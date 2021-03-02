using System;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState;

namespace OryxBot.Bot.Attributes
{
    public class CallOnChangeClusterAttribute : Attribute
    {
        public TradeMissionAction RequiredState {
            get;
            set;
        }
    }
}
