using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OryxBot.Client.Windows
{
    partial class MainForm
    {
        private static readonly Color darkGrayColor =
            System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
        private static readonly Color darkerGrayColor =
            System.Drawing.Color.FromArgb(((int) (((byte) (25)))), ((int) (((byte) (27)))), ((int) (((byte) (30)))));
        private static readonly Color accentColor =
            System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = new();

        private System.ComponentModel.ComponentResourceManager resources;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
        
        public ToolStripMenuItem ToolStripEnableCustomRoutesButton { private set; get; }
        public ToolStripMenuItem ToolStipToggleBotTradeMissionRecordButton { private set; get; }
        public ToolStripLabel ToolStipUsernameLabel { private set; get; }
        public ToolStripMenuItem ToolStipToggleBotTradeMissionRunButton { private set; get; }
        public ToolStripMenuItem ToolStripCloseButton { private set; get; }
        public OpenFileDialog TradeMissionRunRouteFile { private set; get; }
        public ContextMenuStrip ContextMenuStrip { private set; get; }
        public NotifyIcon TrayIcon { private set; get; }

        private void InitializeCustomComponent() {
            
            //
            // toolStipPanelButton
            //
            ToolStripEnableCustomRoutesButton = new ToolStripMenuItem {
                Name = "ToolStripEnableCustomRoutesButton",
                Text = Resources.UIApplicationContext.ToolStripEnableCustomRoutesButton_Text,
            };
            //
            // ToolStipUsernameLabel
            //
            ToolStipUsernameLabel = new ToolStripLabel {
                Name = "toolStipUsernameLabel",
                Text = Resources.UIApplicationContext.ToolStipUsernameLabel_Text,
            };
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
            ContextMenuStrip = new ContextMenuStrip() {
                Name = "contextMenuStrip",
                Items = {
                    ToolStipUsernameLabel,
                    new ToolStripSeparator(),
                    ToolStripEnableCustomRoutesButton,
                    ToolStipToggleBotTradeMissionRecordButton,
                    ToolStipToggleBotTradeMissionRunButton,
                    //ToolStipPanelButton,
                    new ToolStripSeparator(),
                    ToolStripCloseButton
                },
                ShowItemToolTips = false,
            };
            //
            // trayIcon
            //
            TrayIcon = new NotifyIcon(components) {
                Icon = Resources.UIApplicationContext.Icon,
                ContextMenuStrip = ContextMenuStrip,
                Text = Resources.UIApplicationContext.Text+"\n"+Program.Version,
            };
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.newVersionLabel = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.rememberPasswordCheckbox = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.errorMessage = new System.Windows.Forms.Label();
            this.settingsDropdownButton = new OryxBot.Client.Windows.Controls.MenuButton();
            this.settingsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = accentColor;
            this.splitContainer1.Panel1.Controls.Add(this.linkLabel2);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.logoPictureBox);
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            // 
            // linkLabel2
            // 
            this.linkLabel2.ActiveLinkColor = darkerGrayColor;
            resources.ApplyResources(this.linkLabel2, "linkLabel2");
            this.linkLabel2.LinkColor = darkGrayColor;
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.TabStop = true;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");    
            this.label2.ForeColor = darkGrayColor;
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = darkGrayColor;
            this.label1.Name = "label1";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.newVersionLabel);
            this.panel2.Controls.Add(this.settingsDropdownButton);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // newVersionLabel
            // 
            this.newVersionLabel.ActiveLinkColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.newVersionLabel, "newVersionLabel");
            this.newVersionLabel.LinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (50)))), ((int) (((byte) (50)))), ((int) (((byte) (50)))));
            this.newVersionLabel.Name = "newVersionLabel";
            this.newVersionLabel.TabStop = true;
            this.newVersionLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.newVersionLabel_LinkClicked);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.usernameTextBox);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Controls.Add(this.passwordTextBox);
            this.flowLayoutPanel1.Controls.Add(this.rememberPasswordCheckbox);
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.label5);
            this.flowLayoutPanel1.Controls.Add(this.linkLabel1);
            this.flowLayoutPanel1.Controls.Add(this.errorMessage);
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            // 
            // usernameTextBox
            // 
            resources.ApplyResources(this.usernameTextBox, "usernameTextBox");
            this.usernameTextBox.Name = "usernameTextBox";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            // 
            // passwordTextBox
            // 
            resources.ApplyResources(this.passwordTextBox, "passwordTextBox");
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // rememberPasswordCheckbox
            // 
            resources.ApplyResources(this.rememberPasswordCheckbox, "rememberPasswordCheckbox");
            this.rememberPasswordCheckbox.Name = "rememberPasswordCheckbox";
            this.rememberPasswordCheckbox.UseVisualStyleBackColor = true;
            this.rememberPasswordCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.BackColor = accentColor;
            this.button1.ForeColor = darkGrayColor;
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.LinkColor = System.Drawing.SystemColors.ControlLight;
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Click += new System.EventHandler(this.linkLabel1_LinkClicked_1);
            // 
            // errorMessage
            // 
            resources.ApplyResources(this.errorMessage, "errorMessage");
            this.errorMessage.ForeColor = System.Drawing.Color.Crimson;
            this.errorMessage.Name = "errorMessage";
            
            var resetSetttingsLabel = new ToolStripMenuItem();
            resetSetttingsLabel.Click += new System.EventHandler(resetSettings_Clicked);
            resources.ApplyResources(resetSetttingsLabel, "resetSetttingsLabel");
            
            this.settingsContextMenuStrip.Items.AddRange(new [] {
                resetSetttingsLabel
            });
            this.settingsContextMenuStrip.AutoSize = true;
            this.settingsContextMenuStrip.ShowCheckMargin = false;
            this.settingsContextMenuStrip.ShowImageMargin = false;
            this.settingsContextMenuStrip.ShowItemToolTips = false;
            // 
            // settingsDropdownButton
            // 
            this.settingsDropdownButton.ForeColor = darkGrayColor;
            resources.ApplyResources(this.settingsDropdownButton, "settingsDropdownButton");
            this.settingsDropdownButton.Name = "settingsDropdownButton";
            this.settingsDropdownButton.UseVisualStyleBackColor = false;
            this.settingsDropdownButton.Dock = DockStyle.Right;
            this.settingsDropdownButton.Size = new Size(26, 26);
            this.settingsDropdownButton.FlatStyle = FlatStyle.Flat;
            this.settingsDropdownButton.Menu = this.settingsContextMenuStrip;
            this.settingsDropdownButton.Cursor = Cursors.Hand;
            this.settingsDropdownButton.Text = "";
            
            // 
            // WelcomeDialogue
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = darkGrayColor;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private OryxBot.Client.Windows.Controls.MenuButton settingsDropdownButton;
        private System.Windows.Forms.ContextMenuStrip settingsContextMenuStrip;

        private System.Windows.Forms.Panel panel2;

        private System.Windows.Forms.CheckBox rememberPasswordCheckbox;

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

        private System.Windows.Forms.LinkLabel linkLabel2;

        private System.Windows.Forms.LinkLabel newVersionLabel;

        private System.Windows.Forms.PictureBox logoPictureBox;

        private System.Windows.Forms.Label errorMessage;

        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.TextBox usernameTextBox;

        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.LinkLabel linkLabel1;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;

        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.SplitContainer splitContainer1;

        #endregion
    }
}