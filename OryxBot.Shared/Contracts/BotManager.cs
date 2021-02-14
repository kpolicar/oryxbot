using System;

namespace OryxBot.Shared.Contracts
{
    public interface BotManager
    {
        event EventHandler? Started;
        event EventHandler? Stopped;
        
        void ToggleRun();
        void Stop();
    }
}
