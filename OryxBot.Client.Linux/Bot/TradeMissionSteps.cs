using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Bot.Exceptions;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;
using OryxBot.Shared.Game;
using static OryxBot.Shared.RunConfiguration;

namespace OryxBot.Client.Linux.Bot
{
    public partial class TradeMissionRun
    {
        #region BANKING
        
        internal class RunToBank : RunRouteStep
        {
            public override string Name => "run-to-bank";

            protected override Route Route =>
                _routeManager.RouteFromQuestToBank()!;

            public RunToBank(TradeMissionRun run) : base(run) {
            }
        }

        internal class BankItems : InteractionStep
        {
            public override string Name => "bank-items";
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
            public override string Name => "run-to-quest";
            
            protected override Route Route =>
                _routeManager.RouteFromBankToQuest()!;

            public RunToQuest(TradeMissionRun run) : base(run) {
            }
        }
        
        internal class TakeQuest : InteractionStep
        {
            public override string Name => "take-quest";
            
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
            public override string Name => "run-route-to-destination";
            
            protected override Route Route { get; }

            public RunRouteToDestination(TradeMissionRun run, Route route) : base(run) =>
                Route = route;
        }
        
        internal class ProgressQuest : InteractionStep
        {
            public override string Name => "progress-quest";
            
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
            public override string Name => "run-route-back";
            protected override Route Route { get; }

            public RunRouteBack(TradeMissionRun run, Route route) : base(run) =>
                Route = route;
        }
        
        #endregion

        #region FINISH
        
        internal class FinishQuest : InteractionStep
        {
            public override string Name => "finish-quest";
            
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
