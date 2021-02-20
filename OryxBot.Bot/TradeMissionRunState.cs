using System.ComponentModel;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        public class TradeMissionRunState
        {
            public enum Action
            {
                UNBANKING_TOKENS,
                TAKING_QUEST,
                RETAKING_QUEST,
                MOVING,
                CHANGING_CLUSTER,
                BANKING_REWARDS,
            }

            public Action Executing {
                internal set;
                get;
            } = Action.TAKING_QUEST;
        }
    }

}
