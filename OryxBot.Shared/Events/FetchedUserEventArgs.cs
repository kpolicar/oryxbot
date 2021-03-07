using System;

namespace OryxBot.Shared.Events
{
    public class FetchedUserEventArgs : EventArgs
    {
        public readonly User user;

        public FetchedUserEventArgs(User user) {
            this.user = user;
        }
    }
}
