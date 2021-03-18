using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Client.Windows.Bot.Events;
using OryxBot.Client.Windows.Bot.Exceptions;
using OryxBot.Client.Windows.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        public event EventHandler? Finished;
        internal event EventHandler<TradeMissionEvent>? Progress;
        internal event EventHandler? Reset;
        private const int DelayBetweenSteps = 1000;

        private TradeMissionStep _step;
        
        private static TradeMissionRouteProvider routeProvider = null!;
        private readonly LinkedList<TradeMissionRecord.RecordableStep> Route;
        private readonly LinkedList<TradeMissionRecord.RecordableStep> RouteBack;
        private static InputActionFactory actions = null!;
        
        private Thread? runningThread;
        private bool _paused;
        


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps, LinkedList<TradeMissionRecord.RecordableStep> stepsBack) {
            Route = steps;
            RouteBack = stepsBack;
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
            actions = (serviceContainer.GetService<ActionFactory>() as InputActionFactory)!;
        }

        public override void Start() {
            base.Start();
            if (!_paused)
                ResetRun();

            runningThread = new Thread(EntryPoint);
            runningThread.Start();
        }

        public override void Stop() {
            base.Stop();
            _paused = false;
        }

        public void Pause() {
            if (Running) {
                Stop();
                _paused = true;
            }
        }

        private void ResetRun() {
            _step = new RunToBank();
            Reset?.Invoke(this, EventArgs.Empty);
        }

        public void EntryPoint() {
            while (Running) {
                try {
                    _step.Tick();
                } catch (OperationCanceledException) {
                } catch (CharacterDiedException) {
                    Debug.WriteLine(">>>>>>>>>>>>>>>>>>> CHARACTER HAS DIED. RESPAWNING!!");
                    actions.Respawn();
                    Thread.Sleep(2000);
                    _step = new RunToBank();
                }

                if (_step.Finished) {
                    Thread.Sleep(DelayBetweenSteps);
                    ProgressToNextStep();
                } else {
                    Thread.Sleep(_step.Delay);
                }
            }

            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void ProgressToNextStep() {
            _step = _step switch {
                RunToBank => new BankItems(),
                BankItems => new RunToQuest(),
                RunToQuest => new TakeQuest(),
                TakeQuest => new RunRouteToDestination(Route),
                RunRouteToDestination => new ProgressQuest(),
                ProgressQuest => new RunRouteBack(RouteBack),
                RunRouteBack => new FinishQuest(),
                FinishQuest => new RunToBank(),
                _ => throw new ArgumentOutOfRangeException(nameof(_step))
            };
            Progress?.Invoke(this, new TradeMissionEvent(_step));
            Console.WriteLine("Progressed to next step: "+_step.GetType());
        }
    }
}
