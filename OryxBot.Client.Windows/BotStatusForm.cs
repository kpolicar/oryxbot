using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using static OryxBot.Client.Windows.Native.User32;
using Region = OryxBot.Shared.Game.Region;

namespace OryxBot.Client.Windows
{
    public partial class BotStatusForm : Form
    {
        public BotStatusForm() {
            InitializeComponent();
            Location = new Point( 
                Screen.PrimaryScreen.Bounds.Right - Width,
                Screen.PrimaryScreen.Bounds.Bottom / 2 - Height / 2);
        }
        
        
        const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {     
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}

