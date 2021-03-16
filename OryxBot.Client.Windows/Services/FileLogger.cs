using System;
using System.Diagnostics;
using NLog;
using OryxBot.Albion.Protocol;
using OryxBot.Client.Windows.Bot;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Client.Windows.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using BotManager = OryxBot.Shared.Contracts.BotManager;
using LoggerContract = OryxBot.Shared.Contracts.Logger;
using NLogger=NLog.Logger;

namespace OryxBot.Client.Windows.Services
{
    public class FileLogger : LoggerContract
    {
        private static readonly NLogger Common = LogManager.GetLogger("log");
        #if DEBUG
        private static readonly NLogger NetworkEvent = LogManager.GetLogger("networkevent");
        private static readonly NLogger NetworkRequest = LogManager.GetLogger("networkrequest");
        #endif

        public void BindToServices(ServiceContainer services) {
            var bot = services.GetService<BotManager>();
            var dataProvider = (NetworkAlbionDataProvider) services.GetService<AlbionDataProvider>();
            
            BindToDataProvider(dataProvider);
            BindToBot(bot);
            BindToLocalCharacter();
            ResponsivePoint.ResolutionChanged += OnResolutionChanged;
        }

        private void OnResolutionChanged(object? sender, EventArgs e) =>
            Common.Info($"Screen resolution changed: {ResponsivePoint.CurrentResolution}");

        private void BindToDataProvider(NetworkAlbionDataProvider dataProvider) {
            #if DEBUG
            dataProvider.NetworkEvent += (_, packet) => {
                var evcode = (EventCodes) packet.EventCode;
                NetworkEvent.Info(evcode.ToString());
            };
            dataProvider.NetworkRequest += (_, packet) => {
                var opcode = (OperationCodes) packet.OperationCode;
                NetworkRequest.Info(opcode.ToString());
            };
            #endif
        }
        
        private void BindToLocalCharacter() {
            LocalCharacter.Instance.ChangeCluster += (_, _) =>
                Common.Info("Changed cluster: "+LocalCharacter.Instance.Cluster);
            LocalCharacter.Instance.Move += (_, _) =>
                Common.Debug("Move: "+LocalCharacter.Instance.Position);
            LocalCharacter.Instance.MovingChanged += (_, _) =>
                Common.Info("Moving state changed: "+LocalCharacter.Instance.Moving);
            LocalCharacter.Instance.Interaction += (_, _) =>
                Common.Info("Interaction state changed: "+LocalCharacter.Instance.Interacting);
            LocalCharacter.Instance.Died += (_, _) =>
                Common.Warn("Character has died.");
        }

        private void BindToBot(BotManager bot) {
            bot.Started += (_, _) =>
                Common.Info("Bot started.");
            bot.Stopped += (_, _) =>
                Common.Info("Bot stopped.");
            
            if (bot is Bot.BotManager manager) {
                manager.JobChanged += (_, args) => {
                    if (args.Job is TradeMissionRun run) {
                        BindToTradeMissionRunJob(run);
                    }
                };
            }
        }

        private void BindToTradeMissionRunJob(TradeMissionRun run) {
            run.Progress += (_, e) =>
                Common.Info("Trade mission run step progression: "+RunRouteStepResolveName(e.Step));
            run.Reset += (_, _) =>
                Common.Info("Trade mission run reset!");
        }

        private object RunRouteStepResolveName(TradeMissionRun.TradeMissionStep step) => step switch {
            TradeMissionRun.FinishQuest => "Finish quest",
            TradeMissionRun.BankItems => "Bank items",
            TradeMissionRun.ProgressQuest => "Progress quest",
            TradeMissionRun.TakeQuest => "Take quest",
            TradeMissionRun.RunRouteBack => "Run route back",
            TradeMissionRun.RunRouteToDestination => "Run route",
            TradeMissionRun.RunToBank => "Run to bank",
            TradeMissionRun.RunToQuest => "Run to quest",
            _ => "Unknown step"
        };
    }
}
