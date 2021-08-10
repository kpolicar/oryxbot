using System;

namespace OryxBot.Client.Linux.Bot.Events
{
    internal class TradeMissionEvent : EventArgs
    {
        public readonly TradeMissionRun.TradeMissionStep Step;
        
        internal TradeMissionEvent(TradeMissionRun.TradeMissionStep step) =>
            Step = step;
    }
}
