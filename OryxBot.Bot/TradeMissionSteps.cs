using System;
using System.Collections.Generic;
using System.Threading;
using OryxBot.Bot.Exceptions;
using OryxBot.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;

namespace OryxBot.Bot
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
                actions.UnbankTokenItem();

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
            protected override Position _interactablePosition => new(-75.5f, 0);
            
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
                throw new NotImplementedException();
            }
        }

        #endregion

        private abstract class RunRouteStep : TradeMissionStep
        {
            private const float MaxDistance = 4f;
            private const int MaxSkippableSteps = 4;
            
            public bool Finished { get; private set; }
            public int Delay => 10;
            
            protected abstract LinkedList<TradeMissionRecord.RecordableStep> Route { get; }
            private ITwoWayEnumerator<TradeMissionRecord.RecordableStep>? Step;

            public RunRouteStep() {
                Step = Route!.GetTwoWayEnumerator();
                Step.MoveNext();
                LocalCharacter.Instance.ChangeCluster += OnChangeCluster;
            }

            public void Tick() {
                ProgressMoveStepsAndSkipIfAlreadyAhead();
                
                if (Step?.Current is TradeMissionRecord.MoveStep target) {
                    actions.MoveTowards(target.Position);
                }
            }
            
            private void ProgressMoveStepsAndSkipIfAlreadyAhead() {
                if (!(Step?.Current is TradeMissionRecord.MoveStep))
                    return;
                
                var skipped = 0;
                for (int i = 0; i < MaxSkippableSteps; i++) {
                    var hasNext = Step.MoveNext();
                    
                    if (!hasNext || !(Step.Current is TradeMissionRecord.MoveStep)) {
                        break;
                    }
                    skipped++;
                }

                for (int i = 0; i < skipped; i++) {
                    if (IsMoveStepAndCanProgress()) {
                        MoveToNextRouteStep();
                        return;
                    }

                    Step.MovePrevious();
                }
                
                if (IsMoveStepAndCanProgress()) {
                    MoveToNextRouteStep();
                }
            }
            
            private bool IsMoveStepAndCanProgress() =>
                Step?.Current is TradeMissionRecord.MoveStep move &&
                LocalCharacter.Instance.DistanceFrom(move.Position) <= MaxDistance;
            
            private void OnChangeCluster(object? sender, EventArgs e) {
                for (var skips = 0 ;; skips++)
                {
                    if (Step!.Current is TradeMissionRecord.ChangeClusterStep)
                        break;
                    if (skips >= MaxSkippableSteps)
                        throw new RouteException(Step.Current);

                    if (!MoveToNextRouteStep())
                        return;
                }

                var changeCluster = (Step.Current as TradeMissionRecord.ChangeClusterStep)!;
                if (changeCluster.Location != LocalCharacter.Instance.Cluster)
                    throw new RouteException(Step.Current);

                MoveToNextRouteStep();
            }
            
            private bool MoveToNextRouteStep() {
                var hasNext = Step?.MoveNext();
                
                if (hasNext == false) {
                    finishRoute();
                    return false;
                }

                return hasNext != null;
            }
            
            private void finishRoute() {
                actions.StopAllActions();
                Finished = true;
            }
        }

        private abstract class InteractionStep : TradeMissionStep
        {
            private const int DelayBetweenNpcInterfaceActions = 2000;
            private const float MaxDistance = 3f;
            
            public int Delay => CharacterIsNearInteractable || LocalCharacter.Instance.Interacting
                ? 10
                : DelayBetweenNpcInterfaceActions;

            protected abstract Position _interactablePosition { get; }
            public bool CharacterIsNearInteractable =>
                LocalCharacter.Instance.DistanceFrom(_interactablePosition) <= MaxDistance;
            
            public bool Finished { get; private set; }

            public void Tick() {
                if (!LocalCharacter.Instance.Interacting) {
                    AttemptInteraction();
                } else {
                    Finished = DoInteractions();
                }
            }

            protected abstract bool DoInteractions();

            private void AttemptInteraction() {
                if (CharacterIsNearInteractable) {
                    actions.InteractWith(_interactablePosition);
                } else {
                    actions.MoveTowards(_interactablePosition);
                }
            }
        }
    }
}
