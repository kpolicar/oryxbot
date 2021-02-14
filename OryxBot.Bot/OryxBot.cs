using System;
using System.ComponentModel.Design;
using OryxBot.Shared.Contracts;

namespace OryxBot.Bot
{
    public partial class OryxBot : BotManager
    {
        private OryxBotState state = new();


        public event EventHandler? Started;
        public event EventHandler? Stopped;

        public void ToggleRun() {
            state.Running = !state.Running;
            if (state.Running)
                Started?.Invoke(this, EventArgs.Empty);
            else
                Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Stop() {
            var previous = state.Running;
            state.Running = false;
            
            if (state.Running != previous)
                Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
