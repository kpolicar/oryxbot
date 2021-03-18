using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Bot;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using BotManager = OryxBot.Client.Windows.Bot.BotManager;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace Inkybot.Api
{
    public class ApiNotifier : HasDependencies
    {
        private ApiClient api = null!;

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            var bot = (serviceContainer.GetService<BotManagerContract>() as BotManager)!;
            
            bot.TradeMissionRun += (_, e) =>
                (e.Job as TradeMissionRun)!.RunComplete += OnTradeMissionRunComplete;
        }

        private void OnTradeMissionRunComplete(object? sender, EventArgs e) =>
            Task.Run(api.NotifyRunComplete)
                .ConfigureAwait(false);
    }
}
