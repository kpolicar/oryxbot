using System;

namespace OryxBot.Client.Windows.Bot.Exceptions
{
    public class RouteException : ApplicationException
    {
        public readonly TradeMissionRecord.RecordableStep Step;
        
        public RouteException(TradeMissionRecord.RecordableStep step) =>
            Step = step;
    }
}
