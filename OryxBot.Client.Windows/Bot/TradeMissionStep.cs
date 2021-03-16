using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using OryxBot.Client.Windows.Bot.Exceptions;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRun
    {
        internal interface TradeMissionStep
        {
            bool Finished {
                get;
            } 
            int Delay {
                get;
            } 
            void Tick();
        }
        
        internal abstract class RunRouteStep : TradeMissionStep
        {
            static RunRouteStep() {
                var successfulParse =
                    int.TryParse(ConfigurationManager.AppSettings.Get("averageTimeToLoadCluster")!, out AverageClusterChangeDuration);
                if (!successfulParse)
                    AverageClusterChangeDuration = DefaultAverageClusterChangeDuration;
                
                successfulParse =
                    float.TryParse(
                        ConfigurationManager.AppSettings.Get("reachedWaypointDistance")!,
                        NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture.NumberFormat,
                        out MaxDistance);
                if (!successfulParse)
                    MaxDistance = DefaultMaxDistance;
            }
        
            private const int DefaultAverageClusterChangeDuration = 8000;
            private static readonly int AverageClusterChangeDuration;
            
            private const float DefaultMaxDistance = 4f;
            private static readonly float MaxDistance;
            
            private const int MaxSkippableSteps = 4;

            public bool Finished { get; private set; }
            public int Delay => 10;
            private bool preparing = true;
            private bool clusterChanged = false;
            
            protected abstract LinkedList<TradeMissionRecord.RecordableStep> Route { get; }
            private ITwoWayEnumerator<TradeMissionRecord.RecordableStep>? Step;
            private bool _characterHasDied;

            public RunRouteStep() {
                LocalCharacter.Instance.ChangeCluster += (_, _) => clusterChanged = true;
                LocalCharacter.Instance.Died += OnCharacterDied;
            }

            private void OnCharacterDied(object? sender, EventArgs e) {
                Debug.WriteLine(">>>>>>>>>>>>>>>>>>> DEATH HAS BEEN MARKED!!");
                if (Step?.Current != null) {
                    Debug.WriteLine(">>>>>>>>>>>>>>>>>>> DEATH HAS BEEN MARKED TRUE!!");
                    _characterHasDied = true;
                }
            }

            public void Tick() {
                if (clusterChanged) {
                    OnChangeCluster();
                    clusterChanged = false;
                    return;
                }
                
                if (preparing) {
                    actions.CenterCursor();
                    preparing = false;
                    Thread.Sleep(500);
                }
                
                if (Step == null) {
                    Step = Route.GetTwoWayEnumerator();
                    Step.MoveNext();
                }
                #if DEBUG
                if (_characterHasDied)
                    throw new CharacterDiedException(Step.Current);
                #endif
                
                ProgressMoveStepsAndSkipIfAlreadyAhead();
                
                if (Step?.Current is TradeMissionRecord.MoveStep target) {
                    actions.MoveTowards(target.Position);
                }
                if (Step?.Current is TradeMissionRecord.ChangeClusterStep) {
                    Thread.Sleep(1000);
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
            
            private void OnChangeCluster() {
                for (var skips = 0 ;; skips++)
                {
                    if (Step!.Current is TradeMissionRecord.ChangeClusterStep)
                        break;
                    if (skips >= MaxSkippableSteps*2) {
                        Console.WriteLine(@"ROUTE EXCEPTION!");
                        if (Step!.Current is TradeMissionRecord.MoveStep move)
                            Console.WriteLine($@"> current step: {move.Position}");
                        for (var i = 0; i < skips; i++)
                            Step.MovePrevious();
                        return;
                        Console.WriteLine(@"ROUTE EXCEPTION!");
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
                
                Thread.Sleep(AverageClusterChangeDuration);
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

        internal abstract class InteractionStep : TradeMissionStep
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
