using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Client.Windows.Services
{
    public class FileLogger : Logger
    {
        private static NLog.Logger Logger = NLog.LogManager.GetLogger("log");

        public void BindToServices(ServiceContainer services) {
            var bot = services.GetService<BotManager>();
            
            BindToBot(bot);
        }

        private void BindToBot(BotManager bot) {
            bot.Started += (_, _) =>
                Logger.Info("Bot started.");
            bot.Stopped += (_, _) =>
                Logger.Info("Bot stopped.");
        }
    }
}
