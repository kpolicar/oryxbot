using System;
using System.Diagnostics;
using System.Windows.Forms;
using OryxBot.Client.Windows.Bot;
using OryxBot.Shared;

namespace OryxBot.Client.Windows
{
    public partial class MainForm
    {
        private void UpdateControlsForUnauthenticated() {
            TrayIcon.Visible = false;
            ToolStipToggleBotTradeMissionRecordButton.Enabled = false;
            ToolStipToggleBotTradeMissionRunButton.Enabled = false;
            ToolStripEnableCustomRoutesButton.Enabled = false;
            ToolStipUsernameLabel.Text = Resources.UIApplicationContext.ToolStipUsernameLabel_Text;
        }

        private void UpdateControlsForAuthenticated() {
            TrayIcon.Visible = true;
            ToolStipToggleBotTradeMissionRunButton.Enabled = true;
            if (auth.User != null)
                UpdateControlsForUser(auth.User);
        }

        private void UpdateControlsForUser(User user) {
            var tooltipUnauthorizedTrial = "This feature is limited to subscribed users.";
            
            ToolStipToggleBotTradeMissionRecordButton.Enabled =
                user.can_use_custom_routes || ((Bot as TradeMissionRecord)?.Running ?? false);
            ToolStipToggleBotTradeMissionRecordButton.ToolTipText =
                user.can_use_custom_routes || ((Bot as TradeMissionRecord)?.Running ?? false)
                    ? ""
                    : tooltipUnauthorizedTrial;
            ToolStripEnableCustomRoutesButton.Enabled = user.can_use_custom_routes;
            ToolStripEnableCustomRoutesButton.ToolTipText = user.can_use_custom_routes
                ? ""
                : tooltipUnauthorizedTrial;
            if (_routeManager.CustomRoutes && !user.can_use_custom_routes)
                _routeManager.ToggleCustomMode();
        }

        private void OnExitClicked(object? sender, EventArgs e) =>
            Application.Exit();
    }
}
