using System;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState;

namespace OryxBot.Bot.Attributes
{
    public class CallOnRegisterToObjectAttribute : Attribute
    {
        public TradeMissionAction RequiredState {
            get;
            init;
        }
    }
}
