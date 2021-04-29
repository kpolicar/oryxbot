using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OryxBot.Client.Windows
{
    partial class TradeMissionRunBotStatusForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TradeMissionRunBotStatusForm));
            this.formLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.botPositionValueLabel = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.botPositionLabel = new System.Windows.Forms.Label();
            this.botStatusValueLabel = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.botStatusLabel = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.formLayoutPanel.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // formLayoutPanel
            // 
            this.formLayoutPanel.ColumnCount = 2;
            this.formLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.formLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.formLayoutPanel.Controls.Add(this.botPositionValueLabel, 1, 1);
            this.formLayoutPanel.Controls.Add(this.panel5, 0, 1);
            this.formLayoutPanel.Controls.Add(this.botStatusValueLabel, 1, 0);
            this.formLayoutPanel.Controls.Add(this.panel4, 0, 0);
            this.formLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formLayoutPanel.Location = new System.Drawing.Point(0, 21);
            this.formLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.formLayoutPanel.Name = "formLayoutPanel";
            this.formLayoutPanel.RowCount = 2;
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.Size = new System.Drawing.Size(250, 54);
            this.formLayoutPanel.TabIndex = 6;
            this.formLayoutPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // botPositionValueLabel
            // 
            this.botPositionValueLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.botPositionValueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.botPositionValueLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.botPositionValueLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.botPositionValueLabel.Location = new System.Drawing.Point(100, 27);
            this.botPositionValueLabel.Margin = new System.Windows.Forms.Padding(0);
            this.botPositionValueLabel.Name = "botPositionValueLabel";
            this.botPositionValueLabel.Padding = new System.Windows.Forms.Padding(4);
            this.botPositionValueLabel.Size = new System.Drawing.Size(150, 27);
            this.botPositionValueLabel.TabIndex = 2;
            this.botPositionValueLabel.Text = "Unknown position";
            this.botPositionValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.botPositionValueLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.panel5.Controls.Add(this.botPositionLabel);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 27);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(100, 27);
            this.panel5.TabIndex = 3;
            this.panel5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // botPositionLabel
            // 
            this.botPositionLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.botPositionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.botPositionLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.botPositionLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.botPositionLabel.Location = new System.Drawing.Point(0, 0);
            this.botPositionLabel.Margin = new System.Windows.Forms.Padding(0);
            this.botPositionLabel.Name = "botPositionLabel";
            this.botPositionLabel.Padding = new System.Windows.Forms.Padding(4);
            this.botPositionLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.botPositionLabel.Size = new System.Drawing.Size(100, 27);
            this.botPositionLabel.TabIndex = 2;
            this.botPositionLabel.Text = "Position:";
            this.botPositionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.botPositionLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // botStatusValueLabel
            // 
            this.botStatusValueLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.botStatusValueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.botStatusValueLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.botStatusValueLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.botStatusValueLabel.Location = new System.Drawing.Point(100, 0);
            this.botStatusValueLabel.Margin = new System.Windows.Forms.Padding(0);
            this.botStatusValueLabel.Name = "botStatusValueLabel";
            this.botStatusValueLabel.Padding = new System.Windows.Forms.Padding(4);
            this.botStatusValueLabel.Size = new System.Drawing.Size(150, 27);
            this.botStatusValueLabel.TabIndex = 1;
            this.botStatusValueLabel.Text = "Unknown status";
            this.botStatusValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.botStatusValueLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.panel4.Controls.Add(this.botStatusLabel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(100, 27);
            this.panel4.TabIndex = 0;
            this.panel4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // botStatusLabel
            // 
            this.botStatusLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.botStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.botStatusLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.botStatusLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.botStatusLabel.Location = new System.Drawing.Point(0, 0);
            this.botStatusLabel.Margin = new System.Windows.Forms.Padding(0);
            this.botStatusLabel.Name = "botStatusLabel";
            this.botStatusLabel.Padding = new System.Windows.Forms.Padding(4);
            this.botStatusLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.botStatusLabel.Size = new System.Drawing.Size(100, 27);
            this.botStatusLabel.TabIndex = 1;
            this.botStatusLabel.Text = "Status:";
            this.botStatusLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.botStatusLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.panel6.Controls.Add(this.button1);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(0);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.panel6.Size = new System.Drawing.Size(250, 21);
            this.panel6.TabIndex = 0;
            this.panel6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.button1.BackgroundImage = ((System.Drawing.Image) (resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.button1.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (148)))), ((int) (((byte) (140)))), ((int) (((byte) (116)))));
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.button1.Location = new System.Drawing.Point(219, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(21, 21);
            this.button1.TabIndex = 3;
            this.button1.TabStop = false;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // BotStatusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.ClientSize = new System.Drawing.Size(250, 75);
            this.Controls.Add(this.formLayoutPanel);
            this.Controls.Add(this.panel6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 75);
            this.Name = "TradeMissionRunBotStatusForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "OryxBot - Status";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.formLayoutPanel.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Label botStatusLabel;
        private System.Windows.Forms.Label botStatusValueLabel;
        private System.Windows.Forms.Label botPositionLabel;
        private System.Windows.Forms.Label botPositionValueLabel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;

        private System.Windows.Forms.TableLayoutPanel formLayoutPanel;

        #endregion
    }
}

