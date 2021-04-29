using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using static OryxBot.Client.Windows.Native.User32;
using BotManager = OryxBot.Shared.Contracts.BotManager;
using Region = OryxBot.Shared.Game.Region;

namespace OryxBot.Client.Windows
{
    public partial class TradeMissionRunBotStatusForm : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80; //WS_EX_TOOLWINDOW

                return Params;
            }
        }
        
        public TradeMissionRunBotStatusForm() {
            InitializeComponent();
            Location = new Point( 
                Screen.PrimaryScreen.Bounds.Right - Width,
                Screen.PrimaryScreen.Bounds.Bottom / 2 - Height / 2);
            
            var bot = Program.Services.GetService<BotManager>() as Bot.BotManager;
            bot!.TradeMissionRun += (_, record) =>
                (record.Job as TradeMissionRun)!.StatusChanged += OnBotStatusChanged;
            LocalCharacter.Instance.Move += OnCharacterMove;
        }

        private void OnCharacterMove(object? sender, EventArgs e) {
            if (!Visible)
                return;
            var position = LocalCharacter.Instance.Position;
            var speed = LocalCharacter.Instance.Speed;
            Invoke(new Action(() => {
                botPositionValueLabel.Text =
                    position.X.ToString("0.0") + ", "+
                    position.Y.ToString("0.0");
                botSpeedValueLabel.Text = speed.ToString("0.0")+" m/s";
            }));
        }

        private void OnBotStatusChanged(object? sender, BotEventArgs e) {
            if (!Visible)
                return;
            Invoke(new Action(() => {
                var run = (e.Job as TradeMissionRun)!;
                botStatusValueLabel.Text = run.Step switch {
                    TradeMissionRun.FinishQuest => "Finishing quest",
                    TradeMissionRun.BankItems => "Banking",
                    TradeMissionRun.ProgressQuest => "Progressing quest",
                    TradeMissionRun.TakeQuest => "Taking quest",
                    TradeMissionRun.RunRouteBack => "Running route",
                    TradeMissionRun.RunRouteToDestination => "Running route back",
                    TradeMissionRun.RunToBank => "Running to bank",
                    TradeMissionRun.RunToQuest => "Running to quest",
                    _ => "Unknown"
                };
            }));
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

