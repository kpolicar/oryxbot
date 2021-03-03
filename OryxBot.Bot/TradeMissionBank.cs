using System;
using System.Diagnostics;
using System.Threading;
using OryxBot.Bot.Attributes;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        private void RunRouteFromQuestNpcToBank() {
            Console.WriteLine("Running route from Quest NPC to Bank");
            RunRoute(routeProvider.RouteFromQuestToBank()!, OnRouteFromQuestNpcToBankFinished);
        }
        
        private void OnRouteFromQuestNpcToBankFinished(object? sender, EventArgs eventArgs) {
            Console.WriteLine("Route from Quest NPC to Bank finished");
            State.Action = BANKING;
            actions.StopAllActions();
            
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            KeepTryingToInteractUntilValidInteraction(new Position(0f, 0f));
        }
        
        [CallOnRegisterToObject(RequiredState = BANKING)]
        private void BankingOnRegisterToObject(EventArgs e) {
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.BankRewardItems();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.UnbankTokenItem();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);

            RunRouteFromBankToQuestNpc();
        }
    }
}
