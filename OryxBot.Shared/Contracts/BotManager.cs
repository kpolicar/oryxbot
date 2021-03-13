using System;
using OryxBot.Shared.Events;

namespace OryxBot.Shared.Contracts
{
    public interface BotManager
    {
        event EventHandler<BotEventArgs>? Started;
        event EventHandler<BotEventArgs>? Stopped;
        bool IsRunning { get; }
        
        void ToggleTradeMissionRun();
        void ToggleTradeMissionRecord();
        void ToggleTradeMissionPause();
        void Stop();
    }
}
