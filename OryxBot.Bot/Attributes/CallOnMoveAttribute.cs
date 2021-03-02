using System;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState;

namespace OryxBot.Bot.Attributes
{
    public class CallOnMoveAttribute : Attribute
    {
        public TradeMissionAction RequiredState {
            get;
            set;
        }
    }
}
