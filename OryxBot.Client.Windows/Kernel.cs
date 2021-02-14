using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using OryxBot.Bot.Services;
using OryxBot.Client.Windows.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows
{
    static partial class Program
    {
        internal sealed class Kernel : IDisposable
        {
            public readonly ServiceContainer Services = new();

            private readonly Dictionary<Type, object> _services = new() {
                {typeof(Input), new Win32Input()},
                {typeof(Hotkey), new Win32Hotkey()},
                {typeof(AlbionDataProvider), new NetworkAlbionDataProvider()},
                {typeof(BotManager), new Bot.OryxBot()},
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
                var form = (sender as UIApplicationContext)!;
                var botManager = ((BotManager) Services.GetService(typeof(BotManager)))!;
                
                form.ToolStipToggleBotButton.Click += (_, _) => botManager.ToggleRun();
                form.ThreadExit += (_, _) => botManager.Stop();
                botManager.Started += form.OnBotStarted;
                botManager.Stopped += form.OnBotStopped;
            }

            private void BindServices() {
                var hotkey = ((Hotkey) Services.GetService(typeof(Hotkey)))!;
                var botManager = ((BotManager) Services.GetService(typeof(BotManager)))!;
                
                hotkey.F1 += (_, _) => botManager.ToggleRun();
            }
        }
    }
}
