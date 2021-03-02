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
            RunRoute(routeProvider.RouteFromQuestToBank()!, OnRouteFromQuestNpcToBankFinished);
        }
        
        private void OnRouteFromQuestNpcToBankFinished(object? sender, EventArgs eventArgs) {
            State.Action = BANKING;
            actions.StopAllActions();
            
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            actions.InteractWith(State.LastKnownMove!.Position, new Position(0f,0f));
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
