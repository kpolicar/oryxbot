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
                CHANGING_CLUSTER,
            }

            public Action? Executing {
                internal set;
                get;
            }
        }
    }

}
