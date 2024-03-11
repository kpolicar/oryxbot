using System;
using System.Diagnostics;
using NLog;
using NLog.Targets;
using OryxBot.Albion.Protocol;
using OryxBot.Client.Linux.Bot;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Client.Linux.Events;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using BotManager = OryxBot.Shared.Contracts.BotManager;
using LoggerContract = OryxBot.Shared.Contracts.Logger;
using NLogger = NLog.Logger;

namespace OryxBot.Client.Linux.Services
{
    public class WebSocketLogForwarder : LoggerContract
    {
        public event EventHandler<LogEntry>? ReceivedLog;
        private readonly static WebSocketLogForwarder _instance = new WebSocketLogForwarder();
        public static WebSocketLogForwarder Instance => _instance;
        private WebSocketLogForwarder() {}

        public static void Init() {
            MethodCallTarget target = new MethodCallTarget() { Name = "log" };
            target.ClassName = typeof(WebSocketLogForwarder).AssemblyQualifiedName;
            target.MethodName = "Receive";
            target.Parameters.Add(new MethodCallParameter("${longdate}"));
            target.Parameters.Add(new MethodCallParameter("${level}"));
            target.Parameters.Add(new MethodCallParameter("${message}"));
 
            NLog.Config.LoggingConfiguration config = NLog.LogManager.Configuration;
            config.AddRuleForAllLevels(target);
            NLog.LogManager.Configuration = config;
        }
        
        public void FireReceivedForwardRequestEvent(string timestamp, string level, string message) {
            ReceivedLog?.Invoke(this, new LogEntry(timestamp, level, message));
        }
        
        public static void Receive(string timestamp, string level, string message) {
            Instance.FireReceivedForwardRequestEvent(timestamp, level, message);
        }
    }
}
