using System;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;

namespace OryxBot.Shared.Contracts
{
    public interface BotManager
    {
        event EventHandler<BotEventArgs>? Started;
        event EventHandler<BotEventArgs>? Stopped;
        bool IsRunning { get; }
        City ActiveCity { get; }
        
        void ToggleTradeMissionRun();
        void ToggleTradeMissionRecord();
        void ToggleTradeMissionPause();
        void Stop();
        void SetActiveCity(City city);
    }
}
