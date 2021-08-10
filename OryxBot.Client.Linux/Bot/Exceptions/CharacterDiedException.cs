namespace OryxBot.Client.Linux.Bot.Exceptions
{
    public class CharacterDiedException : RouteException
    {
        public CharacterDiedException(TradeMissionRecord.RecordableStep step) : base(step) {
        }
    }
}
