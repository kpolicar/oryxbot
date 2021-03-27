using System;
using System.ComponentModel;

namespace OryxBot.Client.Windows
{
    partial class SelectCityForm
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
            this.labelPanel = new System.Windows.Forms.Panel();
            this.mainLabel = new System.Windows.Forms.Label();
            this.citySelectorComboBox = new System.Windows.Forms.ComboBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.confirmButton = new System.Windows.Forms.Button();
            this.formPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.formActionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.labelPanel.SuspendLayout();
            this.formPanel.SuspendLayout();
            this.formActionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelPanel
            // 
            this.labelPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.labelPanel.Controls.Add(this.mainLabel);
            this.labelPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelPanel.Location = new System.Drawing.Point(0, 0);
            this.labelPanel.Name = "labelPanel";
            this.labelPanel.Padding = new System.Windows.Forms.Padding(20);
            this.labelPanel.Size = new System.Drawing.Size(378, 56);
            this.labelPanel.TabIndex = 0;
            // 
            // mainLabel
            // 
            this.mainLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLabel.Location = new System.Drawing.Point(20, 20);
            this.mainLabel.Name = "mainLabel";
            this.mainLabel.Size = new System.Drawing.Size(338, 16);
            this.mainLabel.TabIndex = 0;
            this.mainLabel.Text = "What city would you like to run?";
            this.mainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // citySelectorComboBox
            // 
            this.citySelectorComboBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.citySelectorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.citySelectorComboBox.FormattingEnabled = true;
            this.citySelectorComboBox.Location = new System.Drawing.Point(14, 3);
            this.citySelectorComboBox.Name = "citySelectorComboBox";
            this.citySelectorComboBox.Size = new System.Drawing.Size(120, 24);
            this.citySelectorComboBox.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (224)))), ((int) (((byte) (224)))), ((int) (((byte) (224)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Location = new System.Drawing.Point(28, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(70, 24);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // confirmButton
            // 
            this.confirmButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (200)))), ((int) (((byte) (189)))), ((int) (((byte) (156)))));
            this.confirmButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.confirmButton.Location = new System.Drawing.Point(104, 3);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(134, 24);
            this.confirmButton.TabIndex = 3;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = false;
            this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // formPanel
            // 
            this.formPanel.AutoSize = true;
            this.formPanel.Controls.Add(this.formActionPanel);
            this.formPanel.Controls.Add(this.citySelectorComboBox);
            this.formPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.formPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.formPanel.Location = new System.Drawing.Point(0, 79);
            this.formPanel.Name = "formPanel";
            this.formPanel.Size = new System.Drawing.Size(378, 30);
            this.formPanel.TabIndex = 4;
            // 
            // formActionPanel
            // 
            this.formActionPanel.AutoSize = true;
            this.formActionPanel.Controls.Add(this.cancelButton);
            this.formActionPanel.Controls.Add(this.confirmButton);
            this.formActionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formActionPanel.Location = new System.Drawing.Point(137, 0);
            this.formActionPanel.Margin = new System.Windows.Forms.Padding(0);
            this.formActionPanel.Name = "formActionPanel";
            this.formActionPanel.Padding = new System.Windows.Forms.Padding(25, 0, 0, 0);
            this.formActionPanel.Size = new System.Drawing.Size(241, 30);
            this.formActionPanel.TabIndex = 4;
            // 
            // SelectCityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (37)))), ((int) (((byte) (40)))), ((int) (((byte) (45)))));
            this.ClientSize = new System.Drawing.Size(378, 109);
            this.Controls.Add(this.formPanel);
            this.Controls.Add(this.labelPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectCityForm";
            this.Text = "Select a City";
            this.TopMost = true;
            this.labelPanel.ResumeLayout(false);
            this.formPanel.ResumeLayout(false);
            this.formPanel.PerformLayout();
            this.formActionPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.FlowLayoutPanel formActionPanel;

        private System.Windows.Forms.FlowLayoutPanel formPanel;

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.ComboBox citySelectorComboBox;

        private System.Windows.Forms.Label mainLabel;

        private System.Windows.Forms.Panel labelPanel;

        #endregion
    }
}

