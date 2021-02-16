using System;
using System.ComponentModel;
using System.Windows.Forms;
using SystemApplicationContext=System.Windows.Forms.ApplicationContext;


namespace OryxBot.Client.Windows
{
    public partial class UIApplicationContext : SystemApplicationContext
    {
        public UIApplicationContext() =>
            InitializeComponents();

        private void OnExitClicked(object? sender, EventArgs e) {
            Application.Exit();
        }

        private void OnPanelClicked(object? sender, EventArgs e) {
            // Todo: Open website
        }
    }
}
