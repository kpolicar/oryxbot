using System;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using OryxBot.Shared.Design;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        
        private Task RunRouteFromBankToQuestNpc() {
            Console.WriteLine("Running route from Bank to Quest NPC");
            return RunRoute(routeProvider.RouteFromBankToQuest()!, OnRouteFromBankToQuestNpcFinished);
        }

        private async Task OnRouteFromBankToQuestNpcFinished(object? sender, EventArgs eventArgs) {
            Console.WriteLine("Route from Bank to Quest NPC finished");
            State.Action = TAKING_QUEST;
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            await KeepTryingToInteractUntilValidInteraction(new Position(-75.5f, 0)).ConfigureAwait(false);
        }
        
        [CallOnRegisterToObject(RequiredState = TAKING_QUEST)]
        private async Task TakingQuestOnRegisterToObject(EventArgs e) {
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.NpcQuestOpenTradeMissionsTab();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.NpcQuestOpenTradeMissionsContractTab();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.NpcQuestSelectTradeMissionsContract();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.NpcQuestAcceptTradeMissionsContract();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);

            await RunTradeMissionRoute().ConfigureAwait(false);
        }
    }
}
