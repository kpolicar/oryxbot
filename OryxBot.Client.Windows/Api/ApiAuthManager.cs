using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using OryxBot.Client.Windows;
using Newtonsoft.Json;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Client.Windows.Domain;
using OryxBot.Client.Windows.Events;
using OryxBot.Client.Windows.Exceptions;
using OryxBot.Shared;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Api
{
    public class ApiAuthManager : AuthManager, HasDependencies
    {
        private const int SubscriptionCheckRequestMaxAttempts = 3;
        private int SubscriptionCheckRequestAttempts = 0;
        private Timer subscriptionCheckTimer = new() {
            Interval = 25000,
        };


        private ApiClient api = null!;

        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;
        public event EventHandler<AuthChangedEvent>? AuthChanged;

        public User? User {
            private set; get;
        }

        public ApiAuthManager() {
            subscriptionCheckTimer.Tick += OnSubscriptionCheckTimer;
            subscriptionCheckTimer.Start();
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            api.UserFetched += (sender, args) => User = args.user;
        }

        private async void OnSubscriptionCheckTimer(object? sender, EventArgs e) {
            if (User == null)
                return;
            SubscriptionCheckRequestAttempts++;
            bool success = false;
            var exception = (Exception?) null;
            
            try {
                var user = await api.User();
                if (!user.is_subscribed && !user.on_free_trial)
                    throw new UserNotSubscribedException();
                success = true;
            } catch (Exception ex) {
                exception = ex;
                if (exception is HttpRequestException && SubscriptionCheckRequestAttempts < SubscriptionCheckRequestMaxAttempts+1) {
                    OnSubscriptionCheckTimer(sender, e);
                    return;
                }
                User = null;
            }
            
            AuthChanged?.Invoke(this, new AuthChangedEvent(success, exception));
            SubscriptionCheckRequestAttempts = 0;
        }

        public async Task<ApiConnection?> Login(string username, string password) {
            var client = new HttpClient();
            var url =  $"{Server.AuthUrl}/token";

            var form_params = new Dictionary<string, string> {
                {"grant_type", "password"},
                {"username", username},
                {"password", password},
                {"client_id", Program.GrantId},
                {"client_secret", Program.GrantSecret},
                {"scope", ""}
            };
            var encrypted = Aes256CbcEncrypter.Encrypt(form_params);
            
            var content = new StringContent(encrypted);
            var response = await client.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await GetResultFromEncryptedResponse(response);
            var authDetails = JsonConvert.DeserializeObject<AuthDetails>(result);
            var connection = new ApiConnection(authDetails);

            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(connection));
            return connection;
        }

        public void Logout() {
            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(null));
        }
        
        private async Task<string> GetResultFromEncryptedResponse(HttpResponseMessage response) =>
            Aes256CbcEncrypter.Decrypt(await response.Content.ReadAsStringAsync());
    }
}
