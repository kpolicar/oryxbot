using System;
using OryxBot.Client.Linux.Api;

namespace OryxBot.Client.Linux.Events
{
    public class ApiConnectionChangedEventArgs : EventArgs
    {
        public readonly ApiConnection? connection;

        public ApiConnectionChangedEventArgs(ApiConnection? connection) {
            this.connection = connection;
        }
    }
}
