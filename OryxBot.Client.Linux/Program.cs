using System;
using System.Linq;
using System.Threading;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Client.Linux.Native;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
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
        
        #else
        
        public const string Url = "https://oryxbot.com";
        public const string GrantId = "2";
        public const string GrantSecret = "***REMOVED***";
        public const string _appKey = "***REMOVED***";
        
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
            Anydesk.CloseAnydesk();
            Anydesk.StartAnydesk();
            
            Thread.Sleep(10000);
            
            
            
            ResponsivePoint.CurrentResolution = (1024, 768);
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            InstanceIdentifier = "instance-" + new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var dataProvider = _kernel.Services.GetService<AlbionDataProvider>() as NetworkAlbionDataProvider;
            dataProvider!.Run();

            var routeManager = _kernel.Services.GetService<TradeMissionRouteManager>();
            routeManager.SetDefaultRouteCity(City.FortSterling);
            
            var botManager = _kernel.Services.GetService<BotManager>();
            botManager.SetRunConfiguration(new RunConfiguration(RunConfiguration.ContractType.Minor));
            
            
            Console.WriteLine("Starting bot.");
            botManager.ToggleTradeMissionRun();
            
            Console.ReadLine();
            
            Console.WriteLine("Stopping bot.");
            botManager.ToggleTradeMissionRun();
            
            Thread.Sleep(2000);
            Console.WriteLine("Exiting program.");
            _kernel.Dispose();
            Console.WriteLine("Exited program successfully.");
            Console.ReadLine();
        }
        
        public static string InstanceIdentifier;
    }
}
