using System;

namespace OryxBot.Shared.Contracts
{
    public interface BotJob
    {
        event EventHandler? Started;
        event EventHandler? Stopped;
        
        void ToggleStart();
        void Start();
        void Stop();
    }
}
