using System;
using System.Diagnostics;
using System.Windows.Forms;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;

namespace OryxBot.Client.Windows
{
    public partial class MainForm
    {
        public void OnBotTradeMissionRecordingStarted(object? sender, EventArgs e) =>
            ToolStipToggleBotTradeMissionRecordButton.Text =
                Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRecordButton_TextStop;

        public void OnBotTradeMissionRecordingStopped(object? sender, EventArgs e) =>
            ToolStipToggleBotTradeMissionRecordButton.Text =
                Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRecordButton_TextStart;
        
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
        
        private void OnRouteProviderModeChanged(object? sender, EventArgs e) {
            ToolStripEnableCustomRoutesButton.Text = routeProvider.CustomRoutes
                ? Resources.UIApplicationContext.ToolStripEnableCustomRoutesButton_Text_Custom
                : Resources.UIApplicationContext.ToolStripEnableCustomRoutesButton_Text;
        }

        public void OnBotTradeMissionRunStarting(TradeMissionRun run) {
            if (run.Route.Origin == null || run.Route.Destination == null)
                return;
            
            var origin = Regions.Name(run.Route.Origin.Value);
            var destination = Regions.Name(run.Route.Destination.Value);
            var message =
                Resources.UIApplicationContext.InfoMessage_Starting_Route
                    .Replace(":name", run.Route.Name)
                    .Replace(":origin", origin)
                    .Replace(":destination", destination);

            MessageBox.Show(
                message,
                Resources.UIApplicationContext.InfoMessage_Starting_Route_Title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
