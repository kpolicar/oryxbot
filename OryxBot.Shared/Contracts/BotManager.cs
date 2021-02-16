using System;
using System.Collections.Generic;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Shared.Contracts
{
    public interface BotManager
    {
        event EventHandler<BotEventArgs>? Started;
        event EventHandler<BotEventArgs>? Stopped;

        void ToggleTradeMissionRun();
        void ToggleTradeMissionRecord();
    }
}
