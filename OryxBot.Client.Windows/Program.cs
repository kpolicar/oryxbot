using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using OryxBot.Albion.Protocol;
using OryxBot.Client.Windows.Bot;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows
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
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            InstanceIdentifier = "instance-" + new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var app = new MainForm();
            app.BindDependencies(Services);
            app.Load += _kernel.OnLoadForm;
            
            Application.ApplicationExit += (_, _) => _kernel.Dispose();
            Application.Run(app);
        }
        
        public static string InstanceIdentifier;
    }
}
