using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OryxBot.Client.Windows.Contracts;
using OryxBot.Client.Windows.Domain;
using OryxBot.Client.Windows.Events;
using OryxBot.Client.Windows.Exceptions;
using OryxBot.Shared;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Api
{
    public class ApiClient : HasDependencies
    {
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
            var response = await client.GetAsync($"{Server.ApiUrl}/user");
            
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }
        
        public async Task<VersionDetails> NewestVersion() {
            var client = new HttpClient();
            var response = await client.GetAsync(Server.ApiUrl);
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            var authManager = serviceContainer.GetService<AuthManager>();
            authManager.ConnectionChanged += OnConnectionChanged;
        }
    }
}
