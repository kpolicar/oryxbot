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
        bool IsPaused { get; }
        RecordingConfiguration RecordingConfig { get; }
        RunConfiguration RunConfig { get; }

        void ToggleTradeMissionRun();
        void ToggleTradeMissionRecord();
        void ToggleTradeMissionPause();
        void Stop();
        void SetRecordingConfiguration(RecordingConfiguration configuration);
        void SetRunConfiguration(RunConfiguration configuration);
    }
}
