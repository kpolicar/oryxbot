using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Inkybot.Api;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;
using OryxBot.Client.Linux.Api;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Client.Linux.Broadcasting;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Native;
using OryxBot.Client.Linux.Services;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using PusherClient;
using BotManager = OryxBot.Shared.Contracts.BotManager;
using Logger = OryxBot.Shared.Contracts.Logger;
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
        public const string WebsocketHost = "oryxbot.test:6001";
        public const bool WebsocketEncrypted = false;
        
        #else
        
        public const string Url = "https://temp.oryxbot.com";
        public const string GrantId = "2";
        public const string GrantSecret = "***REMOVED***";
        public const string _appKey = "***REMOVED***";
        public const string PusherAppKey = "***REMOVED***";
        public const string WebsocketHost = "socket.oryxbot.com:443";
        public const bool WebsocketEncrypted = true;
        
        #endif
        
        public static byte[] AppKey => System.Convert.FromBase64String(_appKey);
        public const string VersionNumber = "7";
        public const string Version = "v1.0";
        public const string VersionEndpoint = "v1";
        
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
             
             (new DiscoverabilityService()).Init();
             WebSocketLogForwarder.Init();
            
             auth.LoginWithToken(
             File.ReadAllText("/etc/oryxbot.apikey").Replace("\n", ""));
             // auth.LoginWithToken("***REMOVED***");

             User? user = null;
             try {
                 user = api.User().Result;
                 FileLogger.Common.Info($"Successfully authenticated user {user.email} with the Oryxbot API");
             } catch (AggregateException exception) {
                 exception.InnerExceptions.ToList().ForEach(ex => {
                     if (ex is HttpRequestException httpException
                         && httpException.StatusCode == HttpStatusCode.Unauthorized) {
                         FileLogger.Common.Error($"Error occured trying to authenticate client with Oryxbot API");
                     }
                 });
             }
             if (user == null) {
                 _kernel.Dispose();
                 return;
             }
             
             _ = api.NotifyServerStatus();
            
            
             var routeManager = _kernel.Services.GetService<TradeMissionRouteManager>();
             routeManager.SetDefaultRouteCity(City.FortSterling);
            
             var botManager = _kernel.Services.GetService<BotManager>();
             botManager.SetRunConfiguration(new RunConfiguration(RunConfiguration.ContractType.Minor));
            
             var dataProvider = _kernel.Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
             dataProvider!.Run();

             Console.ReadLine();
            
             _kernel.Dispose();
        }

        public static void StopProgram() {
            var botManager = _kernel.Services.GetService<BotManager>();
            botManager.Stop();
            
            Vnc.CloseVnc();
            FileLogger.Common.Info($"Successfully stopped the VNC client");
            var api = _kernel.Services.GetService<ApiClient>();
            _ = api.NotifyServerStatus();
        }
        
        public static void RunProgram() {
            var botManager = _kernel.Services.GetService<BotManager>();
            Vnc.CloseVnc();
            var success = Vnc.StartVnc();
            if (!success) {
                FileLogger.Common.Error($"Failed to start the VNC client");
                var api = _kernel.Services.GetService<ApiClient>();
                _ = api.NotifyServerStatus();
                return;
            }
            
            ResponsivePoint.CurrentResolution = (1024, 768);
            ResponsivePoint.CurrentResolution = (
                (int) (Vnc.Dimensions!.Value.x * (Vnc.ScalingPercent!.Value / 100d)),
                (int) (Vnc.Dimensions!.Value.y * (Vnc.ScalingPercent!.Value / 100d))
            );
            FileLogger.Common.Info($"The screen resolution has been detected as: "+ResponsivePoint.CurrentResolution);
            
            var dataProvider = _kernel.Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
            dataProvider!.Stop();
            dataProvider!.Run();
            
            if (!botManager.IsRunning)
                botManager.ToggleTradeMissionRun();
        }

        public static void RunRecordingProgram() {
            var botManager = _kernel.Services.GetService<BotManager>();
            
            var dataProvider = _kernel.Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
            dataProvider!.Stop();
            dataProvider!.Run();
            
            botManager.ToggleTradeMissionRecord();
        }
        
        public static string InstanceIdentifier;
    }
}
