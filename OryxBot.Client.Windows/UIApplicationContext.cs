using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot;
using OryxBot.Client.Windows.Api;
using OryxBot.Shared.Design;
using SystemApplicationContext=System.Windows.Forms.ApplicationContext;


namespace OryxBot.Client.Windows
{
    public partial class UIApplicationContext : SystemApplicationContext, HasDependencies
    {
        private ApiClient api = null!;

        public UIApplicationContext() {
            InitializeComponents();
            WelcomeDialogue.LoggingIn += (_, _) => DisableRouteButtons();
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            api.UserFetched += OnFetchedUser;
        }

        private void OnExitClicked(object? sender, EventArgs e) {
            Application.Exit();
        }

        private void OnPanelClicked(object? sender, EventArgs e) {
            // Todo: Open website
        }

        public void ShowLoginDialogue() {
            var dialogue = new WelcomeDialogue();
            DisableRouteButtons();
            
            var result = dialogue.ShowDialog();
            var loginSuccess = result == DialogResult.OK;

            if (loginSuccess)
                EnableRouteButtons();
            else
                Task.Run(Application.Exit);
        }

        private void DisableRouteButtons() {
            ToolStipToggleBotTradeMissionRecordButton.Enabled = false;
            ToolStipToggleBotTradeMissionRunButton.Enabled = false;
        }

        private void EnableRouteButtons() {
            ToolStipToggleBotTradeMissionRecordButton.Enabled = true;
            ToolStipToggleBotTradeMissionRunButton.Enabled = true;
        }
    }
}
