using System;
using OryxBot.Shared.Contracts;

namespace OryxBot.Shared.Events
{
    public class BotEventArgs : EventArgs
    {
        public readonly BotJob Job;

        public BotEventArgs(BotJob job) =>
            Job = job;
    }
}
