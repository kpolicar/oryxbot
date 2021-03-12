using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OryxBot.Bot.Contracts;
using OryxBot.Bot.Exceptions;
using OryxBot.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Extensions;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        private const int ClusterAvgLoadTime = 5000;
        private const int DelayBetweenSteps = 1000;

        private TradeMissionStep _step;
        
        private static TradeMissionRouteProvider routeProvider = null!;
        private readonly LinkedList<TradeMissionRecord.RecordableStep> Route;
        private readonly LinkedList<TradeMissionRecord.RecordableStep> RouteBack;
        private static InputActionFactory actions = null!;
        
        private Thread? runningThread;


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
            //_step = new RunToBank();

            runningThread = new Thread(EntryPoint);
            runningThread.Start();
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
            Console.WriteLine("Progressed to next step: "+_step.GetType());
        }
    }
}
