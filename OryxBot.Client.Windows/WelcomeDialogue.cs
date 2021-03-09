using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using OryxBot.Client.Windows;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Client.Windows.Domain;
using OryxBot.Client.Windows.Exceptions;
using static OryxBot.Client.Windows.Native.User32;
using User = OryxBot.Shared.User;

namespace Inkybot
{
    public partial class WelcomeDialogue : Form
    {
        private readonly ApiClient api;
        private AuthManager auth;
        public static event EventHandler? LoggingIn;

        public WelcomeDialogue(string errorMessage) : this() {
            this.errorMessage.Text = errorMessage;
        }

        public WelcomeDialogue() {
            InitializeComponent();
            InitializeIcons();
            VisibleChanged += OnVisibleChanged;
            
            errorMessage.Text = "";
            usernameTextBox.Text = ConfigurationManager.AppSettings.Get("email");
            passwordTextBox.Text = ConfigurationManager.AppSettings.Get("password");
            newVersionLabel.Hide();
            rememberPasswordCheckbox.Checked = ConfigurationManager.AppSettings.Get("password")?.Length > 0;
            auth = Program.Services.GetService<AuthManager>();
            api = Program.Services.GetService<ApiClient>();
        }

        private void OnVisibleChanged(object? sender, EventArgs e) {
            if (Visible)
                LoggingIn?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeIcons() {
            var titlebarIcon = (Icon) resources.GetObject("$this.Icon")!;
            var taskbarIcon = (Icon) resources.GetObject("$this.IconTaskbar")!;
            SendMessage(Handle, WM_SETICON, ICON_SMALL, titlebarIcon.Handle);
            SendMessage(Handle, WM_SETICON, ICON_BIG, taskbarIcon.Handle);
        }

        private async void button1_Click(object sender, EventArgs e) {
            errorMessage.Text = "";
            button1.Enabled = false;
            try {
                var connection = await auth.Login(usernameTextBox.Text, passwordTextBox.Text);
                button1.Enabled = true;

                if (connection == null) {
                    errorMessage.Text = resources.GetString("errorMessage.TextIncorrect");
                    return;
                }

                var user = await api.User();
                HandleUserSubscriptionStatus(user);

            } catch (UserNotSubscribedException) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextUnsubscribed");
                return;
            } catch (HttpRequestException) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextConnectionError");
                return;
            }

            UserSettings.AddUpdateAppSettings(new Dictionary<string, string> {
                {"email", usernameTextBox.Text},
                {"password", rememberPasswordCheckbox.Checked ? passwordTextBox.Text : ""},
            });
            
            DialogResult = DialogResult.OK;
        }

        private void HandleUserSubscriptionStatus(User user) {
            if (!user.is_subscribed && !user.on_free_trial)
                throw new UserNotSubscribedException();
        }

        private async void LoginForm_Load(object sender, EventArgs e) {
            try {
                var newestVersion = await api.NewestVersion();
                if (Program.VersionNumber != newestVersion.number) {
                    newVersionLabel.Show();
                    var tooltip = new ToolTip();
                    tooltip.SetToolTip(newVersionLabel, Program.Version+" » "+newestVersion.name);
                }
            } catch (Exception) {
                // ignored
            }
        }

        private void linkLabel1_LinkClicked_1(object sender, EventArgs eventArgs) {
            var psi = new ProcessStartInfo {
                FileName = $"{Server.BaseUrl}/register",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            var psi = new ProcessStartInfo {
                FileName = $"{Server.BaseUrl}",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void newVersionLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            var psi = new ProcessStartInfo {
                FileName = $"{Server.BaseUrl}/release/latest",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void resetSettings_Clicked(object sender, EventArgs eventArgs) {
            UserSettings.Reset();
            Application.Restart();
        }
    }
}
