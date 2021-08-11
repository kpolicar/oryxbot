using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Inkybot.Api;
using OryxBot.Client.Linux.Api;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Services;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Events;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using BotManager = OryxBot.Client.Linux.Bot.BotManager;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Linux
{
    static partial class Program
    {
        internal sealed class Kernel : IDisposable
        {
            public readonly ServiceContainer Services = new();

            private readonly Dictionary<Type, object> _services = new() {
                {typeof(ApiNotifier), new ApiNotifier()},
                {typeof(ApiClient), new ApiClient()},
                {typeof(AuthManager), new ApiAuthManager()},
                {typeof(Input), new LinuxInput()},
                {typeof(ActionFactory), new InputActionFactory()},
                {typeof(AlbionDataProvider), new NetworkAlbionDataProvider()},
                {typeof(Logger), new FileLogger()},
                {typeof(BotManagerContract), new BotManager()},
                {typeof(TradeMissionRouteManager), new DefaultTradeMissionRouteProvider()},
            };

            
            public Kernel() {
                BootstrapServices();
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
                Services.Dispose();
            }
        }
    }
}
