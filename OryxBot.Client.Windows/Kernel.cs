using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Client.Windows.Bot.Services;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Client.Windows.Events;
using OryxBot.Client.Windows.Native;
using OryxBot.Client.Windows.Services;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using BotManager = OryxBot.Client.Windows.Bot.BotManager;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows
{
    static partial class Program
    {
        internal sealed class Kernel : IDisposable
        {
            private MainForm app = null!;
            public readonly ServiceContainer Services = new();

            private readonly Dictionary<Type, object> _services = new() {
                {typeof(ApiNotifier), new ApiNotifier()},
                {typeof(ApiClient), new ApiClient()},
                {typeof(AuthManager), new ApiAuthManager()},
                {typeof(Input), new Win32Input()},
                {typeof(Hotkey), new Win32Hotkey()},
                {typeof(ActionFactory), new InputActionFactory()},
                {typeof(AlbionDataProvider), new NetworkAlbionDataProvider()},
                {typeof(Logger), new FileLogger()},
                {typeof(BotManagerContract), new Bot.BotManager()},
                {typeof(TradeMissionRouteManager), new FileDialogTradeMissionRouteManager()},
            };

            
            public Kernel() {
                BootstrapServices();
                BindServices();
                Task.Run(() => { 
                    ResponsivePoint.CurrentResolution = (
                        User32.GetSystemMetrics(User32.SM_CXSCREEN), 
                        User32.GetSystemMetrics(User32.SM_CYSCREEN));
                    var networkDataProvider = Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
                    networkDataProvider?.Run();
                });
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
                app = (sender as MainForm)!;
                var bot = Services.GetService<BotManagerContract>();
                var tradeMissionRouteManager = (FileDialogTradeMissionRouteManager) Services.GetService<TradeMissionRouteManager>();
                
                app.ToolStipToggleBotTradeMissionRecordButton.Click += (_, _) =>
                    AuthorizedToggleTradeMissionRecord();
                app.ToolStipToggleBotTradeMissionRunButton.Click += (_, _) =>
                    AuthorizedToggleTradeMissionRun();
                app.ToolStripEnableCustomRoutesButton.Click += (_, _) =>
                    AuthorizedToggleCustomTradeMissionMode();
                
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
                
                tradeMissionRouteManager.BindToApp(app);
                
                var api = Services.GetService<AuthManager>();
                api.AuthChanged += app.OnAuthChanged;
                api.AuthChanged += AuthChanged;
                
                var hotkey = Services.GetService<Hotkey>();
                hotkey.Insert += (_, _) => AuthorizedShowContextMenuStrip(app);
                hotkey.F3 += (_, _) => AuthorizedShowContextMenuStrip(app);
            }

            private void AuthorizedShowContextMenuStrip(MainForm app) {
                var auth = Services.GetService<AuthManager>();
                
                if (auth.User?.is_subscribed ?? false)
                    if (!app.ContextMenuStrip.Visible)
                        app.ContextMenuStrip.Show(new Point(ResponsivePoint.CurrentScreenWidth-100-app.ContextMenuStrip.Width, ResponsivePoint.CurrentScreenHeight-50));
                    else
                        app.ContextMenuStrip.Hide();
            }

            private void AuthChanged(object? sender, AuthChangedEvent e) {
                if (e.Succeeded)
                    return;
                var bot = Services.GetService<BotManagerContract>();
                bot.Stop();
            }

            private void BindServices() {
                var hotkey = Services.GetService<Hotkey>();
                var logger = (FileLogger) Services.GetService<Logger>();

                hotkey.F1 += (_, _) => AuthorizedToggleTradeMissionRecord();
                hotkey.F2 += (_, _) => AuthorizedToggleTradeMissionRun();
                hotkey.Space += (_, _) => AuthorizedPauseTradeMissionRun();
                logger.BindToServices(Services);
                NLog.LogManager.Shutdown();
            }

            private void AuthorizedPauseTradeMissionRun() {
                var bot = Services.GetService<BotManagerContract>();
                var auth = Services.GetService<AuthManager>();
                
                if (auth.User?.is_subscribed ?? false)
                    bot.ToggleTradeMissionPause();
            }

            private void AuthorizedToggleTradeMissionRecord() {
                var bot = Services.GetService<BotManagerContract>();
                var auth = Services.GetService<AuthManager>();
                
                if (!bot.IsRunning) {
                    var configuration = app.ShowConfigureRecordingForm();
                    if (!configuration.HasValue)
                        return;
                    
                    bot.SetRecordingConfiguration(new RecordingConfiguration(
                        configuration.Value.origin,
                        configuration.Value.destination,
                        configuration.Value.name
                        ));
                }
                
                if (auth.User?.is_subscribed ?? false)
                    bot.ToggleTradeMissionRecord();
                
            }
            private void AuthorizedToggleTradeMissionRun() {
                var bot = Services.GetService<BotManagerContract>();
                var auth = Services.GetService<AuthManager>();
                var routeProvider = Services.GetService<TradeMissionRouteManager>();

                if (!(auth.User?.is_subscribed ?? false))
                    return;

                if (!bot.IsRunning && !routeProvider.CustomRoutes && !bot.IsPaused) {
                    var selectedCity = app.ShowSelectCityForm();
                    if (!selectedCity.HasValue)
                        return;
                    routeProvider.SetDefaultRouteCity(selectedCity.Value);
                }
                
                bot.ToggleTradeMissionRun();
            }
            private void AuthorizedToggleCustomTradeMissionMode() {
                var routeProvider = Services.GetService<TradeMissionRouteManager>();
                var auth = Services.GetService<AuthManager>();
                
                if (auth.User?.is_subscribed ?? false)
                    routeProvider.ToggleCustomMode();
            }
        }
    }
}
