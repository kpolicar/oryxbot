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
        private ComponentResourceManager resources;

        public ToolStripMenuItem ToolStipPanelButton { private set; get; }
        public ToolStripMenuItem ToolStipToggleBotButton { private set; get; }
        public ToolStripMenuItem ToolStripCloseButton { private set; get; }
        private ContextMenuStrip contextMenuStrip;
        private NotifyIcon trayIcon;

        private void InitializeComponents() {
            components = new();
            resources = new(typeof(Resources.UIApplicationContext));

            //
            // toolStipPanelButton
            //
            ToolStipPanelButton = new ToolStripMenuItem {
                Name = "toolStipPanelButton",
                Text = "Panel",
            };
            ToolStipPanelButton.Click += OnPanelClicked;
            //
            // toolStipToggleBotButton
            //
            ToolStipToggleBotButton = new ToolStripMenuItem {
                Name = "toolStipToggleBotButton",
                Text = "Start (F2)",
            };
            ToolStipToggleBotButton.Click += OnToggleBotClicked;
            //
            // toolStripCloseButton
            //
            ToolStripCloseButton = new ToolStripMenuItem {
                Name = "toolStripCloseButton",
                Text = "Exit",
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
                Icon = (System.Drawing.Icon) resources.GetObject("Icon")!,
                ContextMenuStrip = contextMenuStrip,
                Text = resources.GetString("Text"),
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
