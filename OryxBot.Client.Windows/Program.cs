using System;
using System.Windows.Forms;
using OryxBot.Bot;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows
{
    static partial class Program
    {
        #if DEBUG
        
        public const string Url = "http://oryxbot.test";
        public const string GrantId = "1";
        public const string GrantSecret = "***REMOVED***";
        
        #else
        
        public const string Url = "https://oryxbot.com";
        public const string GrantId = "2";
        public const string GrantSecret = "***REMOVED***";
        
        #endif
        public const string VersionNumber = "1";
        public const string Version = "v0.1";
        public const string VersionEndpoint = "v0.1";
        
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

            var ui = new UIApplicationContext();
            ui.BindDependencies(Services);
            ui.Load += _kernel.OnLoadForm;
            
            ui.Show();
            Application.ApplicationExit += (_, _) => _kernel.Dispose();
            Application.Run(ui);
        }
    }
}
