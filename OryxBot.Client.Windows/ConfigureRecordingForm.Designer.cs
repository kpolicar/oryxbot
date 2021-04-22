using System;
using System.ComponentModel;

namespace OryxBot.Client.Windows
{
    partial class ConfigureRecordingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureRecordingForm));
            this.originComboBox = new System.Windows.Forms.ComboBox();
            this.actionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.confirmButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.formLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.nameLabel = new System.Windows.Forms.Label();
            this.destinationComboBox = new System.Windows.Forms.ComboBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.destinationLabel = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.originLabel = new System.Windows.Forms.Label();
            this.actionsPanel.SuspendLayout();
            this.formLayoutPanel.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // originComboBox
            // 
            this.originComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.originComboBox.FormattingEnabled = true;
            this.originComboBox.Location = new System.Drawing.Point(144, 2);
            this.originComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.originComboBox.Name = "originComboBox";
            this.originComboBox.Size = new System.Drawing.Size(134, 21);
            this.originComboBox.TabIndex = 2;
            // 
            // actionsPanel
            // 
            this.actionsPanel.AutoSize = true;
            this.actionsPanel.Controls.Add(this.confirmButton);
            this.actionsPanel.Controls.Add(this.cancelButton);
            this.actionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.actionsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.actionsPanel.Location = new System.Drawing.Point(0, 105);
            this.actionsPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.actionsPanel.Name = "actionsPanel";
            this.actionsPanel.Size = new System.Drawing.Size(284, 24);
            this.actionsPanel.TabIndex = 1;
            // 
            // confirmButton
            // 
            this.confirmButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.confirmButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.confirmButton.Location = new System.Drawing.Point(182, 2);
            this.confirmButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(100, 20);
            this.confirmButton.TabIndex = 5;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = false;
            this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (224)))), ((int) (((byte) (224)))), ((int) (((byte) (224)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Location = new System.Drawing.Point(126, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(52, 20);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // formLayoutPanel
            // 
            this.formLayoutPanel.ColumnCount = 2;
            this.formLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.Controls.Add(this.nameTextBox, 1, 2);
            this.formLayoutPanel.Controls.Add(this.panel6, 0, 2);
            this.formLayoutPanel.Controls.Add(this.destinationComboBox, 1, 1);
            this.formLayoutPanel.Controls.Add(this.panel5, 0, 1);
            this.formLayoutPanel.Controls.Add(this.originComboBox, 1, 0);
            this.formLayoutPanel.Controls.Add(this.panel4, 0, 0);
            this.formLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.formLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.formLayoutPanel.Name = "formLayoutPanel";
            this.formLayoutPanel.RowCount = 3;
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.formLayoutPanel.Size = new System.Drawing.Size(284, 105);
            this.formLayoutPanel.TabIndex = 6;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(144, 72);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(134, 20);
            this.nameTextBox.TabIndex = 6;
            this.nameTextBox.Text = "Custom Route";
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.panel6.Controls.Add(this.nameLabel);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 70);
            this.panel6.Margin = new System.Windows.Forms.Padding(0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(142, 35);
            this.panel6.TabIndex = 5;
            // 
            // nameLabel
            // 
            this.nameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nameLabel.Location = new System.Drawing.Point(0, 0);
            this.nameLabel.Margin = new System.Windows.Forms.Padding(0);
            this.nameLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nameLabel.Size = new System.Drawing.Size(142, 35);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "How would you like to name your route?";
            // 
            // destinationComboBox
            // 
            this.destinationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.destinationComboBox.FormattingEnabled = true;
            this.destinationComboBox.Location = new System.Drawing.Point(144, 37);
            this.destinationComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.destinationComboBox.Name = "destinationComboBox";
            this.destinationComboBox.Size = new System.Drawing.Size(134, 21);
            this.destinationComboBox.TabIndex = 4;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.panel5.Controls.Add(this.destinationLabel);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 35);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(142, 35);
            this.panel5.TabIndex = 3;
            // 
            // destinationLabel
            // 
            this.destinationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.destinationLabel.Location = new System.Drawing.Point(0, 0);
            this.destinationLabel.Margin = new System.Windows.Forms.Padding(0);
            this.destinationLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.destinationLabel.Name = "destinationLabel";
            this.destinationLabel.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.destinationLabel.Size = new System.Drawing.Size(142, 35);
            this.destinationLabel.TabIndex = 2;
            this.destinationLabel.Text = "What is your destination?";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.panel4.Controls.Add(this.originLabel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(142, 35);
            this.panel4.TabIndex = 0;
            // 
            // originLabel
            // 
            this.originLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.originLabel.Location = new System.Drawing.Point(0, 0);
            this.originLabel.Margin = new System.Windows.Forms.Padding(0);
            this.originLabel.MaximumSize = new System.Drawing.Size(142, 0);
            this.originLabel.Name = "originLabel";
            this.originLabel.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.originLabel.Size = new System.Drawing.Size(142, 35);
            this.originLabel.TabIndex = 1;
            this.originLabel.Text = "What city are you starting in?";
            // 
            // ConfigureRecordingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.ClientSize = new System.Drawing.Size(284, 129);
            this.Controls.Add(this.formLayoutPanel);
            this.Controls.Add(this.actionsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureRecordingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OryxBot - Create Custom Route";
            this.TopMost = true;
            this.actionsPanel.ResumeLayout(false);
            this.formLayoutPanel.ResumeLayout(false);
            this.formLayoutPanel.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ComboBox destinationComboBox;
        private System.Windows.Forms.Label originLabel;
        private System.Windows.Forms.Label destinationLabel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TextBox nameTextBox;

        private System.Windows.Forms.TableLayoutPanel formLayoutPanel;

        private System.Windows.Forms.ComboBox originComboBox;

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button confirmButton;

        private System.Windows.Forms.FlowLayoutPanel actionsPanel;

        #endregion
    }
}

