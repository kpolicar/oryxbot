using System;
using System.Diagnostics;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using OryxBot.Shared.Game;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Client.Windows.Bot
{
    public class BotManager : BotManagerContract, HasDependencies
    {
        public event EventHandler<BotEventArgs>? Started;
        public event EventHandler<BotEventArgs>? Stopped;
        public event EventHandler<BotEventArgs>? JobChanged;
        public event EventHandler<BotEventArgs>? TradeMissionRun;
        public event EventHandler<BotEventArgs>? TradeMissionRecord;
        private ServiceContainer serviceContainer = null!;
        private TradeMissionRouteProvider routeProvider = null!;
        public BotJob? Bot;
        
        public bool IsRunning { get; private set; }
        public City ActiveCity { get; private set; } = City.Lymhurst;
        

        public void BindDependencies(ServiceContainer serviceContainer) {
            this.serviceContainer = serviceContainer;
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
            routeProvider.ModeChanged += (_, _) => Bot = null;
        }

        public void Stop() =>
            Bot?.Stop();

        public void SetActiveCity(City city) =>
            ActiveCity = city;

        public void ToggleTradeMissionRun() {
            if (!(Bot is TradeMissionRun)) {
                var route = routeProvider.Route();
                if (route == null) // Todo: Error
                    return;
                var routeBack = routeProvider.RouteBack();
                if (routeBack == null) // Todo: Error
                    return;

                EnforceBotServiceType(
                    typeof(TradeMissionRun), 
                    () => new TradeMissionRun(ActiveCity,route, routeBack));
            }
            Bot!.ToggleStart();
        }

        public void ToggleTradeMissionPause() {
            if (Bot is TradeMissionRun run) {
                run.Pause();
            }
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
                Bot.Started += (_, _) => IsRunning = (Bot as Job)!.Running;
                Bot.Stopped += (_, _) => IsRunning = (Bot as Job)!.Running;
                Bot.Started += (_, _) => Started?.Invoke(this, new BotEventArgs(Bot));
                Bot.Stopped += (_, _) => Stopped?.Invoke(this, new BotEventArgs(Bot));
                
                JobChanged?.Invoke(this, new BotEventArgs(Bot));
                if (Bot is TradeMissionRun)
                    TradeMissionRun?.Invoke(this, new BotEventArgs(Bot));
                else if (Bot is TradeMissionRecord)
                    TradeMissionRecord?.Invoke(this, new BotEventArgs(Bot));
            }
        }
    }
}
