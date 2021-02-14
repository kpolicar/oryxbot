using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Bot;
using OryxBot.Client.Windows.Native;

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
            Application.Run(ui);
            Application.ApplicationExit += (sender, e) => _kernel.Dispose();
        }
    }
}
