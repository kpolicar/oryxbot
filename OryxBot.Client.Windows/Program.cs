using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Bot;

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

            // form.Load += _kernel.OnLoadForm;
            Application.Run(new ApplicationContext());
            Application.ApplicationExit += (sender, e) => _kernel.Dispose();
        }
    }
}
