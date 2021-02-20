using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Windows.Forms;
using OryxBot.Client.Windows.Native;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows
{
    static partial class Program
    {
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
            ui.Load += _kernel.OnLoadForm;
            
            ui.Show();
            Application.ApplicationExit += (_, _) => _kernel.Dispose();
            Application.Run(ui);
        }
    }
}
