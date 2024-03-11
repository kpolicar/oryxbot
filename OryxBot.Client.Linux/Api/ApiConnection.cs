using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Domain;
using OryxBot.Shared;

namespace OryxBot.Client.Linux.Api
{
    public class ApiConnection : IDisposable
    {
        private AuthDetails? authDetails;
        private readonly Timer? refreshTokenTimer;
        private readonly string? AccessToken;

        public ApiConnection(string accessToken) {
            AccessToken = accessToken;
        }

        public ApiConnection(AuthDetails authDetails) {
            this.authDetails = authDetails;
            refreshTokenTimer = new Timer();
            refreshTokenTimer.Interval = 55000;
            refreshTokenTimer.Elapsed += OnRefreshTokenTimer;
            refreshTokenTimer.Start();
        }

        public void Terminate() =>
            Dispose();

        public Task? RefreshTask { private set; get; }

        public AuthenticationHeaderValue AuthenticationHeader =>
            new AuthenticationHeaderValue("Bearer", AccessToken ?? authDetails!.access_token);

        public HttpClient Request() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthenticationHeader;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(Server.BaseUrl);
            return client;
        }

        private void OnRefreshTokenTimer(object sender, EventArgs eventArgs) {
            RefreshTask = RefreshToken();
        }

        public async Task<bool> RefreshToken() {
            var client = new HttpClient();
            var url =  $"{Server.AuthUrl}/token";

            var form_params = new Dictionary<string, string> {
                {"grant_type", "refresh_token"},
                {"refresh_token", authDetails.refresh_token},
                {"client_id", Program.GrantId},
                {"client_secret", Program.GrantSecret},
                {"scope", ""},
                {"_passport_token_name", Program.InstanceIdentifier},
            };
            var encrypted = Aes256CbcEncrypter.Encrypt(form_params);
            
            var content = new StringContent(encrypted);
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await GetResultFromEncryptedResponse(response);
            authDetails = JsonConvert.DeserializeObject<AuthDetails>(result);
            Debug.WriteLine("Http response: "+result);
            return true;
        }
        
        private async Task<string> GetResultFromEncryptedResponse(HttpResponseMessage response) =>
            Aes256CbcEncrypter.Decrypt(await response.Content.ReadAsStringAsync());

        public void Dispose() {
            refreshTokenTimer?.Close();
            RefreshTask?.Dispose();
        }
    }
}
