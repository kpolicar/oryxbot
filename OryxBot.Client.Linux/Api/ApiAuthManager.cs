using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using OryxBot.Client.Linux;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Events;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Exceptions;
using OryxBot.Shared;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Linux.Api
{
    public class ApiAuthManager : AuthManager, HasDependencies
    {
        private ApiClient api = null!;

        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;
        public event EventHandler<AuthChangedEvent>? AuthChanged;

        public User? User {
            private set; get;
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            api.UserFetched += (sender, args) => User = args.user;
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
                {"scope", ""},
                {"_passport_token_name", Program.InstanceIdentifier},
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
