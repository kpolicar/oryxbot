namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        private interface TradeMissionStep
        {
            bool Finished {
                get;
            } 
            int Delay {
                get;
            } 
            void Tick();
        }
    }
}
