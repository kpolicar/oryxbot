using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using BotManager = OryxBot.Client.Windows.Bot.BotManager;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace Inkybot.Api
{
    public class ApiNotifier : HasDependencies
    {
        private ApiClient api = null!;
        private AuthManager auth = null!;

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            auth = serviceContainer.GetService<AuthManager>();
            var bot = (serviceContainer.GetService<BotManagerContract>() as BotManager)!;
            
            bot.TradeMissionRun += (_, e) =>
                (e.Job as TradeMissionRun)!.RunComplete += OnTradeMissionRunComplete;
            bot.Starting += OnBotStarting;
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
    }
}
