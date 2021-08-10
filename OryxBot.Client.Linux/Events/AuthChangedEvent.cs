using System;

namespace OryxBot.Client.Linux.Events
{
    public class AuthChangedEvent : EventArgs
    {
        public readonly bool Succeeded;
        public readonly Exception? Exception;

        public AuthChangedEvent(bool succeeded, Exception? exception = null) =>
            (Succeeded, Exception) = (succeeded, exception);
    }
}
