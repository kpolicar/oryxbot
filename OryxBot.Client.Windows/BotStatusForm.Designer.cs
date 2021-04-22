using System;
using System.ComponentModel;

namespace OryxBot.Client.Windows
{
    partial class BotStatusForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BotStatusForm));
            this.formLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.destinationValueLabel = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.destinationLabel = new System.Windows.Forms.Label();
            this.originValueLabel = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.originLabel = new System.Windows.Forms.Label();
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
            this.formLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.Controls.Add(this.destinationValueLabel, 1, 1);
            this.formLayoutPanel.Controls.Add(this.panel5, 0, 1);
            this.formLayoutPanel.Controls.Add(this.originValueLabel, 1, 0);
            this.formLayoutPanel.Controls.Add(this.panel4, 0, 0);
            this.formLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formLayoutPanel.Location = new System.Drawing.Point(0, 21);
            this.formLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.formLayoutPanel.Name = "formLayoutPanel";
            this.formLayoutPanel.RowCount = 2;
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.Size = new System.Drawing.Size(284, 53);
            this.formLayoutPanel.TabIndex = 6;
            this.formLayoutPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // destinationValueLabel
            // 
            this.destinationValueLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.destinationValueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.destinationValueLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.destinationValueLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.destinationValueLabel.Location = new System.Drawing.Point(142, 26);
            this.destinationValueLabel.Margin = new System.Windows.Forms.Padding(0);
            this.destinationValueLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.destinationValueLabel.Name = "destinationValueLabel";
            this.destinationValueLabel.Padding = new System.Windows.Forms.Padding(4);
            this.destinationValueLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.destinationValueLabel.Size = new System.Drawing.Size(142, 27);
            this.destinationValueLabel.TabIndex = 2;
            this.destinationValueLabel.Text = "Unknown position";
            this.destinationValueLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.panel5.Controls.Add(this.destinationLabel);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 26);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(142, 27);
            this.panel5.TabIndex = 3;
            this.panel5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // destinationLabel
            // 
            this.destinationLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.destinationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.destinationLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.destinationLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.destinationLabel.Location = new System.Drawing.Point(0, 0);
            this.destinationLabel.Margin = new System.Windows.Forms.Padding(0);
            this.destinationLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.destinationLabel.Name = "destinationLabel";
            this.destinationLabel.Padding = new System.Windows.Forms.Padding(4);
            this.destinationLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.destinationLabel.Size = new System.Drawing.Size(142, 27);
            this.destinationLabel.TabIndex = 2;
            this.destinationLabel.Text = "Position";
            this.destinationLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // originValueLabel
            // 
            this.originValueLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.originValueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.originValueLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.originValueLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.originValueLabel.Location = new System.Drawing.Point(142, 0);
            this.originValueLabel.Margin = new System.Windows.Forms.Padding(0);
            this.originValueLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.originValueLabel.Name = "originValueLabel";
            this.originValueLabel.Padding = new System.Windows.Forms.Padding(4);
            this.originValueLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.originValueLabel.Size = new System.Drawing.Size(142, 26);
            this.originValueLabel.TabIndex = 1;
            this.originValueLabel.Text = "Unknown status";
            this.originValueLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.panel4.Controls.Add(this.originLabel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(142, 26);
            this.panel4.TabIndex = 0;
            this.panel4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // originLabel
            // 
            this.originLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.originLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.originLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.originLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.originLabel.Location = new System.Drawing.Point(0, 0);
            this.originLabel.Margin = new System.Windows.Forms.Padding(0);
            this.originLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.originLabel.Name = "originLabel";
            this.originLabel.Padding = new System.Windows.Forms.Padding(4);
            this.originLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.originLabel.Size = new System.Drawing.Size(142, 26);
            this.originLabel.TabIndex = 1;
            this.originLabel.Text = "Status";
            this.originLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
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
            this.panel6.Size = new System.Drawing.Size(284, 21);
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
            this.button1.Location = new System.Drawing.Point(253, 0);
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
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.ClientSize = new System.Drawing.Size(284, 74);
            this.Controls.Add(this.formLayoutPanel);
            this.Controls.Add(this.panel6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BotStatusForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Oryxbot - Status";
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

        private System.Windows.Forms.Label originLabel;
        private System.Windows.Forms.Label originValueLabel;
        private System.Windows.Forms.Label destinationLabel;
        private System.Windows.Forms.Label destinationValueLabel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;

        private System.Windows.Forms.TableLayoutPanel formLayoutPanel;

        #endregion
    }
}

