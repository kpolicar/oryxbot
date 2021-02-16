using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Bot
{
    public abstract class Job : BotJob
    {
        public event EventHandler? Started;
        public event EventHandler? Stopped;

        public bool Running { get; private set; } = false;


        protected EventHandler<T> RuntimeEventListener<T>(EventHandler<T> callback) =>
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
    }
}
