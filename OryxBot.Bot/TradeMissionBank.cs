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
        private Task RunRouteFromQuestNpcToBank() {
            Console.WriteLine("Running route from Quest NPC to Bank");
            return RunRoute(routeProvider.RouteFromQuestToBank()!, OnRouteFromQuestNpcToBankFinished);
        }
        
        private async Task OnRouteFromQuestNpcToBankFinished(object? sender, EventArgs eventArgs) {
            Console.WriteLine("Route from Quest NPC to Bank finished");
            State.Action = BANKING;
            
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            await KeepTryingToInteractUntilValidInteraction(new Position(0f, 0f)).ConfigureAwait(false);
        }
        
        [CallOnRegisterToObject(RequiredState = BANKING)]
        private async Task BankingOnRegisterToObject(EventArgs e) {
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.BankRewardItems();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            actions.UnbankTokenItem();
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);

            await RunRouteFromBankToQuestNpc().ConfigureAwait(false);
        }
    }
}
