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
