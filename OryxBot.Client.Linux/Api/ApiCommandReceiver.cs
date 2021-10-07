using System;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Broadcasting;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Events;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using PusherClient;

namespace OryxBot.Client.Linux.Api
{
    public class ApiCommandReceiver : HasDependencies, IDisposable
    {
        private ApiClient api;
        private BotManager botManager;
        private AuthManager auth;
        private bool connectedToSocketServer;
        private Pusher pusher;
        
        private BotManager bot;

        public void BindDependencies(ServiceContainer serviceContainer) {
            botManager = serviceContainer.GetService<BotManager>();
            api = serviceContainer.GetService<ApiClient>();
            auth = serviceContainer.GetService<AuthManager>();
            bot = serviceContainer.GetService<BotManager>();
            auth.ConnectionChanged += OnConnectionChanged;
        }

        private void OnConnectionChanged(object? sender, ApiConnectionChangedEventArgs e) =>
            EnforceConnectedToSocketServer();

        private void EnforceConnectedToSocketServer() {
            if (api.Connection == null || connectedToSocketServer)
                return;
            
            pusher = new Pusher("***REMOVED***", new PusherOptions() {
                Cluster = "eu",
                Host = "127.0.0.1:6001",
                Encrypted = false,
                Authorizer = new HttpAuthorizer(Server.BroadcastingAuthUrl) {
                    AuthenticationHeader = api.Connection!.AuthenticationHeader,
                },
            });
            
            pusher.Connected += _ => Console.WriteLine("Connected to pusher.");
            pusher.Disconnected += _ => Console.WriteLine("Disconnected to pusher.");
            pusher.Subscribed += (_, channel) => Console.WriteLine("Subscribed to "+channel.Name);
            pusher.Error += (_, exception) => Console.WriteLine("Error: "+exception.Message);

            pusher.SubscribeAsync("private-App.Models.User.3");
            pusher.Bind(@"App\Events\RequestBotRunningChanged", OnRequestBotRunningChanged);
            
            pusher.ConnectAsync();
            
            connectedToSocketServer = true;
        }

        public void OnRequestBotRunningChanged(PusherEvent eventData) {
            var data = JsonConvert.DeserializeObject<RequestBotRunningChanged>(eventData.Data)!;
            Console.WriteLine($"Message from '{data.Running}': {data.InstanceId}");
            
            if (data.Running && !bot.IsRunning)
                bot.ToggleTradeMissionRun();
            else if (!data.Running && bot.IsRunning)
                bot.Stop();
        }

        public void Dispose() {
            pusher.DisconnectAsync();
        }
    }
}
