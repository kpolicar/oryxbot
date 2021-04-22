using System;

namespace OryxBot.Shared.Contracts
{
    public interface BotJob
    {
        bool IsPaused { get; }
        bool Running { get; }
        event EventHandler? Started;
        event EventHandler? Stopped;
        
        void ToggleStart();
        void Start();
        void Stop();
    }
}
