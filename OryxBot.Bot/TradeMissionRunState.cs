using System.ComponentModel;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        public class TradeMissionRunState
        {
            public enum Action
            {
                MOVING,
            }

            public Action? Executing {
                internal set;
                get;
            }
        }
    }

}
