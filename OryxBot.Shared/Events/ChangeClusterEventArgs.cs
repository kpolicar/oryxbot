using System;

namespace OryxBot.Shared.Events
{
    public class ChangeClusterEventArgs : EventArgs
    {
        public readonly string Location;
        public readonly string? Alias;

        public ChangeClusterEventArgs(string location, string? alias=null) =>
            (Location, Alias) = (location, alias);
    }
}
