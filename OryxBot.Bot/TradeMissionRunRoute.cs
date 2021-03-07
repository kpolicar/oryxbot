using System;
using System.Threading;
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
            if (!(Step?.Current is TradeMissionRecord.MoveStep target))
                return;
            
            while (CanProgressToNextStep(moveEvent.Position)) {
                if (!MoveToNextRouteStep())
                    return;
                var nextMove = Step.Current as TradeMissionRecord.MoveStep;
                if (nextMove == null)
                    continue;
                State.Action = RUNNING_ROUTE;
                target = nextMove;
            }
            actions.MoveTowards(moveEvent.Position, target.Position);
        }

        private bool CanProgressToNextStep(Position currentPosition) =>
            Step!.Current is TradeMissionRecord.MoveStep move &&
            Helpers.Math.Distance(move.Position, currentPosition) <= MaxDistance;
        
        [CallOnChangeCluster(RequiredState = RUNNING_ROUTE)]
        private void RunningRouteOnChangeCluster(ChangeClusterEventArgs e) {
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
                Thread.Sleep(ClusterAvgLoadTime);
                KeepTryingToMoveUntilValidMovement(move);
            }
        }

        private bool MoveToNextRouteStep() {
            var hasNext = Step?.MoveNext();
            
            if (hasNext == false) {
                actions.StopAllActions();
                RouteFinished?.Invoke(this, EventArgs.Empty);
                return false;
            }

            return hasNext != null;
        }
        
        private void RunTradeMissionRoute() {
            Console.WriteLine("Running trade mission route.");
            RunRoute(Route, OnRunTradeMissionRouteFinished);
        }
        
        private void RunTradeMissionRouteBack() {
            Console.WriteLine("Running trade mission route back.");
            RunRoute(RouteBack, (o, args) => Console.WriteLine("Trade route finished!"));
        }

        private void OnRunTradeMissionRouteFinished(object? arg1, EventArgs arg2) {
            Console.WriteLine("User route finished.");
            
            State.Action = PROGRESSING_QUEST;
            Thread.Sleep(DelayBetweenNpcInterfaceActions);
            
            KeepTryingToInteractUntilValidInteraction(new Position(50f,188f));
        }
    }
}
