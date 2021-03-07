using System;
using System.Threading;
using OryxBot.Bot.Attributes;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {

        
        [CallOnRegisterToObject(RequiredState = PROGRESSING_QUEST)]
        private void ProgressingQuestOnRegisterToObject(EventArgs e) {
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestProgress();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            RunTradeMissionRouteBack();
        }
    }
}
