using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using OryxBot.Bot.Exceptions;
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

            while (Step.Current is TradeMissionRecord.MoveStep move &&
                   Helpers.Math.Distance(move.Position, moveEvent.Position) <= MaxDistance) {
                
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
            RunRoute(Route);
        }
    }
}
