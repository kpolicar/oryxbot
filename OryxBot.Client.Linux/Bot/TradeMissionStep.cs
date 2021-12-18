using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using OryxBot.Client.Linux.Bot.Exceptions;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;

namespace OryxBot.Client.Linux.Bot
{
    public partial class TradeMissionRun
    {
        internal interface TradeMissionStep
        {
            string Name {
                get;
            }
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
                AverageClusterChangeDuration = DefaultAverageClusterChangeDuration;
                MaxDistance = DefaultMaxDistance;
            }
        
            private const int DefaultAverageClusterChangeDuration = 8000;
            private static readonly int AverageClusterChangeDuration;
            
            private const float DefaultMaxDistance = 6f;
            private static readonly float MaxDistance;
            
            private const int MaxSkippableSteps = 4;

            public abstract string Name { get; }
            public bool Finished { get; private set; }
            public int Delay => 10;
            private bool preparing = true;
            private bool clusterChanged = false;
            
            protected abstract Route Route { get; }
            private ITwoWayEnumerator<TradeMissionRecord.RecordableStep>? Step;
            private bool _characterHasDied;
            private bool hasMadeFirstMove = false;
            
            private ExpiringTimestampsList stuckTimestamps = new (IdleTimeout);
            public event EventHandler? Stuck;
            internal const int IdleTimeout = 30000;
            internal const int TryToGetUnstuckAfterIdleDuration = 1000;
            internal const int MaxTriesToGetUnstuckWithinTimeout = 5;

            public RunRouteStep(TradeMissionRun run) {
                LocalCharacter.Instance.ChangeCluster += (_, _) => clusterChanged = true;
                LocalCharacter.Instance.Died += OnCharacterDied;
                LocalCharacter.Instance.Move += (_, _) => hasMadeFirstMove = true;
                run.Started += OnRunStarted;
            }

            public bool SkipRouteToStep(string alias, Position approximatePosition) {
                var step = Route.GetTwoWayEnumerator();
                step.MoveNext();

                var arrivedAtClusterAlias = false;
                var moveStepsForThisAlias = new List<TradeMissionRecord.MoveStep>();
                
                while (true) {
                    if (step?.Current is TradeMissionRecord.ChangeClusterStep changeClusterStep) {
                        Console.WriteLine("alias is "+changeClusterStep);
                        if (arrivedAtClusterAlias)
                            break;
                        arrivedAtClusterAlias = changeClusterStep.Alias == alias;
                    } else if (step?.Current is TradeMissionRecord.MoveStep moveStep) {
                        if (arrivedAtClusterAlias)
                            moveStepsForThisAlias.Add(moveStep);
                    }
                    
                    var hasNext = step.MoveNext();
                    if (!hasNext)
                        break;
                }

                Console.WriteLine("moveStepsForThisAlias count is "+moveStepsForThisAlias.Count +" for " + alias);
                if (moveStepsForThisAlias.Count == 0)
                    return false;

                var closestPosition = moveStepsForThisAlias.MinBy(moveStep =>
                    Helpers.Math.Distance(moveStep.Position, approximatePosition));
                
                Console.WriteLine("Closest move step to current character position is "+closestPosition);
                
                // Traverse back to the closest Position
                while (true) {
                    var hasNext = step.MovePrevious(); // The last step we arrived at was a change cluster step
                    var moveStep = step.Current as TradeMissionRecord.MoveStep;
                    
                    if (!hasNext || moveStep == null) {
                        Console.WriteLine("Skipping route to a certain step failed for some reason");
                        return false;
                    }
                    if (moveStep == closestPosition) {
                        break;
                    }
                }

                Console.WriteLine("successfully executed resume to "+alias);
                Step = step;
                return true;
            }

            private void MoveCharacterTowardsMoveStep() {
                if (Step?.Current is TradeMissionRecord.MoveStep target) {
                    var tryToGetUnstuck =
                        hasMadeFirstMove &&
                        !LocalCharacter.Instance.Moving &&
                        LocalCharacter.Instance.IdleDuration >= TryToGetUnstuckAfterIdleDuration;
                    
                    actions.MoveTowards(target.Position, tryToGetUnstuck);
                    
                    if (tryToGetUnstuck) {
                        stuckTimestamps.RemoveExpired();
                        stuckTimestamps.Enqueue(DateTime.Now);
                    }
                }
            }

            private void OnRunStarted(object? sender, EventArgs e) {
                hasMadeFirstMove = false;
                stuckTimestamps = new ExpiringTimestampsList(IdleTimeout);
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
                    hasMadeFirstMove = false;
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
                // if (_characterHasDied)
                //     throw new CharacterDiedException(Step.Current);
                #endif
                
                ProgressMoveStepsAndSkipIfAlreadyAhead();

                if (Step?.Current is TradeMissionRecord.MoveStep) {
                    MoveCharacterTowardsMoveStep();
                }

                if (Step?.Current is TradeMissionRecord.ChangeClusterStep) {
                    actions.MoveInSameDirection();
                }
                
                if (stuckTimestamps.Count >= MaxTriesToGetUnstuckWithinTimeout)
                    Stuck?.Invoke(this, EventArgs.Empty);
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
                Console.WriteLine($@"> current step: {Step!.Current}");
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

                MoveToNextRouteStep();
                
                Thread.Sleep(AverageClusterChangeDuration);
                
                // Update his current position so as not to accidentally go through portal again
                if (Step?.Current is TradeMissionRecord.MoveStep nextMove)
                    LocalCharacter.Instance.Position = nextMove.Position;
            }
            
            private bool MoveToNextRouteStep() {
                Console.WriteLine(Step?.Current.CsvFormat);
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

        internal class ExpiringTimestampsList : Queue<DateTime>
        {
            public readonly int ExpiresInMilliSeconds;
            
            public ExpiringTimestampsList(int expiresInMilliSeconds) =>
                ExpiresInMilliSeconds = expiresInMilliSeconds;

            public void RemoveExpired() {
                var now = DateTime.Now;
                while (Count > 0 && now.Subtract(Peek()).TotalMilliseconds >= ExpiresInMilliSeconds) {
                    Dequeue();
                }
            }
        }

        internal abstract class InteractionStep : TradeMissionStep
        {
            private const int DelayBetweenNpcInterfaceActions = 2000;
            private const float MaxDistance = 3f;
            private bool attemptingInteraction;
            
            public int Delay => LocalCharacter.Instance.Interacting
                ? DelayBetweenNpcInterfaceActions
                : attemptingInteraction ? 1500 : 10;

            protected abstract Position _interactablePosition { get; }
            public bool CharacterIsNearInteractable =>
                LocalCharacter.Instance.DistanceFrom(_interactablePosition) <= MaxDistance;

            public abstract string Name { get; }
            public bool Finished { get; private set; }

            public void Tick() {
                if (!LocalCharacter.Instance.Interacting) {
                    AttemptInteraction();
                } else {
                    Thread.Sleep(DelayBetweenNpcInterfaceActions);
                    if (!LocalCharacter.Instance.Interacting)
                        return;
                    Finished = DoInteractions();

                    Thread.Sleep(500);
                    actions.CenterCursor();
                    Thread.Sleep(500);
                }
            }

            protected abstract bool DoInteractions();

            private void AttemptInteraction() {
                if (attemptingInteraction && !LocalCharacter.Instance.Interacting)
                    Thread.Sleep(1500);
                
                if (attemptingInteraction && !LocalCharacter.Instance.Interacting &&  !LocalCharacter.Instance.Moving) {
                    attemptingInteraction = false;
                    actions.MoveAwayFrom(_interactablePosition);
                    Console.WriteLine("moving away from!");
                    Thread.Sleep(1000);
                    
                } else if (CharacterIsNearInteractable) {
                    if (!attemptingInteraction) {
                        actions.StopAllActions();
                        Thread.Sleep(500);
                    }

                    attemptingInteraction = true;
                    actions.InteractWith(_interactablePosition);
                } else {
                    attemptingInteraction = false;
                    actions.MoveTowards(_interactablePosition);
                }
            }
        }
    }
}
