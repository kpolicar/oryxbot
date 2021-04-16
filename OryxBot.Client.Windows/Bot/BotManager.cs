using System;
using System.Diagnostics;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Shared;
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
        public event EventHandler<BotEventArgs>? Starting;
        public event EventHandler<BotEventArgs>? Started;
        public event EventHandler<BotEventArgs>? Stopped;
        public event EventHandler<BotEventArgs>? JobChanged;
        public event EventHandler<BotEventArgs>? TradeMissionRun;
        public event EventHandler<BotEventArgs>? TradeMissionRecord;
        private ServiceContainer serviceContainer = null!;
        private TradeMissionRouteManager _routeManager = null!;
        public BotJob? Bot;

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public RecordingConfiguration RecordingConfig { get; private set; }
        public RunConfiguration RunConfig { get; private set; }
        

        public void BindDependencies(ServiceContainer serviceContainer) {
            this.serviceContainer = serviceContainer;
            _routeManager = serviceContainer.GetService<TradeMissionRouteManager>();
            _routeManager.ModeChanged += (_, _) => Bot = null;
        }

        public void Stop() =>
            Bot?.Stop();

        public void SetRecordingConfiguration(RecordingConfiguration config) =>
            RecordingConfig = config;

        public void SetRunConfiguration(RunConfiguration config) =>
            RunConfig = config;

        public void ToggleTradeMissionRun() {
            if (!(Bot is TradeMissionRun) || !Bot.Running) {
                var route = _routeManager.Route();
                if (route == null)
                    return;
                _routeManager.SetDefaultRouteCity((City)route.Origin!.Value);
                var routeBack = _routeManager.RouteBack();
                if (routeBack == null)
                    return;

                EnforceBotServiceType(
                    typeof(TradeMissionRun), 
                    () => new TradeMissionRun(route, routeBack, RunConfig.Hearts));
            }
            
            if (!Bot!.Running)
                Starting?.Invoke(this, new BotEventArgs(Bot!));
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
            
            if (!Bot!.Running)
                Starting?.Invoke(this, new BotEventArgs(Bot!));
            Bot!.ToggleStart();
        }
        
        private void EnforceBotServiceType(Type botServiceType, Func<BotJob> constructorCallback) {
            Bot?.Stop();
            Bot = constructorCallback();
            if (Bot is HasDependencies dependant)
                dependant.BindDependencies(serviceContainer);
            Bot.Started += (_, _) => (IsRunning, IsPaused) = ((Bot as Job)!.Running, (Bot as Job)!.IsPaused);
            Bot.Stopped += (_, _) => (IsRunning, IsPaused) = ((Bot as Job)!.Running, (Bot as Job)!.IsPaused);
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
