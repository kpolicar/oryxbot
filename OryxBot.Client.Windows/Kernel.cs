using System;
using System.Collections.Generic;
using OryxBot.Bot;
using OryxBot.Bot.Contracts;
using OryxBot.Bot.Game;
using OryxBot.Bot.Services;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Client.Windows.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using BotManager = OryxBot.Bot.BotManager;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows
{
    static partial class Program
    {
        internal sealed class Kernel : IDisposable
        {
            public readonly ServiceContainer Services = new();

            private readonly Dictionary<Type, object> _services = new() {
                {typeof(ApiClient), new ApiClient()},
                {typeof(AuthManager), new ApiAuthManager()},
                {typeof(Input), new Win32Input()},
                {typeof(Hotkey), new Win32Hotkey()},
                {typeof(ActionFactory), new InputActionFactory()},
                {typeof(AlbionDataProvider), new NetworkAlbionDataProvider()},
                {typeof(Logger), new FileLogger()},
                {typeof(BotManagerContract), new Bot.BotManager()},
                {typeof(TradeMissionRouteProvider), new FileDialogTradeMissionRouteProvider()},
            };

            
            public Kernel() {
                BootstrapServices();
                BindServices();
            }

            private void BootstrapServices() {
                foreach (var serviceBinding in _services) {
                    var (@abstract, concrete) = (serviceBinding.Key, serviceBinding.Value);

                    Services.AddService(@abstract, concrete);
                }

                // Dependency Injection
                foreach (var serviceBinding in _services) {
                    var concrete = serviceBinding.Value;
                    if (concrete is HasDependencies service) {
                        service.BindDependencies(Services);
                    }
                }
                
                LocalCharacter.Instance.BindDependencies(Services);
            }

            public void Dispose() {
                foreach (var serviceBinding in _services) {
                    var (@abstract, _) = (serviceBinding.Key, serviceBinding.Value);

                    var concrete = Services.GetService(@abstract);
                    if (concrete is IDisposable disposable)
                        disposable.Dispose();
                }
            }

            public void OnLoadForm(object? sender, EventArgs e) {
                var app = (sender as UIApplicationContext)!;
                var bot = Services.GetService<BotManagerContract>();
                var tradeMissionRouteProvider = (FileDialogTradeMissionRouteProvider) Services.GetService<TradeMissionRouteProvider>();
                
                app.ToolStipToggleBotTradeMissionRecordButton.Click += (_, _) => bot.ToggleTradeMissionRecord();
                app.ToolStipToggleBotTradeMissionRunButton.Click += (_, _) => bot.ToggleTradeMissionRun();
                
                bot.Started += (sender, e) => {
                    if (e.Job is TradeMissionRecord)
                        app.OnBotTradeMissionRecordingStarted(sender, e);
                    else if (e.Job is TradeMissionRun)
                        app.OnBotTradeMissionRunStarted(sender, e);
                };
                bot.Stopped += (sender, e) => {
                    if (e.Job is TradeMissionRecord)
                        app.OnBotTradeMissionRecordingStopped(sender, e);
                    else if (e.Job is TradeMissionRun)
                        app.OnBotTradeMissionRunStopped(sender, e);
                };
                tradeMissionRouteProvider.BindToApp(app);
                app.ShowLoginDialogue();
            }

            private void BindServices() {
                var hotkey = Services.GetService<Hotkey>();
                var bot = Services.GetService<BotManagerContract>();
                var routeProvider = Services.GetService<TradeMissionRouteProvider>();
                var input = Services.GetService<Input>();
                var actions = Services.GetService<ActionFactory>();
                var logger = (FileLogger) Services.GetService<Logger>();
                
                hotkey.F1 += (_, _) => bot.ToggleTradeMissionRecord();
                hotkey.F2 += (_, _) => bot.ToggleTradeMissionRun();
                logger.BindToServices(Services);
                NLog.LogManager.Shutdown();
            }
        }
    }
}
