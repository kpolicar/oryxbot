using System;
using System.Diagnostics;
using System.Windows.Forms;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Shared;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;

namespace OryxBot.Client.Windows
{
    public partial class MainForm
    {
        public void OnBotTradeMissionRecordingStarted(object? sender, EventArgs e) {
            ToolStipToggleBotTradeMissionRecordButton.Text =
                Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRecordButton_TextStop;
            BotStatusForm.Show();
        }

        public void OnBotTradeMissionRecordingStopped(object? sender, EventArgs e) {
            ToolStipToggleBotTradeMissionRecordButton.Text =
                Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRecordButton_TextStart;
            BotStatusForm.Hide();
            if (auth.User != null)
                UpdateControlsForUser(auth.User);
        }
        
        public void OnBotTradeMissionRunStarted(object? sender, EventArgs e) =>
            ToolStipToggleBotTradeMissionRunButton.Text =
                Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRunButton_TextStop;

        public void OnBotTradeMissionRunStopped(object? sender, EventArgs e) =>
            ToolStipToggleBotTradeMissionRunButton.Text =
                Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRunButton_TextStart;

        private void OnFetchedUser(object? sender, FetchedUserEventArgs e) {
            ToolStipUsernameLabel.Text = Resources.UIApplicationContext.ToolStipUsernameLabel_TextLoggedInAs
                .Replace(":name", e.user.name);
        }
        
        private void OnRouteManagerModeChanged(object? sender, EventArgs e) {
            ToolStripEnableCustomRoutesButton.Text = _routeManager.CustomRoutes
                ? Resources.UIApplicationContext.ToolStripEnableCustomRoutesButton_Text_Custom
                : Resources.UIApplicationContext.ToolStripEnableCustomRoutesButton_Text;
        }

        private void OnUserFetched(object? sender, FetchedUserEventArgs e) {
            UpdateControlsForUser(e.user);
        }
    }
}
