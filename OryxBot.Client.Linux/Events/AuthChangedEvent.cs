using System;

namespace OryxBot.Client.Linux.Events
{
    public class AuthChangedEvent : EventArgs
    {
        public readonly Exception? Exception;
        public readonly bool Succeeded;

        public AuthChangedEvent(bool succeeded, Exception? exception = null) {
            (Succeeded, Exception) = (succeeded, exception);
        }
    }
}
