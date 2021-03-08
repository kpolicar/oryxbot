using System;
using System.Collections.Generic;
using System.Threading;
using OryxBot.Bot.Contracts;
using OryxBot.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        private const int ClusterAvgLoadTime = 5000;

        private TradeMissionStep _step = new BankItems();
        
        private AlbionDataProvider dataProvider = null!;
        private static TradeMissionRouteProvider routeProvider = null!;
        private LinkedList<TradeMissionRecord.RecordableStep> Route;
        private LinkedList<TradeMissionRecord.RecordableStep> RouteBack;
        private static InputActionFactory actions = null!;
        
        private Thread? runningThread;


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps, LinkedList<TradeMissionRecord.RecordableStep> stepsBack) {
            Route = steps;
            RouteBack = stepsBack;
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
            actions = (serviceContainer.GetService<ActionFactory>() as InputActionFactory)!;
        }

        public override void Start() {
            base.Start();

            runningThread = new Thread(EntryPoint);
            runningThread.Start();
        }

        public override void Stop() {
            base.Stop();
            _step = new BankItems();
        }

        public void EntryPoint() {
            while (Running) {
                _step.Tick();
                Thread.Sleep(_step.Delay);
                
                if (_step.Finished)
                    ProgressToNextStep();
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
        }
    }
}
