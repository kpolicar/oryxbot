using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun
    {
        public class TradeMissionMovementTracker : HasDependencies
        {
            private Thread? _thread;
            private bool running = false;
            private const int SleepDuration = 100;
            private const int StandStillDuration = 2500;
            private TradeMissionRun Bot;
            private Stopwatch sw = new();

            public TradeMissionMovementTracker(TradeMissionRun bot) =>
                Bot = bot;

            public void BindDependencies(ServiceContainer serviceContainer) {
                var dataProvider = serviceContainer.GetService<AlbionDataProvider>();
                dataProvider.Move += OnMove;
            }

            public void Start() {
                if (_thread == null || !_thread.IsAlive)
                    _thread = new Thread(EntryPoint);
                running = true;
                _thread.Start();
            }

            public void Stop() =>
                running = false;

            public void EntryPoint() {
                sw.Start();
                while (running) {
                    Bot.State.Moving = sw.ElapsedMilliseconds < StandStillDuration;
                    Thread.Sleep(SleepDuration);
                }
            }

            private void OnMove(object? sender, MoveEventArgs e) {
                Task.Run(() => {
                    var lastKnownMove = Bot.State.LastKnownMove;
                    if (lastKnownMove == null || lastKnownMove.Position.Equals(e.Position))
                        return;
                    sw.Restart();
                });
            }
        }
    }
}
