using System;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {

        
        [CallOnRegisterToObject(RequiredState = PROGRESSING_QUEST)]
        private async Task ProgressingQuestOnRegisterToObject(EventArgs e) {
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.NpcQuestProgress();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            await RunTradeMissionRouteBack().ConfigureAwait(false);
        }
    }
}
