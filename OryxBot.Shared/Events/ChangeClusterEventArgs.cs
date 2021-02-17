using System;

namespace OryxBot.Shared.Events
{
    public class ChangeClusterEventArgs : EventArgs
    {
        public readonly string Location;

        public ChangeClusterEventArgs(string location) =>
            Location = location;
    }
}
