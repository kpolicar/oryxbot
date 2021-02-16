using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;

namespace OryxBot.Client.Windows
{
    public partial class UIApplicationContext
    {
        public event EventHandler Load;
        
        private Container components;

        public ToolStripMenuItem ToolStipPanelButton { private set; get; }
        public ToolStripMenuItem ToolStipToggleBotTradeMissionRecordButton { private set; get; }
        public ToolStripMenuItem ToolStipToggleBotTradeMissionRunButton { private set; get; }
        public ToolStripMenuItem ToolStripCloseButton { private set; get; }
        public OpenFileDialog TradeMissionRunRouteFile { private set; get; }
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
            // toolStipToggleBotTradeMissionRecordButton
            //
            ToolStipToggleBotTradeMissionRecordButton = new ToolStripMenuItem {
                Name = "toolStipToggleBotTradeMissionRecordButton",
                Text = Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRecordButton_TextStart,
            };
            //
            // toolStipToggleBotTradeMissionRunButton
            //
            ToolStipToggleBotTradeMissionRunButton = new ToolStripMenuItem {
                Name = "toolStipToggleBotTradeMissionRunButton",
                Text = Resources.UIApplicationContext.ToolStipToggleBotTradeMissionRunButton_TextStart,
            };
            //
            // TradeMissionRunRouteFile
            //
            TradeMissionRunRouteFile = new OpenFileDialog {
                Title = Resources.UIApplicationContext.TradeMissionRunRouteFile_Title,
                Filter = "Route Files (*.csv)|*.csv",
            };
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
                Items = {
                    ToolStipToggleBotTradeMissionRecordButton,
                    ToolStipToggleBotTradeMissionRunButton,
                    ToolStipPanelButton,
                    new ToolStripSeparator(),
                    ToolStripCloseButton
                },
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

        public void Show() {
            trayIcon.Visible = true;
            Load?.Invoke(this, EventArgs.Empty);
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            ExitThread();
            base.Dispose(disposing);
        }
    }
}
