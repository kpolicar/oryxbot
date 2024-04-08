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
        private bool connectingToSocketServer;
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
                    FileLogger.Common.Warn($"Failed to forward log to websocket server");
                }
            }).ConfigureAwait(false);

        private void OnUserFetched(object? sender, FetchedUserEventArgs e) =>
            Task.Run(EnforceConnectedToSocketServer).ConfigureAwait(false);

        private async Task EnforceConnectedToSocketServer() {
            if (api.Connection == null || connectedToSocketServer || connectingToSocketServer)
                return;
            connectingToSocketServer = true;

            //FileLogger.Common.Info($"Establishing connection to websocket server at {Server.WebsocketHost}");
            pusher = new Pusher(Server.PusherAppKey, new PusherOptions() {
                Host = Server.WebsocketHost,
                Encrypted = Server.WebsocketEncrypted,
                Authorizer = new HttpAuthorizer(Server.BroadcastingAuthUrl) {
                    AuthenticationHeader = api.Connection!.AuthenticationHeader,
                },
                
            });
            
            pusher.Error += (_, exception) => {
                FileLogger.Common.Info($"Error occured with websocket connection: "+exception.Message);
            };
            pusher.ConnectionStateChanged += (_, state) => {
                FileLogger.Common.Info($"Connection to websocket server changed: " + state switch {
                    ConnectionState.Uninitialized => "Uninitialized",
                    ConnectionState.Connecting => "Connecting",
                    ConnectionState.Connected => "Connected",
                    ConnectionState.Disconnecting => "Disconnecting",
                    ConnectionState.Disconnected => "Disconnected",
                    ConnectionState.WaitingToReconnect => "Waiting to Reconnect",
                    _ => "?"
                });
            };

            channel = await pusher.SubscribeAsync("private-App.Models.User." + auth.User!.id);
            pusher.Bind(@"App\Events\RequestBotRunningChanged", OnRequestBotRunningChanged);
            pusher.Bind(@"App\Events\RequestBotRecordStart", OnRequestBotRecordStart);
            pusher.Bind(@"App\Events\RequestBotResume", OnRequestBotResume);
            pusher.Bind(@"App\Events\RequestStatus", OnRequestStatus);
            
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
                        await Task.Delay(10000);
                    }
                } while (error);
            });

        private void OnRequestStatus(PusherEvent eventData) {
            _ = api.NotifyServerStatus();
        }

        public void OnRequestBotResume(PusherEvent eventData) {
            FileLogger.Common.Info($"Received command to resume Oryxbot");
            
            var data = JsonConvert.DeserializeObject<RequestBotResume>(eventData.Data)!;
            FileLogger.Common.Info($"Bot requested to resume with configuration: city: {data.City}, region: {data.Alias}, progressed: {data.Progressed}");
            
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
                var data = JsonConvert.DeserializeObject<RequestBotRunningChanged>(eventData.Data)!;
                if (data.Running) {
                    FileLogger.Common.Info($"Bot requested to begin trade mission with configuration: city: {data.City}, hearts: {data.Hearts}");
                } else {
                    FileLogger.Common.Info($"Bot requested to stop trade mission run");
                }

                if (data.City != null) {
                    routeManager.SetDefaultRouteCity(Cities.City(data.City));
                }

                if (data.Hearts != null) {
                    bot.SetRunConfiguration(new RunConfiguration((RunConfiguration.ContractType)data.Hearts));
                }

                if (data.Running) {
                    Program.RunProgram();
                } else {
                    Program.StopProgram();
                }
            } catch (Exception e) {
                FileLogger.Common.Error($"Failed to process request for change bot running state");
            }
        }
        
        private void OnRequestBotRecordStart(PusherEvent eventData) {
            var data = JsonConvert.DeserializeObject<RequestRecordStart>(eventData.Data)!;
            FileLogger.Common.Info($"Bot requested to begin recording trade mission route with configuration: city: {data.City}, destination: {data.Destination}");

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
