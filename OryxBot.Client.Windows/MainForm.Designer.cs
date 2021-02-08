namespace OryxBot.Client.Windows
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.gameWindowPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // gameWindowPanel
            // 
            this.gameWindowPanel.AutoSize = true;
            this.gameWindowPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gameWindowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameWindowPanel.Location = new System.Drawing.Point(0, 0);
            this.gameWindowPanel.Margin = new System.Windows.Forms.Padding(2);
            this.gameWindowPanel.Name = "gameWindowPanel";
            this.gameWindowPanel.Size = new System.Drawing.Size(1200, 731);
            this.gameWindowPanel.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 731);
            this.Controls.Add(this.gameWindowPanel);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(484, 399);
            this.Name = "MainForm";
            this.Text = "OryxBot";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel gameWindowPanel;

        #endregion
    }
}
