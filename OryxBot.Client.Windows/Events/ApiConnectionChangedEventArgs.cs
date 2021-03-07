using System;
using OryxBot.Client.Windows.Api;

namespace OryxBot.Client.Windows.Events
{
    public class ApiConnectionChangedEventArgs : EventArgs
    {
        public readonly ApiConnection? connection;

        public ApiConnectionChangedEventArgs(ApiConnection? connection) {
            this.connection = connection;
        }
    }
}
