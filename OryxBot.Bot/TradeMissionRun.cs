using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Exceptions;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        private const float MaxDistance = 4f;
        private const int MaxSkippableSteps = 4;
        private const int ClusterAvgLoadTime = 5000;
        private const int DelayBetweenNpcInterfaceActions = 2000;

        public TradeMissionRunState State {
            get;
            private set;
        } = new();
        private AlbionDataProvider dataProvider = null!;
        private LinkedList<TradeMissionRecord.RecordableStep> Route;
        private IEnumerator<TradeMissionRecord.RecordableStep> Step = null!;
        private ActionFactory actions = null!;


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps) =>
            Route = steps;
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            dataProvider.Move += RuntimeEventListener<MoveEventArgs>(OnCharacterMove);
            dataProvider.ChangeCluster += RuntimeEventListener<ChangeClusterEventArgs>(OnChangeCluster);
            dataProvider.RegisterToObject += RuntimeEventListener(OnRegisterToObject);
            // todo InventoryMoveItem
            actions = serviceContainer.GetService<ActionFactory>();
        }

        public override void Start() {
            Step = Route.GetEnumerator();
            Step.MoveNext();
            base.Start();
        }

        public override void Stop() {
            base.Stop();
            State = new();
            Step.Reset();
        }

        private void OnRegisterToObject(object? sender, EventArgs e) {
            switch (State.Executing) {
                case TradeMissionRunState.Action.TAKING_QUEST:
                    TakingQuestOnRegisterToObject();
                    break;
                case TradeMissionRunState.Action.BANKING_REWARDS:
                    BankingRewardsOnRegisterToObject();
                    break;
                default:
                    return;
            }
        }

        private void OnCharacterMove(object? sender, MoveEventArgs moveEvent) {
            switch (State.Executing) {
                case TradeMissionRunState.Action.TAKING_QUEST:
                    TakingQuestOnMove(moveEvent);
                    break;
                case TradeMissionRunState.Action.RETAKING_QUEST:
                    RetakingQuestOnMove(moveEvent);
                    break;
                case TradeMissionRunState.Action.MOVING:
                    RunningRouteOnMove(moveEvent);
                    break;
                case TradeMissionRunState.Action.BANKING_REWARDS:
                    BankingRewardsOnMove(moveEvent);
                    break;
                default:
                    return;
            }
        }

        private void BankingRewardsOnRegisterToObject() {
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.BankRewardItems();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.UnbankTokenItem();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            // move
            State.Executing = TradeMissionRunState.Action.RETAKING_QUEST;
        }

        private void TakingQuestOnRegisterToObject() {
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestOpenTradeMissionsTab();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestOpenTradeMissionsContractTab();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestSelectTradeMissionsContract();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            actions.NpcQuestAcceptTradeMissionsContract();
            Thread.Sleep(DelayBetweenNpcInterfaceActions);

            // move
            State.Executing = TradeMissionRunState.Action.MOVING;
        }

        private void RetakingQuestOnMove(MoveEventArgs moveEvent) {
            var target = new Position(0, 0);

            if (Helpers.Math.Distance(moveEvent.Position, target) <= MaxDistance) {
                // Click on NPC and wait for object interaction
            } else {
                actions.MoveTowards(moveEvent.Position, target);
            }
        }

        private void TakingQuestOnMove(MoveEventArgs moveEvent) {
            var target = new Position(0, 0); // todo: taking quest position

            if (Helpers.Math.Distance(moveEvent.Position, target) <= MaxDistance) {
                // Click on NPC and wait for object interaction
            } else {
                actions.MoveTowards(moveEvent.Position, target);
            }
        }

        private void BankingRewardsOnMove(MoveEventArgs moveEvent) {
            var target = new Position(0, 0); // todo: taking quest position
            
            if (Helpers.Math.Distance(moveEvent.Position, target) <= MaxDistance) {
                // Click on NPC and wait for object interaction
            } else {
                actions.MoveTowards(moveEvent.Position, target);
            }
        }

        private void RunningRouteOnMove(MoveEventArgs moveEvent) {
            if (!(Step.Current is TradeMissionRecord.MoveStep target))
                return;

            while (Step.Current is TradeMissionRecord.MoveStep move &&
                   Helpers.Math.Distance(move.Position, moveEvent.Position) <= MaxDistance)
            {
                Step.MoveNext();
                State.Executing = TradeMissionRunState.Action.MOVING;
                target = move;
            }
            actions.MoveTowards(moveEvent.Position, target.Position);
        }

        private void OnChangeCluster(object? sender, ChangeClusterEventArgs e) {
            for (var skips = 0 ;; skips++)
            {
                if (Step.Current is TradeMissionRecord.ChangeClusterStep)
                    break;
                if (skips >= MaxSkippableSteps)
                    throw new RouteException(Step.Current);

                Step.MoveNext();
            }

            var changeCluster = (Step.Current as TradeMissionRecord.ChangeClusterStep)!;
            if (changeCluster.Location != e.Location)
                throw new RouteException(Step.Current);
            
            Step.MoveNext();
            State.Executing = TradeMissionRunState.Action.CHANGING_CLUSTER;

            if (Step.Current is TradeMissionRecord.MoveStep move) {
                Thread.Sleep(ClusterAvgLoadTime);
                Task.Run(() => KeepTryingToMoveUntilStateChange(move, State.Executing));
            }
        }

        private void KeepTryingToMoveUntilStateChange(TradeMissionRecord.MoveStep move, TradeMissionRunState.Action state) {
            while (State.Executing == state) {
                actions.MoveTowards(default, move.Position);
                Thread.Sleep(1000);
            }
        }
    }
}
