using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OryxBot.Client.Windows.Domain;
using OryxBot.Shared;
using Timer = System.Windows.Forms.Timer;

namespace OryxBot.Client.Windows.Api
{
    public class ApiConnection
    {
        private AuthDetails authDetails;
        private readonly Timer refreshTokenTimer;

        public ApiConnection(AuthDetails authDetails) {
            this.authDetails = authDetails;
            refreshTokenTimer = new Timer();
            refreshTokenTimer.Interval = 55000;
            refreshTokenTimer.Tick += OnRefreshTokenTimer;
            refreshTokenTimer.Start();
        }

        public void Terminate() {
            refreshTokenTimer.Dispose();
        }

        public Task? RefreshTask { private set; get; }

        public HttpClient Request() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authDetails.access_token);
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
                {"scope", ""}
            };
            var jsonMessage = JsonConvert.SerializeObject(form_params);
            var encrypted = Aes256CbcEncrypter.Encrypt(jsonMessage);
            
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
    }
}
