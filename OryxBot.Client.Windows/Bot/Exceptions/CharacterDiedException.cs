namespace OryxBot.Client.Windows.Bot.Exceptions
{
    public class CharacterDiedException : RouteException
    {
        public CharacterDiedException(TradeMissionRecord.RecordableStep step) : base(step) {
        }
    }
}
