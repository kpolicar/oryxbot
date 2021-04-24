using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Api;
using Microsoft.VisualBasic.ApplicationServices;
using OryxBot.Client.Windows;
using OryxBot.Client.Windows.Api;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Client.Windows.Domain;
using OryxBot.Client.Windows.Events;
using OryxBot.Client.Windows.Exceptions;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using static OryxBot.Client.Windows.Native.User32;
using BotManager = OryxBot.Client.Windows.Bot.BotManager;
using Region = OryxBot.Shared.Game.Region;
using User = OryxBot.Shared.User;
using BotManagerContract = OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Client.Windows
{
    public partial class MainForm : Form
    {
        private ApiClient api;
        private AuthManager auth;
        private TradeMissionRouteManager _routeManager;
        public static event EventHandler? LoggingIn;
        private SelectCityForm selectCityForm;
        private SelectHeartsForm selectHeartsForm;
        private ConfigureRecordingForm configureRecordingForm;
        private BotStatusForm botStatusForm;
        private BotManager botManager;
        private BotJob Bot;

        public MainForm() {
            InitializeComponent();
            InitializeCustomComponent();
            InitializeIcons();
            
            LoggingIn += (_, _) => UpdateControlsForUnauthenticated();
            VisibleChanged += OnVisibleChanged;
            
            errorMessage.Text = "";
            usernameTextBox.Text = ConfigurationManager.AppSettings.Get("email");
            passwordTextBox.Text = ConfigurationManager.AppSettings.Get("password");
            #if DEBUG
            usernameTextBox.Text = "admin@oryxbot.com";
            passwordTextBox.Text = "***REMOVED***";
            #endif
            newVersionLabel.Hide();
            rememberPasswordCheckbox.Checked = ConfigurationManager.AppSettings.Get("password")?.Length > 0;
            auth = Program.Services.GetService<AuthManager>();
            api = Program.Services.GetService<ApiClient>();
            _routeManager = Program.Services.GetService<TradeMissionRouteManager>();
            botManager = (BotManager) Program.Services.GetService<BotManagerContract>();
            _routeManager.ModeChanged += OnRouteManagerModeChanged;
            selectCityForm = new SelectCityForm();
            selectHeartsForm = new SelectHeartsForm();
            configureRecordingForm = new ConfigureRecordingForm();
            botStatusForm = new BotStatusForm();
            api.UserFetched += OnUserFetched;
            botManager.JobChanged += (_, e) => Bot = e.Job;
        }

        private void OnVisibleChanged(object? sender, EventArgs e) {
            if (Visible)
                LoggingIn?.Invoke(this, EventArgs.Empty);
            #if DEBUG
            if (Visible)
                button1_Click(this, EventArgs.Empty);
            #endif
        }

        private void InitializeIcons() {
            var titlebarIcon = (Icon) resources.GetObject("$this.Icon")!;
            var taskbarIcon = (Icon) resources.GetObject("$this.IconTaskbar")!;
            SendMessage(Handle, WM_SETICON, ICON_SMALL, titlebarIcon.Handle);
            SendMessage(Handle, WM_SETICON, ICON_BIG, taskbarIcon.Handle);
        }

        public (City origin, Region destination, string name)? ShowConfigureRecordingForm() {
            if (configureRecordingForm.Visible)
                return null;
            
            var result = configureRecordingForm.ShowDialog(this);
            return result == DialogResult.OK
                ? (
                    configureRecordingForm.SelectedOrigin,
                    configureRecordingForm.SelectedDestination,
                    configureRecordingForm.SelectedName
                    )
                : null;
        }

        public City? ShowSelectCityForm() {
            if (selectCityForm.Visible)
                return null;
            
            var result = selectCityForm.ShowDialog(this);
            return result == DialogResult.OK
                ? selectCityForm.SelectedCity
                : null;
        }

        public RunConfiguration.ContractType? ShowHeartsForm() {
            if (selectHeartsForm.Visible)
                return null;
            
            var result = selectHeartsForm.ShowDialog(this);
            return result == DialogResult.OK
                ? selectHeartsForm.SelectedContractType
                : null;
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
            } catch (HttpRequestException ex) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextConnectionError");
                return;
            } catch (Exception) {
                button1.Enabled = true;
                errorMessage.Text = resources.GetString("errorMessage.TextUnknownError");
                return;
            }

            UserSettings.AddUpdateAppSettings(new Dictionary<string, string> {
                {"email", usernameTextBox.Text},
                {"password", rememberPasswordCheckbox.Checked ? passwordTextBox.Text : ""},
            });

            Hide();
        }

        private void HandleUserSubscriptionStatus(User user) {
            if (!user.is_subscribed && !user.on_free_trial)
                throw new UserNotSubscribedException();
        }

        private async void MainForm_Load(object sender, EventArgs e) {
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

        private void MainForm_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                UpdateControlsForUnauthenticated();
            } else {
                UpdateControlsForAuthenticated();
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
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            api.UserFetched += OnFetchedUser;
        }

        public void ShowLoginDialogue(string message = "") {
            errorMessage.Text = message;
            Show();
        }

        public void OnAuthChanged(object? sender, AuthChangedEvent e) {
            if (e.Succeeded)
                return;
            
            var message = e.Exception switch {
                HttpRequestException _ => Resources.UIApplicationContext.ErrorMessage_Http,
                UserNotSubscribedException _ =>
                    Resources.UIApplicationContext.ErrorMessage_NoLongerSubscribed+"\n"+
                    Resources.UIApplicationContext.ErrorMessage_PleaseExtend,
                _ => ""
            };
            ShowLoginDialogue(message);
        }
    }
}
