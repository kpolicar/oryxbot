using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;

namespace OryxBot.Client.Windows
{
    internal partial class ApplicationContext
    {
        private Container components;
        private ComponentResourceManager resources;
        
        private ToolStripMenuItem toolStipPanelButton;
        private ToolStripMenuItem toolStipToggleBotButton;
        private ToolStripMenuItem toolStripCloseButton;
        private ContextMenuStrip contextMenuStrip;
        private NotifyIcon trayIcon;

        private void InitializeComponents() {
            components = new();
            resources = new(typeof(Resources.ApplicationContext));

            //
            // toolStipPanelButton
            //
            toolStipPanelButton = new ToolStripMenuItem {
                Name = "toolStipPanelButton",
                Text = "Panel",
            };
            toolStipPanelButton.Click += OnPanelClicked;
            //
            // toolStipToggleBotButton
            //
            toolStipToggleBotButton = new ToolStripMenuItem {
                Name = "toolStipToggleBotButton",
                Text = "Start (F2)",
            };
            toolStipToggleBotButton.Click += OnToggleBotClicked;
            //
            // toolStripCloseButton
            //
            toolStripCloseButton = new ToolStripMenuItem {
                Name = "toolStripCloseButton",
                Text = "Exit",
            };
            toolStripCloseButton.Click += OnExitClicked;
            //
            // contextMenuStrip
            //
            contextMenuStrip = new ContextMenuStrip(components) {
                Name = "contextMenuStrip",
                Items = { toolStipToggleBotButton, toolStipPanelButton, new ToolStripSeparator(), toolStripCloseButton },
                ShowItemToolTips = false,
            };
            //
            // trayIcon
            //
            trayIcon = new NotifyIcon {
                Icon = (System.Drawing.Icon) resources.GetObject("Icon")!,
                ContextMenuStrip = contextMenuStrip,
                Text = resources.GetString("Text"),
                Visible = true,
            };
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
