using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OryxBot.Client.Linux.Api;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Bot.Events;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Native;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using BotManager = OryxBot.Client.Linux.Bot.BotManager;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace Inkybot.Api
{
    public class ApiNotifier : HasDependencies
    {
        private ApiClient api = null!;
        private AuthManager auth = null!;
        private BotManager bot;
        private Task? notifyLocationChanged;

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            auth = serviceContainer.GetService<AuthManager>();
            bot = (serviceContainer.GetService<BotManagerContract>() as BotManager)!;

            bot.TradeMissionRun += (_, e) => {
                var run = (e.Job as TradeMissionRun)!;
                run.RunComplete += OnTradeMissionRunComplete;
                run.Stuck += OnTradeMissionStuck;
                run.StepChanged += OnTradeMissionStepChanged;
            };
            bot.Starting += OnBotStarting;
            bot.Started += OnBotRunningChanged;
            bot.Stopped += OnBotRunningChanged;
            LocalCharacter.Instance.Move += OnCharacterLocationChanged;
            Anydesk.ConnectionEstablished += OnRemoteDesktopConnectionEstablished;
        }

        public void OnBotRunningChanged(object? sender, BotEventArgs e) {
            Task.Run(api.NotifyBotRunningChanged).ConfigureAwait(false);
        }

        private void OnBotStarting(object? sender, BotEventArgs e) {
            if (!(e.Job is TradeMissionRun run) || run.Route.Origin == null || run.Route.Destination == null)
                return;
            
            var origin = Regions.Name(run.Route.Origin.Value);
            var destination = Regions.Name(run.Route.Destination.Value);
            
            var message = 
                "You have selected to run a trade mission route from :origin to :destination."
                    .Replace(":origin", origin)
                    .Replace(":destination", destination);
            
            Task.Run(() => api.NotifyRunStarting(run.Route.Name, message))
                .ConfigureAwait(false);
        }

        private void OnTradeMissionRunComplete(object? sender, EventArgs e) {
            Task.Run(api.NotifyRunComplete)
                .ConfigureAwait(false);
        }
        
        private void OnTradeMissionStuck(object? sender, TradeMissionEvent e) {
            Task.Run(api.NotifyTradeMissionStuck)
                .ConfigureAwait(false);
        }
        
        public void OnRemoteDesktopConnectionEstablished(object? sender, EventArgs e) {
            Task.Run(api.NotifyRemoteDesktopConnectionEstablished)
                .ConfigureAwait(false);
        }
        
        public void OnCharacterLocationChanged(object? sender, EventArgs eventArgs) {
            if (/*bot.IsRunning && */(notifyLocationChanged?.IsCompleted ?? true)) {
                notifyLocationChanged = Task.Run(async () => {
                    await Task.Delay(1000);
                    await api.NotifyCharacterMoved();
                });
                notifyLocationChanged.ConfigureAwait(false);
            }
        }
        
        private void OnTradeMissionStepChanged(object? sender, BotEventArgs e) {
            Task.Run(api.NotifyStepChanged)
                .ConfigureAwait(false);
        }
    }
}
