using System;
using System.ComponentModel;
using System.Windows.Forms;
using SystemApplicationContext=System.Windows.Forms.ApplicationContext;


namespace OryxBot.Client.Windows
{
    internal partial class ApplicationContext : SystemApplicationContext
    {
        public ApplicationContext() {
            InitializeComponents();
        }

        private void OnExitClicked(object? sender, EventArgs e) {
            Application.Exit();
        }

        private void OnPanelClicked(object? sender, EventArgs e) {
            // Todo: Open website
        }

        private void OnToggleBotClicked(object? sender, EventArgs e) {
            // Todo: Toggle bot
        }
    }
}
