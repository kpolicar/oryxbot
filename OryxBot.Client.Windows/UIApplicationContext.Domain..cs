using System;

namespace OryxBot.Client.Windows
{
    internal partial class UIApplicationContext
    {
        public void OnBotStarted(object? sender, EventArgs e) =>
            ToolStipToggleBotButton.Text = Resources.UIApplicationContext.ToolStipToggleBotButton_TextStop;

        public void OnBotStopped(object? sender, EventArgs e) =>
            ToolStipToggleBotButton.Text = Resources.UIApplicationContext.ToolStipToggleBotButton_TextStart;
    }
}
