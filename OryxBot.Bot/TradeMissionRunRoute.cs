using System;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using OryxBot.Bot.Exceptions;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        [CallOnMove(RequiredState = RUNNING_ROUTE)]
        private void RunningRouteOnMove(MoveEventArgs moveEvent) {
            var finished = ProgressMoveStepsAndSkipIfAlreadyAhead(moveEvent);

            if (!(Step?.Current is TradeMissionRecord.MoveStep target))
                return;
            actions.MoveTowards(moveEvent.Position, target.Position);
        }

        private bool ProgressMoveStepsAndSkipIfAlreadyAhead(MoveEventArgs moveEvent) {
            if (!(Step?.Current is TradeMissionRecord.MoveStep))
                return false;
            
            var skipped = 0;
            for (int i = 0; i < MaxSkippableSteps; i++) {
                var hasNext = Step.MoveNext();
                
                if (!hasNext || !(Step.Current is TradeMissionRecord.MoveStep)) {
                    break;
                }
                skipped++;
            }

            for (int i = 0; i < skipped; i++) {
                var target = (Step!.Current as TradeMissionRecord.MoveStep)!;
                if (IsMoveStepAndCanProgress(moveEvent.Position)) {
                    return !MoveToNextRouteStep();
                }

                Step.MovePrevious();
            }
            
            if (IsMoveStepAndCanProgress(moveEvent.Position)) {
                return !MoveToNextRouteStep();
            }

            return false;
        }
        
        private bool IsMoveStepAndCanProgress(Position currentPosition) =>
            Step?.Current is TradeMissionRecord.MoveStep move &&
            Helpers.Math.Distance(move.Position, currentPosition) <= MaxDistance;
        
        [CallOnChangeCluster(RequiredState = RUNNING_ROUTE)]
        private async Task RunningRouteOnChangeCluster(ChangeClusterEventArgs e) {
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
            if (changeCluster.Location != e.Location)
                throw new RouteException(Step.Current);
            
            if (!MoveToNextRouteStep())
                return;

            if (Step.Current is TradeMissionRecord.MoveStep move) {
                await Task.Delay(ClusterAvgLoadTime).ConfigureAwait(false); 
                await KeepTryingToMoveUntilValidMovement(move).ConfigureAwait(false);
            }
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
            RouteFinished?.Invoke(this, EventArgs.Empty);
        }
        
        private Task RunTradeMissionRoute() {
            Console.WriteLine("Running trade mission route.");
            return RunRoute(Route, OnRunTradeMissionRouteFinished);
        }
        
        private Task RunTradeMissionRouteBack() {
            Console.WriteLine("Running trade mission route back.");
            return RunRoute(RouteBack, async (o, args) => Console.WriteLine("Trade route finished!"));
        }

        private async Task OnRunTradeMissionRouteFinished(object? arg1, EventArgs arg2) {
            Console.WriteLine("User route finished.");
            
            State.Action = PROGRESSING_QUEST;
            await Task.Delay(DelayBetweenNpcInterfaceActions).ConfigureAwait(false);
            
            await KeepTryingToInteractUntilValidInteraction(new Position(50f,188f)).ConfigureAwait(false);
        }
    }
}
