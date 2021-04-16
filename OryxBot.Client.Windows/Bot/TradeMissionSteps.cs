using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OryxBot.Client.Windows.Bot.Exceptions;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;
using OryxBot.Shared.Game;
using static OryxBot.Shared.RunConfiguration;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRun
    {
        #region BANKING
        
        internal class RunToBank : RunRouteStep
        {
            protected override Route Route =>
                _routeManager.RouteFromQuestToBank()!;
        }

        internal class BankItems : InteractionStep
        {
            private ContractType Contract;

            public BankItems(ContractType contract) =>
                Contract = contract;

            protected override Position _interactablePosition => new(0, 0);
            
            protected override bool DoInteractions() {
                actions.BankRewardItems();
                Thread.Sleep(Delay);
                actions.UnbankTokenItem(Contract, Delay/3);

                return true;
            }
        }
        
        #endregion

        #region TAKE_QUEST

        internal class RunToQuest : RunRouteStep
        {
            protected override Route Route =>
                _routeManager.RouteFromBankToQuest()!;
        }
        
        internal class TakeQuest : InteractionStep
        {
            private City City;
            private City Destination;
            private ContractType Contract;
            
            public TakeQuest(City origin, City destination, ContractType contract) =>
                (City, Destination, Contract) = (origin, destination, contract);

            protected override Position _interactablePosition => Npc.FactionLeader.Position[City];
            
            protected override bool DoInteractions() {
                actions.NpcQuestOpenTradeMissionsTab();
                Thread.Sleep(Delay);
                actions.NpcQuestOpenTradeMissionsContractTab(City, Destination);
                Thread.Sleep(Delay);
                actions.NpcQuestSelectTradeMissionsContract(City, Destination, Contract);
                Thread.Sleep(Delay);
                actions.NpcQuestAcceptTradeMissionsContract();
                
                return true;
            }
        }

        #endregion
        
        #region RUN_QUEST
        
        internal class RunRouteToDestination : RunRouteStep
        {
            protected override Route Route { get; }

            public RunRouteToDestination(Route route) =>
                Route = route;
        }
        
        internal class ProgressQuest : InteractionStep
        {
            protected override Position _interactablePosition =>
                Npc.FactionEmissary.Position
                    .Select(diplomatData => diplomatData.Value)
                    .OrderBy(diplomatPosition => LocalCharacter.Instance.DistanceFrom(diplomatPosition))
                    .First();
            
            protected override bool DoInteractions() {
                actions.NpcQuestProgress();
                return true;
            }
        }
        
        internal class RunRouteBack : RunRouteStep
        {
            protected override Route Route { get; }

            public RunRouteBack(Route route) =>
                Route = route;
        }
        
        #endregion

        #region FINISH
        
        internal class FinishQuest : InteractionStep
        {
            private City City;
            public FinishQuest(City city) =>
                City = city;
            
            protected override Position _interactablePosition => Npc.FactionLeader.Position[City];
            
            protected override bool DoInteractions() {
                actions.NpcQuestProgress();
                return true;
            }
        }

        #endregion
    }
}
