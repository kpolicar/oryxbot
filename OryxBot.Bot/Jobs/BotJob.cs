using System.Threading;
using System.Threading.Tasks;

namespace OryxBot.Bot.Jobs
{
    public abstract class BotJob
    {
        public bool Running => !Task.IsCompleted;
        private Task Task;
        private CancellationTokenSource cancellationSource = new();

        public void Run() =>
            Task = Task.Run(EntryPoint, cancellationSource.Token);

        protected abstract void EntryPoint();

        public void Stop() =>
            cancellationSource.Cancel();
    }
}
