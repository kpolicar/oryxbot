using System;
using OryxBot.Shared.Contracts;

namespace OryxBot.Client.Linux.Bot
{
    public abstract class Job : BotJob, IDisposable
    {
        public event EventHandler? Started;
        public event EventHandler? Stopped;

        public bool Running { get; private set; } = false;
        public abstract bool IsPaused { get; }


        protected EventHandler RuntimeEventListener(EventHandler callback) =>
            (sender, e) => {
                if (!Running)
                    return;
                callback(sender, e);
            };

        public void ToggleStart() {
            if (Running)
                Stop();
            else
                Start();
        }

        public virtual void Start() {
            Running = true;
            Started?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Stop() {
            Running = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() {
            if (Running)
                Stop();
        }
    }
}
