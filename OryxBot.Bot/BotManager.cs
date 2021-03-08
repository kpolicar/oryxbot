using System;
using System.Diagnostics;
using OryxBot.Bot.Contracts;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Bot
{
    public class BotManager : BotManagerContract, HasDependencies
    {
        public event EventHandler<BotEventArgs>? Started;
        public event EventHandler<BotEventArgs>? Stopped;
        public event EventHandler<BotEventArgs>? JobChanged;
        private ServiceContainer serviceContainer = null!;
        private TradeMissionRouteProvider routeProvider = null!;
        public BotJob? Bot;


        public void BindDependencies(ServiceContainer serviceContainer) {
            this.serviceContainer = serviceContainer;
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
        }

        public void ToggleTradeMissionRun() {
            if (!(Bot is TradeMissionRun)) {
                var route = routeProvider.Route();
                if (route == null) // Todo: Error
                    return;
                var routeBack = routeProvider.Route();
                if (routeBack == null) // Todo: Error
                    return;

                EnforceBotServiceType(
                    typeof(TradeMissionRun), 
                    () => new TradeMissionRun(route, routeBack));
            }
            Bot!.ToggleStart();
        }

        public void ToggleTradeMissionRecord() {
            EnforceBotServiceType(
                typeof(TradeMissionRecord), 
                () => new TradeMissionRecord());
            Bot!.ToggleStart();
        }
        
        private void EnforceBotServiceType(Type botServiceType, Func<BotJob> constructorCallback) {
            if (Bot == null || Bot.GetType() != botServiceType) {
                Bot?.Stop();
                Bot = constructorCallback();
                if (Bot is HasDependencies dependant)
                    dependant.BindDependencies(serviceContainer);
                Bot.Started += (_, _) => Started?.Invoke(this, new BotEventArgs(Bot));
                Bot.Stopped += (_, _) => Stopped?.Invoke(this, new BotEventArgs(Bot));
                
                JobChanged?.Invoke(this, new BotEventArgs(Bot));
            }
        }
    }
}
