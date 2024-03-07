using System;

namespace OryxBot.Client.Linux.Events
{
    public class LogEntry : EventArgs
    {
        public readonly string Timestamp;
        public readonly string Level;
        public readonly string Message;

        public LogEntry(string timestamp, string level, string message) {
            (Timestamp, Level, Message) = (timestamp, level, message);
        }
    }
}
