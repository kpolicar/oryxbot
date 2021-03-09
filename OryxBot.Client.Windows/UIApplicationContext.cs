using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Events;
using OryxBot.Client.Windows.Exceptions;
using OryxBot.Shared.Design;
using SystemApplicationContext=System.Windows.Forms.ApplicationContext;


namespace OryxBot.Client.Windows
{
    public partial class UIApplicationContext : SystemApplicationContext, HasDependencies
    {
        private ApiClient api = null!;

        public UIApplicationContext() {
            InitializeComponents();
            WelcomeDialogue.LoggingIn += (_, _) => UpdateControlsForUnauthenticated();
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

        public void ShowLoginDialogue(string message = "") {
            var dialogue = new WelcomeDialogue(message);
            UpdateControlsForUnauthenticated();
            
            var result = dialogue.ShowDialog();
            var loginSuccess = result == DialogResult.OK;

            if (loginSuccess)
                UpdateControlsForAuthenticated();
            else
                Task.Run(Application.Exit);
        }

        private void UpdateControlsForUnauthenticated() {
            ToolStipToggleBotTradeMissionRecordButton.Enabled = false;
            ToolStipToggleBotTradeMissionRunButton.Enabled = false;
            ToolStipUsernameLabel.Text = Resources.UIApplicationContext.ToolStipUsernameLabel_Text;
        }

        private void UpdateControlsForAuthenticated() {
            ToolStipToggleBotTradeMissionRecordButton.Enabled = true;
            ToolStipToggleBotTradeMissionRunButton.Enabled = true;
        }

        public void OnAuthChanged(object? sender, AuthChangedEvent e) {
            if (e.Succeeded)
                return;
            
            var message = e.Exception switch {
                HttpRequestException _ => Resources.UIApplicationContext.ErrorMessage_Http,
                UserNotSubscribedException _ =>
                    Resources.UIApplicationContext.ErrorMessage_NoLongerSubscribed+"\n"+
                    Resources.UIApplicationContext.ErrorMessage_PleaseExtend,
                _ => ""
            };
            ShowLoginDialogue(message);
        }
    }
}
