using System;
using System.Windows.Forms;

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
            ToolStipToggleBotTradeMissionRecordButton.Enabled = true;
            ToolStipToggleBotTradeMissionRunButton.Enabled = true;
            if (auth.User != null)
                ToolStripEnableCustomRoutesButton.Enabled = auth.User.can_use_custom_routes;
        }

        private void OnExitClicked(object? sender, EventArgs e) =>
            Application.Exit();
    }
}
