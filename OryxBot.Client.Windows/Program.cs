using System;
using System.Windows.Forms;
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
        
        #else
        
        public const string Url = "https://oryxbot.com";
        public const string GrantId = "2";
        public const string GrantSecret = "***REMOVED***";
        
        #endif
        public const string VersionNumber = "2";
        public const string Version = "v0.2 Beta";
        public const string VersionEndpoint = "v0.2";
        
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

            var app = new MainForm();
            app.BindDependencies(Services);
            app.Load += _kernel.OnLoadForm;
            
            Application.ApplicationExit += (_, _) => _kernel.Dispose();
            Application.Run(app);
        }
    }
}
