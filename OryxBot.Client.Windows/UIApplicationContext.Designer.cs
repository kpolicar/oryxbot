using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;

namespace OryxBot.Client.Windows
{
    internal partial class UIApplicationContext
    {
        public event EventHandler Load;
        
        private Container components;

        public ToolStripMenuItem ToolStipPanelButton { private set; get; }
        public ToolStripMenuItem ToolStipToggleBotButton { private set; get; }
        public ToolStripMenuItem ToolStripCloseButton { private set; get; }
        private ContextMenuStrip contextMenuStrip;
        private NotifyIcon trayIcon;

        private void InitializeComponents() {
            components = new();

            //
            // toolStipPanelButton
            //
            ToolStipPanelButton = new ToolStripMenuItem {
                Name = "toolStipPanelButton",
                Text = Resources.UIApplicationContext.ToolStipPanelButton_Text,
            };
            ToolStipPanelButton.Click += OnPanelClicked;
            //
            // toolStipToggleBotButton
            //
            ToolStipToggleBotButton = new ToolStripMenuItem {
                Name = "toolStipToggleBotButton",
                Text = Resources.UIApplicationContext.ToolStipToggleBotButton_TextStart,
            };
            ToolStipToggleBotButton.Click += OnToggleBotClicked;
            //
            // toolStripCloseButton
            //
            ToolStripCloseButton = new ToolStripMenuItem {
                Name = "toolStripCloseButton",
                Text = Resources.UIApplicationContext.ToolStripCloseButton_Text,
            };
            ToolStripCloseButton.Click += OnExitClicked;
            //
            // contextMenuStrip
            //
            contextMenuStrip = new ContextMenuStrip(components) {
                Name = "contextMenuStrip",
                Items = { ToolStipToggleBotButton, ToolStipPanelButton, new ToolStripSeparator(), ToolStripCloseButton },
                ShowItemToolTips = false,
            };
            //
            // trayIcon
            //
            trayIcon = new NotifyIcon {
                Icon = Resources.UIApplicationContext.Icon,
                ContextMenuStrip = contextMenuStrip,
                Text = Resources.UIApplicationContext.Text,
            };
        }

        private void LoadUI() {
            trayIcon.Visible = true;
            Load?.Invoke(this, EventArgs.Empty);
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
