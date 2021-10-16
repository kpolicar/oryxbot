using System;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Broadcasting;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Events;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
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
            api.UserFetched += OnUserFetched;
        }

        private void OnUserFetched(object? sender, FetchedUserEventArgs e) =>
            EnforceConnectedToSocketServer();

        private void EnforceConnectedToSocketServer() {
            if (api.Connection == null || connectedToSocketServer)
                return;

            Console.WriteLine("Preparing connection");
            pusher = new Pusher(Server.PusherAppKey, new PusherOptions() {
                Host = Server.WebsocketHost,
                Encrypted = Server.WebsocketEncrypted,
                Authorizer = new HttpAuthorizer(Server.BroadcastingAuthUrl) {
                    AuthenticationHeader = api.Connection!.AuthenticationHeader,
                },
            });
            
            pusher.Connected += _ => Console.WriteLine("Connected to pusher.");
            pusher.Disconnected += _ => Console.WriteLine("Disconnected to pusher.");
            pusher.Subscribed += (_, channel) => Console.WriteLine("Subscribed to "+channel.Name);
            pusher.Error += (_, exception) => Console.WriteLine("Error: "+exception.Message);

            Console.WriteLine($"User: {auth.User?.id}\t{auth.User?.email}");
            pusher.SubscribeAsync("private-App.Models.User."+auth.User!.id);
            pusher.Bind(@"App\Events\RequestBotRunningChanged", OnRequestBotRunningChanged);
            pusher.Bind(@"App\Events\RequestStatus", OnRequestStatus);
            
            Console.WriteLine("Connected to socket server: "+pusher.State);
            Console.WriteLine(Server.WebsocketHost);
            Console.WriteLine(Server.WebsocketEncrypted);
            Console.WriteLine(Server.BroadcastingAuthUrl);
            Console.WriteLine(Server.PusherAppKey);
            pusher.ConnectAsync();
            Console.WriteLine("Connected to socket server: "+pusher.State);
            
            connectedToSocketServer = true;
        }

        private void OnRequestStatus(PusherEvent eventData) {
            Console.WriteLine($"Message for status");
            _ = api.NotifyServerStatus();
        }

        public void OnRequestBotRunningChanged(PusherEvent eventData) {
            var data = JsonConvert.DeserializeObject<RequestBotRunningChanged>(eventData.Data)!;
            Console.WriteLine($"Message from '{data.Running}': {data.InstanceId}");

            if (data.Running) {
                Program.RunProgram();
            } else {
                Program.StopProgram();
            }
        }

        public void Dispose() {
            pusher.DisconnectAsync();
        }
    }
}
