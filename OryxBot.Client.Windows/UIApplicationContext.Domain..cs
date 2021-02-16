using System;

namespace OryxBot.Client.Windows
{
    public partial class UIApplicationContext
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

    }
}
