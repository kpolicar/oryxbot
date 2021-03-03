using System;
using System.Threading;
using OryxBot.Bot.Attributes;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        
        private void RunRouteFromBankToQuestNpc() {
            Console.WriteLine("Running route from Bank to Quest NPC");
            RunRoute(routeProvider.RouteFromBankToQuest()!, OnRouteFromBankToQuestNpcFinished);
        }

        private void OnRouteFromBankToQuestNpcFinished(object? sender, EventArgs eventArgs) {
            Console.WriteLine("Route from Bank to Quest NPC finished");
            State.Action = TAKING_QUEST;
            actions.StopAllActions();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            KeepTryingToInteractUntilValidInteraction(new Position(-75.5f, 0));
        }
        
        [CallOnRegisterToObject(RequiredState = TAKING_QUEST)]
        private void TakingQuestOnRegisterToObject(EventArgs e) {
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestOpenTradeMissionsTab();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestOpenTradeMissionsContractTab();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestSelectTradeMissionsContract();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestAcceptTradeMissionsContract();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);

            RunTradeMissionRoute();
        }
    }
}
