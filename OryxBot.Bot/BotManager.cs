using System;
using System.Collections.Generic;
using OryxBot.Bot.Contracts;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Bot
{
    public class BotManager : BotManagerContract, HasDependencies
    {
        public event EventHandler<BotEventArgs>? Started;
        public event EventHandler<BotEventArgs>? Stopped;
        private ServiceContainer serviceContainer = null!;
        private TradeMissionRouteProvider routeProvider = null!;
        private BotJob? Bot;


        public void BindDependencies(ServiceContainer serviceContainer) {
            this.serviceContainer = serviceContainer;
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
        }

        public void ToggleTradeMissionRun() {
            if (!(Bot is TradeMissionRun)) {
                var route = routeProvider.Route();
                if (route == null) // Todo: Error
                    return;
                EnforceBotServiceType(typeof(TradeMissionRun), new object?[] {route});
            }
            Bot!.ToggleStart();
        }

        public void ToggleTradeMissionRecord() {
            EnforceBotServiceType(typeof(TradeMissionRecord));
            Bot!.ToggleStart();
        }
        
        private void EnforceBotServiceType(Type botServiceType, object?[]? objects=null) {
            if (Bot == null || Bot.GetType() != botServiceType) {
                Bot?.Stop();
                Bot = (BotJob) Activator.CreateInstance(botServiceType, objects)!;
                if (Bot is HasDependencies dependant)
                    dependant.BindDependencies(serviceContainer);
                Bot.Started += (_, _) => Started?.Invoke(this, new BotEventArgs(Bot));
                Bot.Stopped += (_, _) => Stopped?.Invoke(this, new BotEventArgs(Bot));
            }
        }
    }
}
