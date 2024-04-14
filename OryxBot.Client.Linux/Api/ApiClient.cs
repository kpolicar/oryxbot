using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Events;
using OryxBot.Client.Linux.Exceptions;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Native;
using OryxBot.Shared;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Client.Linux.Api
{
    public class ApiClient : HasDependencies, IDisposable
    {
        private BotManagerContract bot;
        public ApiConnection? Connection { private set; get; }

        public event EventHandler<FetchedUserEventArgs>? UserFetched;

        private void OnConnectionChanged(object sender, ApiConnectionChangedEventArgs e) {
            Connection?.Terminate();
            Connection = e.connection;
        }

        private async Task WaitForStableConnection() {
            if (Connection == null)
                throw new ApiConnectionNotEstablishedException();
            if (Connection.RefreshTask != null && !Connection.RefreshTask.IsCompleted)
                await Connection.RefreshTask;
        }

        public async Task<User> User() {
            await WaitForStableConnection();

            var client = Connection!.Request();
            var response = await client.GetAsync($"{Server.ApiBaseUrl}/user");
            response.EnsureSuccessStatusCode();
            var result = await GetResultFromEncryptedResponse(response);

            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }

        public async Task<VersionDetails> NewestVersion() {
            var client = new HttpClient();
            var response = await client.GetAsync(Server.ApiBaseUrl);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            
            Debug.WriteLine("Http response: "+result);
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }

        public async Task NotifyRunComplete() {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/trademission/complete", new StringContent(""));
        }
        
        public async Task NotifyTradeMissionStuck() {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/trademission/stuck", new StringContent(""));
        }

        public async Task NotifyStepChanged() {
            await WaitForStableConnection();

            var data = BotStepData();
            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/data/stepchanged", new StringContent(encrypted));
        }
        
        public async Task NotifyCharacterMoved() {
            await WaitForStableConnection();
            var data = LocalCharacterPositionData();
            
            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/data/moved", new StringContent(encrypted));
        }
        
        public async Task NotifyRemoteDesktopConnectionEstablished() {
            await WaitForStableConnection();
            var data = RemoteDesktopStateData();
            
            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/data/remotedesktop", new StringContent(encrypted));
        }

        public async Task NotifyRunStarting(string title, string message) {
            await WaitForStableConnection();
            
            if (title.Length > 40)
                title = title.Substring(0, 40) + "...";
            
            var form_params = new Dictionary<string, string> {
                {"title", title},
                {"message", message}
            };
            var encrypted = Aes256CbcEncrypter.Encrypt(form_params);
            
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/trademission/starting", new StringContent(encrypted));
        }
        
        private async Task<string> GetResultFromEncryptedResponse(HttpResponseMessage response) =>
            Aes256CbcEncrypter.Decrypt(await response.Content.ReadAsStringAsync());

        public void BindDependencies(ServiceContainer serviceContainer) {
            var authManager = serviceContainer.GetService<AuthManager>();
            authManager.ConnectionChanged += OnConnectionChanged;
            bot = serviceContainer.GetService<BotManagerContract>();
        }

        public void Dispose() {
            Connection?.Dispose();
        }

        public async Task NotifyClientVersion() {
            await WaitForStableConnection();
            var data = new Dictionary<string, string>() {
                {"version", Program.Version}
            };

            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            var response = await Connection!.Request()
                .PostAsync($"{Server.ApiUrl}/data/clientversion", new StringContent(encrypted));
        }

        public async Task NotifyServerStatus() {
            await WaitForStableConnection();
            var data = BotStepData()
                .MergeLeft(LocalCharacterPositionData())
                .MergeLeft(RemoteDesktopStateData())
                .MergeLeft(BotRunningData());

            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            var response = await Connection!.Request()
                .PostAsync($"{Server.ApiUrl}/data/status", new StringContent(encrypted));
        }

        public async Task NotifyBotRunningChanged() {
            await WaitForStableConnection();
            var data = BotRunningData();
            
            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/data/runningchanged", new StringContent(encrypted));
        }
        
        
        protected Dictionary<string, string> BotStepData() =>
            new() {
                {"bot_step", bot.IsRunning ? ((bot as BotManager)?.Bot as TradeMissionRun)?.Step.Name ?? "" : ""}
            };

        protected Dictionary<string, string> LocalCharacterPositionData() =>
            new() {
                {"character_x", LocalCharacter.Instance.Position.X.ToString(CultureInfo.InvariantCulture)},
                {"character_y", LocalCharacter.Instance.Position.Y.ToString(CultureInfo.InvariantCulture)},
                {"character_speed", LocalCharacter.Instance.Speed.ToString(CultureInfo.InvariantCulture)},
            };
        
        protected Dictionary<string, string> RemoteDesktopStateData() =>
            new() {
                {"remote_desktop_connected", Vnc.Connected.ToString()},
                {"remote_desktop_resolution_x", Vnc.Dimensions?.x.ToString() ?? ""},
                {"remote_desktop_resolution_y", Vnc.Dimensions?.y.ToString() ?? ""},
            };
        
        protected Dictionary<string, string> BotRunningData() =>
            new() {
                {"bot_running", bot.IsRunning.ToString()},
                {"bot_recording_running", (((bot as BotManager)!.Bot as TradeMissionRecord)?.Running ?? false).ToString()},
            };
    }
}
