using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Bot.Services;
using OryxBot.Bot.Services;
using OryxBot.Client.Windows.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows
{
    static partial class Program
    {
        internal class Kernel : IDisposable
        {
            public readonly ServiceContainer Services = new();

            private readonly Dictionary<Type, object> _services = new() {
                {typeof(Input), new Win32Input()},
                {typeof(Hotkey), new Win32Hotkey()},
                {typeof(AlbionDataProvider), new NetworkAlbionDataProvider()},
            };

            public Kernel() {
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
                var win32Input = (Win32Input) Services.GetService(typeof(Input))!;
                var form = (MainForm) sender!;
                win32Input.BindToControl(form.GameWindowPanel);
            }
        }
    }
}
