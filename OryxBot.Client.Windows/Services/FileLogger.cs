using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using OryxBot.Albion.Protocol;
using OryxBot.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using LoggerContract = OryxBot.Shared.Contracts.Logger;
using NLogger=NLog.Logger;

namespace OryxBot.Client.Windows.Services
{
    public class FileLogger : LoggerContract
    {
        private static readonly NLogger Common = LogManager.GetLogger("log");
        private static readonly NLogger NetworkEvent = LogManager.GetLogger("networkevent");
        private static readonly NLogger NetworkRequest = LogManager.GetLogger("networkrequest");

        public void BindToServices(ServiceContainer services) {
            var bot = services.GetService<BotManager>();
            var dataProvider = (NetworkAlbionDataProvider) services.GetService<AlbionDataProvider>();
            
            BindToDataProvider(dataProvider);
            BindToBot(bot);
        }

        private void BindToDataProvider(NetworkAlbionDataProvider dataProvider) {
            dataProvider.NetworkEvent += (_, packet) => {
                var evcode = (EventCodes) packet.EventCode;
                NetworkEvent.Info(evcode);
            };
            dataProvider.NetworkRequest += (_, packet) => {
                var opcode = (OperationCodes) packet.OperationCode;
                NetworkRequest.Info(opcode);
            };
        }

        private void BindToBot(BotManager bot) {
            bot.Started += (_, _) =>
                Common.Info("Bot started.");
            bot.Stopped += (_, _) =>
                Common.Info("Bot stopped.");
        }
    }
}
