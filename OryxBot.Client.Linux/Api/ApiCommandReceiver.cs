using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Broadcasting;
using OryxBot.Client.Linux.Contracts;
using OryxBot.Client.Linux.Domain;
using OryxBot.Client.Linux.Events;
using OryxBot.Client.Linux.Services;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using PusherClient;

namespace OryxBot.Client.Linux.Api
{
    public class ApiCommandReceiver : HasDependencies, IDisposable
    {
        public event EventHandler<LogEntry>? LogForwardedFailed;
        private ApiClient api;
        private AuthManager auth;
        private bool connectedToSocketServer;
        private Pusher? pusher;
        private BotManager bot;
        private TradeMissionRouteManager routeManager;
        private Channel channel;

        public void BindDependencies(ServiceContainer serviceContainer) {
            bot = serviceContainer.GetService<BotManager>();
            api = serviceContainer.GetService<ApiClient>();
            auth = serviceContainer.GetService<AuthManager>();
            routeManager = serviceContainer.GetService<TradeMissionRouteManager>();
            api.UserFetched += OnUserFetched;
            WebSocketLogForwarder.Instance.ReceivedLog += OnReceivedLog;
        }

        private void OnReceivedLog(object? sender, LogEntry e) =>
            Task.Run(async () => {
                await EnforceConnectedToSocketServer();
                
                var attempts = 0;
                while (attempts++ < 5 && !channel.IsSubscribed) {
                    await Task.Delay(2000);
                }
                try {
                    await channel.TriggerAsync(@"client-LogEntry", e).ConfigureAwait(false);
                } catch (Exception ex) {
                    LogForwardedFailed?.Invoke(this, e);
                    Console.WriteLine("Failed to forward log "+e);
                    Console.WriteLine(ex);
                }
            }).ConfigureAwait(false);

        private void OnUserFetched(object? sender, FetchedUserEventArgs e) =>
            EnforceConnectedToSocketServer();

        private async Task EnforceConnectedToSocketServer() {
            if (api.Connection == null || connectedToSocketServer)
                return;

            Console.WriteLine("Preparing connection");
            pusher = new Pusher(Server.PusherAppKey, new PusherOptions() {
                Host = Server.WebsocketHost,
                Encrypted = Server.WebsocketEncrypted,
                Authorizer = new HttpAuthorizer(Server.BroadcastingAuthUrl) {
                    AuthenticationHeader = api.Connection!.AuthenticationHeader,
                },
                // TraceLogger = new PusherDebugTracer()
            });
            
            pusher.Connected += _ => Console.WriteLine("Connected to pusher.");
            pusher.Disconnected += _ => Console.WriteLine("Disconnected to pusher.");
            pusher.Subscribed += (_, channel) => Console.WriteLine("Subscribed to "+channel.Name);
            pusher.Error += (_, exception) => {
                Console.WriteLine("Error: " + exception.Message);
                Console.WriteLine("Error code: " + exception.PusherCode);
                Console.WriteLine("Error state: " + pusher.State);
            };

            Console.WriteLine($"User: {auth.User?.id}\t{auth.User?.email}");
            
            channel = await pusher.SubscribeAsync("private-App.Models.User." + auth.User!.id);
            pusher.Bind(@"App\Events\RequestBotRunningChanged", OnRequestBotRunningChanged);
            pusher.Bind(@"App\Events\RequestBotRecordStart", OnRequestBotRecordStart);
            pusher.Bind(@"App\Events\RequestBotResume", OnRequestBotResume);
            pusher.Bind(@"App\Events\RequestStatus", OnRequestStatus);
            
            Console.WriteLine("Connected to socket server: "+pusher.State);
            Console.WriteLine(Server.WebsocketHost);
            Console.WriteLine(Server.WebsocketEncrypted);
            Console.WriteLine(Server.BroadcastingAuthUrl);
            Console.WriteLine(Server.PusherAppKey);

            KeepConnectingToPusherUntilConnected();
            
            connectedToSocketServer = true;
        }

        private void KeepConnectingToPusherUntilConnected() =>
            Task.Run(async () => {
                var error = false;
                do {
                    try {
                        await pusher.ConnectAsync();
                        error = false;
                    } catch (Exception exception) {
                        error = true;
                        Console.WriteLine("Error in socket server connection: " + exception);
                        await Task.Delay(10000);
                    }

                    Console.WriteLine("Connected to socket server: " + pusher.State);
                } while (error);
            });

        private void OnRequestStatus(PusherEvent eventData) {
            Console.WriteLine($"Message for status2");
            _ = api.NotifyServerStatus();
        }

        public void OnRequestBotResume(PusherEvent eventData) {
            FileLogger.Common.Info($"Received command to resume Oryxbot");
            
            var data = JsonConvert.DeserializeObject<RequestBotResume>(eventData.Data)!;
            Console.WriteLine($"Message from '{data.InstanceId}': city: {data.City}, region: {data.Alias}, progressed: {data.Progressed}");
            
            routeManager.SetDefaultRouteCity(Cities.City(data.City));
            bot.SetRunConfiguration(new RunConfiguration(
                (RunConfiguration.ContractType) data.Hearts,
                data.Alias,
                data.Progressed)
            );
            
            Program.RunProgram();
        }

        public void OnRequestBotRunningChanged(PusherEvent eventData) {
            try {
                Console.WriteLine(eventData.Data);
                var data = JsonConvert.DeserializeObject<RequestBotRunningChanged>(eventData.Data)!;
                Console.WriteLine($"Message from '{data.InstanceId}': {data.Running}");

                if (data.City != null) {
                    routeManager.SetDefaultRouteCity(Cities.City(data.City));
                }

                if (data.Hearts != null) {
                    bot.SetRunConfiguration(new RunConfiguration((RunConfiguration.ContractType)data.Hearts));
                }

                if (data.Running) {
                    FileLogger.Common.Info($"Received command to start Oryxbot");
                    Program.RunProgram();
                } else {
                    FileLogger.Common.Info($"Received command to stop Oryxbot");
                    Program.StopProgram();
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine(e.StackTrace);
            }
        }
        
        private void OnRequestBotRecordStart(PusherEvent eventData) {
            var data = JsonConvert.DeserializeObject<RequestRecordStart>(eventData.Data)!;
            Console.WriteLine($"Message from '{data.InstanceId}': city: {data.City}, destination: {data.Destination}, name: {data.Name}");

            bot.SetRecordingConfiguration(new RecordingConfiguration(
                Cities.City(data.City),
                Regions.Region(data.Destination)!.Value,
                data.Name));
            
            Program.RunRecordingProgram();
        }

        public void Dispose() {
            pusher?.DisconnectAsync();
        }
    }
}
