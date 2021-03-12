using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OryxBot.Bot.Exceptions;
using OryxBot.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        private interface TradeMissionStep
        {
            bool Finished {
                get;
            } 
            int Delay {
                get;
            } 
            void Tick();
        }
        
        private abstract class RunRouteStep : TradeMissionStep
        {
            private const float MaxDistance = 4f;
            private const int MaxSkippableSteps = 4;
            
            public bool Finished { get; private set; }
            public int Delay => 10;
            
            protected abstract LinkedList<TradeMissionRecord.RecordableStep> Route { get; }
            private ITwoWayEnumerator<TradeMissionRecord.RecordableStep>? Step;

            public RunRouteStep() {
                LocalCharacter.Instance.ChangeCluster += OnChangeCluster;
            }

            public void Tick() {
                if (Step == null) {
                    Step = Route.GetTwoWayEnumerator();
                    Step.MoveNext();
                }
                
                ProgressMoveStepsAndSkipIfAlreadyAhead();
                
                if (Step?.Current is TradeMissionRecord.MoveStep target) {
                    actions.MoveTowards(target.Position);
                }
                if (Step?.Current is TradeMissionRecord.ChangeClusterStep) {
                    actions.MoveInSameDirection();
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
                    if (skips >= MaxSkippableSteps) {
                        for (var i = 0; i < skips; i++)
                            Step.MovePrevious();
                        break;
                        Console.WriteLine(@"ROUTE EXCEPTION!");
                        if (Step!.Current is TradeMissionRecord.MoveStep move)
                            Console.WriteLine($@"> current step: {move.Position}");
                        throw new RouteException(Step.Current);
                    }

                    if (!MoveToNextRouteStep())
                        return;
                }

                var changeCluster = (Step.Current as TradeMissionRecord.ChangeClusterStep)!;
                if (changeCluster.Location != LocalCharacter.Instance.Cluster)
                    throw new RouteException(Step.Current);

                MoveToNextRouteStep();

                // Update his current position so as not to accidentally go through portal again
                if (Step?.Current is TradeMissionRecord.MoveStep nextMove)
                    LocalCharacter.Instance.Position = nextMove.Position;
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
                Console.WriteLine(@"route finished!");
                actions.StopAllActions();
                Finished = true;
            }
        }

        private abstract class InteractionStep : TradeMissionStep
        {
            private const int DelayBetweenNpcInterfaceActions = 2000;
            private const float MaxDistance = 3f;
            
            public int Delay => CharacterIsNearInteractable || LocalCharacter.Instance.Interacting
                ? DelayBetweenNpcInterfaceActions
                : 10;

            protected abstract Position _interactablePosition { get; }
            public bool CharacterIsNearInteractable =>
                LocalCharacter.Instance.DistanceFrom(_interactablePosition) <= MaxDistance;
            
            public bool Finished { get; private set; }

            public void Tick() {
                if (!LocalCharacter.Instance.Interacting) {
                    AttemptInteraction();
                } else {
                    actions.StopAllActions();
                    Finished = DoInteractions();

                    Thread.Sleep(500);
                    actions.CenterCursor();
                    Thread.Sleep(500);
                }
            }

            protected abstract bool DoInteractions();

            private void AttemptInteraction() {
                if (CharacterIsNearInteractable) {
                    actions.StopAllActions();
                    Thread.Sleep(DelayBetweenNpcInterfaceActions);
                    actions.InteractWith(_interactablePosition);
                } else {
                    actions.MoveTowards(_interactablePosition);
                }
            }
        }
    }
}
