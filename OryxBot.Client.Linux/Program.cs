using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using Inkybot.Api;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Api;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Client.Linux.Broadcasting;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Native;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using PusherClient;
using BotManager = OryxBot.Shared.Contracts.BotManager;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Linux
{
    static partial class Program
    {
        #if DEBUG
        
        public const string Url = "http://oryxbot.test";
        public const string GrantId = "1";
        public const string GrantSecret = "***REMOVED***";
        public const string _appKey = "***REMOVED***";
        public const string PusherAppKey = "***REMOVED***";
        public const string WebsocketHost = "127.0.0.1:6001";
        public const bool WebsocketEncrypted = false;
        
        #else
        
        public const string Url = "https://oryxbot.com";
        public const string GrantId = "2";
        public const string GrantSecret = "***REMOVED***";
        public const string _appKey = "***REMOVED***";
        public const string PusherAppKey = "7b2zEWNNKjzRS6QSQsDzL9gz";
        public const string WebsocketHost = "socket.oryxbot.com:443";
        public const bool WebsocketEncrypted = true;
        
        #endif
        
        public static byte[] AppKey => System.Convert.FromBase64String(_appKey);
        public const string VersionNumber = "6";
        public const string Version = "v0.6 Beta";
        public const string VersionEndpoint = "v0.6beta";
        
        public static readonly Kernel _kernel = new();
        public static ServiceContainer Services => _kernel.Services;
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        static void Main() {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            InstanceIdentifier = "instance-" + new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            
            var auth = _kernel.Services.GetService<AuthManager>();
            var api = _kernel.Services.GetService<ApiClient>();
            var notifier = _kernel.Services.GetService<ApiNotifier>();
            
            Console.WriteLine("connecting to server...");
            
            _ = auth.Login("naltamer14@gmail.com", "***REMOVED***").Result;
            //_ = auth.Login("admin@oryxbot.com", "***REMOVED***").Result;

            _ = api.User().Result;
            _ = api.NotifyServerStatus();
            
            
            var routeManager = _kernel.Services.GetService<TradeMissionRouteManager>();
            routeManager.SetDefaultRouteCity(City.FortSterling);
            
            var botManager = _kernel.Services.GetService<BotManager>();
            botManager.SetRunConfiguration(new RunConfiguration(RunConfiguration.ContractType.Minor));
            

            Console.ReadLine();
            
            _kernel.Dispose();
        }

        public static void StopProgram() {
            var botManager = _kernel.Services.GetService<BotManager>();
            botManager.Stop();
            
            var dataProvider = _kernel.Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
            dataProvider!.Stop();
            
            Anydesk.CloseAnydesk();
        }
        
        public static void RunProgram() {
            var botManager = _kernel.Services.GetService<BotManager>();
            Anydesk.CloseAnydesk();
            Anydesk.StartAnydesk();
            
            ResponsivePoint.CurrentResolution = (1024, 768);
            ResponsivePoint.CurrentResolution = (
                (int) (Anydesk.Dimensions!.Value.x * (Anydesk.ScalingPercent!.Value / 100d)),
                (int) (Anydesk.Dimensions!.Value.y * (Anydesk.ScalingPercent!.Value / 100d))
            );
            Console.WriteLine("Resolution: "+ResponsivePoint.CurrentResolution);
            
            var dataProvider = _kernel.Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
            dataProvider!.Run();
            
            if (!botManager.IsRunning)
                botManager.ToggleTradeMissionRun();
        }
        
        public static string InstanceIdentifier;
    }
}
