using System;
using System.Collections.Generic;
using System.Threading;
using OryxBot.Client.Windows.Bot.Exceptions;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRun
    {
        #region BANKING
        
        private class RunToBank : RunRouteStep
        {
            protected override LinkedList<TradeMissionRecord.RecordableStep> Route =>
                routeProvider.RouteFromQuestToBank()!;
        }

        private class BankItems : InteractionStep
        {
            protected override Position _interactablePosition => new(0, 0);
            
            protected override bool DoInteractions() {
                actions.BankRewardItems();
                Thread.Sleep(Delay);
                actions.UnbankTokenItem(Delay);

                return true;
            }
        }
        
        #endregion

        #region TAKE_QUEST

        private class RunToQuest : RunRouteStep
        {
            protected override LinkedList<TradeMissionRecord.RecordableStep> Route =>
                routeProvider.RouteFromBankToQuest()!;
        }
        
        private class TakeQuest : InteractionStep
        {
            protected override Position _interactablePosition => new(-75.5f, 0);
            
            protected override bool DoInteractions() {
                actions.NpcQuestOpenTradeMissionsTab();
                Thread.Sleep(Delay);
                actions.NpcQuestOpenTradeMissionsContractTab();
                Thread.Sleep(Delay);
                actions.NpcQuestSelectTradeMissionsContract();
                Thread.Sleep(Delay);
                actions.NpcQuestAcceptTradeMissionsContract();
                
                return true;
            }
        }

        #endregion
        
        #region RUN_QUEST
        
        private class RunRouteToDestination : RunRouteStep
        {
            protected override LinkedList<TradeMissionRecord.RecordableStep> Route { get; }

            public RunRouteToDestination(LinkedList<TradeMissionRecord.RecordableStep> route) =>
                Route = route;
        }
        
        private class ProgressQuest : InteractionStep
        {
            protected override Position _interactablePosition => new(50.012352f, 190.30232f);
            
            protected override bool DoInteractions() {
                actions.NpcQuestProgress();
                return true;
            }
        }
        
        private class RunRouteBack : RunRouteStep
        {
            protected override LinkedList<TradeMissionRecord.RecordableStep> Route { get; }

            public RunRouteBack(LinkedList<TradeMissionRecord.RecordableStep> route) =>
                Route = route;
        }
        
        #endregion

        #region FINISH
        
        private class FinishQuest : InteractionStep
        {
            protected override Position _interactablePosition => new(-75.5f, 0);
            
            protected override bool DoInteractions() {
                actions.NpcQuestProgress();
                return true;
            }
        }

        #endregion
    }
}
